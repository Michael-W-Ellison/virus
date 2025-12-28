using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BiochemSimulator.ViewModels
{
    /// <summary>
    /// ViewModel for the Chemical/Biochemistry Simulator phase
    /// </summary>
    public class ChemicalSimulatorViewModel : ViewModelBase
    {
        private readonly GameManager _gameManager;

        private string _tutorialText = "Welcome to the Biochemistry Simulator!";
        private string _phaseText = "Amino Acids";
        private string _beakerContentText = "Empty";
        private string _statusText = "Ready to begin";
        private bool _isMicroscopeVisible;
        private bool _isTrashCanVisible;

        public ChemicalSimulatorViewModel(GameManager gameManager)
        {
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));

            // Initialize collections
            AvailableChemicals = new ObservableCollection<Chemical>();
            BeakerContents = new ObservableCollection<Chemical>();

            // Initialize commands
            AddChemicalCommand = new RelayCommand<Chemical>(OnAddChemical);
            ClearBeakerCommand = new RelayCommand(OnClearBeaker);
            DisposeOrganismsCommand = new RelayCommand(OnDisposeOrganisms);

            // Load initial chemicals
            RefreshChemicals();
        }

        #region Properties

        public ObservableCollection<Chemical> AvailableChemicals { get; }
        public ObservableCollection<Chemical> BeakerContents { get; }

        public string TutorialText
        {
            get => _tutorialText;
            set => SetProperty(ref _tutorialText, value);
        }

        public string PhaseText
        {
            get => _phaseText;
            set => SetProperty(ref _phaseText, value);
        }

        public string BeakerContentText
        {
            get => _beakerContentText;
            set => SetProperty(ref _beakerContentText, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsMicroscopeVisible
        {
            get => _isMicroscopeVisible;
            set => SetProperty(ref _isMicroscopeVisible, value);
        }

        public bool IsTrashCanVisible
        {
            get => _isTrashCanVisible;
            set => SetProperty(ref _isTrashCanVisible, value);
        }

        #endregion

        #region Commands

        public ICommand AddChemicalCommand { get; }
        public ICommand ClearBeakerCommand { get; }
        public ICommand DisposeOrganismsCommand { get; }

        #endregion

        #region Events

        public event EventHandler<Chemical>? ChemicalAdded;
        public event EventHandler? BeakerCleared;
        public event EventHandler<ChemicalReaction>? ReactionOccurred;

        #endregion

        #region Methods

        public void RefreshChemicals()
        {
            AvailableChemicals.Clear();
            var chemicals = _gameManager.GetAvailableChemicalsForCurrentPhase();
            foreach (var chemical in chemicals)
            {
                AvailableChemicals.Add(chemical);
            }
        }

        public void UpdatePhase(ExperimentPhase phase)
        {
            PhaseText = phase.ToString();
            RefreshChemicals();
        }

        private void OnAddChemical(Chemical? chemical)
        {
            if (chemical == null) return;

            _gameManager.AddChemicalToBeaker(chemical);
            BeakerContents.Add(chemical);
            UpdateBeakerContentText();

            ChemicalAdded?.Invoke(this, chemical);

            // Check for chemical reactions
            var reaction = _gameManager.Chemistry.TryReact(_gameManager.CurrentBeaker);
            if (reaction != null)
            {
                ReactionOccurred?.Invoke(this, reaction);
                StatusText = $"Reaction: {reaction.Description}";
            }
        }

        private void OnClearBeaker()
        {
            _gameManager.ClearBeaker();
            BeakerContents.Clear();
            BeakerContentText = "Empty";

            BeakerCleared?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisposeOrganisms()
        {
            _gameManager.DisposeOrganismsInTrash();
        }

        private void UpdateBeakerContentText()
        {
            if (BeakerContents.Count == 0)
            {
                BeakerContentText = "Empty";
            }
            else
            {
                BeakerContentText = $"Contains {BeakerContents.Count} chemical(s)";
            }
        }

        public void ShowMicroscope()
        {
            IsMicroscopeVisible = true;
            IsTrashCanVisible = false;
        }

        public void ShowTrashCan()
        {
            IsMicroscopeVisible = false;
            IsTrashCanVisible = true;
        }

        public void HideBothViews()
        {
            IsMicroscopeVisible = false;
            IsTrashCanVisible = false;
        }

        #endregion
    }
}
