using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // Atomic chemistry fields
        private Atom? _selectedAtom;
        private List<AtomVisual> _atomVisuals;
        private Atom? _draggingAtom;
        private Point _dragStartPoint;
        private bool _isDragging;
        private List<Molecule> _createdMolecules;

        // Profile and Achievement System
        private PlayerProfile _currentProfile;
        private SaveManager _saveManager;
        private AchievementManager _achievementManager;
        private DateTime _sessionStartTime;

        public MainWindow(PlayerProfile profile)
        {
            try
            {
                InitializeComponent();

                _random = new Random();
                _activeChemicals = new Dictionary<Point, ChemicalSpray>();
                _atomVisuals = new List<AtomVisual>();
                _createdMolecules = new List<Molecule>();

                // Initialize profile system
                _currentProfile = profile;
                _saveManager = new SaveManager();
                _achievementManager = new AchievementManager(_currentProfile);
                _sessionStartTime = DateTime.Now;

                // Subscribe to achievement events
                _achievementManager.AchievementUnlocked += OnAchievementUnlocked;

                // Update window title with player name
                Title = $"Biochemistry Simulator - {_currentProfile.PlayerName}";

                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in MainWindow constructor: {ex.Message}\n\n{ex.StackTrace}",
                    "Constructor Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                _gameManager = new GameManager(screenWidth, screenHeight);
                _gameManager.StateChanged += OnStateChanged;
                _gameManager.PhaseChanged += OnPhaseChanged;
                _gameManager.TutorialMessageChanged += OnTutorialMessageChanged;
                _gameManager.AlarmTriggered += OnAlarmTriggered;
                _gameManager.AtomicReactionOccurred += OnAtomicReactionOccurred;

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

                // Record game start in profile
                _currentProfile.RecordGameStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing game: {ex.Message}\n\n{ex.StackTrace}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                // Hide all views first
                AtomicWorkspaceView.Visibility = Visibility.Collapsed;
                SimulatorView.Visibility = Visibility.Collapsed;
                DesktopOverlay.Visibility = Visibility.Collapsed;
                DesktopUI.Visibility = Visibility.Collapsed;

                switch (state)
                {
                    case GameState.AtomicChemistry:
                        AtomicWorkspaceView.Visibility = Visibility.Visible;
                        InitializeAtomicWorkspace();
                        break;

                    case GameState.BiochemSimulator:
                        SimulatorView.Visibility = Visibility.Visible;
                        UpdateChemicalInventory();
                        break;

                    case GameState.ObservingLife:
                        MicroscopeView.Visibility = Visibility.Visible;
                        TrashCan.Visibility = Visibility.Visible;
                        StartMicroscopeAnimation();
                        break;

                    case GameState.VirusOutbreak:
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

        // ==================== ATOMIC CHEMISTRY METHODS ====================

        private void InitializeAtomicWorkspace()
        {
            // Populate atom inventory
            AtomInventoryPanel.Children.Clear();

            var atoms = _gameManager.Atomic.GetAllAtoms();
            foreach (var atom in atoms)
            {
                var button = new Button
                {
                    Style = (Style)FindResource("ChemicalButton"),
                    Tag = atom.Symbol,
                    Margin = new Thickness(5)
                };

                var sp = new StackPanel();
                sp.Children.Add(new TextBlock
                {
                    Text = atom.Symbol,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    Foreground = new SolidColorBrush(atom.Color),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                sp.Children.Add(new TextBlock
                {
                    Text = atom.Name,
                    FontSize = 10,
                    Foreground = Brushes.White,
                    Opacity = 0.8,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                sp.Children.Add(new TextBlock
                {
                    Text = $"Bonds: {atom.MaxBonds}",
                    FontSize = 9,
                    Foreground = Brushes.LightGray,
                    Opacity = 0.6,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                button.Content = sp;
                button.Click += AtomButton_Click;

                AtomInventoryPanel.Children.Add(button);
            }
        }

        private void AtomButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string symbol)
            {
                _selectedAtom = _gameManager.Atomic.GetAtom(symbol);

                if (_selectedAtom != null)
                {
                    SelectedAtomText.Text = $"{_selectedAtom.Symbol} - {_selectedAtom.Name}";
                    AtomPropertiesText.Text = $"Atomic #: {_selectedAtom.AtomicNumber}\n" +
                                             $"Valence e⁻: {_selectedAtom.ValenceElectrons}\n" +
                                             $"Max Bonds: {_selectedAtom.MaxBonds}\n" +
                                             $"Reactivity: {_selectedAtom.Reactivity}/10";

                    AtomicStatusText.Text = $"Selected {_selectedAtom.Name}. Click on workspace to place.";
                }
            }
        }

        private void AtomicCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedAtom != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(AtomicCanvas);

                // Create a new atom instance
                var atom = _selectedAtom.Clone();
                atom.Position = position;

                // Add to game manager
                _gameManager.AddAtomToWorkspace(atom);

                // Track stats and achievements
                _currentProfile.TotalAtomsPlaced++;
                _currentProfile.DiscoverAtom(atom.Symbol);
                _achievementManager.CheckAchievements(GameEvent.AtomUsed, atom.Symbol);

                // Check for radioactive achievement
                if (atom.IsRadioactive)
                {
                    _currentProfile.RadioactiveElementsUsed++;
                    _achievementManager.CheckAchievements(GameEvent.RadioactiveUsed, atom.Symbol);
                }

                // Create visual
                CreateAtomVisual(atom);

                // Update display
                WorkspaceAtomsList.ItemsSource = null;
                WorkspaceAtomsList.ItemsSource = _gameManager.CurrentAtomWorkspace;

                WorkspaceInstructions.Visibility = Visibility.Collapsed;

                // Check hazards
                CheckHazards();

                AtomicStatusText.Text = $"Placed {atom.Name}. Select more atoms or build molecule.";
            }
        }

        private void CreateAtomVisual(Atom atom)
        {
            var ellipse = new Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = new RadialGradientBrush(
                    atom.Color,
                    Color.FromArgb(150, atom.Color.R, atom.Color.G, atom.Color.B)
                ),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Tag = atom,
                ToolTip = CreateAtomTooltip(atom)
            };

            // Add radiation glow effect for radioactive atoms
            if (atom.IsRadioactive)
            {
                var glowColor = GetRadiationGlowColor(atom.RadiationLevel, atom.Color);
                ellipse.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = glowColor,
                    BlurRadius = 15 + (atom.RadiationLevel * 2),
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
                ellipse.StrokeThickness = 3;
                ellipse.Stroke = new SolidColorBrush(glowColor);

                // Animate the glow for high radiation
                if (atom.RadiationLevel >= 7.0)
                {
                    var animation = new System.Windows.Media.Animation.DoubleAnimation
                    {
                        From = 0.6,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(1),
                        AutoReverse = true,
                        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
                    };
                    ellipse.BeginAnimation(UIElement.OpacityProperty, animation);
                }
            }

            var text = new TextBlock
            {
                Text = atom.Symbol,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(ellipse, atom.Position.X - 20);
            Canvas.SetTop(ellipse, atom.Position.Y - 20);
            Canvas.SetLeft(text, atom.Position.X - 10);
            Canvas.SetTop(text, atom.Position.Y - 10);

            AtomicCanvas.Children.Add(ellipse);
            AtomicCanvas.Children.Add(text);

            _atomVisuals.Add(new AtomVisual { Atom = atom, Ellipse = ellipse, Text = text });
        }

        private Color GetRadiationGlowColor(double radiationLevel, Color atomColor)
        {
            if (radiationLevel >= 9.0)
                return Color.FromRgb(255, 0, 255); // Magenta for extreme radiation
            else if (radiationLevel >= 7.0)
                return Color.FromRgb(255, 50, 50); // Red for high radiation
            else if (radiationLevel >= 4.0)
                return Color.FromRgb(255, 200, 0); // Yellow for moderate radiation
            else
                return Color.FromRgb(100, 255, 100); // Green for low radiation
        }

        private string CreateAtomTooltip(Atom atom)
        {
            var tooltip = new System.Text.StringBuilder();
            tooltip.AppendLine($"═══ {atom.Name} ({atom.Symbol}) ═══");
            tooltip.AppendLine();

            // Basic Properties
            tooltip.AppendLine("⚛️ ATOMIC PROPERTIES");
            tooltip.AppendLine($"  Atomic Number: {atom.AtomicNumber}");
            tooltip.AppendLine($"  Atomic Mass: {atom.AtomicMass:F3} amu");
            tooltip.AppendLine($"  Protons: {atom.Protons}");
            tooltip.AppendLine($"  Neutrons: {atom.Neutrons}");
            tooltip.AppendLine($"  Electrons: {atom.Electrons}");
            tooltip.AppendLine($"  Valence Electrons: {atom.ValenceElectrons}");
            tooltip.AppendLine($"  Max Bonds: {atom.MaxBonds}");
            tooltip.AppendLine($"  Electronegativity: {atom.Electronegativity:F2}");
            tooltip.AppendLine($"  Reactivity: {atom.Reactivity:F1}/10");
            tooltip.AppendLine();

            // Radiological Properties
            if (atom.IsRadioactive)
            {
                tooltip.AppendLine("☢️ RADIOLOGICAL PROPERTIES");
                tooltip.AppendLine($"  ⚠️ RADIOACTIVE!");
                tooltip.AppendLine($"  Decay Type: {GetDecayTypeText(atom.DecayType)}");
                tooltip.AppendLine($"  Half-Life: {FormatHalfLife(atom.HalfLife)}");
                tooltip.AppendLine($"  Radiation Level: {atom.RadiationLevel:F1}/10");
                tooltip.AppendLine();
            }

            // Electrical Properties
            tooltip.AppendLine("⚡ ELECTRICAL PROPERTIES");
            tooltip.AppendLine($"  Conductivity: {FormatConductivity(atom.ElectricalConductivity)}");
            tooltip.AppendLine($"  Type: {(atom.IsConductor ? "Conductor" : "Insulator")}");
            tooltip.AppendLine($"  Ionization Energy: {atom.IonizationEnergy:F2} eV");
            tooltip.AppendLine($"  Ion Charge: {(atom.IonCharge == 0 ? "Neutral" : $"{atom.IonCharge:+0;-0}")}");
            tooltip.AppendLine();

            // Thermal Properties
            tooltip.AppendLine("🌡️ THERMAL PROPERTIES");
            tooltip.AppendLine($"  Melting Point: {atom.MeltingPoint:F1}°C");
            tooltip.AppendLine($"  Boiling Point: {atom.BoilingPoint:F1}°C");
            tooltip.AppendLine($"  Phase at 25°C: {atom.PhaseAtRoomTemp}");
            tooltip.AppendLine($"  Thermal Conductivity: {atom.ThermalConductivity:F3} W/(m·K)");
            tooltip.AppendLine($"  Heat Capacity: {atom.HeatCapacity:F2} J/(mol·K)");

            return tooltip.ToString();
        }

        private string GetDecayTypeText(RadioactiveDecayType decayType)
        {
            return decayType switch
            {
                RadioactiveDecayType.Alpha => "Alpha (α) - Helium nuclei",
                RadioactiveDecayType.Beta => "Beta (β) - Electron emission",
                RadioactiveDecayType.Gamma => "Gamma (γ) - EM radiation",
                RadioactiveDecayType.Positron => "Positron (β+)",
                RadioactiveDecayType.Fission => "Nuclear Fission ⚠️",
                _ => "Stable"
            };
        }

        private string FormatHalfLife(double halfLifeYears)
        {
            if (halfLifeYears == 0)
                return "Stable (∞)";
            else if (halfLifeYears < 0.01)
                return $"{halfLifeYears * 365.25 * 24:F1} hours";
            else if (halfLifeYears < 1)
                return $"{halfLifeYears * 365.25:F1} days";
            else if (halfLifeYears < 1000)
                return $"{halfLifeYears:F1} years";
            else if (halfLifeYears < 1000000)
                return $"{halfLifeYears / 1000:F1}k years";
            else if (halfLifeYears < 1000000000)
                return $"{halfLifeYears / 1000000:F1}M years";
            else
                return $"{halfLifeYears / 1000000000:F2}B years";
        }

        private string FormatConductivity(double conductivity)
        {
            if (conductivity >= 1000000)
                return $"{conductivity / 1000000:F2} MS/m (Excellent)";
            else if (conductivity >= 100000)
                return $"{conductivity / 1000:F2} kS/m (Very Good)";
            else if (conductivity >= 1000)
                return $"{conductivity / 1000:F2} kS/m (Good)";
            else if (conductivity >= 1)
                return $"{conductivity:F2} S/m (Moderate)";
            else if (conductivity >= 0.001)
                return $"{conductivity * 1000:F2} mS/m (Poor)";
            else
                return $"{conductivity:E2} S/m (Insulator)";
        }

        private void AtomicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // For future: dragging atoms
        }

        private void AtomicCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // For future: complete dragging
        }

        private void BuildMolecule_Click(object sender, RoutedEventArgs e)
        {
            if (_gameManager.CurrentAtomWorkspace.Count < 2)
            {
                MessageBox.Show("Need at least 2 atoms to build a molecule!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Auto-bond nearby atoms
            var bonds = AutoBondAtoms(_gameManager.CurrentAtomWorkspace);
            var molecule = _gameManager.TryBuildMolecule(_gameManager.CurrentAtomWorkspace, bonds);

            if (molecule != null)
            {
                _createdMolecules.Add(molecule);

                // Track stats and achievements
                _currentProfile.TotalMoleculesCreated++;
                _currentProfile.DiscoverMolecule(molecule.Formula);
                _achievementManager.CheckAchievements(GameEvent.MoleculeCreated, molecule.Formula);

                // Display molecule info
                MoleculeDisplayPanel.Visibility = Visibility.Visible;
                MoleculeFormulaText.Text = molecule.Formula;
                MoleculeNameText.Text = molecule.Name;

                var stabilityColor = molecule.Stability switch
                {
                    MoleculeStability.Stable => Brushes.LightGreen,
                    MoleculeStability.Metastable => Brushes.Yellow,
                    MoleculeStability.Unstable => Brushes.Orange,
                    MoleculeStability.Explosive => Brushes.Red,
                    _ => Brushes.White
                };

                MoleculeStabilityText.Text = $"Stability: {molecule.Stability}";
                MoleculeStabilityText.Foreground = stabilityColor;

                // Draw bonds
                DrawMoleculeBonds(molecule);

                // Enable combine button
                if (_createdMolecules.Count >= 2)
                {
                    CombineMoleculesButton.IsEnabled = true;
                }

                AtomicStatusText.Text = $"Created {molecule.Name} ({molecule.Formula})! " +
                    $"Stability: {molecule.Stability}";

                // Check for hazards
                CheckHazards();
            }
            else
            {
                MessageBox.Show("Could not form stable molecule from these atoms.", "Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<(int, int, BondType)> AutoBondAtoms(List<Atom> atoms)
        {
            var bonds = new List<(int, int, BondType)>();

            for (int i = 0; i < atoms.Count; i++)
            {
                for (int j = i + 1; j < atoms.Count; j++)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(atoms[i].Position.X - atoms[j].Position.X, 2) +
                        Math.Pow(atoms[i].Position.Y - atoms[j].Position.Y, 2)
                    );

                    // Bond if close enough (within 100 pixels)
                    if (distance < 100)
                    {
                        BondType bondType = DetermineBondType(atoms[i], atoms[j]);
                        bonds.Add((i, j, bondType));
                    }
                }
            }

            return bonds;
        }

        private BondType DetermineBondType(Atom atom1, Atom atom2)
        {
            double electronegativityDiff = Math.Abs(atom1.Electronegativity - atom2.Electronegativity);

            if (electronegativityDiff > 1.7)
                return BondType.Ionic;

            // Default to single covalent bond
            return BondType.Single;
        }

        private void DrawMoleculeBonds(Molecule molecule)
        {
            foreach (var bond in molecule.Bonds)
            {
                var line = new Line
                {
                    X1 = bond.Atom1.Position.X,
                    Y1 = bond.Atom1.Position.Y,
                    X2 = bond.Atom2.Position.X,
                    Y2 = bond.Atom2.Position.Y,
                    Stroke = Brushes.White,
                    StrokeThickness = bond.Type == BondType.Double ? 4 : bond.Type == BondType.Triple ? 6 : 2,
                    Opacity = 0.6
                };

                AtomicCanvas.Children.Insert(0, line); // Add to back
            }
        }

        private void CombineMolecules_Click(object sender, RoutedEventArgs e)
        {
            if (_createdMolecules.Count < 2)
            {
                MessageBox.Show("Need at least 2 molecules to combine!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Combine the last two molecules
            var mol1 = _createdMolecules[_createdMolecules.Count - 2];
            var mol2 = _createdMolecules[_createdMolecules.Count - 1];

            _gameManager.TryMolecularReaction(mol1, mol2);
        }

        private void OnAtomicReactionOccurred(object? sender, MoleculeReactionResult result)
        {
            Dispatcher.Invoke(() =>
            {
                // Show reaction effect
                switch (result.VisualEffect)
                {
                    case ReactionVisualEffect.Explosion:
                        ShowExplosionEffect();
                        break;
                    case ReactionVisualEffect.Fire:
                        ShowFireEffect();
                        break;
                    case ReactionVisualEffect.Flash:
                        ShowFlashEffect();
                        break;
                    case ReactionVisualEffect.Bubbling:
                        ShowBubblingEffect();
                        break;
                }

                // Track achievements
                if (result.IsExplosive)
                {
                    _currentProfile.ExplosionsCaused++;
                    _achievementManager.CheckAchievements(GameEvent.ExplosionCaused);
                    ShowExtremeHazardWarning("EXPLOSIVE REACTION OCCURRED!");
                }

                // Track reaction discovery
                _currentProfile.DiscoverReaction(result.Description);
                _achievementManager.CheckAchievements(GameEvent.ReactionDiscovered);

                // Display reaction message
                MessageBox.Show(result.Description, "Chemical Reaction!",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            });
        }

        private void ShowExplosionEffect()
        {
            ExplosionEffectCanvas.Visibility = Visibility.Visible;
            ExplosionEffectCanvas.Children.Clear();

            // Create expanding circles
            for (int i = 0; i < 5; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = new SolidColorBrush(Color.FromArgb(200, 255, (byte)(100 + i * 30), 0)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ExplosionEffectCanvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, ActualWidth / 2 - 25);
                Canvas.SetTop(ellipse, ActualHeight / 2 - 25);

                // Animate expansion
                var scaleAnim = new DoubleAnimation(1, 10 + i * 2, TimeSpan.FromSeconds(0.5 + i * 0.1));
                var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5 + i * 0.1));

                var scaleTransform = new ScaleTransform(1, 1, 25, 25);
                ellipse.RenderTransform = scaleTransform;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                ellipse.BeginAnimation(OpacityProperty, opacityAnim);
            }

            // Hide after animation
            Task.Delay(1500).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => ExplosionEffectCanvas.Visibility = Visibility.Collapsed);
            });
        }

        private void ShowFireEffect()
        {
            FireEffectCanvas.Visibility = Visibility.Visible;
            FireEffectCanvas.Children.Clear();

            // Create fire particles
            for (int i = 0; i < 20; i++)
            {
                var particle = new Ellipse
                {
                    Width = 10 + _random.Next(10),
                    Height = 10 + _random.Next(10),
                    Fill = new SolidColorBrush(Color.FromArgb(200, 255, (byte)(100 + _random.Next(155)), 0))
                };

                FireEffectCanvas.Children.Add(particle);
                Canvas.SetLeft(particle, ActualWidth / 2 + _random.Next(-50, 50));
                Canvas.SetTop(particle, ActualHeight / 2);

                // Animate upward
                var moveAnim = new DoubleAnimation(ActualHeight / 2, ActualHeight / 2 - 200, TimeSpan.FromSeconds(1 + _random.NextDouble()));
                var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1 + _random.NextDouble()));

                Canvas.SetTop(particle, ActualHeight / 2);
                particle.BeginAnimation(Canvas.TopProperty, moveAnim);
                particle.BeginAnimation(OpacityProperty, opacityAnim);
            }

            Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => FireEffectCanvas.Visibility = Visibility.Collapsed);
            });
        }

        private void ShowFlashEffect()
        {
            FlashEffect.Visibility = Visibility.Visible;

            var flashAnim = new DoubleAnimation
            {
                From = 0,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            flashAnim.Completed += (s, e) => FlashEffect.Visibility = Visibility.Collapsed;
            FlashEffect.BeginAnimation(OpacityProperty, flashAnim);
        }

        private void ShowBubblingEffect()
        {
            BubblingEffectCanvas.Visibility = Visibility.Visible;
            BubblingEffectCanvas.Children.Clear();

            for (int i = 0; i < 15; i++)
            {
                var bubble = new Ellipse
                {
                    Width = 15 + _random.Next(15),
                    Height = 15 + _random.Next(15),
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 2,
                    Opacity = 0.7
                };

                BubblingEffectCanvas.Children.Add(bubble);
                Canvas.SetLeft(bubble, ActualWidth / 2 + _random.Next(-100, 100));
                Canvas.SetTop(bubble, ActualHeight / 2);

                var moveAnim = new DoubleAnimation(ActualHeight / 2, ActualHeight / 2 - 150, TimeSpan.FromSeconds(2 + _random.NextDouble()));
                var opacityAnim = new DoubleAnimation(0.7, 0, TimeSpan.FromSeconds(2 + _random.NextDouble()));

                bubble.BeginAnimation(Canvas.TopProperty, moveAnim);
                bubble.BeginAnimation(OpacityProperty, opacityAnim);
            }

            Task.Delay(3000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => BubblingEffectCanvas.Visibility = Visibility.Collapsed);
            });
        }

        private void ShowExtremeHazardWarning(string message)
        {
            ExtremeHazardMessage.Text = message;
            ExtremeHazardOverlay.Visibility = Visibility.Visible;

            var blinkAnim = new DoubleAnimation
            {
                From = 0,
                To = 0.9,
                Duration = TimeSpan.FromMilliseconds(300),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            blinkAnim.Completed += (s, e) => ExtremeHazardOverlay.Visibility = Visibility.Collapsed;
            ExtremeHazardOverlay.BeginAnimation(OpacityProperty, blinkAnim);
        }

        private void CheckHazards()
        {
            var hazard = _gameManager.Atomic.AnalyzeHazard(_createdMolecules);

            if (hazard.Level == HazardLevel.Safe)
            {
                HazardWarningPanel.Visibility = Visibility.Collapsed;
                return;
            }

            HazardWarningPanel.Visibility = Visibility.Visible;
            HazardLevelText.Text = $"Level: {hazard.Level.ToString().ToUpper()}";

            // Change color based on level
            HazardWarningPanel.Background = hazard.Level switch
            {
                HazardLevel.Low => new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                HazardLevel.Moderate => new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                HazardLevel.High => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                HazardLevel.Extreme => new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                _ => new SolidColorBrush(Color.FromRgb(231, 76, 60))
            };

            HazardWarningsList.ItemsSource = hazard.Warnings;

            if (hazard.Level == HazardLevel.Extreme)
            {
                ShowExtremeHazardWarning(string.Join("\n", hazard.Warnings));
            }
        }

        private void ClearAtomicWorkspace_Click(object sender, RoutedEventArgs e)
        {
            _gameManager.ClearAtomWorkspace();
            AtomicCanvas.Children.Clear();
            _atomVisuals.Clear();
            _createdMolecules.Clear();

            WorkspaceAtomsList.ItemsSource = null;
            MoleculeDisplayPanel.Visibility = Visibility.Collapsed;
            HazardWarningPanel.Visibility = Visibility.Collapsed;
            WorkspaceInstructions.Visibility = Visibility.Visible;
            CombineMoleculesButton.IsEnabled = false;

            AtomicStatusText.Text = "Workspace cleared. Select atoms to begin.";
        }

        private void OnTutorialMessageChanged(object? sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (_gameManager.CurrentState == GameState.AtomicChemistry)
                {
                    AtomicTutorialText.Text = message;
                }
                else
                {
                    TutorialText.Text = message;
                }
            });
        }

        private void OnAchievementUnlocked(object? sender, AchievementUnlockedEventArgs e)
        {
            // Show achievement notification
            Dispatcher.Invoke(() =>
            {
                ShowAchievementNotification(e.Achievement);
            });
        }

        private void ShowAchievementNotification(Achievement achievement)
        {
            // Create a notification window/popup for achievement
            string message = $"{achievement.Icon} Achievement Unlocked!\n\n" +
                           $"{achievement.Name}\n" +
                           $"{achievement.Description}\n\n" +
                           $"+{achievement.Points} Points";

            MessageBox.Show(message, "Achievement Unlocked!",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save profile on exit
            SaveProfileOnExit();
        }

        private void SaveProfileOnExit()
        {
            try
            {
                // Calculate session time
                var sessionTime = (int)(DateTime.Now - _sessionStartTime).TotalMinutes;
                _currentProfile.AddPlayTime(sessionTime);
                _currentProfile.UpdateLastPlayed();

                // Save profile
                _saveManager.SaveProfile(_currentProfile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profile: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    // Helper class for atom visuals
    public class AtomVisual
    {
        public Atom Atom { get; set; } = null!;
        public Ellipse Ellipse { get; set; } = null!;
        public TextBlock Text { get; set; } = null!;
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
