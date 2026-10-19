using BiochemSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BiochemSimulator.Core.Primitives;

namespace BiochemSimulator.Engine
{
    public class GameManager
    {
        private AtomicEngine _atomicEngine;
        private ChemistryEngine _chemistryEngine;
        private OrganismManager _organismManager;
        private GameState _currentState;
        private ExperimentPhase _currentPhase;
        private DateTime _phaseStartTime;
        private DateTime _alarmScheduledTime;
        private List<Chemical> _currentBeaker;
        private List<Atom> _currentAtomWorkspace;
        private List<Molecule> _currentMolecules;
        private Dictionary<ExperimentPhase, bool> _phasesCompleted;
        private readonly IGameScheduler _scheduler;

        public event EventHandler<GameState>? StateChanged;
        public event EventHandler<ExperimentPhase>? PhaseChanged;
        public event EventHandler<string>? TutorialMessageChanged;
        public event EventHandler? AlarmTriggered;
        public event EventHandler<MoleculeReactionResult>? AtomicReactionOccurred;

        public GameState CurrentState => _currentState;
        public ExperimentPhase CurrentPhase => _currentPhase;
        public AtomicEngine Atomic => _atomicEngine;
        public ChemistryEngine Chemistry => _chemistryEngine;
        public OrganismManager Organisms => _organismManager;
        public List<Chemical> CurrentBeaker => _currentBeaker;
        public List<Atom> CurrentAtomWorkspace => _currentAtomWorkspace;
        public List<Molecule> CurrentMolecules => _currentMolecules;

        public GameManager(double screenWidth, double screenHeight, IGameScheduler? scheduler = null)
        {
            _scheduler = scheduler ?? new DefaultGameScheduler();
            _atomicEngine = new AtomicEngine();
            _chemistryEngine = new ChemistryEngine();
            _organismManager = new OrganismManager(_chemistryEngine, screenWidth, screenHeight);
            _currentState = GameState.Introduction;
            _currentPhase = ExperimentPhase.SimpleMolecules;
            _currentBeaker = new List<Chemical>();
            _currentAtomWorkspace = new List<Atom>();
            _currentMolecules = new List<Molecule>();
            _phasesCompleted = new Dictionary<ExperimentPhase, bool>();
            _phaseStartTime = DateTime.Now;
        }

        public void StartGame()
        {
            ChangeState(GameState.AtomicChemistry);
            _currentPhase = ExperimentPhase.SimpleMolecules;
            ShowTutorialMessage(GetPhaseInstructions(_currentPhase));
        }

        public void ChangeState(GameState newState)
        {
            ChangeState(newState, initializeOutbreak: true);
        }

        private void ChangeState(GameState newState, bool initializeOutbreak)
        {
            _currentState = newState;
            _phaseStartTime = DateTime.Now;
            StateChanged?.Invoke(this, newState);

            // When entering the outbreak normally we seed a fresh population, but a
            // restore supplies its own saved organisms and must not regenerate them.
            if (newState == GameState.VirusOutbreak && initializeOutbreak)
            {
                _organismManager.InitializeOutbreak(15);
            }
        }

        public void AddChemicalToBeaker(Chemical chemical)
        {
            _currentBeaker.Add(chemical);
            CheckForReaction();
        }

        public void ClearBeaker()
        {
            _currentBeaker.Clear();
        }

        private void CheckForReaction()
        {
            var reaction = _chemistryEngine.TryReact(_currentBeaker);

            if (reaction != null)
            {
                ShowTutorialMessage($"Success! {reaction.Description}");

                // Check if this completes the current phase
                if (DoesReactionCompletePhase(reaction, _currentPhase))
                {
                    CompletePhase();
                }
            }
        }

        private bool DoesReactionCompletePhase(ChemicalReaction reaction, ExperimentPhase phase)
        {
            switch (phase)
            {
                case ExperimentPhase.AminoAcids:
                    return reaction.Products.Any(p => p.Name == "Protein");
                case ExperimentPhase.RNA:
                    return reaction.Products.Any(p => p.Name == "RNA");
                case ExperimentPhase.DNA:
                    return reaction.Products.Any(p => p.Name == "DNA");
                case ExperimentPhase.CellMembrane:
                    return reaction.Products.Any(p => p.Name == "CellMembrane");
                default:
                    return false;
            }
        }

        public void AddAtomToWorkspace(Atom atom)
        {
            _currentAtomWorkspace.Add(atom);
        }

        public void ClearAtomWorkspace()
        {
            _currentAtomWorkspace.Clear();
        }

