using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using BiochemSimulator.ViewModels;
using System.Windows;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class AtomicWorkspaceViewModelTests
    {
        private readonly GameManager _gameManager;
        private readonly AtomicWorkspaceViewModel _viewModel;

        public AtomicWorkspaceViewModelTests()
        {
            _gameManager = new GameManager(1920, 1080);
            _viewModel = new AtomicWorkspaceViewModel(_gameManager);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeCollections()
        {
            // Assert
            Assert.NotNull(_viewModel.AvailableAtoms);
            Assert.NotNull(_viewModel.WorkspaceAtoms);
            Assert.NotNull(_viewModel.HazardWarnings);
            Assert.NotNull(_viewModel.CreatedMolecules);
        }

        [Fact]
        public void Constructor_ShouldInitializeCommands()
        {
            // Assert
            Assert.NotNull(_viewModel.SelectAtomCommand);
            Assert.NotNull(_viewModel.BuildMoleculeCommand);
            Assert.NotNull(_viewModel.CombineMoleculesCommand);
            Assert.NotNull(_viewModel.ClearWorkspaceCommand);
        }

        [Fact]
        public void Constructor_ShouldLoadAvailableAtoms()
        {
            // Assert
            Assert.NotEmpty(_viewModel.AvailableAtoms);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Assert
            Assert.Equal("None", _viewModel.SelectedAtomText);
            Assert.Equal("", _viewModel.AtomPropertiesText);
            Assert.False(_viewModel.IsHazardVisible);
            Assert.False(_viewModel.IsMoleculeDisplayVisible);
            Assert.False(_viewModel.CanCombineMolecules);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullGameManager()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new AtomicWorkspaceViewModel(null!));
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SelectedAtom_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);

            // Act
            _viewModel.SelectedAtom = hydrogen;

            // Assert
            Assert.NotNull(changedProperty);
        }

        [Fact]
        public void SelectedAtom_ShouldUpdateSelectedAtomText()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);

            // Act
            _viewModel.SelectedAtom = hydrogen;

            // Assert
            Assert.Contains("Hydrogen", _viewModel.SelectedAtomText);
            Assert.Contains("H", _viewModel.SelectedAtomText);
        }

        [Fact]
        public void SelectedAtom_ShouldUpdateAtomPropertiesText()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);

            // Act
            _viewModel.SelectedAtom = hydrogen;

            // Assert
            Assert.Contains("Atomic #: 1", _viewModel.AtomPropertiesText);
            Assert.Contains("Valence: 1", _viewModel.AtomPropertiesText);
        }

        [Fact]
        public void SelectedAtom_ShouldClearTexts_WhenSetToNull()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;

            // Act
            _viewModel.SelectedAtom = null;

            // Assert
            Assert.Equal("None", _viewModel.SelectedAtomText);
            Assert.Equal("", _viewModel.AtomPropertiesText);
        }

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
        public void StatusText_ShouldRaisePropertyChanged()
        {
            // Arrange
            string? changedProperty = null;
            _viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            _viewModel.StatusText = "New status";

            // Assert
            Assert.Equal("StatusText", changedProperty);
            Assert.Equal("New status", _viewModel.StatusText);
        }

        #endregion

        #region PlaceAtomAtPosition Tests

        [Fact]
        public void PlaceAtomAtPosition_ShouldNotAdd_WhenNoAtomSelected()
        {
            // Arrange
            _viewModel.SelectedAtom = null;
            var position = new Point(100, 100);

            // Act
            _viewModel.PlaceAtomAtPosition(position);

            // Assert
            Assert.Empty(_viewModel.WorkspaceAtoms);
        }

        [Fact]
        public void PlaceAtomAtPosition_ShouldAddAtom_WhenAtomSelected()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;
            var position = new Point(100, 100);

            // Act
            _viewModel.PlaceAtomAtPosition(position);

            // Assert
            Assert.Single(_viewModel.WorkspaceAtoms);
            Assert.Equal("H", _viewModel.WorkspaceAtoms[0].Symbol);
        }

        [Fact]
        public void PlaceAtomAtPosition_ShouldClearSelectedAtom_AfterPlacing()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;

            // Act
            _viewModel.PlaceAtomAtPosition(new Point(100, 100));

            // Assert
            Assert.Null(_viewModel.SelectedAtom);
            Assert.Equal("None", _viewModel.SelectedAtomText);
        }

        [Fact]
        public void PlaceAtomAtPosition_ShouldRaiseAtomPlacedEvent()
        {
            // Arrange
            Atom? placedAtom = null;
            _viewModel.AtomPlaced += (s, a) => placedAtom = a;
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;

            // Act
            _viewModel.PlaceAtomAtPosition(new Point(100, 100));

            // Assert
            Assert.NotNull(placedAtom);
            Assert.Equal("H", placedAtom.Symbol);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void SelectAtomCommand_ShouldSetSelectedAtom()
        {
            // Arrange
            var atomVm = _viewModel.AvailableAtoms[0];

            // Act
            _viewModel.SelectAtomCommand.Execute(atomVm);

            // Assert
            Assert.NotNull(_viewModel.SelectedAtom);
            Assert.Equal(atomVm.Symbol, _viewModel.SelectedAtom.Symbol);
        }

        [Fact]
        public void ClearWorkspaceCommand_ShouldClearAllCollections()
        {
            // Arrange - Add some atoms to workspace
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;
            _viewModel.PlaceAtomAtPosition(new Point(100, 100));

            // Act
            _viewModel.ClearWorkspaceCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.WorkspaceAtoms);
            Assert.Empty(_viewModel.CreatedMolecules);
            Assert.Empty(_viewModel.HazardWarnings);
            Assert.False(_viewModel.IsMoleculeDisplayVisible);
            Assert.False(_viewModel.IsHazardVisible);
        }

        [Fact]
        public void BuildMoleculeCommand_CanExecute_ShouldReturnFalse_WhenLessThanTwoAtoms()
        {
            // Arrange - Add only one atom
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;
            _viewModel.PlaceAtomAtPosition(new Point(100, 100));

            // Act
            bool canExecute = _viewModel.BuildMoleculeCommand.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void BuildMoleculeCommand_CanExecute_ShouldReturnTrue_WhenTwoOrMoreAtoms()
        {
            // Arrange - Add two atoms
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008, 1, 1.0, 2.20);
            _viewModel.SelectedAtom = hydrogen;
            _viewModel.PlaceAtomAtPosition(new Point(100, 100));

            _viewModel.SelectedAtom = hydrogen.Clone();
            _viewModel.PlaceAtomAtPosition(new Point(150, 100));

            // Act
            bool canExecute = _viewModel.BuildMoleculeCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        #endregion

        #region AtomViewModel Tests

        [Fact]
        public void AtomViewModel_ShouldExposeAtomProperties()
        {
            // Arrange
            var atom = new Atom("O", "Oxygen", 8, 15.999, 6, 2.0, 3.44);

            // Act
            var viewModel = new AtomViewModel(atom);

            // Assert
            Assert.Equal("O", viewModel.Symbol);
            Assert.Equal("Oxygen", viewModel.Name);
            Assert.Equal(8, viewModel.AtomicNumber);
        }

        [Fact]
        public void AtomViewModel_Constructor_ShouldThrowOnNullAtom()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new AtomViewModel(null!));
        }

        #endregion
    }
}
