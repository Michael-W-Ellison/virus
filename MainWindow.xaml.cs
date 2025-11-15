using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BiochemSimulator
{
    public partial class MainWindow : Window
    {
        private GameManager _gameManager;
        private DispatcherTimer _gameTimer;
        private DispatcherTimer _microscopeTimer;
        private Chemical? _selectedWeapon;
        private Dictionary<Point, ChemicalSpray> _activeChemicals;
        private Random _random;

        public MainWindow()
        {
            InitializeComponent();
            _random = new Random();
            _activeChemicals = new Dictionary<Point, ChemicalSpray>();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            _gameManager = new GameManager(screenWidth, screenHeight);
            _gameManager.StateChanged += OnStateChanged;
            _gameManager.PhaseChanged += OnPhaseChanged;
            _gameManager.TutorialMessageChanged += OnTutorialMessageChanged;
            _gameManager.AlarmTriggered += OnAlarmTriggered;

            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
            };
            _gameTimer.Tick += GameTimer_Tick;

            _microscopeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _microscopeTimer.Tick += MicroscopeTimer_Tick;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            _gameManager.StartGame();
            StartButton.Visibility = Visibility.Collapsed;
            UpdateChemicalInventory();
            _gameTimer.Start();
        }

        private void OnStateChanged(object? sender, GameState state)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"State: {state}";

                switch (state)
                {
                    case GameState.BiochemSimulator:
                        SimulatorView.Visibility = Visibility.Visible;
                        DesktopOverlay.Visibility = Visibility.Collapsed;
                        DesktopUI.Visibility = Visibility.Collapsed;
                        UpdateChemicalInventory();
                        break;

                    case GameState.ObservingLife:
                        MicroscopeView.Visibility = Visibility.Visible;
                        TrashCan.Visibility = Visibility.Visible;
                        StartMicroscopeAnimation();
                        break;

                    case GameState.VirusOutbreak:
                        SimulatorView.Visibility = Visibility.Collapsed;
                        DesktopOverlay.Visibility = Visibility.Visible;
                        DesktopUI.Visibility = Visibility.Visible;
                        OrganismStats.Visibility = Visibility.Visible;
                        UpdateWeaponInventory();
                        break;

                    case GameState.Victory:
                        ShowVictoryScreen();
                        break;

                    case GameState.GameOver:
                        ShowGameOverScreen();
                        break;
                }
            });
        }

        private void OnPhaseChanged(object? sender, ExperimentPhase phase)
        {
            Dispatcher.Invoke(() =>
            {
                PhaseText.Text = phase.ToString();
                UpdateChemicalInventory();
            });
        }

        private void OnTutorialMessageChanged(object? sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                TutorialText.Text = message;
            });
        }

        private void OnAlarmTriggered(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayAlarmAnimation();
            });
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            _gameManager.Update(0.05);

            if (_gameManager.CurrentState == GameState.VirusOutbreak ||
                _gameManager.CurrentState == GameState.ChemicalWarfare)
            {
                UpdateOrganismDisplay();
                UpdateOrganismStats();
                UpdateChemicalSprays();
            }
        }

        private void UpdateChemicalInventory()
        {
            var chemicals = _gameManager.GetAvailableChemicalsForCurrentPhase();
            ChemicalInventory.ItemsSource = chemicals;
        }

        private void UpdateWeaponInventory()
        {
            var weapons = _gameManager.Chemistry.GetDisinfectants();
            WeaponInventory.ItemsSource = weapons;
        }

        private void ChemicalButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string chemicalName)
            {
                var chemical = _gameManager.Chemistry.GetChemical(chemicalName);
                if (chemical != null)
                {
                    _gameManager.AddChemicalToBeaker(chemical);
                    UpdateBeakerDisplay();
                }
            }
        }

        private void WeaponButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string chemicalName)
            {
                _selectedWeapon = _gameManager.Chemistry.GetChemical(chemicalName);
                SelectedWeaponText.Text = $"Selected: {_selectedWeapon?.Name ?? "None"}";
            }
        }

        private void UpdateBeakerDisplay()
        {
            BeakerContents.ItemsSource = null;
            BeakerContents.ItemsSource = _gameManager.CurrentBeaker;

            if (_gameManager.CurrentBeaker.Count == 0)
            {
                BeakerContentText.Text = "Empty";
            }
            else
            {
                BeakerContentText.Text = string.Join(", ",
                    _gameManager.CurrentBeaker.Select(c => c.Name));
            }

            // Visual representation
            BeakerCanvas.Children.Clear();
            double yPos = BeakerCanvas.Height - 50;

            foreach (var chemical in _gameManager.CurrentBeaker)
            {
                var rect = new Rectangle
                {
                    Width = BeakerCanvas.Width,
                    Height = 30,
                    Fill = new SolidColorBrush(chemical.Color),
                    Opacity = 0.6
                };
                Canvas.SetLeft(rect, 0);
                Canvas.SetTop(rect, yPos);
                BeakerCanvas.Children.Add(rect);
                yPos -= 35;
            }
        }

        private void ClearBeaker_Click(object sender, RoutedEventArgs e)
        {
            _gameManager.ClearBeaker();
            UpdateBeakerDisplay();
        }

        private void StartMicroscopeAnimation()
        {
            MicroscopeCanvas.Children.Clear();
            _microscopeTimer.Start();
        }

        private void MicroscopeTimer_Tick(object? sender, EventArgs e)
        {
            MicroscopeCanvas.Children.Clear();

            // Draw animated cells
            for (int i = 0; i < 5; i++)
            {
                var cell = new Ellipse
                {
                    Width = 20 + _random.Next(20),
                    Height = 20 + _random.Next(20),
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)(100 + _random.Next(100)),
                        (byte)(150 + _random.Next(50)),
                        (byte)(50 + _random.Next(50))
                    )),
                    Opacity = 0.7
                };

                double x = _random.NextDouble() * (MicroscopeCanvas.Width - 40);
                double y = _random.NextDouble() * (MicroscopeCanvas.Height - 40);

                Canvas.SetLeft(cell, x);
                Canvas.SetTop(cell, y);
                MicroscopeCanvas.Children.Add(cell);

                // Add nucleus
                var nucleus = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.DarkRed
                };
                Canvas.SetLeft(nucleus, x + cell.Width / 2 - 4);
                Canvas.SetTop(nucleus, y + cell.Height / 2 - 4);
                MicroscopeCanvas.Children.Add(nucleus);
            }
        }

        private void TrashCan_MouseEnter(object sender, MouseEventArgs e)
        {
            TrashCan.Background = new SolidColorBrush(Color.FromRgb(192, 57, 43));
        }

        private void TrashCan_MouseLeave(object sender, MouseEventArgs e)
        {
            TrashCan.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60));
        }

        private void TrashCan_Click(object sender, MouseButtonEventArgs e)
        {
            _microscopeTimer.Stop();
            _gameManager.DisposeOrganismsInTrash();
            MicroscopeView.Visibility = Visibility.Collapsed;
            TrashCan.Visibility = Visibility.Collapsed;
        }

        private void UpdateOrganismDisplay()
        {
            DesktopOverlay.Children.Clear();

            foreach (var organism in _gameManager.Organisms.Organisms.Where(o => o.IsAlive))
            {
                var ellipse = new Ellipse
                {
                    Width = organism.Size,
                    Height = organism.Size,
                    Fill = new RadialGradientBrush(
                        organism.Color,
                        Color.FromArgb(100, organism.Color.R, organism.Color.G, organism.Color.B)
                    ),
                    Opacity = 0.7 + (organism.Health / 200.0)
                };

                Canvas.SetLeft(ellipse, organism.Position.X - organism.Size / 2);
                Canvas.SetTop(ellipse, organism.Position.Y - organism.Size / 2);

                DesktopOverlay.Children.Add(ellipse);

                // Add fuzzy effect for more evolved organisms
                if (organism.Generation > 3)
                {
                    ellipse.Effect = new System.Windows.Media.Effects.BlurEffect
                    {
                        Radius = organism.Generation / 2.0
                    };
                }
            }

            // Draw active chemical sprays
            foreach (var spray in _activeChemicals.Values.ToList())
            {
                var sprayEllipse = new Ellipse
                {
                    Width = spray.Radius * 2,
                    Height = spray.Radius * 2,
                    Fill = new RadialGradientBrush(
                        Color.FromArgb(100, spray.Color.R, spray.Color.G, spray.Color.B),
                        Colors.Transparent
                    ),
                    Opacity = spray.Opacity
                };

                Canvas.SetLeft(sprayEllipse, spray.Location.X - spray.Radius);
                Canvas.SetTop(sprayEllipse, spray.Location.Y - spray.Radius);

                DesktopOverlay.Children.Add(sprayEllipse);
            }
        }

        private void UpdateOrganismStats()
        {
            AliveCountText.Text = _gameManager.Organisms.GetAliveCount().ToString();
            GenerationText.Text = _gameManager.Organisms.GenerationsEvolved.ToString();
            TotalCreatedText.Text = _gameManager.Organisms.TotalOrganismsCreated.ToString();

            var resistances = _gameManager.Organisms.GetResistanceStats();
            ResistanceList.Items.Clear();

            foreach (var resistance in resistances)
            {
                var panel = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };

                var nameText = new TextBlock
                {
                    Text = resistance.Key,
                    Foreground = Brushes.White,
                    FontSize = 11
                };

                var progressBar = new ProgressBar
                {
                    Value = resistance.Value * 100,
                    Maximum = 100,
                    Height = 8,
                    Foreground = resistance.Value > 0.5 ? Brushes.Red : Brushes.Orange
                };

                panel.Children.Add(nameText);
                panel.Children.Add(progressBar);

                ResistanceList.Items.Add(panel);
            }
        }

        private void DesktopOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedWeapon != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point location = e.GetPosition(DesktopOverlay);
                double radius = 80;

                _gameManager.ApplyChemicalToDesktop(_selectedWeapon, location, radius);

                // Add visual spray effect
                var spray = new ChemicalSpray
                {
                    Location = location,
                    Radius = radius,
                    Color = _selectedWeapon.Color,
                    Opacity = 0.6,
                    Chemical = _selectedWeapon
                };

                _activeChemicals[location] = spray;

                // Check for chemical interactions
                CheckChemicalInteractions(location, _selectedWeapon);

                // Handle caustic effects
                if (_selectedWeapon.IsCaustic)
                {
                    ApplyCausticEffect(location, radius);
                }
            }
        }

        private void UpdateChemicalSprays()
        {
            var toRemove = new List<Point>();

            foreach (var kvp in _activeChemicals)
            {
                kvp.Value.Opacity -= 0.01;
                if (kvp.Value.Opacity <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var point in toRemove)
            {
                _activeChemicals.Remove(point);
            }
        }

        private void CheckChemicalInteractions(Point location, Chemical newChemical)
        {
            foreach (var spray in _activeChemicals.Values)
            {
                double distance = Math.Sqrt(
                    Math.Pow(spray.Location.X - location.X, 2) +
                    Math.Pow(spray.Location.Y - location.Y, 2)
                );

                if (distance < 100 && spray.Chemical.Name != newChemical.Name)
                {
                    var reaction = _gameManager.Chemistry.TryReact(
                        new List<Chemical> { spray.Chemical, newChemical });

                    if (reaction != null)
                    {
                        ShowReactionEffect(location, reaction);
                    }

                    if (_gameManager.Chemistry.WillReactDangerously(spray.Chemical, newChemical))
                    {
                        ShowDangerousReactionWarning(location);
                    }
                }
            }
        }

        private void ShowReactionEffect(Point location, ChemicalReaction reaction)
        {
            if (reaction.VisualEffect.HasValue)
            {
                var effect = new Ellipse
                {
                    Width = 150,
                    Height = 150,
                    Fill = new SolidColorBrush(reaction.VisualEffect.Value),
                    Opacity = 0.8
                };

                Canvas.SetLeft(effect, location.X - 75);
                Canvas.SetTop(effect, location.Y - 75);
                DesktopOverlay.Children.Add(effect);

                var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(2));
                fadeOut.Completed += (s, e) => DesktopOverlay.Children.Remove(effect);
                effect.BeginAnimation(OpacityProperty, fadeOut);

                MessageBox.Show(reaction.VisualDescription, "Chemical Reaction!",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowDangerousReactionWarning(Point location)
        {
            MessageBox.Show("⚠️ DANGEROUS REACTION! Toxic fumes generated!",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Visual warning effect
            var warning = new Border
            {
                Width = 200,
                Height = 200,
                Background = new SolidColorBrush(Colors.Yellow),
                Opacity = 0.7,
                CornerRadius = new CornerRadius(100)
            };

            Canvas.SetLeft(warning, location.X - 100);
            Canvas.SetTop(warning, location.Y - 100);
            DesktopOverlay.Children.Add(warning);

            var blink = new DoubleAnimation { From = 0.7, To = 0, Duration = TimeSpan.FromSeconds(1) };
            blink.Completed += (s, e) => DesktopOverlay.Children.Remove(warning);
            warning.BeginAnimation(OpacityProperty, blink);
        }

        private void ApplyCausticEffect(Point location, double radius)
        {
            // Create "melting" effect
            var melt = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Color.FromArgb(100, 50, 50, 50)),
                Opacity = 0.5
            };

            Canvas.SetLeft(melt, location.X - radius);
            Canvas.SetTop(melt, location.Y - radius);
            DesktopOverlay.Children.Add(melt);

            // Slow fade
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(5));
            melt.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void PlayAlarmAnimation()
        {
            AlarmOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 0.95, TimeSpan.FromSeconds(0.5));
            var fadeOut = new DoubleAnimation(0.95, 0, TimeSpan.FromSeconds(0.5))
            {
                BeginTime = TimeSpan.FromSeconds(2.5)
            };

            fadeOut.Completed += (s, e) => AlarmOverlay.Visibility = Visibility.Collapsed;

            AlarmOverlay.BeginAnimation(OpacityProperty, fadeIn);
            Task.Delay(2500).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => AlarmOverlay.BeginAnimation(OpacityProperty, fadeOut));
            });
        }

        private void ShowVictoryScreen()
        {
            MessageBox.Show(
                $"🎉 VICTORY! 🎉\n\n" +
                $"You've successfully eradicated the biohazard!\n\n" +
                $"Statistics:\n" +
                $"Total Organisms Created: {_gameManager.Organisms.TotalOrganismsCreated}\n" +
                $"Generations Evolved: {_gameManager.Organisms.GenerationsEvolved}",
                "Victory!",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _gameTimer.Stop();
        }

        private void ShowGameOverScreen()
        {
            MessageBox.Show(
                $"💀 GAME OVER 💀\n\n" +
                $"The organisms have overwhelmed your system!\n\n" +
                $"Statistics:\n" +
                $"Total Organisms Created: {_gameManager.Organisms.TotalOrganismsCreated}\n" +
                $"Generations Evolved: {_gameManager.Organisms.GenerationsEvolved}",
                "Game Over",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            _gameTimer.Stop();
        }
    }

    public class ChemicalSpray
    {
        public Point Location { get; set; }
        public double Radius { get; set; }
        public Color Color { get; set; }
        public double Opacity { get; set; }
        public Chemical Chemical { get; set; } = null!;
    }
}
