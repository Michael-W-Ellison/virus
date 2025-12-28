using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace BiochemSimulator.ViewModels
{
    /// <summary>
    /// ViewModel for the Virus Outbreak phase
    /// </summary>
    public class VirusOutbreakViewModel : ViewModelBase
    {
        private readonly GameManager _gameManager;

        private Chemical? _selectedWeapon;
        private string _selectedWeaponText = "No weapon selected";
        private int _aliveCount;
        private int _generationCount;
        private int _totalCreated;
        private bool _isOverlayVisible;

        public VirusOutbreakViewModel(GameManager gameManager)
        {
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));

            // Initialize collections
            AvailableWeapons = new ObservableCollection<Chemical>();
            ResistanceStats = new ObservableCollection<ResistanceStat>();

            // Initialize commands
            SelectWeaponCommand = new RelayCommand<Chemical>(OnSelectWeapon);
            ApplyChemicalCommand = new RelayCommand<Point>(OnApplyChemical, _ => SelectedWeapon != null);

            // Load weapons
            RefreshWeapons();
        }

        #region Properties

        public ObservableCollection<Chemical> AvailableWeapons { get; }
        public ObservableCollection<ResistanceStat> ResistanceStats { get; }

        public Chemical? SelectedWeapon
        {
            get => _selectedWeapon;
            set
            {
                if (SetProperty(ref _selectedWeapon, value))
                {
                    SelectedWeaponText = value != null
                        ? $"Selected: {value.Name}"
                        : "No weapon selected";
                }
            }
        }

        public string SelectedWeaponText
        {
            get => _selectedWeaponText;
            set => SetProperty(ref _selectedWeaponText, value);
        }

        public int AliveCount
        {
            get => _aliveCount;
            set => SetProperty(ref _aliveCount, value);
        }

        public int GenerationCount
        {
            get => _generationCount;
            set => SetProperty(ref _generationCount, value);
        }

        public int TotalCreated
        {
            get => _totalCreated;
            set => SetProperty(ref _totalCreated, value);
        }

        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        #endregion

        #region Commands

        public ICommand SelectWeaponCommand { get; }
        public ICommand ApplyChemicalCommand { get; }

        #endregion

        #region Events

        public event EventHandler<ChemicalApplicationResult>? ChemicalApplied;
        public event EventHandler? AllOrganismsDefeated;

        #endregion

        #region Methods

        public void RefreshWeapons()
        {
            AvailableWeapons.Clear();
            var weapons = _gameManager.Chemistry.GetDisinfectants();
            foreach (var weapon in weapons)
            {
                AvailableWeapons.Add(weapon);
            }
        }

        public void UpdateStats()
        {
            AliveCount = _gameManager.Organisms.GetAliveCount();
            GenerationCount = _gameManager.Organisms.GenerationsEvolved;
            TotalCreated = _gameManager.Organisms.TotalOrganismsCreated;

            UpdateResistanceStats();
        }

        private void UpdateResistanceStats()
        {
            ResistanceStats.Clear();
            var stats = _gameManager.Organisms.GetResistanceStats();

            foreach (var stat in stats)
            {
                ResistanceStats.Add(new ResistanceStat
                {
                    ChemicalName = stat.Key,
                    ResistanceLevel = stat.Value,
                    ResistancePercent = (int)(stat.Value * 100)
                });
            }
        }

        private void OnSelectWeapon(Chemical? weapon)
        {
            SelectedWeapon = weapon;
        }

        private void OnApplyChemical(Point? location)
        {
            if (SelectedWeapon == null || location == null) return;

            double radius = 100.0; // Default spray radius
            _gameManager.ApplyChemicalToDesktop(SelectedWeapon, location.Value, radius);

            UpdateStats();

            var result = new ChemicalApplicationResult
            {
                Chemical = SelectedWeapon,
                Location = location.Value,
                Radius = radius,
                OrganismsAffected = _gameManager.Organisms.GetAliveCount()
            };

            ChemicalApplied?.Invoke(this, result);

            if (AliveCount == 0)
            {
                AllOrganismsDefeated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ApplyChemicalAt(Point location)
        {
            OnApplyChemical(location);
        }

        public void Show()
        {
            IsOverlayVisible = true;
            RefreshWeapons();
            UpdateStats();
        }

        public void Hide()
        {
            IsOverlayVisible = false;
        }

        #endregion
    }

    /// <summary>
    /// Represents a resistance statistic for display
    /// </summary>
    public class ResistanceStat : ViewModelBase
    {
        private string _chemicalName = "";
        private double _resistanceLevel;
        private int _resistancePercent;

        public string ChemicalName
        {
            get => _chemicalName;
            set => SetProperty(ref _chemicalName, value);
        }

        public double ResistanceLevel
        {
            get => _resistanceLevel;
            set => SetProperty(ref _resistanceLevel, value);
        }

        public int ResistancePercent
        {
            get => _resistancePercent;
            set => SetProperty(ref _resistancePercent, value);
        }
    }

    /// <summary>
    /// Result of applying a chemical
    /// </summary>
    public class ChemicalApplicationResult
    {
        public Chemical Chemical { get; set; } = null!;
        public Point Location { get; set; }
        public double Radius { get; set; }
        public int OrganismsAffected { get; set; }
    }
}
