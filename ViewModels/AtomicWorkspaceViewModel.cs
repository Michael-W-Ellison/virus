using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BiochemSimulator.ViewModels
{
    /// <summary>
    /// ViewModel for the Atomic Chemistry Workspace
    /// </summary>
    public class AtomicWorkspaceViewModel : ViewModelBase
    {
        private readonly GameManager _gameManager;

        private Atom? _selectedAtom;
        private string _tutorialText = "Welcome to Atomic Chemistry!";
        private string _statusText = "Select atoms and build molecules";
        private string _selectedAtomText = "None";
        private string _atomPropertiesText = "";
        private bool _isHazardVisible;
        private string _hazardLevelText = "Level: SAFE";
        private Brush _hazardBackground = Brushes.Transparent;
        private bool _isMoleculeDisplayVisible;
        private string _moleculeFormula = "";
        private string _moleculeName = "";
        private string _moleculeStabilityText = "";
        private Brush _moleculeStabilityColor = Brushes.White;
        private bool _canCombineMolecules;

        public AtomicWorkspaceViewModel(GameManager gameManager)
        {
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));

            // Initialize collections
            AvailableAtoms = new ObservableCollection<AtomViewModel>();
            WorkspaceAtoms = new ObservableCollection<Atom>();
            HazardWarnings = new ObservableCollection<string>();
            CreatedMolecules = new ObservableCollection<Molecule>();

            // Initialize commands
            SelectAtomCommand = new RelayCommand<AtomViewModel>(OnSelectAtom);
            BuildMoleculeCommand = new RelayCommand(OnBuildMolecule, () => WorkspaceAtoms.Count >= 2);
            CombineMoleculesCommand = new RelayCommand(OnCombineMolecules, () => CanCombineMolecules);
            ClearWorkspaceCommand = new RelayCommand(OnClearWorkspace);

            // Load available atoms
            LoadAvailableAtoms();
        }

        #region Properties

        public ObservableCollection<AtomViewModel> AvailableAtoms { get; }
        public ObservableCollection<Atom> WorkspaceAtoms { get; }
        public ObservableCollection<string> HazardWarnings { get; }
        public ObservableCollection<Molecule> CreatedMolecules { get; }

        public Atom? SelectedAtom
        {
            get => _selectedAtom;
            set
            {
                if (SetProperty(ref _selectedAtom, value))
                {
                    UpdateSelectedAtomDisplay();
                }
            }
        }

        public string TutorialText
        {
            get => _tutorialText;
            set => SetProperty(ref _tutorialText, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string SelectedAtomText
        {
            get => _selectedAtomText;
            set => SetProperty(ref _selectedAtomText, value);
        }

        public string AtomPropertiesText
        {
            get => _atomPropertiesText;
            set => SetProperty(ref _atomPropertiesText, value);
        }

        public bool IsHazardVisible
        {
            get => _isHazardVisible;
            set => SetProperty(ref _isHazardVisible, value);
        }

        public string HazardLevelText
        {
            get => _hazardLevelText;
            set => SetProperty(ref _hazardLevelText, value);
        }

        public Brush HazardBackground
        {
            get => _hazardBackground;
            set => SetProperty(ref _hazardBackground, value);
        }

        public bool IsMoleculeDisplayVisible
        {
            get => _isMoleculeDisplayVisible;
            set => SetProperty(ref _isMoleculeDisplayVisible, value);
        }

        public string MoleculeFormula
        {
            get => _moleculeFormula;
            set => SetProperty(ref _moleculeFormula, value);
        }

        public string MoleculeName
        {
            get => _moleculeName;
            set => SetProperty(ref _moleculeName, value);
        }

        public string MoleculeStabilityText
        {
            get => _moleculeStabilityText;
            set => SetProperty(ref _moleculeStabilityText, value);
        }

        public Brush MoleculeStabilityColor
        {
            get => _moleculeStabilityColor;
            set => SetProperty(ref _moleculeStabilityColor, value);
        }

        public bool CanCombineMolecules
        {
            get => _canCombineMolecules;
            set => SetProperty(ref _canCombineMolecules, value);
        }

        #endregion

        #region Commands

        public ICommand SelectAtomCommand { get; }
        public ICommand BuildMoleculeCommand { get; }
        public ICommand CombineMoleculesCommand { get; }
        public ICommand ClearWorkspaceCommand { get; }

        #endregion

        #region Events

        public event EventHandler<Atom>? AtomPlaced;
        public event EventHandler<Molecule>? MoleculeCreated;
        public event EventHandler<MoleculeReactionResult>? ReactionOccurred;
        public event EventHandler<ReactionHazard>? HazardDetected;

        #endregion

        #region Methods

        private void LoadAvailableAtoms()
        {
            var atoms = _gameManager.Atomic.GetAllAtoms();
            foreach (var atom in atoms)
            {
                AvailableAtoms.Add(new AtomViewModel(atom));
            }
        }

        private void OnSelectAtom(AtomViewModel? atomVm)
        {
            if (atomVm?.Atom == null) return;

            SelectedAtom = atomVm.Atom.Clone();
            StatusText = $"Click workspace to place {SelectedAtom.Name}";
        }

        public void PlaceAtomAtPosition(Point position)
        {
            if (SelectedAtom == null) return;

            var atom = SelectedAtom.Clone();
            atom.Position = position;

            _gameManager.AddAtomToWorkspace(atom);
            WorkspaceAtoms.Add(atom);

            AtomPlaced?.Invoke(this, atom);
            CheckHazards();

            StatusText = $"Placed {atom.Name}. Select more atoms or build molecule.";
            SelectedAtom = null;
            SelectedAtomText = "None";
            AtomPropertiesText = "";
        }

        private void OnBuildMolecule()
        {
            if (WorkspaceAtoms.Count < 2)
            {
                StatusText = "Need at least 2 atoms to build a molecule";
                return;
            }

            var atoms = WorkspaceAtoms.ToList();
            var bondInstructions = AutoBondAtoms(atoms);

            var molecule = _gameManager.TryBuildMolecule(atoms, bondInstructions);

            if (molecule != null)
            {
                CreatedMolecules.Add(molecule);
                UpdateMoleculeDisplay(molecule);
                MoleculeCreated?.Invoke(this, molecule);
                CanCombineMolecules = CreatedMolecules.Count >= 2;
            }
            else
            {
                StatusText = "Could not create molecule with current atoms";
            }
        }

        private void OnCombineMolecules()
        {
            if (CreatedMolecules.Count < 2)
            {
                StatusText = "Need at least 2 molecules to combine";
                return;
            }

            var mol1 = CreatedMolecules[0];
            var mol2 = CreatedMolecules[1];

            _gameManager.TryMolecularReaction(mol1, mol2);
        }

        private void OnClearWorkspace()
        {
            WorkspaceAtoms.Clear();
            CreatedMolecules.Clear();
            _gameManager.ClearAtomWorkspace();

            IsMoleculeDisplayVisible = false;
            IsHazardVisible = false;
            HazardWarnings.Clear();
            CanCombineMolecules = false;

            StatusText = "Workspace cleared";
        }

        private void UpdateSelectedAtomDisplay()
        {
            if (SelectedAtom == null)
            {
                SelectedAtomText = "None";
                AtomPropertiesText = "";
                return;
            }

            SelectedAtomText = $"{SelectedAtom.Name} ({SelectedAtom.Symbol})";
            AtomPropertiesText = $"Atomic #: {SelectedAtom.AtomicNumber}\n" +
                                 $"Mass: {SelectedAtom.AtomicMass:F2}\n" +
                                 $"Valence: {SelectedAtom.ValenceElectrons}\n" +
                                 $"Max Bonds: {SelectedAtom.MaxBonds}";
        }

        private void UpdateMoleculeDisplay(Molecule molecule)
        {
            IsMoleculeDisplayVisible = true;
            MoleculeFormula = molecule.Formula;
            MoleculeName = molecule.Name;

            switch (molecule.Stability)
            {
                case MoleculeStability.Stable:
                    MoleculeStabilityText = "Stability: Stable";
                    MoleculeStabilityColor = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    break;
                case MoleculeStability.Metastable:
                    MoleculeStabilityText = "Stability: Metastable";
                    MoleculeStabilityColor = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                    break;
                case MoleculeStability.Unstable:
                    MoleculeStabilityText = "Stability: UNSTABLE";
                    MoleculeStabilityColor = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    break;
                default:
                    MoleculeStabilityText = $"Stability: {molecule.Stability}";
                    MoleculeStabilityColor = Brushes.White;
                    break;
            }
        }

        private void CheckHazards()
        {
            var molecules = CreatedMolecules.ToList();
            var hazard = _gameManager.Atomic.AnalyzeHazard(molecules);

            HazardWarnings.Clear();

            if (hazard.Level > HazardLevel.Safe)
            {
                IsHazardVisible = true;
                HazardLevelText = $"Level: {hazard.Level}";

                foreach (var warning in hazard.Warnings)
                {
                    HazardWarnings.Add(warning);
                }

                HazardBackground = hazard.Level switch
                {
                    HazardLevel.Low => new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                    HazardLevel.Moderate => new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                    HazardLevel.High => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    HazardLevel.Extreme => new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    _ => Brushes.Transparent
                };

                HazardDetected?.Invoke(this, hazard);
            }
            else
            {
                IsHazardVisible = false;
            }
        }

        private System.Collections.Generic.List<(int, int, BondType)> AutoBondAtoms(System.Collections.Generic.List<Atom> atoms)
        {
            var bonds = new System.Collections.Generic.List<(int, int, BondType)>();

            // Simple auto-bonding algorithm
            for (int i = 0; i < atoms.Count - 1; i++)
            {
                for (int j = i + 1; j < atoms.Count; j++)
                {
                    if (atoms[i].CanBond() && atoms[j].CanBond())
                    {
                        var bondType = DetermineBondType(atoms[i], atoms[j]);
                        bonds.Add((i, j, bondType));
                    }
                }
            }

            return bonds;
        }

        private BondType DetermineBondType(Atom a1, Atom a2)
        {
            // Determine electronegativity difference for ionic vs covalent
            double diff = Math.Abs(a1.Electronegativity - a2.Electronegativity);

            if (diff > 1.7)
                return BondType.Ionic;

            // Check for hydrogen bonding
            if ((a1.Symbol == "H" && (a2.Symbol == "O" || a2.Symbol == "N" || a2.Symbol == "F")) ||
                (a2.Symbol == "H" && (a1.Symbol == "O" || a1.Symbol == "N" || a1.Symbol == "F")))
                return BondType.Hydrogen;

            // Default to single covalent
            return BondType.Single;
        }

        #endregion
    }

    /// <summary>
    /// ViewModel wrapper for Atom display in the periodic table
    /// </summary>
    public class AtomViewModel : ViewModelBase
    {
        public Atom Atom { get; }

        public string Symbol => Atom.Symbol;
        public string Name => Atom.Name;
        public int AtomicNumber => Atom.AtomicNumber;
        public Color Color => Atom.Color;
        public bool IsRadioactive => Atom.IsRadioactive;

        public AtomViewModel(Atom atom)
        {
            Atom = atom ?? throw new ArgumentNullException(nameof(atom));
        }
    }
}
