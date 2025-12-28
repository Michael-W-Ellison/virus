using BiochemSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

        public GameManager(double screenWidth, double screenHeight)
        {
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
            _currentState = newState;
            _phaseStartTime = DateTime.Now;
            StateChanged?.Invoke(this, newState);

            if (newState == GameState.VirusOutbreak)
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
            return molecule;
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
                    ChangeState(GameState.MolecularChemistry);
                    ShowTutorialMessage("Excellent! Now let's combine your molecules into larger structures. " +
                        "Try combining water with carbon dioxide, or create organic compounds. " +
                        "Click 'Advance to Biochemistry' when ready to proceed.");
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
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ChangeState(GameState.ObservingLife);
                            ShowTutorialMessage("Life created! Use the microscope to observe the organisms.");
                        });
                    });
                    break;
            }

            PhaseChanged?.Invoke(this, _currentPhase);

            // Delay clearing to let user see their accomplishment (3 seconds)
            Task.Delay(3000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ClearBeaker();
                    ClearAtomWorkspace();
                });
            });
        }

        /// <summary>
        /// Advances from MolecularChemistry state to BiochemSimulator state
        /// </summary>
        public void AdvanceToBiochemistry()
        {
            if (_currentState != GameState.MolecularChemistry)
                return;

            _currentPhase = ExperimentPhase.AminoAcids;
            ChangeState(GameState.BiochemSimulator);
            ShowTutorialMessage("Great! Now you understand molecular chemistry. Let's move to biochemistry and create the building blocks of life!");
            PhaseChanged?.Invoke(this, _currentPhase);
        }

        public void DisposeOrganismsInTrash()
        {
            ShowTutorialMessage("Organisms disposed. Continue your experiments...");
            ChangeState(GameState.PostExperimentTasks);

            // Schedule the alarm for 30 seconds later
            _alarmScheduledTime = DateTime.Now.AddSeconds(30);
            Task.Delay(30000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TriggerAlarm();
                });
            });
        }

        private void TriggerAlarm()
        {
            ChangeState(GameState.BiohazardAlarm);
            AlarmTriggered?.Invoke(this, EventArgs.Empty);
            ShowTutorialMessage("⚠️ BIOHAZARD ALERT! Organisms have escaped and are mutating on your desktop!");

            Task.Delay(3000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChangeState(GameState.VirusOutbreak);
                    ShowTutorialMessage("The virus is spreading! Create chemicals to eradicate it.");
                });
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
                // Escalate to ChemicalWarfare when organisms become too evolved or dangerous
                else if (_currentState == GameState.VirusOutbreak && ShouldEscalateToChemicalWarfare())
                {
                    EscalateToChemicalWarfare();
                }
            }
        }

        private bool ShouldEscalateToChemicalWarfare()
        {
            // Escalate when organisms reach generation 5+ or SuperVirus types appear
            if (_organismManager.GenerationsEvolved >= 5)
                return true;

            // Check for SuperVirus organisms
            var organisms = _organismManager.Organisms;
            if (organisms.Any(o => o.IsAlive && o.Type == OrganismType.SuperVirus))
                return true;

            // Escalate if organisms become highly resistant (average resistance > 50%)
            var aliveOrganisms = organisms.Where(o => o.IsAlive).ToList();
            if (aliveOrganisms.Count > 0)
            {
                double avgResistance = aliveOrganisms
                    .Where(o => o.Resistances.Any())
                    .SelectMany(o => o.Resistances.Values)
                    .DefaultIfEmpty(0)
                    .Average();
                if (avgResistance > 0.5)
                    return true;
            }

            return false;
        }

        private void EscalateToChemicalWarfare()
        {
            ChangeState(GameState.ChemicalWarfare);
            ShowTutorialMessage("☣️ CRITICAL: Organisms have evolved into a SuperVirus! " +
                "This is now CHEMICAL WARFARE. Use stronger chemicals to survive!");
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
            if (_currentState == GameState.MolecularChemistry)
            {
                // In molecular chemistry, player can combine molecules they've created
                return _chemistryEngine.GetMolecularChemicals();
            }
            else if (_currentState == GameState.BiochemSimulator ||
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

        #region Workspace Serialization

        /// <summary>
        /// Creates a GameSave object capturing the complete current workspace state
        /// </summary>
        public GameSave CreateGameSave(string saveName, string playerName, int playTimeSeconds)
        {
            var gameSave = new GameSave
            {
                SaveName = saveName,
                PlayerName = playerName,
                SaveDate = DateTime.Now,
                PlayTimeSeconds = playTimeSeconds,
                CurrentState = _currentState,
                CurrentPhase = _currentPhase,
                TotalOrganismsCreated = _organismManager.TotalOrganismsCreated,
                GenerationsEvolved = _organismManager.GenerationsEvolved
            };

            // Serialize workspace atoms
            int atomId = 0;
            var atomIdMap = new Dictionary<Atom, int>();
            foreach (var atom in _currentAtomWorkspace)
            {
                atomIdMap[atom] = atomId;
                gameSave.WorkspaceAtoms.Add(new SerializableAtom
                {
                    Id = atomId++,
                    Symbol = atom.Symbol,
                    PositionX = atom.Position.X,
                    PositionY = atom.Position.Y,
                    CurrentBonds = atom.Bonds.Count
                });
            }

            // Serialize molecules with their atoms and bonds
            foreach (var molecule in _currentMolecules)
            {
                var serMolecule = new SerializableMolecule
                {
                    Id = molecule.Id.ToString(),
                    Name = molecule.Name,
                    Formula = molecule.Formula,
                    CenterX = molecule.CenterOfMass.X,
                    CenterY = molecule.CenterOfMass.Y,
                    Stability = (int)molecule.Stability,
                    IsExplosive = molecule.IsExplosive,
                    IsFlammable = molecule.IsFlammable,
                    IsToxic = molecule.IsToxic
                };

                // Serialize atoms within molecule with local IDs
                int molAtomId = 0;
                var molAtomIdMap = new Dictionary<Atom, int>();
                foreach (var atom in molecule.Atoms)
                {
                    molAtomIdMap[atom] = molAtomId;
                    serMolecule.Atoms.Add(new SerializableAtom
                    {
                        Id = molAtomId++,
                        Symbol = atom.Symbol,
                        PositionX = atom.Position.X,
                        PositionY = atom.Position.Y,
                        CurrentBonds = atom.Bonds.Count
                    });
                }

                // Serialize bonds (avoiding duplicates)
                var processedBonds = new HashSet<Bond>();
                foreach (var atom in molecule.Atoms)
                {
                    foreach (var bond in atom.Bonds)
                    {
                        if (!processedBonds.Contains(bond) &&
                            molAtomIdMap.ContainsKey(bond.Atom1) &&
                            molAtomIdMap.ContainsKey(bond.Atom2))
                        {
                            serMolecule.Bonds.Add(new SerializableBond
                            {
                                Atom1Id = molAtomIdMap[bond.Atom1],
                                Atom2Id = molAtomIdMap[bond.Atom2],
                                Type = bond.Type,
                                BondEnergy = bond.BondEnergy,
                                BondLength = bond.BondLength
                            });
                            processedBonds.Add(bond);
                        }
                    }
                }

                gameSave.WorkspaceMolecules.Add(serMolecule);
            }

            // Serialize beaker contents
            foreach (var chemical in _currentBeaker)
            {
                gameSave.BeakerChemicals.Add(chemical.Name);
            }

            // Serialize organisms
            foreach (var organism in _organismManager.Organisms.Where(o => o.IsAlive))
            {
                gameSave.ActiveOrganisms.Add(new SerializableOrganism
                {
                    Id = organism.Id.ToString(),
                    PositionX = organism.Position.X,
                    PositionY = organism.Position.Y,
                    Health = organism.Health,
                    Size = organism.Size,
                    Generation = organism.Generation,
                    ColorR = organism.Color.R,
                    ColorG = organism.Color.G,
                    ColorB = organism.Color.B,
                    ReproductionRate = organism.ReproductionRate,
                    MutationRate = organism.MutationRate,
                    Type = (int)organism.Type,
                    Resistances = new Dictionary<string, double>(organism.Resistances)
                });
            }

            return gameSave;
        }

        /// <summary>
        /// Restores workspace state from a GameSave object
        /// </summary>
        public void RestoreFromGameSave(GameSave gameSave)
        {
            // Restore game state and phase
            _currentState = gameSave.CurrentState;
            _currentPhase = gameSave.CurrentPhase;

            // Clear current workspace
            _currentAtomWorkspace.Clear();
            _currentMolecules.Clear();
            _currentBeaker.Clear();

            // Restore workspace atoms
            foreach (var serAtom in gameSave.WorkspaceAtoms)
            {
                var atom = _atomicEngine.GetAtom(serAtom.Symbol);
                if (atom != null)
                {
                    atom.Position = new Point(serAtom.PositionX, serAtom.PositionY);
                    _currentAtomWorkspace.Add(atom);
                }
            }

            // Restore molecules
            foreach (var serMolecule in gameSave.WorkspaceMolecules)
            {
                var molecule = new Molecule
                {
                    Id = Guid.TryParse(serMolecule.Id, out var id) ? id : Guid.NewGuid(),
                    Name = serMolecule.Name,
                    Formula = serMolecule.Formula,
                    CenterOfMass = new Point(serMolecule.CenterX, serMolecule.CenterY),
                    Stability = (MoleculeStability)serMolecule.Stability,
                    IsExplosive = serMolecule.IsExplosive,
                    IsFlammable = serMolecule.IsFlammable,
                    IsToxic = serMolecule.IsToxic
                };

                // Restore atoms in molecule
                var atomsById = new Dictionary<int, Atom>();
                foreach (var serAtom in serMolecule.Atoms)
                {
                    var atom = _atomicEngine.GetAtom(serAtom.Symbol);
                    if (atom != null)
                    {
                        atom.Position = new Point(serAtom.PositionX, serAtom.PositionY);
                        molecule.Atoms.Add(atom);
                        atomsById[serAtom.Id] = atom;
                    }
                }

                // Restore bonds
                foreach (var serBond in serMolecule.Bonds)
                {
                    if (atomsById.TryGetValue(serBond.Atom1Id, out var atom1) &&
                        atomsById.TryGetValue(serBond.Atom2Id, out var atom2))
                    {
                        var bond = new Bond(atom1, atom2, serBond.Type, serBond.BondEnergy, serBond.BondLength);
                        molecule.Bonds.Add(bond);
                        atom1.Bonds.Add(bond);
                        atom2.Bonds.Add(bond);
                    }
                }

                _currentMolecules.Add(molecule);
            }

            // Restore beaker contents
            foreach (var chemicalName in gameSave.BeakerChemicals)
            {
                var chemical = _chemistryEngine.GetChemical(chemicalName);
                if (chemical != null)
                {
                    _currentBeaker.Add(chemical);
                }
            }

            // Restore organisms
            _organismManager.RestoreOrganisms(gameSave.ActiveOrganisms,
                gameSave.TotalOrganismsCreated,
                gameSave.GenerationsEvolved);

            // Notify listeners
            StateChanged?.Invoke(this, _currentState);
            PhaseChanged?.Invoke(this, _currentPhase);
        }

        #endregion
    }
}
