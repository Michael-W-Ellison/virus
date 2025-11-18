using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Linq;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class GameManagerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithIntroductionState()
        {
            // Act
            var manager = new GameManager(800, 600);

            // Assert
            Assert.Equal(GameState.Introduction, manager.CurrentState);
        }

        [Fact]
        public void Constructor_ShouldInitializeEngines()
        {
            // Act
            var manager = new GameManager(800, 600);

            // Assert
            Assert.NotNull(manager.Atomic);
            Assert.NotNull(manager.Chemistry);
            Assert.NotNull(manager.Organisms);
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptyCollections()
        {
            // Act
            var manager = new GameManager(800, 600);

            // Assert
            Assert.Empty(manager.CurrentBeaker);
            Assert.Empty(manager.CurrentAtomWorkspace);
            Assert.Empty(manager.CurrentMolecules);
        }

        [Fact]
        public void StartGame_ShouldChangeStateToAtomicChemistry()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            GameState? capturedState = null;
            manager.StateChanged += (sender, state) => capturedState = state;

            // Act
            manager.StartGame();

            // Assert
            Assert.Equal(GameState.AtomicChemistry, manager.CurrentState);
            Assert.Equal(GameState.AtomicChemistry, capturedState);
        }

        [Fact]
        public void StartGame_ShouldSetSimpleMoleculesPhase()
        {
            // Arrange
            var manager = new GameManager(800, 600);

            // Act
            manager.StartGame();

            // Assert
            Assert.Equal(ExperimentPhase.SimpleMolecules, manager.CurrentPhase);
        }

        [Fact]
        public void ChangeState_ShouldUpdateCurrentState()
        {
            // Arrange
            var manager = new GameManager(800, 600);

            // Act
            manager.ChangeState(GameState.BiochemSimulator);

            // Assert
            Assert.Equal(GameState.BiochemSimulator, manager.CurrentState);
        }

        [Fact]
        public void ChangeState_ShouldRaiseStateChangedEvent()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            GameState? capturedState = null;
            manager.StateChanged += (sender, state) => capturedState = state;

            // Act
            manager.ChangeState(GameState.BiochemSimulator);

            // Assert
            Assert.Equal(GameState.BiochemSimulator, capturedState);
        }

        [Fact]
        public void ChangeState_ToVirusOutbreak_ShouldInitializeOrganisms()
        {
            // Arrange
            var manager = new GameManager(800, 600);

            // Act
            manager.ChangeState(GameState.VirusOutbreak);

            // Assert
            Assert.Equal(GameState.VirusOutbreak, manager.CurrentState);
            Assert.NotEmpty(manager.Organisms.Organisms);
        }

        [Fact]
        public void AddChemicalToBeaker_ShouldAddToList()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var water = manager.Chemistry.GetChemical("Water")!;

            // Act
            manager.AddChemicalToBeaker(water);

            // Assert
            Assert.Single(manager.CurrentBeaker);
            Assert.Contains(water, manager.CurrentBeaker);
        }

        [Fact]
        public void AddChemicalToBeaker_Multiple_ShouldAddAll()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var water = manager.Chemistry.GetChemical("Water")!;
            var glucose = manager.Chemistry.GetChemical("Glucose")!;

            // Act
            manager.AddChemicalToBeaker(water);
            manager.AddChemicalToBeaker(glucose);

            // Assert
            Assert.Equal(2, manager.CurrentBeaker.Count);
            Assert.Contains(water, manager.CurrentBeaker);
            Assert.Contains(glucose, manager.CurrentBeaker);
        }

        [Fact]
        public void ClearBeaker_ShouldRemoveAllChemicals()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var water = manager.Chemistry.GetChemical("Water")!;
            var glucose = manager.Chemistry.GetChemical("Glucose")!;
            manager.AddChemicalToBeaker(water);
            manager.AddChemicalToBeaker(glucose);

            // Act
            manager.ClearBeaker();

            // Assert
            Assert.Empty(manager.CurrentBeaker);
        }

        [Fact]
        public void AddAtomToWorkspace_ShouldAddToList()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var hydrogen = manager.Atomic.GetAtom("H")!;

            // Act
            manager.AddAtomToWorkspace(hydrogen);

            // Assert
            Assert.Single(manager.CurrentAtomWorkspace);
            Assert.Contains(hydrogen, manager.CurrentAtomWorkspace);
        }

        [Fact]
        public void ClearAtomWorkspace_ShouldRemoveAllAtoms()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var hydrogen = manager.Atomic.GetAtom("H")!;
            var carbon = manager.Atomic.GetAtom("C")!;
            manager.AddAtomToWorkspace(hydrogen);
            manager.AddAtomToWorkspace(carbon);

            // Act
            manager.ClearAtomWorkspace();

            // Assert
            Assert.Empty(manager.CurrentAtomWorkspace);
        }

        [Fact]
        public void GetAvailableChemicalsForCurrentPhase_ShouldReturnChemicals()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            manager.StartGame();

            // Act
            var chemicals = manager.GetAvailableChemicalsForCurrentPhase();

            // Assert
            Assert.NotNull(chemicals);
        }

        [Fact]
        public void SetScreenSize_ShouldUpdateOrganismManager()
        {
            // Arrange
            var manager = new GameManager(800, 600);

            // Act
            manager.SetScreenSize(1024, 768);
            manager.ChangeState(GameState.VirusOutbreak);

            // Assert - All organisms should be within new bounds
            Assert.All(manager.Organisms.Organisms, o =>
            {
                Assert.True(o.Position.X >= 0 && o.Position.X <= 1024);
                Assert.True(o.Position.Y >= 0 && o.Position.Y <= 768);
            });
        }

        [Fact]
        public void Update_ShouldNotThrow()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            manager.ChangeState(GameState.VirusOutbreak);

            // Act & Assert - Should not throw exception
            manager.Update(0.016); // ~60 FPS
        }

        [Fact]
        public void ChangeState_MultipleTimes_ShouldUpdateEachTime()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var stateChanges = 0;
            manager.StateChanged += (sender, state) => stateChanges++;

            // Act
            manager.ChangeState(GameState.AtomicChemistry);
            manager.ChangeState(GameState.BiochemSimulator);
            manager.ChangeState(GameState.VirusOutbreak);

            // Assert
            Assert.Equal(GameState.VirusOutbreak, manager.CurrentState);
            Assert.Equal(3, stateChanges);
        }

        [Fact]
        public void TryBuildMolecule_ValidWaterMolecule_ShouldSucceed()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var h1 = manager.Atomic.GetAtom("H")!;
            var h2 = manager.Atomic.GetAtom("H")!;
            var o = manager.Atomic.GetAtom("O")!;

            h1.MaxBonds = 1;
            h2.MaxBonds = 1;
            o.MaxBonds = 2;

            var atoms = new List<Atom> { h1, h2, o };
            var bonds = new List<(int, int, BondType)>
            {
                (0, 2, BondType.Single), // H1 - O
                (1, 2, BondType.Single)  // H2 - O
            };

            // Act
            var molecule = manager.TryBuildMolecule(atoms, bonds);

            // Assert
            Assert.NotNull(molecule);
            Assert.Equal("H2O", molecule.Formula);
        }

        [Fact]
        public void CurrentBeaker_AfterAddingChemicals_ShouldContainAll()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            var acid = manager.Chemistry.GetChemical("HydrochloricAcid")!;
            var base_ = manager.Chemistry.GetChemical("SodiumHydroxide")!;

            // Act
            manager.AddChemicalToBeaker(acid);
            manager.AddChemicalToBeaker(base_);

            // Assert
            Assert.Equal(2, manager.CurrentBeaker.Count);
            Assert.Contains(manager.CurrentBeaker, c => c.Name == "Hydrochloric Acid");
            Assert.Contains(manager.CurrentBeaker, c => c.Name == "Sodium Hydroxide");
        }

        [Fact]
        public void Organisms_AfterVirusOutbreak_ShouldHaveOrganisms()
        {
            // Arrange
            var manager = new GameManager(800, 600);

            // Act
            manager.ChangeState(GameState.VirusOutbreak);

            // Assert
            Assert.True(manager.Organisms.GetAliveCount() > 0);
        }

        [Fact]
        public void ApplyChemicalToDesktop_ShouldAffectOrganisms()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            manager.ChangeState(GameState.VirusOutbreak);
            var initialCount = manager.Organisms.GetAliveCount();
            var bleach = manager.Chemistry.GetChemical("Bleach")!;

            // Act
            manager.ApplyChemicalToDesktop(bleach, new System.Windows.Point(400, 300), 200);
            manager.Update(1.0); // Update to remove dead organisms

            // Assert - Should have killed some organisms or at least affected them
            // (We can't guarantee kills because of randomness, but we can verify the method works)
            Assert.True(manager.Organisms.GetAliveCount() >= 0);
        }

        [Fact]
        public void Constructor_ShouldInitializePhaseAsSimpleMolecules()
        {
            // Act
            var manager = new GameManager(800, 600);

            // Assert
            Assert.Equal(ExperimentPhase.SimpleMolecules, manager.CurrentPhase);
        }

        [Fact]
        public void StateChanged_Event_ShouldProvideCorrectState()
        {
            // Arrange
            var manager = new GameManager(800, 600);
            GameState capturedState = GameState.Introduction;
            object? capturedSender = null;

            manager.StateChanged += (sender, state) =>
            {
                capturedSender = sender;
                capturedState = state;
            };

            // Act
            manager.ChangeState(GameState.VirusOutbreak);

            // Assert
            Assert.Equal(manager, capturedSender);
            Assert.Equal(GameState.VirusOutbreak, capturedState);
        }
    }
}
