using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiochemSimulator
{
    public partial class ProfileSelectionWindow : Window
    {
        private readonly SaveManager _saveManager;
        private PlayerProfile? _selectedProfile;

        public PlayerProfile? SelectedProfile => _selectedProfile;

        public ProfileSelectionWindow()
        {
            InitializeComponent();
            _saveManager = new SaveManager();
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            try
            {
                var profiles = _saveManager.GetAllProfiles();
                ProfilesList.ItemsSource = profiles;

                if (profiles.Count == 0)
                {
                    // No profiles exist, focus on creating one
                    NewProfileNameTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profiles: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfileItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string playerName)
            {
                try
                {
                    _selectedProfile = _saveManager.LoadProfile(playerName);

                    if (_selectedProfile != null)
                    {
                        PlayButton.IsEnabled = true;
                        DeleteProfileButton.IsEnabled = true;

                        // Visual feedback - highlight selected profile
                        foreach (var item in ProfilesList.Items)
                        {
                            var container = ProfilesList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                            if (container != null)
                            {
                                var itemBorder = FindVisualChild<Border>(container);
                                if (itemBorder != null)
                                {
                                    if (itemBorder.Tag as string == playerName)
                                    {
                                        itemBorder.BorderBrush = System.Windows.Media.Brushes.Cyan;
                                        itemBorder.BorderThickness = new Thickness(2);
                                    }
                                    else
                                    {
                                        itemBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush");
                                        itemBorder.BorderThickness = new Thickness(1);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading profile: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            string playerName = NewProfileNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(playerName))
            {
                MessageBox.Show("Please enter a player name.", "Invalid Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (playerName.Length < 3)
            {
                MessageBox.Show("Player name must be at least 3 characters.", "Invalid Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_saveManager.ProfileExists(playerName))
            {
                MessageBox.Show("A profile with this name already exists.", "Duplicate Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newProfile = new PlayerProfile
                {
                    PlayerName = playerName,
                    CreatedDate = DateTime.Now,
                    LastPlayedDate = DateTime.Now
                };

                _saveManager.SaveProfile(newProfile);
                _selectedProfile = newProfile;

                LoadProfiles();
                NewProfileNameTextBox.Clear();

                PlayButton.IsEnabled = true;
                DeleteProfileButton.IsEnabled = true;

                MessageBox.Show($"Profile '{playerName}' created successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile == null)
            {
                MessageBox.Show("Please select a profile to delete.", "No Profile Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the profile '{_selectedProfile.PlayerName}'?\n\n" +
                "This will delete all saves and progress for this profile. This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _saveManager.DeleteProfile(_selectedProfile.PlayerName);
                    _selectedProfile = null;

                    LoadProfiles();
                    PlayButton.IsEnabled = false;
                    DeleteProfileButton.IsEnabled = false;

                    MessageBox.Show("Profile deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting profile: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile == null)
            {
                MessageBox.Show("Please select a profile.", "No Profile Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Helper method to find visual children
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
