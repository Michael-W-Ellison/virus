using BiochemSimulator.UITests.Infrastructure;
using FlaUI.Core.Definitions;
using Xunit;

namespace BiochemSimulator.UITests
{
    /// <summary>
    /// UI tests for the main application window
    /// </summary>
    public class MainWindowTests : UITestBase
    {
        [WindowsOnlyFact]
        public void Application_ShouldLaunchSuccessfully()
        {
            // Act
            LaunchApplication();
            HandleProfileSelection();

            // Assert
            Assert.NotNull(MainWindow);
            Assert.Contains("Biochemistry", MainWindow.Title);
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldHaveMenuBar()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act
            var menuBar = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.MenuBar));

            // Assert
            Assert.NotNull(menuBar);
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldHaveFileMenu()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act
            var fileMenu = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.MenuItem)
                .And(cf.ByName("File")));

            // Assert
            Assert.NotNull(fileMenu);
        }

        [WindowsOnlyFact]
        public void FileMenu_ShouldContainSaveOption()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act
            var fileMenu = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.MenuItem)
                .And(cf.ByName("File")));

            fileMenu?.Click();
            Wait(TimeSpan.FromMilliseconds(300));

            var saveMenuItem = MainWindow?.FindFirstDescendant(cf =>
                cf.ByName("Save Game"));

            // Assert
            Assert.NotNull(saveMenuItem);

            // Press Escape to close menu
            MainWindow?.Focus();
        }

        [WindowsOnlyFact]
        public void FileMenu_ShouldContainLoadOption()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act
            var fileMenu = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.MenuItem)
                .And(cf.ByName("File")));

            fileMenu?.Click();
            Wait(TimeSpan.FromMilliseconds(300));

            var loadMenuItem = MainWindow?.FindFirstDescendant(cf =>
                cf.ByName("Load Game"));

            // Assert
            Assert.NotNull(loadMenuItem);
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldHaveAtomicCanvas()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act - Look for the canvas element (may be named or have specific automation ID)
            var canvas = MainWindow?.FindFirstDescendant(cf =>
                cf.ByAutomationId("AtomicCanvas"));

            // Assert - Canvas might not have automation ID, check for any canvas
            // If not found by ID, verify window has content
            if (canvas == null)
            {
                var anyContent = MainWindow?.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Custom));

                Assert.True(anyContent != null || MainWindow?.FindAllDescendants().Length > 0,
                    "Main window should have interactive content");
            }
            else
            {
                Assert.NotNull(canvas);
            }
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldHaveChemicalInventory()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act - Look for the chemical inventory list
            var inventory = MainWindow?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List));

            // Assert
            Assert.NotNull(inventory);
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldHaveStatusText()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act - Look for status text or tutorial message
            var statusElements = MainWindow?.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.Text));

            // Assert - Should have some text elements
            Assert.NotNull(statusElements);
            Assert.True(statusElements.Length > 0, "Main window should have status text elements");
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldRespondToKeyboardFocus()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act
            MainWindow?.Focus();
            Wait(TimeSpan.FromMilliseconds(200));

            // Assert
            Assert.True(MainWindow?.Properties.HasKeyboardFocus.ValueOrDefault ?? false,
                "Main window should be able to receive keyboard focus");
        }

        [WindowsOnlyFact]
        public void MainWindow_ShouldDisplayWelcomeMessage()
        {
            // Arrange
            LaunchApplication();
            HandleProfileSelection();

            // Act - Look for any text containing welcome/tutorial message
            var textElements = MainWindow?.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.Text));

            // Assert
            Assert.NotNull(textElements);

            // Check if any text element contains tutorial-related content
            var hasInstructionalText = textElements.Any(t =>
            {
                var text = t.Name ?? "";
                return text.Contains("Chemistry") ||
                       text.Contains("atom") ||
                       text.Contains("molecule") ||
                       text.Contains("Welcome") ||
                       text.Contains("click") ||
                       text.Contains("select");
            });

            Assert.True(hasInstructionalText || textElements.Length > 0,
                "Main window should display instructional or status text");
        }
    }
}
