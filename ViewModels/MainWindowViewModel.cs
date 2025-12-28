using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace BiochemSimulator.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application window
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly GameManager _gameManager;
        private readonly SaveManager _saveManager;
        private readonly AchievementManager _achievementManager;
        private readonly PlayerProfile _currentProfile;

        private GameState _currentState;
        private ExperimentPhase _currentPhase;
        private string _windowTitle = "Biochemistry Simulator";
        private bool _isAtomicWorkspaceVisible;
        private bool _isSimulatorVisible;
        private bool _isDesktopOverlayVisible;
        private bool _isAlarmVisible;

        public MainWindowViewModel(PlayerProfile profile, double screenWidth, double screenHeight)
        {
            _currentProfile = profile ?? throw new ArgumentNullException(nameof(profile));

            // Initialize managers
            _gameManager = new GameManager(screenWidth, screenHeight);
            _saveManager = new SaveManager();
            _achievementManager = new AchievementManager(_currentProfile);

            // Initialize sub-ViewModels
            AtomicWorkspace = new AtomicWorkspaceViewModel(_gameManager);
            ChemicalSimulator = new ChemicalSimulatorViewModel(_gameManager);
            VirusOutbreak = new VirusOutbreakViewModel(_gameManager);

            // Initialize commands
            StartGameCommand = new RelayCommand(OnStartGame);
            SaveGameCommand = new AsyncRelayCommand(OnSaveGame);
            LoadGameCommand = new AsyncRelayCommand(OnLoadGame);
            ExitCommand = new RelayCommand(OnExit);
            AboutCommand = new RelayCommand(OnAbout);

            // Subscribe to game manager events
            _gameManager.StateChanged += OnStateChanged;
            _gameManager.PhaseChanged += OnPhaseChanged;
            _gameManager.TutorialMessageChanged += OnTutorialMessageChanged;
            _gameManager.AlarmTriggered += OnAlarmTriggered;

            // Subscribe to achievement events
            _achievementManager.AchievementUnlocked += OnAchievementUnlocked;

            // Set initial state
            WindowTitle = $"Biochemistry Simulator - {_currentProfile.PlayerName}";
            _currentState = _gameManager.CurrentState;
            _currentPhase = _gameManager.CurrentPhase;
        }

        #region Properties

        public AtomicWorkspaceViewModel AtomicWorkspace { get; }
        public ChemicalSimulatorViewModel ChemicalSimulator { get; }
        public VirusOutbreakViewModel VirusOutbreak { get; }

        public GameManager GameManager => _gameManager;
        public PlayerProfile CurrentProfile => _currentProfile;
        public SaveManager SaveManager => _saveManager;
        public AchievementManager AchievementManager => _achievementManager;

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        public GameState CurrentState
        {
            get => _currentState;
            private set => SetProperty(ref _currentState, value);
        }

        public ExperimentPhase CurrentPhase
        {
            get => _currentPhase;
            private set => SetProperty(ref _currentPhase, value);
        }

        public bool IsAtomicWorkspaceVisible
        {
            get => _isAtomicWorkspaceVisible;
            set => SetProperty(ref _isAtomicWorkspaceVisible, value);
        }

        public bool IsSimulatorVisible
        {
            get => _isSimulatorVisible;
            set => SetProperty(ref _isSimulatorVisible, value);
        }

        public bool IsDesktopOverlayVisible
        {
            get => _isDesktopOverlayVisible;
            set => SetProperty(ref _isDesktopOverlayVisible, value);
        }

        public bool IsAlarmVisible
        {
            get => _isAlarmVisible;
            set => SetProperty(ref _isAlarmVisible, value);
        }

        #endregion

        #region Commands

        public ICommand StartGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand LoadGameCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        #endregion

        #region Events

        public event EventHandler<string>? TutorialMessageChanged;
        public event EventHandler? AlarmTriggered;
        public event EventHandler<Achievement>? AchievementUnlocked;
        public event EventHandler<GameState>? StateChanged;
        public event Func<string, string?>? SaveNameRequested;
        public event Func<System.Collections.Generic.List<string>, string?>? SaveSelectionRequested;
        public event Action<string, string>? MessageRequested;

        #endregion

        #region Methods

        public void Initialize()
        {
            // Show initial state
            UpdateVisibility();
        }

        private void OnStartGame()
        {
            _gameManager.StartGame();
        }

        private async System.Threading.Tasks.Task OnSaveGame(object? parameter)
        {
            try
            {
                var saveName = SaveNameRequested?.Invoke("Enter save name:");
                if (string.IsNullOrWhiteSpace(saveName)) return;

                var gameSave = _gameManager.CreateGameSave(saveName, _currentProfile.PlayerName, 0);
                _saveManager.SaveGame(gameSave, _currentProfile.PlayerName);

                _currentProfile.CurrentSaveFile = saveName;
                _currentProfile.HasActiveSave = true;
                _saveManager.SaveProfile(_currentProfile);

                MessageRequested?.Invoke("Success", $"Game saved: {saveName}");
            }
            catch (Exception ex)
            {
                MessageRequested?.Invoke("Error", $"Failed to save: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task OnLoadGame(object? parameter)
        {
            try
            {
                var savesWithPaths = _saveManager.GetSavesWithPaths(_currentProfile.PlayerName);

                if (savesWithPaths.Count == 0)
                {
                    MessageRequested?.Invoke("Info", "No saved games found.");
                    return;
                }

                var saveNames = new System.Collections.Generic.List<string>();
                foreach (var (save, _) in savesWithPaths)
                {
                    saveNames.Add($"{save.SaveName} - {save.SaveDate:g}");
                }

                var selectedSave = SaveSelectionRequested?.Invoke(saveNames);
                if (selectedSave == null) return;

                int index = saveNames.IndexOf(selectedSave);
                if (index < 0) return;

                var (_, filePath) = savesWithPaths[index];
                var gameSave = _saveManager.LoadGame(filePath);

                if (gameSave != null)
                {
                    _gameManager.RestoreFromGameSave(gameSave);
                    MessageRequested?.Invoke("Success", "Game loaded successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageRequested?.Invoke("Error", $"Failed to load: {ex.Message}");
            }
        }

        private void OnExit()
        {
            SaveProfileOnExit();
            Application.Current.Shutdown();
        }

        private void OnAbout()
        {
            MessageRequested?.Invoke("About",
                $"Biochemistry Simulator\nVersion 1.0\n\n" +
                $"Profile: {_currentProfile.PlayerName}\n" +
                $"Play Time: {_currentProfile.TotalPlayTime} minutes");
        }

        private void OnStateChanged(object? sender, GameState state)
        {
            CurrentState = state;
            UpdateVisibility();
            StateChanged?.Invoke(this, state);
        }

        private void OnPhaseChanged(object? sender, ExperimentPhase phase)
        {
            CurrentPhase = phase;
            ChemicalSimulator.UpdatePhase(phase);
        }

        private void OnTutorialMessageChanged(object? sender, string message)
        {
            AtomicWorkspace.TutorialText = message;
            ChemicalSimulator.TutorialText = message;
            TutorialMessageChanged?.Invoke(this, message);
        }

        private void OnAlarmTriggered(object? sender, EventArgs e)
        {
            IsAlarmVisible = true;
            AlarmTriggered?.Invoke(this, EventArgs.Empty);
        }

        private void OnAchievementUnlocked(object? sender, AchievementUnlockedEventArgs e)
        {
            AchievementUnlocked?.Invoke(this, e.Achievement);
        }

        private void UpdateVisibility()
        {
            IsAtomicWorkspaceVisible = false;
            IsSimulatorVisible = false;
            IsDesktopOverlayVisible = false;

            switch (_currentState)
            {
                case GameState.Introduction:
                    IsSimulatorVisible = true;
                    break;
                case GameState.AtomicChemistry:
                    IsAtomicWorkspaceVisible = true;
                    break;
                case GameState.BiochemSimulator:
                case GameState.CreatingLife:
                case GameState.ObservingLife:
                case GameState.PostExperimentTasks:
                    IsSimulatorVisible = true;
                    break;
                case GameState.BiohazardAlarm:
                    IsSimulatorVisible = true;
                    IsAlarmVisible = true;
                    break;
                case GameState.VirusOutbreak:
                case GameState.ChemicalWarfare:
                    IsDesktopOverlayVisible = true;
                    VirusOutbreak.Show();
                    break;
                case GameState.Victory:
                case GameState.GameOver:
                    IsDesktopOverlayVisible = true;
                    break;
            }
        }

        public void Update(double deltaTime)
        {
            _gameManager.Update(deltaTime);

            if (_currentState == GameState.VirusOutbreak || _currentState == GameState.ChemicalWarfare)
            {
                VirusOutbreak.UpdateStats();
            }
        }

        private void SaveProfileOnExit()
        {
            try
            {
                _currentProfile.LastPlayedDate = DateTime.Now;
                _saveManager.SaveProfile(_currentProfile);
                _saveManager.SaveAchievementProgress(
                    _currentProfile.PlayerName,
                    _currentProfile.UnlockedAchievements);
            }
            catch
            {
                // Ignore save errors on exit
            }
        }

        public void Cleanup()
        {
            SaveProfileOnExit();
        }

        #endregion
    }
}
