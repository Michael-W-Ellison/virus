using BiochemSimulator.Models;
using System.Windows;

namespace BiochemSimulator.Engine
{
    public class GameManager
    {
        private ChemistryEngine _chemistryEngine;
        private OrganismManager _organismManager;
        private GameState _currentState;
        private ExperimentPhase _currentPhase;
        private DateTime _phaseStartTime;
        private DateTime _alarmScheduledTime;
        private List<Chemical> _currentBeaker;
        private Dictionary<ExperimentPhase, bool> _phasesCompleted;

        public event EventHandler<GameState>? StateChanged;
        public event EventHandler<ExperimentPhase>? PhaseChanged;
        public event EventHandler<string>? TutorialMessageChanged;
        public event EventHandler? AlarmTriggered;

        public GameState CurrentState => _currentState;
        public ExperimentPhase CurrentPhase => _currentPhase;
        public ChemistryEngine Chemistry => _chemistryEngine;
        public OrganismManager Organisms => _organismManager;
        public List<Chemical> CurrentBeaker => _currentBeaker;

        public GameManager(double screenWidth, double screenHeight)
        {
            _chemistryEngine = new ChemistryEngine();
            _organismManager = new OrganismManager(_chemistryEngine, screenWidth, screenHeight);
            _currentState = GameState.Introduction;
            _currentPhase = ExperimentPhase.AminoAcids;
            _currentBeaker = new List<Chemical>();
            _phasesCompleted = new Dictionary<ExperimentPhase, bool>();
            _phaseStartTime = DateTime.Now;
        }

        public void StartGame()
        {
            ChangeState(GameState.BiochemSimulator);
            _currentPhase = ExperimentPhase.AminoAcids;
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

        private void CompletePhase()
        {
            _phasesCompleted[_currentPhase] = true;

            switch (_currentPhase)
            {
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
                case ExperimentPhase.AminoAcids:
                    return "Welcome to the Biochemistry Simulator! Let's start by creating proteins from amino acids. " +
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
