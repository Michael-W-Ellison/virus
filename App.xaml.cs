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
                    var mainWindow = new MainWindow(profileWindow.SelectedProfile);
                    mainWindow.Show();
                }
                else
                {
                    // No profile selected or user cancelled, exit application
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