        public Molecule? TryBuildMolecule(List<Atom> atoms, List<(int, int, BondType)> bondInstructions)
        {
            List<Bond> bonds = new List<Bond>();

            foreach (var (idx1, idx2, bondType) in bondInstructions)
            {
                if (idx1 >= atoms.Count || idx2 >= atoms.Count)
                    continue;

                var bond = _atomicEngine.CreateBond(atoms[idx1], atoms[idx2], bondType);
                if (bond != null)
                {
                    bonds.Add(bond);
                }
            }

            var molecule = _atomicEngine.CreateMolecule(atoms, bonds);
            if (molecule != null)
            {
                _currentMolecules.Add(molecule);
            }
            return molecule;
        }

        public void ClearMolecules()
        {
            _currentMolecules.Clear();
        }

        public void TryMolecularReaction(Molecule mol1, Molecule mol2)
        {
            var result = _atomicEngine.TryReact(mol1, mol2);

            if (result.Success)
            {
                ShowTutorialMessage($"REACTION! {result.Description}");
                AtomicReactionOccurred?.Invoke(this, result);

                // Check if this completes the current phase
                CheckAtomicPhaseCompletion(result);
            }
        }

        private void CheckAtomicPhaseCompletion(MoleculeReactionResult result)
        {
            switch (_currentPhase)
            {
                case ExperimentPhase.SimpleMolecules:
                    // Looking for H2O creation
                    if (result.Products.Any(p => p.Formula == "H2O"))
                    {
                        CompletePhase();
                    }
                    break;
                case ExperimentPhase.ComplexMolecules:
                    // Looking for CH4 or CO2
                    if (result.Products.Any(p => p.Formula == "CH4" || p.Formula == "CO2"))
                    {
                        CompletePhase();
                    }
                    break;
            }
        }

        private void CompletePhase()
        {
            _phasesCompleted[_currentPhase] = true;

            switch (_currentPhase)
            {
                case ExperimentPhase.SimpleMolecules:
                    _currentPhase = ExperimentPhase.ComplexMolecules;
                    ShowTutorialMessage("Excellent! You've created water. Now let's try more complex molecules like methane or carbon dioxide.");
                    break;

                case ExperimentPhase.ComplexMolecules:
                    _currentPhase = ExperimentPhase.AminoAcids;
                    ChangeState(GameState.BiochemSimulator);
                    ShowTutorialMessage("Great! Now you understand molecular chemistry. Let's move to biochemistry and create the building blocks of life!");
                    break;

                case ExperimentPhase.AminoAcids:
                    _currentPhase = ExperimentPhase.RNA;
                    ShowTutorialMessage("Excellent! You've created proteins. Now let's make RNA.");
                    break;
                case ExperimentPhase.RNA:
                    _currentPhase = ExperimentPhase.DNA;
                    ShowTutorialMessage("Great! RNA formed. Now let's create DNA, the blueprint of life.");
                    break;
                case ExperimentPhase.DNA:
                    _currentPhase = ExperimentPhase.CellMembrane;
                    ShowTutorialMessage("DNA created! Now we need a cell membrane to contain everything.");
                    break;
                case ExperimentPhase.CellMembrane:
                    _currentPhase = ExperimentPhase.PrimitiveCell;
                    ChangeState(GameState.CreatingLife);
                    ShowTutorialMessage("Amazing! All components are ready. Life is forming...");
                    // Schedule the completion of life creation
                    _scheduler.ScheduleDelayed(TimeSpan.FromMilliseconds(3000), () =>
                    {
                        ChangeState(GameState.ObservingLife);
                        ShowTutorialMessage("Life created! Use the microscope to observe the organisms.");
                    });
                    break;
            }

            PhaseChanged?.Invoke(this, _currentPhase);

            // Delay clearing to let user see their accomplishment (3 seconds)
            _scheduler.ScheduleDelayed(TimeSpan.FromMilliseconds(3000), () =>
            {
                ClearBeaker();
                ClearAtomWorkspace();
            });
        }

        public void DisposeOrganismsInTrash()
        {
            ShowTutorialMessage("Organisms disposed. Continue your experiments...");
            ChangeState(GameState.PostExperimentTasks);

            // Schedule the alarm for 30 seconds later
            _alarmScheduledTime = DateTime.Now.AddSeconds(30);
            _scheduler.ScheduleDelayed(TimeSpan.FromMilliseconds(30000), () =>
            {
                TriggerAlarm();
            });
        }

        private void TriggerAlarm()
        {
            ChangeState(GameState.BiohazardAlarm);
            AlarmTriggered?.Invoke(this, EventArgs.Empty);
            ShowTutorialMessage("⚠️ BIOHAZARD ALERT! Organisms have escaped and are mutating on your desktop!");

            _scheduler.ScheduleDelayed(TimeSpan.FromMilliseconds(3000), () =>
            {
                ChangeState(GameState.VirusOutbreak);
                ShowTutorialMessage("The virus is spreading! Create chemicals to eradicate it.");
            });
        }

