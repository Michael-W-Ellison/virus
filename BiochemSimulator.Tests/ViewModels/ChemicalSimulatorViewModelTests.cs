using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using BiochemSimulator.ViewModels;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class ChemicalSimulatorViewModelTests
    {
        private readonly GameManager _gameManager;
        private readonly ChemicalSimulatorViewModel _viewModel;

        public ChemicalSimulatorViewModelTests()
        {
            _gameManager = new GameManager(1920, 1080);
            _viewModel = new ChemicalSimulatorViewModel(_gameManager);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeCollections()
        {
            // Assert
            Assert.NotNull(_viewModel.AvailableChemicals);
            Assert.NotNull(_viewModel.BeakerContents);
        }

        [Fact]
        public void Constructor_ShouldInitializeCommands()
        {
            // Assert
            Assert.NotNull(_viewModel.AddChemicalCommand);
            Assert.NotNull(_viewModel.ClearBeakerCommand);
            Assert.NotNull(_viewModel.DisposeOrganismsCommand);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Assert
            Assert.Equal("Empty", _viewModel.BeakerContentText);
            Assert.Equal("Ready to begin", _viewModel.StatusText);
            Assert.False(_viewModel.IsMicroscopeVisible);
            Assert.False(_viewModel.IsTrashCanVisible);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullGameManager()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new ChemicalSimulatorViewModel(null!));
        }

        #endregion

        #region Property Tests

        [Fact]
        public void TutorialText_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.TutorialText = "New tutorial text";

            // Assert
            Assert.Equal("TutorialText", changedProperty);
            Assert.Equal("New tutorial text", _viewModel.TutorialText);
        }

        [Fact]
        public void PhaseText_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.PhaseText = "DNA";

            // Assert
            Assert.Equal("PhaseText", changedProperty);
            Assert.Equal("DNA", _viewModel.PhaseText);
        }

        [Fact]
        public void BeakerContentText_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.BeakerContentText = "Contains 2 chemical(s)";

            // Assert
            Assert.Equal("BeakerContentText", changedProperty);
        }

        [Fact]
        public void StatusText_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.StatusText = "Reaction in progress";

            // Assert
            Assert.Equal("StatusText", changedProperty);
        }

        [Fact]
        public void IsMicroscopeVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsMicroscopeVisible = true;

            // Assert
            Assert.Equal("IsMicroscopeVisible", changedProperty);
            Assert.True(_viewModel.IsMicroscopeVisible);
        }

        [Fact]
        public void IsTrashCanVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsTrashCanVisible = true;

            // Assert
            Assert.Equal("IsTrashCanVisible", changedProperty);
            Assert.True(_viewModel.IsTrashCanVisible);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddChemicalCommand_ShouldAddToBeaker()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();
            _viewModel.RefreshChemicals();

            if (_viewModel.AvailableChemicals.Count > 0)
            {
                var chemical = _viewModel.AvailableChemicals[0];
                int initialCount = _viewModel.BeakerContents.Count;

                // Act
                _viewModel.AddChemicalCommand.Execute(chemical);

                // Assert
                Assert.Equal(initialCount + 1, _viewModel.BeakerContents.Count);
                Assert.Contains(chemical, _viewModel.BeakerContents);
            }
        }

        [Fact]
        public void AddChemicalCommand_ShouldRaiseChemicalAddedEvent()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();
            _viewModel.RefreshChemicals();

            Chemical? addedChemical = null;
            _viewModel.ChemicalAdded += (s, c) => addedChemical = c;

            if (_viewModel.AvailableChemicals.Count > 0)
            {
                var chemical = _viewModel.AvailableChemicals[0];

                // Act
                _viewModel.AddChemicalCommand.Execute(chemical);

                // Assert
                Assert.NotNull(addedChemical);
                Assert.Equal(chemical, addedChemical);
            }
        }

        [Fact]
        public void AddChemicalCommand_ShouldUpdateBeakerContentText()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();
            _viewModel.RefreshChemicals();

            if (_viewModel.AvailableChemicals.Count > 0)
            {
                var chemical = _viewModel.AvailableChemicals[0];

                // Act
                _viewModel.AddChemicalCommand.Execute(chemical);

                // Assert
                Assert.Contains("1 chemical", _viewModel.BeakerContentText);
            }
        }

        [Fact]
        public void ClearBeakerCommand_ShouldClearBeaker()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();
            _viewModel.RefreshChemicals();

            if (_viewModel.AvailableChemicals.Count > 0)
            {
                _viewModel.AddChemicalCommand.Execute(_viewModel.AvailableChemicals[0]);
            }

            // Act
            _viewModel.ClearBeakerCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.BeakerContents);
            Assert.Equal("Empty", _viewModel.BeakerContentText);
        }

        [Fact]
        public void ClearBeakerCommand_ShouldRaiseBeakerClearedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.BeakerCleared += (s, e) => eventRaised = true;

            // Act
            _viewModel.ClearBeakerCommand.Execute(null);

            // Assert
            Assert.True(eventRaised);
        }

        #endregion

        #region Method Tests

        [Fact]
        public void RefreshChemicals_ShouldLoadChemicalsForCurrentPhase()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();

            // Act
            _viewModel.RefreshChemicals();

            // Assert - BiochemSimulator state should have chemicals
            Assert.NotEmpty(_viewModel.AvailableChemicals);
        }

        [Fact]
        public void UpdatePhase_ShouldUpdatePhaseText()
        {
            // Act
            _viewModel.UpdatePhase(ExperimentPhase.DNA);

            // Assert
            Assert.Equal("DNA", _viewModel.PhaseText);
        }

        [Fact]
        public void UpdatePhase_ShouldRefreshChemicals()
        {
            // Arrange
            _gameManager.StartGame();
            _gameManager.AdvanceToBiochemistry();

            // Act
            _viewModel.UpdatePhase(ExperimentPhase.DNA);

            // Assert - DNA phase should have specific chemicals
            Assert.NotEmpty(_viewModel.AvailableChemicals);
        }

        [Fact]
        public void ShowMicroscope_ShouldSetVisibility()
        {
            // Act
            _viewModel.ShowMicroscope();

            // Assert
            Assert.True(_viewModel.IsMicroscopeVisible);
            Assert.False(_viewModel.IsTrashCanVisible);
        }

        [Fact]
        public void ShowTrashCan_ShouldSetVisibility()
        {
            // Act
            _viewModel.ShowTrashCan();

            // Assert
            Assert.False(_viewModel.IsMicroscopeVisible);
            Assert.True(_viewModel.IsTrashCanVisible);
        }

        [Fact]
        public void HideBothViews_ShouldHideBoth()
        {
            // Arrange
            _viewModel.ShowMicroscope();

            // Act
            _viewModel.HideBothViews();

            // Assert
            Assert.False(_viewModel.IsMicroscopeVisible);
            Assert.False(_viewModel.IsTrashCanVisible);
        }

        #endregion
    }
}
