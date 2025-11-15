using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BiochemSimulator.Models
{
    public class Atom
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public int AtomicNumber { get; set; }
        public double AtomicMass { get; set; }
        public int Protons { get; set; }
        public int Neutrons { get; set; }
        public int Electrons { get; set; }
        public int ValenceElectrons { get; set; }
        public int MaxBonds { get; set; }
        public List<int> ElectronShells { get; set; }
        public Color Color { get; set; }
        public double Electronegativity { get; set; }
        public AtomState State { get; set; }

        // Radiological Properties
        public bool IsRadioactive { get; set; }
        public double HalfLife { get; set; } // in years (0 = stable)
        public RadioactiveDecayType DecayType { get; set; }
        public double RadiationLevel { get; set; } // 0-10 scale

        // Electrical Properties
        public double ElectricalConductivity { get; set; } // Siemens/meter (S/m)
        public double IonizationEnergy { get; set; } // in eV
        public int IonCharge { get; set; } // 0 = neutral, +/- for ions
        public bool IsConductor { get; set; }
        public bool IsInsulator { get; set; }

        // Thermal Properties
        public double MeltingPoint { get; set; } // in Celsius
        public double BoilingPoint { get; set; } // in Celsius
        public double ThermalConductivity { get; set; } // W/(m·K)
        public double HeatCapacity { get; set; } // J/(mol·K)
        public MaterialPhase PhaseAtRoomTemp { get; set; }

        public double Reactivity { get; set; } // 0-10 scale
        public Point Position { get; set; }
        public List<Bond> Bonds { get; set; }

        public Atom(string symbol, string name, int atomicNumber, double atomicMass)
        {
            Symbol = symbol;
            Name = name;
            AtomicNumber = atomicNumber;
            AtomicMass = atomicMass;
            Protons = atomicNumber;
            Electrons = atomicNumber;
            Neutrons = (int)(atomicMass - atomicNumber);
            ElectronShells = new List<int>();
            Bonds = new List<Bond>();
            State = AtomState.Stable;
            DecayType = RadioactiveDecayType.None;
            PhaseAtRoomTemp = MaterialPhase.Solid;
        }

        public bool CanBond()
        {
            return Bonds.Count < MaxBonds;
        }

        public int AvailableBondingSites()
        {
            return MaxBonds - Bonds.Count;
        }

        public Atom Clone()
        {
            return new Atom(Symbol, Name, AtomicNumber, AtomicMass)
            {
                Protons = Protons,
                Neutrons = Neutrons,
                Electrons = Electrons,
                ValenceElectrons = ValenceElectrons,
                MaxBonds = MaxBonds,
                ElectronShells = new List<int>(ElectronShells),
                Color = Color,
                Electronegativity = Electronegativity,
                State = State,
                IsRadioactive = IsRadioactive,
                HalfLife = HalfLife,
                DecayType = DecayType,
                RadiationLevel = RadiationLevel,
                ElectricalConductivity = ElectricalConductivity,
                IonizationEnergy = IonizationEnergy,
                IonCharge = IonCharge,
                IsConductor = IsConductor,
                IsInsulator = IsInsulator,
                MeltingPoint = MeltingPoint,
                BoilingPoint = BoilingPoint,
                ThermalConductivity = ThermalConductivity,
                HeatCapacity = HeatCapacity,
                PhaseAtRoomTemp = PhaseAtRoomTemp,
                Reactivity = Reactivity,
                Position = Position
            };
        }
    }

    public class Bond
    {
        public Atom Atom1 { get; set; }
        public Atom Atom2 { get; set; }
        public BondType Type { get; set; }
        public double BondEnergy { get; set; } // in kJ/mol
        public double BondLength { get; set; } // in angstroms
        public bool IsStable { get; set; }

        public Bond(Atom atom1, Atom atom2, BondType type, double energy, double length)
        {
            Atom1 = atom1;
            Atom2 = atom2;
            Type = type;
            BondEnergy = energy;
            BondLength = length;
            IsStable = true;
        }
    }

    public class Molecule
    {
        public Guid Id { get; set; }
        public string Formula { get; set; }
        public string Name { get; set; }
        public List<Atom> Atoms { get; set; }
        public List<Bond> Bonds { get; set; }
        public MoleculeStability Stability { get; set; }
        public double TotalEnergy { get; set; }
        public bool IsExplosive { get; set; }
        public bool IsFlammable { get; set; }
        public bool IsToxic { get; set; }
        public Point CenterOfMass { get; set; }
        public Color MoleculeColor { get; set; }

        public Molecule()
        {
            Id = Guid.NewGuid();
            Formula = string.Empty;
            Name = "Unknown";
            Atoms = new List<Atom>();
            Bonds = new List<Bond>();
            Stability = MoleculeStability.Stable;
        }

        public void CalculateFormula()
        {
            var atomCounts = new Dictionary<string, int>();
            foreach (var atom in Atoms)
            {
                if (!atomCounts.ContainsKey(atom.Symbol))
                    atomCounts[atom.Symbol] = 0;
                atomCounts[atom.Symbol]++;
            }

            // Order by element priority (C, H, O, N, then alphabetical)
            var ordered = atomCounts.OrderBy(kvp =>
            {
                if (kvp.Key == "C") return 0;
                if (kvp.Key == "H") return 1;
                if (kvp.Key == "O") return 2;
                if (kvp.Key == "N") return 3;
                return 4;
            }).ThenBy(kvp => kvp.Key);

            Formula = string.Join("", ordered.Select(kvp =>
                kvp.Value == 1 ? kvp.Key : $"{kvp.Key}{kvp.Value}"));
        }

        public void CheckStability()
        {
            // Check for unbonded atoms
            bool hasUnbondedAtoms = Atoms.Any(a => a.Bonds.Count == 0 && Atoms.Count > 1);

            // Check for overbonded atoms
            bool hasOverbondedAtoms = Atoms.Any(a => a.Bonds.Count > a.MaxBonds);

            // Check for highly reactive combinations
            bool hasReactiveCombination = HasDangerousCombination();

            if (hasOverbondedAtoms || hasReactiveCombination)
            {
                Stability = MoleculeStability.Unstable;
            }
            else if (hasUnbondedAtoms)
            {
                Stability = MoleculeStability.Metastable;
            }
            else
            {
                Stability = MoleculeStability.Stable;
            }
        }

        private bool HasDangerousCombination()
        {
            // Check for explosive combinations
            var symbols = Atoms.Select(a => a.Symbol).ToList();

            // Check for peroxide (O-O bond)
            var oxygens = Atoms.Where(a => a.Symbol == "O").ToList();
            if (oxygens.Count >= 2)
            {
                foreach (var oxygen in oxygens)
                {
                    if (oxygen.Bonds.Any(b =>
                        (b.Atom1.Symbol == "O" && b.Atom2.Symbol == "O")))
                    {
                        IsExplosive = true;
                        return true;
                    }
                }
            }

            // N-N triple bonds (explosive)
            var nitrogens = Atoms.Where(a => a.Symbol == "N").ToList();
            if (nitrogens.Count >= 2)
            {
                foreach (var nitrogen in nitrogens)
                {
                    if (nitrogen.Bonds.Any(b =>
                        b.Type == BondType.Triple &&
                        (b.Atom1.Symbol == "N" || b.Atom2.Symbol == "N")))
                    {
                        IsExplosive = true;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public enum AtomState
    {
        Stable,
        Excited,
        Ionized,
        Radioactive
    }

    public enum BondType
    {
        Single,
        Double,
        Triple,
        Ionic,
        Hydrogen,
        Metallic
    }

    public enum MoleculeStability
    {
        Stable,
        Metastable,
        Unstable,
        HighlyReactive,
        Explosive
    }

    public enum RadioactiveDecayType
    {
        None,           // Stable element
        Alpha,          // α decay - emits helium nucleus
        Beta,           // β decay - electron emission
        Gamma,          // γ decay - electromagnetic radiation
        Positron,       // β+ decay - positron emission
        Fission         // Nuclear fission
    }

    public enum MaterialPhase
    {
        Solid,
        Liquid,
        Gas,
        Plasma
    }
}
