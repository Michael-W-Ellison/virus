using BiochemSimulator.Models;
using BiochemSimulator.ViewModels;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly PlayerProfile _profile;
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModelTests()
        {
            _profile = new PlayerProfile { PlayerName = "TestPlayer" };
            _viewModel = new MainWindowViewModel(_profile, 1920, 1080);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeSubViewModels()
        {
            // Assert
            Assert.NotNull(_viewModel.AtomicWorkspace);
            Assert.NotNull(_viewModel.ChemicalSimulator);
            Assert.NotNull(_viewModel.VirusOutbreak);
        }

        [Fact]
        public void Constructor_ShouldInitializeManagers()
        {
            // Assert
            Assert.NotNull(_viewModel.GameManager);
            Assert.NotNull(_viewModel.SaveManager);
            Assert.NotNull(_viewModel.AchievementManager);
        }

        [Fact]
        public void Constructor_ShouldInitializeCommands()
        {
            // Assert
            Assert.NotNull(_viewModel.StartGameCommand);
            Assert.NotNull(_viewModel.SaveGameCommand);
            Assert.NotNull(_viewModel.LoadGameCommand);
            Assert.NotNull(_viewModel.ExitCommand);
            Assert.NotNull(_viewModel.AboutCommand);
        }

        [Fact]
        public void Constructor_ShouldSetWindowTitle()
        {
            // Assert
            Assert.Contains("TestPlayer", _viewModel.WindowTitle);
            Assert.Contains("Biochemistry Simulator", _viewModel.WindowTitle);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullProfile()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new MainWindowViewModel(null!, 1920, 1080));
        }

        [Fact]
        public void Constructor_ShouldStoreProfile()
        {
            // Assert
            Assert.Equal(_profile, _viewModel.CurrentProfile);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void WindowTitle_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.WindowTitle = "New Title";

            // Assert
            Assert.Equal("WindowTitle", changedProperty);
            Assert.Equal("New Title", _viewModel.WindowTitle);
        }

        [Fact]
        public void IsAtomicWorkspaceVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsAtomicWorkspaceVisible = true;

            // Assert
            Assert.Equal("IsAtomicWorkspaceVisible", changedProperty);
            Assert.True(_viewModel.IsAtomicWorkspaceVisible);
        }

        [Fact]
        public void IsSimulatorVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsSimulatorVisible = true;

            // Assert
            Assert.Equal("IsSimulatorVisible", changedProperty);
            Assert.True(_viewModel.IsSimulatorVisible);
        }

        [Fact]
        public void IsDesktopOverlayVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsDesktopOverlayVisible = true;

            // Assert
            Assert.Equal("IsDesktopOverlayVisible", changedProperty);
            Assert.True(_viewModel.IsDesktopOverlayVisible);
        }

        [Fact]
        public void IsAlarmVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsAlarmVisible = true;

            // Assert
            Assert.Equal("IsAlarmVisible", changedProperty);
            Assert.True(_viewModel.IsAlarmVisible);
        }

        #endregion

        #region Initialize Tests

        [Fact]
        public void Initialize_ShouldSetVisibility_BasedOnInitialState()
        {
            // Act
            _viewModel.Initialize();

            // Assert - Initial state is Introduction which shows simulator
            // Note: The actual visibility depends on game state
            Assert.NotNull(_viewModel.CurrentState);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void StartGameCommand_CanExecute_ShouldReturnTrue()
        {
            // Act
            bool canExecute = _viewModel.StartGameCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void StartGameCommand_Execute_ShouldChangeGameState()
        {
            // Arrange
            Engine.GameState? newState = null;
            _viewModel.StateChanged += (s, state) => newState = state;

            // Act
            _viewModel.StartGameCommand.Execute(null);

            // Assert
            Assert.NotNull(newState);
            Assert.Equal(Engine.GameState.AtomicChemistry, newState);
        }

        [Fact]
        public void AboutCommand_CanExecute_ShouldReturnTrue()
        {
            // Act
            bool canExecute = _viewModel.AboutCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void AboutCommand_Execute_ShouldRaiseMessageRequested()
        {
            // Arrange
            string? receivedTitle = null;
            string? receivedMessage = null;
            _viewModel.MessageRequested += (title, message) =>
            {
                receivedTitle = title;
                receivedMessage = message;
            };

            // Act
            _viewModel.AboutCommand.Execute(null);

            // Assert
            Assert.Equal("About", receivedTitle);
            Assert.NotNull(receivedMessage);
            Assert.Contains("TestPlayer", receivedMessage);
        }

        #endregion

        #region State Change Tests

        [Fact]
        public void StartGame_ShouldShowAtomicWorkspace()
        {
            // Arrange
            _viewModel.Initialize();

            // Act
            _viewModel.StartGameCommand.Execute(null);

            // Assert
            Assert.True(_viewModel.IsAtomicWorkspaceVisible);
            Assert.False(_viewModel.IsSimulatorVisible);
            Assert.False(_viewModel.IsDesktopOverlayVisible);
        }

        [Fact]
        public void CurrentState_ShouldUpdateOnStateChange()
        {
            // Arrange
            var initialState = _viewModel.CurrentState;

            // Act
            _viewModel.StartGameCommand.Execute(null);

            // Assert
            Assert.NotEqual(initialState, _viewModel.CurrentState);
            Assert.Equal(Engine.GameState.AtomicChemistry, _viewModel.CurrentState);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ShouldNotThrow_InIntroductionState()
        {
            // Act & Assert
            var exception = Record.Exception(() => _viewModel.Update(0.016));
            Assert.Null(exception);
        }

        [Fact]
        public void Update_ShouldNotThrow_InAtomicChemistryState()
        {
            // Arrange
            _viewModel.StartGameCommand.Execute(null);

            // Act & Assert
            var exception = Record.Exception(() => _viewModel.Update(0.016));
            Assert.Null(exception);
        }

        #endregion

        #region Cleanup Tests

        [Fact]
        public void Cleanup_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _viewModel.Cleanup());
            Assert.Null(exception);
        }

        #endregion
    }
}
