using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace BiochemSimulator.UITests.Infrastructure
{
    /// <summary>
    /// Base class for UI tests providing app lifecycle management and common utilities
    /// </summary>
    public abstract class UITestBase : IDisposable
    {
        protected Application? App { get; private set; }
        protected UIA3Automation? Automation { get; private set; }
        protected Window? MainWindow { get; private set; }
        protected ConditionFactory? CF => Automation?.ConditionFactory;

        private static readonly string AppPath = GetApplicationPath();
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        private bool _disposed;

        /// <summary>
        /// Checks if running on Windows platform
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets the path to the built application executable
        /// </summary>
        private static string GetApplicationPath()
        {
            // Try to find the executable in various locations
            var possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Debug", "net8.0-windows", "BiochemSimulator.exe"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Release", "net8.0-windows", "BiochemSimulator.exe"),
                Path.Combine(AppContext.BaseDirectory, "BiochemSimulator.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "BiochemSimulator.exe")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Default to assuming it's in the output directory
            return Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Debug", "net8.0-windows", "BiochemSimulator.exe");
        }

        /// <summary>
        /// Launches the application and waits for the main window
        /// </summary>
        protected void LaunchApplication()
        {
            if (!IsWindows)
            {
                throw new PlatformNotSupportedException("UI tests can only run on Windows");
            }

            if (!File.Exists(AppPath))
            {
                throw new FileNotFoundException(
                    $"Application not found at: {AppPath}. Please build the application first.",
                    AppPath);
            }

            Automation = new UIA3Automation();
            App = Application.Launch(AppPath);

            // Wait for the main window to appear
            MainWindow = WaitForMainWindow();
        }

        /// <summary>
        /// Waits for the main window to become available
        /// </summary>
        private Window WaitForMainWindow()
        {
            var retry = Retry.WhileNull(
                () => App?.GetMainWindow(Automation!),
                DefaultTimeout,
                TimeSpan.FromMilliseconds(500));

            if (!retry.Success || retry.Result == null)
            {
                throw new TimeoutException("Main window did not appear within timeout");
            }

            return retry.Result;
        }

        /// <summary>
        /// Waits for a profile selection window if it appears, and creates/selects a test profile
        /// </summary>
        protected void HandleProfileSelection(string profileName = "TestProfile")
        {
            // Check if profile selection window appeared
            var profileWindow = WaitForWindow("Select Profile", TimeSpan.FromSeconds(3));

            if (profileWindow != null)
            {
                // Look for "Create New" button
                var createButton = profileWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                    .And(cf.ByName("Create New")));

                if (createButton != null)
                {
                    createButton.Click();

                    // Wait for name input
                    var nameInput = WaitForElement(profileWindow,
                        cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit),
                        TimeSpan.FromSeconds(2));

                    if (nameInput != null)
                    {
                        nameInput.AsTextBox().Text = profileName;

                        // Click Create/OK button
                        var okButton = profileWindow.FindFirstDescendant(cf =>
                            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                            .And(cf.ByName("Create")));

                        okButton?.Click();
                    }
                }
                else
                {
                    // Select existing profile
                    var listBox = profileWindow.FindFirstDescendant(cf =>
                        cf.ByControlType(FlaUI.Core.Definitions.ControlType.List));

                    if (listBox != null)
                    {
                        var items = listBox.FindAllChildren();
                        if (items.Length > 0)
                        {
                            items[0].Click();

                            var selectButton = profileWindow.FindFirstDescendant(cf =>
                                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                                .And(cf.ByName("Select")));

                            selectButton?.Click();
                        }
                    }
                }

                // Wait for profile window to close and main window to be ready
                WaitForWindowToClose(profileWindow, TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Waits for a window with the specified title
        /// </summary>
        protected Window? WaitForWindow(string title, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? DefaultTimeout;
            var retry = Retry.WhileNull(
                () => App?.GetAllTopLevelWindows(Automation!)
                    .FirstOrDefault(w => w.Title.Contains(title)),
                actualTimeout,
                TimeSpan.FromMilliseconds(200));

            return retry.Result;
        }

        /// <summary>
        /// Waits for a window to close
        /// </summary>
        protected void WaitForWindowToClose(Window window, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? DefaultTimeout;
            Retry.WhileFalse(
                () =>
                {
                    try { return window.IsOffscreen; }
                    catch { return true; }
                },
                actualTimeout,
                TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// Waits for an element matching the condition
        /// </summary>
        protected AutomationElement? WaitForElement(
            AutomationElement parent,
            Func<ConditionFactory, ConditionBase> conditionFunc,
            TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? DefaultTimeout;
            var retry = Retry.WhileNull(
                () => parent.FindFirstDescendant(conditionFunc),
                actualTimeout,
                TimeSpan.FromMilliseconds(200));

            return retry.Result;
        }

        /// <summary>
        /// Finds a button by name in the main window
        /// </summary>
        protected Button? FindButton(string name)
        {
            var element = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                .And(cf.ByName(name)));

            return element?.AsButton();
        }

        /// <summary>
        /// Finds a menu item by name
        /// </summary>
        protected MenuItem? FindMenuItem(string menuName, string itemName)
        {
            var menu = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu));

            if (menu != null)
            {
                var menuItem = menu.FindFirstDescendant(cf => cf.ByName(menuName));
                menuItem?.Click();

                Wait.UntilInputIsProcessed(TimeSpan.FromMilliseconds(200));

                return MainWindow?.FindFirstDescendant(cf => cf.ByName(itemName))?.AsMenuItem();
            }

            return null;
        }

        /// <summary>
        /// Finds a text block by automation ID or name
        /// </summary>
        protected TextBox? FindTextBox(string name)
        {
            var element = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
                .And(cf.ByName(name)));

            return element?.AsTextBox();
        }

        /// <summary>
        /// Finds a combo box by name
        /// </summary>
        protected ComboBox? FindComboBox(string name)
        {
            var element = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox)
                .And(cf.ByName(name)));

            return element?.AsComboBox();
        }

        /// <summary>
        /// Finds a list box by name
        /// </summary>
        protected ListBox? FindListBox(string name)
        {
            var element = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.List)
                .And(cf.ByName(name)));

            return element?.AsListBox();
        }

        /// <summary>
        /// Takes a screenshot of the main window for debugging
        /// </summary>
        protected void TakeScreenshot(string fileName)
        {
            try
            {
                var screenshot = MainWindow?.Capture();
                if (screenshot != null)
                {
                    var path = Path.Combine(
                        AppContext.BaseDirectory,
                        "Screenshots",
                        $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    screenshot.ToFile(path);
                }
            }
            catch
            {
                // Ignore screenshot failures
            }
        }

        /// <summary>
        /// Waits for a specified time (use sparingly)
        /// </summary>
        protected static void Wait(TimeSpan duration)
        {
            Thread.Sleep(duration);
        }

        /// <summary>
        /// Closes any message boxes that may have appeared
        /// </summary>
        protected void CloseMessageBoxes()
        {
            var messageBoxes = App?.GetAllTopLevelWindows(Automation!)
                .Where(w => w.ClassName == "#32770") // Standard Windows message box class
                .ToList();

            if (messageBoxes != null)
            {
                foreach (var messageBox in messageBoxes)
                {
                    var okButton = messageBox.FindFirstDescendant(cf =>
                        cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                        .And(cf.ByName("OK")));

                    okButton?.Click();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        App?.Close();
                        App?.Dispose();
                    }
                    catch
                    {
                        // Force kill if graceful close fails
                        try { App?.Kill(); } catch { }
                    }

                    Automation?.Dispose();
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Trait for marking tests that require Windows
    /// </summary>
    public class WindowsOnlyFactAttribute : FactAttribute
    {
        public WindowsOnlyFactAttribute()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "This test only runs on Windows";
            }
        }
    }

    /// <summary>
    /// Trait for marking theories that require Windows
    /// </summary>
    public class WindowsOnlyTheoryAttribute : TheoryAttribute
    {
        public WindowsOnlyTheoryAttribute()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "This test only runs on Windows";
            }
        }
    }
}