        public void ApplyChemicalToDesktop(Chemical chemical, Point location, double radius)
        {
            var affected = _organismManager.ApplyChemical(chemical, location, radius);

            if (affected.Count > 0)
            {
                int killed = affected.Count(o => !o.IsAlive);
                ShowTutorialMessage($"Applied {chemical.Name}. Killed {killed} organisms. {_organismManager.GetAliveCount()} remain.");

                if (_organismManager.GetAliveCount() == 0)
                {
                    ChangeState(GameState.Victory);
                    ShowTutorialMessage("🎉 Victory! You've eradicated all the organisms!");
                }
            }
        }

        public void Update(double deltaTime)
        {
            if (_currentState == GameState.VirusOutbreak || _currentState == GameState.ChemicalWarfare)
            {
                _organismManager.Update(deltaTime);

                // Check if organisms have overwhelmed the system
                if (_organismManager.GetAliveCount() > 300)
                {
                    ChangeState(GameState.GameOver);
                    ShowTutorialMessage("💀 Game Over! The organisms have overwhelmed your system!");
                }
            }
        }

        private void ShowTutorialMessage(string message)
        {
            TutorialMessageChanged?.Invoke(this, message);
        }

        private string GetPhaseInstructions(ExperimentPhase phase)
        {
            switch (phase)
            {
                case ExperimentPhase.SimpleMolecules:
                    return "Welcome to the Chemistry Learning Lab! Let's start with the basics - atoms and molecules. " +
                           "Try combining Hydrogen (H) and Oxygen (O) atoms to create water (H2O). " +
                           "WARNING: Some combinations are highly reactive or explosive!";
                case ExperimentPhase.ComplexMolecules:
                    return "Great! Now let's create more complex molecules. Try making methane (CH4) from Carbon and Hydrogen, " +
                           "or carbon dioxide (CO2) from Carbon and Oxygen. Watch out for unstable combinations!";
                case ExperimentPhase.AminoAcids:
                    return "Now we move to biochemistry! Let's create proteins from amino acids. " +
                           "Combine Glycine, Alanine, and Cysteine in the beaker.";
                case ExperimentPhase.RNA:
                    return "Now let's create RNA. Combine Adenine, Uracil, Cytosine, and Guanine.";
                case ExperimentPhase.DNA:
                    return "Time to make DNA! Combine Adenine, Thymine, Cytosine, and Guanine. " +
                           "Note: DNA uses Thymine instead of Uracil.";
                case ExperimentPhase.CellMembrane:
                    return "Finally, let's create a cell membrane. Combine Phospholipid and Cholesterol.";
                default:
                    return "";
            }
        }

        public List<Chemical> GetAvailableChemicalsForCurrentPhase()
        {
            if (_currentState == GameState.BiochemSimulator ||
                _currentState == GameState.CreatingLife)
            {
                return _chemistryEngine.GetChemicalsForPhase(_currentPhase);
            }
            else if (_currentState == GameState.VirusOutbreak ||
                     _currentState == GameState.ChemicalWarfare)
            {
                return _chemistryEngine.GetDisinfectants();
            }

            return new List<Chemical>();
        }

        public void SetScreenSize(double width, double height)
        {
            _organismManager.SetScreenSize(width, height);
        }

        #region Save / Restore

        /// <summary>
        /// Captures the live workspace (placed atoms, built molecules, active
        /// organisms) together with the current state and phase into a GameSave.
        /// </summary>
        public GameSave CreateGameSave(string saveName, string playerName, int playTimeSeconds, int organismsDefeated = 0)
        {
            var save = new GameSave
            {
                SaveName = saveName,
                PlayerName = playerName,
                SaveDate = DateTime.Now,
                PlayTimeSeconds = playTimeSeconds,
                CurrentState = _currentState,
                CurrentPhase = _currentPhase,
                SessionMoleculesCreated = _currentMolecules.Count,
                SessionAtomsPlaced = _currentAtomWorkspace.Count,
                SessionOrganismsDefeated = organismsDefeated
            };

            foreach (var atom in _currentAtomWorkspace)
            {
                save.WorkspaceAtoms.Add(ToSerializable(atom));
            }

            foreach (var molecule in _currentMolecules)
            {
                save.WorkspaceMolecules.Add(ToSerializable(molecule));
            }

            foreach (var organism in _organismManager.Organisms.Where(o => o.IsAlive))
            {
                save.ActiveOrganisms.Add(ToSerializable(organism));
            }

            return save;
        }

