using System;
using System.Windows;

namespace BiochemSimulator
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Set global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            try
            {
                // Show profile selection window first
                var profileWindow = new ProfileSelectionWindow();
                var result = profileWindow.ShowDialog();

                if (result == true && profileWindow.SelectedProfile != null)
                {
                    // Profile selected, launch main game window
                    MessageBox.Show($"About to create MainWindow for profile: {profileWindow.SelectedProfile.PlayerName}",
                        "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                    var mainWindow = new MainWindow(profileWindow.SelectedProfile);

                    MessageBox.Show("MainWindow created successfully. About to show window.",
                        "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                    mainWindow.Show();

                    MessageBox.Show("MainWindow.Show() called successfully.",
                        "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // No profile selected or user cancelled, exit application
                    MessageBox.Show("No profile selected. Shutting down.",
                        "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Critical error: {e.ExceptionObject}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Application error: {e.Exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
