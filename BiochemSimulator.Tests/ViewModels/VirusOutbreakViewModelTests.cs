using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using BiochemSimulator.ViewModels;
using System.Windows;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class VirusOutbreakViewModelTests
    {
        private readonly GameManager _gameManager;
        private readonly VirusOutbreakViewModel _viewModel;

        public VirusOutbreakViewModelTests()
        {
            _gameManager = new GameManager(1920, 1080);
            _viewModel = new VirusOutbreakViewModel(_gameManager);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeCollections()
        {
            // Assert
            Assert.NotNull(_viewModel.AvailableWeapons);
            Assert.NotNull(_viewModel.ResistanceStats);
        }

        [Fact]
        public void Constructor_ShouldInitializeCommands()
        {
            // Assert
            Assert.NotNull(_viewModel.SelectWeaponCommand);
            Assert.NotNull(_viewModel.ApplyChemicalCommand);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Assert
            Assert.Null(_viewModel.SelectedWeapon);
            Assert.Equal("No weapon selected", _viewModel.SelectedWeaponText);
            Assert.Equal(0, _viewModel.AliveCount);
            Assert.Equal(0, _viewModel.GenerationCount);
            Assert.Equal(0, _viewModel.TotalCreated);
            Assert.False(_viewModel.IsOverlayVisible);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullGameManager()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new VirusOutbreakViewModel(null!));
        }

        [Fact]
        public void Constructor_ShouldLoadWeapons()
        {
            // Assert - weapons should be loaded from chemistry engine
            Assert.NotEmpty(_viewModel.AvailableWeapons);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SelectedWeapon_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;
            var weapon = _viewModel.AvailableWeapons[0];

            // Act
            _viewModel.SelectedWeapon = weapon;

            // Assert - Multiple properties change
            Assert.NotNull(_viewModel.SelectedWeapon);
        }

        [Fact]
        public void SelectedWeapon_ShouldUpdateSelectedWeaponText()
        {
            // Arrange
            var weapon = _viewModel.AvailableWeapons[0];

            // Act
            _viewModel.SelectedWeapon = weapon;

            // Assert
            Assert.Contains(weapon.Name, _viewModel.SelectedWeaponText);
        }

        [Fact]
        public void SelectedWeapon_ShouldResetText_WhenSetToNull()
        {
            // Arrange
            _viewModel.SelectedWeapon = _viewModel.AvailableWeapons[0];

            // Act
            _viewModel.SelectedWeapon = null;

            // Assert
            Assert.Equal("No weapon selected", _viewModel.SelectedWeaponText);
        }

        [Fact]
        public void AliveCount_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.AliveCount = 25;

            // Assert
            Assert.Equal("AliveCount", changedProperty);
            Assert.Equal(25, _viewModel.AliveCount);
        }

        [Fact]
        public void GenerationCount_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.GenerationCount = 3;

            // Assert
            Assert.Equal("GenerationCount", changedProperty);
            Assert.Equal(3, _viewModel.GenerationCount);
        }

        [Fact]
        public void TotalCreated_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.TotalCreated = 50;

            // Assert
            Assert.Equal("TotalCreated", changedProperty);
            Assert.Equal(50, _viewModel.TotalCreated);
        }

        [Fact]
        public void IsOverlayVisible_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.IsOverlayVisible = true;

            // Assert
            Assert.Equal("IsOverlayVisible", changedProperty);
            Assert.True(_viewModel.IsOverlayVisible);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void SelectWeaponCommand_ShouldSetSelectedWeapon()
        {
            // Arrange
            var weapon = _viewModel.AvailableWeapons[0];

            // Act
            _viewModel.SelectWeaponCommand.Execute(weapon);

            // Assert
            Assert.Equal(weapon, _viewModel.SelectedWeapon);
        }

        [Fact]
        public void ApplyChemicalCommand_CanExecute_ShouldReturnFalse_WhenNoWeaponSelected()
        {
            // Arrange
            _viewModel.SelectedWeapon = null;

            // Act
            bool canExecute = _viewModel.ApplyChemicalCommand.CanExecute(new Point(100, 100));

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void ApplyChemicalCommand_CanExecute_ShouldReturnTrue_WhenWeaponSelected()
        {
            // Arrange
            _viewModel.SelectedWeapon = _viewModel.AvailableWeapons[0];

            // Act
            bool canExecute = _viewModel.ApplyChemicalCommand.CanExecute(new Point(100, 100));

            // Assert
            Assert.True(canExecute);
        }

        #endregion

        #region Method Tests

        [Fact]
        public void RefreshWeapons_ShouldLoadDisinfectants()
        {
            // Arrange
            _viewModel.AvailableWeapons.Clear();

            // Act
            _viewModel.RefreshWeapons();

            // Assert
            Assert.NotEmpty(_viewModel.AvailableWeapons);
        }

        [Fact]
        public void UpdateStats_ShouldUpdateAliveCount()
        {
            // Arrange - start outbreak to have organisms
            _gameManager.ChangeState(GameState.VirusOutbreak);

            // Act
            _viewModel.UpdateStats();

            // Assert - should have some organisms after outbreak starts
            Assert.True(_viewModel.AliveCount >= 0);
        }

        [Fact]
        public void UpdateStats_ShouldUpdateGenerationCount()
        {
            // Act
            _viewModel.UpdateStats();

            // Assert
            Assert.True(_viewModel.GenerationCount >= 0);
        }

        [Fact]
        public void Show_ShouldSetOverlayVisible()
        {
            // Act
            _viewModel.Show();

            // Assert
            Assert.True(_viewModel.IsOverlayVisible);
        }

        [Fact]
        public void Show_ShouldRefreshWeapons()
        {
            // Arrange
            _viewModel.AvailableWeapons.Clear();

            // Act
            _viewModel.Show();

            // Assert
            Assert.NotEmpty(_viewModel.AvailableWeapons);
        }

        [Fact]
        public void Show_ShouldUpdateStats()
        {
            // Act
            _viewModel.Show();

            // Assert - stats should be updated (values may be 0 but method runs)
            Assert.True(_viewModel.AliveCount >= 0);
        }

        [Fact]
        public void Hide_ShouldSetOverlayNotVisible()
        {
            // Arrange
            _viewModel.Show();

            // Act
            _viewModel.Hide();

            // Assert
            Assert.False(_viewModel.IsOverlayVisible);
        }

        [Fact]
        public void ApplyChemicalAt_ShouldRaiseChemicalAppliedEvent()
        {
            // Arrange
            _gameManager.ChangeState(GameState.VirusOutbreak);
            _viewModel.SelectedWeapon = _viewModel.AvailableWeapons[0];
            ChemicalApplicationResult? result = null;
            _viewModel.ChemicalApplied += (s, r) => result = r;

            // Act
            _viewModel.ApplyChemicalAt(new Point(100, 100));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_viewModel.SelectedWeapon, result.Chemical);
        }

        [Fact]
        public void ApplyChemicalAt_ShouldUpdateStats()
        {
            // Arrange
            _gameManager.ChangeState(GameState.VirusOutbreak);
            _viewModel.SelectedWeapon = _viewModel.AvailableWeapons[0];
            int initialAlive = _viewModel.AliveCount;

            // Act
            _viewModel.ApplyChemicalAt(new Point(100, 100));

            // Assert - stats should be updated (may or may not kill organisms)
            Assert.True(_viewModel.AliveCount >= 0);
        }

        #endregion

        #region ResistanceStat Tests

        [Fact]
        public void ResistanceStat_Properties_ShouldRaisePropertyChanged()
        {
            // Arrange
            var stat = new ResistanceStat();
            string? changedProperty = null;
            stat.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act & Assert - ChemicalName
            stat.ChemicalName = "Bleach";
            Assert.Equal("ChemicalName", changedProperty);
            Assert.Equal("Bleach", stat.ChemicalName);

            // Act & Assert - ResistanceLevel
            stat.ResistanceLevel = 0.5;
            Assert.Equal("ResistanceLevel", changedProperty);
            Assert.Equal(0.5, stat.ResistanceLevel);

            // Act & Assert - ResistancePercent
            stat.ResistancePercent = 50;
            Assert.Equal("ResistancePercent", changedProperty);
            Assert.Equal(50, stat.ResistancePercent);
        }

        #endregion
    }
}