        /// <summary>
        /// Rebuilds the live workspace from a saved game and restores the state and
        /// phase. The outbreak is NOT re-seeded; the saved organisms are restored
        /// as-is. StateChanged is raised so the UI can switch views, but
        /// PhaseChanged is intentionally not raised (loading is not a phase
        /// completion), so the caller refreshes phase-dependent UI itself.
        /// </summary>
        public void LoadGameSave(GameSave save)
        {
            if (save == null)
                return;

            _currentPhase = save.CurrentPhase;

            _currentAtomWorkspace = save.WorkspaceAtoms
                .Select(FromSerializable)
                .Where(a => a != null)
                .Select(a => a!)
                .ToList();

            _currentMolecules = save.WorkspaceMolecules
                .Select(FromSerializable)
                .Where(m => m != null)
                .Select(m => m!)
                .ToList();

            var organisms = save.ActiveOrganisms.Select(FromSerializable).ToList();
            _organismManager.RestoreOrganisms(organisms);

            _currentBeaker.Clear();

            ChangeState(save.CurrentState, initializeOutbreak: false);
        }

        private static SerializableAtom ToSerializable(Atom atom)
        {
            return new SerializableAtom
            {
                Symbol = atom.Symbol,
                PositionX = atom.Position.X,
                PositionY = atom.Position.Y,
                CurrentBonds = atom.Bonds.Count
            };
        }

        private Atom? FromSerializable(SerializableAtom serialized)
        {
            // Rebuild a full atom from the periodic table so all element data is
            // restored; the serialized form only carries symbol and position.
            var atom = _atomicEngine.GetAtom(serialized.Symbol);
            if (atom == null)
                return null;

            atom.Position = new Point(serialized.PositionX, serialized.PositionY);
            return atom;
        }

        private SerializableMolecule ToSerializable(Molecule molecule)
        {
            var serialized = new SerializableMolecule
            {
                Name = molecule.Name,
                Formula = molecule.Formula,
                PositionX = molecule.CenterOfMass.X,
                PositionY = molecule.CenterOfMass.Y,
                Atoms = molecule.Atoms.Select(ToSerializable).ToList()
            };

            foreach (var bond in molecule.Bonds)
            {
                int index1 = molecule.Atoms.IndexOf(bond.Atom1);
                int index2 = molecule.Atoms.IndexOf(bond.Atom2);

                if (index1 >= 0 && index2 >= 0)
                {
                    serialized.Bonds.Add(new SerializableBond
                    {
                        AtomIndex1 = index1,
                        AtomIndex2 = index2,
                        Type = bond.Type
                    });
                }
            }

            return serialized;
        }

        private Molecule? FromSerializable(SerializableMolecule serialized)
        {
            var atoms = new List<Atom>();
            foreach (var serializedAtom in serialized.Atoms)
            {
                var atom = FromSerializable(serializedAtom);
                if (atom != null)
                    atoms.Add(atom);
            }

            if (atoms.Count == 0)
                return null;

            var bonds = new List<Bond>();
            foreach (var serializedBond in serialized.Bonds)
            {
                if (serializedBond.AtomIndex1 < 0 || serializedBond.AtomIndex1 >= atoms.Count ||
                    serializedBond.AtomIndex2 < 0 || serializedBond.AtomIndex2 >= atoms.Count)
                    continue;

                var bond = _atomicEngine.CreateBond(
                    atoms[serializedBond.AtomIndex1],
                    atoms[serializedBond.AtomIndex2],
                    serializedBond.Type);

                if (bond != null)
                    bonds.Add(bond);
            }

            var molecule = _atomicEngine.CreateMolecule(atoms, bonds);
            molecule.CenterOfMass = new Point(serialized.PositionX, serialized.PositionY);

            // Preserve the saved identity if the engine could not re-identify it.
            if (molecule.Name == "Unknown" && !string.IsNullOrEmpty(serialized.Name))
                molecule.Name = serialized.Name;

            return molecule;
        }

        private static SerializableOrganism ToSerializable(Organism organism)
        {
            return new SerializableOrganism
            {
                PositionX = organism.Position.X,
                PositionY = organism.Position.Y,
                Health = organism.Health,
                Size = organism.Size,
                Generation = organism.Generation,
                ReproductionRate = organism.ReproductionRate,
                MutationRate = organism.MutationRate,
                ColorR = organism.Color.R,
                ColorG = organism.Color.G,
                ColorB = organism.Color.B,
                Resistances = new Dictionary<string, double>(organism.Resistances)
            };
        }

        private static Organism FromSerializable(SerializableOrganism serialized)
        {
            return new Organism
            {
                Position = new Point(serialized.PositionX, serialized.PositionY),
                Health = serialized.Health,
                Size = serialized.Size,
                Generation = serialized.Generation,
                ReproductionRate = serialized.ReproductionRate,
                MutationRate = serialized.MutationRate,
                Color = Color.FromRgb(serialized.ColorR, serialized.ColorG, serialized.ColorB),
                Resistances = new Dictionary<string, double>(serialized.Resistances),
                IsAlive = true,
                Type = OrganismType.Virus
            };
        }

        #endregion
    }
}
