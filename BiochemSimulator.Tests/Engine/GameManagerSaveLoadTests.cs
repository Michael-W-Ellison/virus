using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System.Collections.Generic;
using System.Linq;
using BiochemSimulator.Core.Primitives;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    /// <summary>
    /// Round-trip tests for saving and restoring the live workspace
    /// (placed atoms, built molecules, and active organisms) along with the
    /// game state and phase.
    /// </summary>
    public class GameManagerSaveLoadTests
    {
        private static GameManager NewManager() => new GameManager(800, 600);

        [Fact]
        public void CreateGameSave_ShouldCaptureHeaderFields()
        {
            var manager = NewManager();
            manager.StartGame();

            var save = manager.CreateGameSave("My Save", "Alice", 123);

            Assert.Equal("My Save", save.SaveName);
            Assert.Equal("Alice", save.PlayerName);
            Assert.Equal(123, save.PlayTimeSeconds);
            Assert.Equal(manager.CurrentState, save.CurrentState);
            Assert.Equal(manager.CurrentPhase, save.CurrentPhase);
        }

        [Fact]
        public void SaveLoad_ShouldRoundTripWorkspaceAtomsWithPositions()
        {
            var manager = NewManager();
            manager.StartGame();

            var hydrogen = manager.Atomic.GetAtom("H")!;
            hydrogen.Position = new Point(10, 20);
            var oxygen = manager.Atomic.GetAtom("O")!;
            oxygen.Position = new Point(30, 40);

            manager.AddAtomToWorkspace(hydrogen);
            manager.AddAtomToWorkspace(oxygen);

            var save = manager.CreateGameSave("s", "p", 0);
            Assert.Equal(2, save.WorkspaceAtoms.Count);

            var restored = NewManager();
            restored.LoadGameSave(save);

            Assert.Equal(2, restored.CurrentAtomWorkspace.Count);

            var restoredHydrogen = restored.CurrentAtomWorkspace.First(a => a.Symbol == "H");
            Assert.Equal(10, restoredHydrogen.Position.X);
            Assert.Equal(20, restoredHydrogen.Position.Y);
            // A rebuilt atom carries full element data, not just the symbol.
            Assert.Equal("Hydrogen", restoredHydrogen.Name);
            Assert.Equal(1, restoredHydrogen.MaxBonds);
        }

        [Fact]
        public void SaveLoad_ShouldRoundTripMoleculeWithBonds()
        {
            var manager = NewManager();
            manager.StartGame();

            var atoms = new List<Atom>
            {
                manager.Atomic.GetAtom("O")!,
                manager.Atomic.GetAtom("H")!,
                manager.Atomic.GetAtom("H")!
            };
            atoms[0].Position = new Point(100, 100);
            atoms[1].Position = new Point(120, 100);
            atoms[2].Position = new Point(80, 100);

            var bonds = new List<(int, int, BondType)>
            {
                (0, 1, BondType.Single),
                (0, 2, BondType.Single)
            };

            var molecule = manager.TryBuildMolecule(atoms, bonds);
            Assert.NotNull(molecule);
            Assert.Equal("H2O", molecule!.Formula);
            Assert.Single(manager.CurrentMolecules);

            var save = manager.CreateGameSave("s", "p", 0);
            Assert.Single(save.WorkspaceMolecules);
            Assert.Equal(2, save.WorkspaceMolecules[0].Bonds.Count);

            var restored = NewManager();
            restored.LoadGameSave(save);

            Assert.Single(restored.CurrentMolecules);
            var restoredMolecule = restored.CurrentMolecules[0];
            Assert.Equal("H2O", restoredMolecule.Formula);
            Assert.Equal("Water", restoredMolecule.Name);
            Assert.Equal(3, restoredMolecule.Atoms.Count);
            Assert.Equal(2, restoredMolecule.Bonds.Count);
        }

        [Fact]
        public void SaveLoad_ShouldRoundTripActiveOrganisms()
        {
            var manager = NewManager();
            manager.ChangeState(GameState.VirusOutbreak); // seeds 15 organisms

            var first = manager.Organisms.Organisms[0];
            first.Position = new Point(250, 175);
            first.DevelopResistance("Bleach", 0.5);

            var save = manager.CreateGameSave("s", "p", 0);
            Assert.Equal(15, save.ActiveOrganisms.Count);

            var restored = NewManager();
            restored.LoadGameSave(save);

            Assert.Equal(15, restored.Organisms.Organisms.Count);
            var resistant = restored.Organisms.Organisms.First(o => o.Resistances.ContainsKey("Bleach"));
            Assert.Equal(0.5, resistant.Resistances["Bleach"]);
            Assert.Equal(250, resistant.Position.X);
            Assert.Equal(175, resistant.Position.Y);
        }

        [Fact]
        public void LoadGameSave_ShouldNotReseedOutbreak()
        {
            var manager = NewManager();
            manager.ChangeState(GameState.VirusOutbreak); // seeds 15 organisms

            // Kill five so the saved alive population is a non-default count.
            foreach (var organism in manager.Organisms.Organisms.Take(5))
            {
                organism.TakeDamage(1000, "lethal");
            }

            var save = manager.CreateGameSave("s", "p", 0);
            Assert.Equal(10, save.ActiveOrganisms.Count);

            var restored = NewManager();
            restored.LoadGameSave(save);

            // If the outbreak had been re-seeded this would be 15, not 10.
            Assert.Equal(GameState.VirusOutbreak, restored.CurrentState);
            Assert.Equal(10, restored.Organisms.Organisms.Count);
        }

        [Fact]
        public void LoadGameSave_ShouldRestoreStateAndPhase()
        {
            var manager = NewManager();
            manager.ChangeState(GameState.VirusOutbreak);

            var save = manager.CreateGameSave("s", "p", 0);

            var restored = NewManager();
            Assert.Equal(GameState.Introduction, restored.CurrentState); // precondition

            restored.LoadGameSave(save);

            Assert.Equal(save.CurrentState, restored.CurrentState);
            Assert.Equal(save.CurrentPhase, restored.CurrentPhase);
        }

        [Fact]
        public void LoadGameSave_ShouldRaiseStateChanged()
        {
            var manager = NewManager();
            manager.ChangeState(GameState.VirusOutbreak);
            var save = manager.CreateGameSave("s", "p", 0);

            var restored = NewManager();
            GameState? captured = null;
            restored.StateChanged += (sender, state) => captured = state;

            restored.LoadGameSave(save);

            Assert.Equal(GameState.VirusOutbreak, captured);
        }

        [Fact]
        public void ClearMolecules_ShouldEmptyCurrentMolecules()
        {
            var manager = NewManager();
            var atoms = new List<Atom>
            {
                manager.Atomic.GetAtom("H")!,
                manager.Atomic.GetAtom("H")!
            };
            manager.TryBuildMolecule(atoms, new List<(int, int, BondType)> { (0, 1, BondType.Single) });
            Assert.NotEmpty(manager.CurrentMolecules);

            manager.ClearMolecules();

            Assert.Empty(manager.CurrentMolecules);
        }
    }
}
