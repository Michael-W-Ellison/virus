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
            ClearBeaker();
            ClearAtomWorkspace();
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
    }
}
