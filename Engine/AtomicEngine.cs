using BiochemSimulator.Models;
using System.Windows;
using System.Windows.Media;

namespace BiochemSimulator.Engine
{
    public class AtomicEngine
    {
        private Dictionary<string, Atom> _atomicTable;
        private List<Molecule> _knownMolecules;
        private Random _random;

        public AtomicEngine()
        {
            _atomicTable = new Dictionary<string, Atom>();
            _knownMolecules = new List<Molecule>();
            _random = new Random();
            InitializePeriodicTable();
            InitializeKnownMolecules();
        }

        private void InitializePeriodicTable()
        {
            // Hydrogen
            _atomicTable["H"] = new Atom("H", "Hydrogen", 1, 1.008)
            {
                ValenceElectrons = 1,
                MaxBonds = 1,
                ElectronShells = new List<int> { 1 },
                Color = Colors.White,
                Electronegativity = 2.20,
                Reactivity = 7.0
            };

            // Helium
            _atomicTable["He"] = new Atom("He", "Helium", 2, 4.003)
            {
                ValenceElectrons = 2,
                MaxBonds = 0,
                ElectronShells = new List<int> { 2 },
                Color = Colors.Cyan,
                Electronegativity = 0,
                Reactivity = 0.0
            };

            // Carbon
            _atomicTable["C"] = new Atom("C", "Carbon", 6, 12.011)
            {
                ValenceElectrons = 4,
                MaxBonds = 4,
                ElectronShells = new List<int> { 2, 4 },
                Color = Colors.Black,
                Electronegativity = 2.55,
                Reactivity = 5.0
            };

            // Nitrogen
            _atomicTable["N"] = new Atom("N", "Nitrogen", 7, 14.007)
            {
                ValenceElectrons = 5,
                MaxBonds = 3,
                ElectronShells = new List<int> { 2, 5 },
                Color = Colors.Blue,
                Electronegativity = 3.04,
                Reactivity = 6.0
            };

            // Oxygen
            _atomicTable["O"] = new Atom("O", "Oxygen", 8, 15.999)
            {
                ValenceElectrons = 6,
                MaxBonds = 2,
                ElectronShells = new List<int> { 2, 6 },
                Color = Colors.Red,
                Electronegativity = 3.44,
                Reactivity = 8.0
            };

            // Fluorine
            _atomicTable["F"] = new Atom("F", "Fluorine", 9, 18.998)
            {
                ValenceElectrons = 7,
                MaxBonds = 1,
                ElectronShells = new List<int> { 2, 7 },
                Color = Colors.LightGreen,
                Electronegativity = 3.98,
                Reactivity = 10.0
            };

            // Sodium
            _atomicTable["Na"] = new Atom("Na", "Sodium", 11, 22.990)
            {
                ValenceElectrons = 1,
                MaxBonds = 1,
                ElectronShells = new List<int> { 2, 8, 1 },
                Color = Colors.Yellow,
                Electronegativity = 0.93,
                Reactivity = 9.0
            };

            // Magnesium
            _atomicTable["Mg"] = new Atom("Mg", "Magnesium", 12, 24.305)
            {
                ValenceElectrons = 2,
                MaxBonds = 2,
                ElectronShells = new List<int> { 2, 8, 2 },
                Color = Colors.Silver,
                Electronegativity = 1.31,
                Reactivity = 7.0
            };

            // Phosphorus
            _atomicTable["P"] = new Atom("P", "Phosphorus", 15, 30.974)
            {
                ValenceElectrons = 5,
                MaxBonds = 5,
                ElectronShells = new List<int> { 2, 8, 5 },
                Color = Colors.Orange,
                Electronegativity = 2.19,
                Reactivity = 7.0
            };

            // Sulfur
            _atomicTable["S"] = new Atom("S", "Sulfur", 16, 32.065)
            {
                ValenceElectrons = 6,
                MaxBonds = 6,
                ElectronShells = new List<int> { 2, 8, 6 },
                Color = Colors.Yellow,
                Electronegativity = 2.58,
                Reactivity = 6.0
            };

            // Chlorine
            _atomicTable["Cl"] = new Atom("Cl", "Chlorine", 17, 35.453)
            {
                ValenceElectrons = 7,
                MaxBonds = 1,
                ElectronShells = new List<int> { 2, 8, 7 },
                Color = Colors.Green,
                Electronegativity = 3.16,
                Reactivity = 9.0
            };

            // Potassium
            _atomicTable["K"] = new Atom("K", "Potassium", 19, 39.098)
            {
                ValenceElectrons = 1,
                MaxBonds = 1,
                ElectronShells = new List<int> { 2, 8, 8, 1 },
                Color = Colors.Violet,
                Electronegativity = 0.82,
                Reactivity = 9.5
            };

            // Calcium
            _atomicTable["Ca"] = new Atom("Ca", "Calcium", 20, 40.078)
            {
                ValenceElectrons = 2,
                MaxBonds = 2,
                ElectronShells = new List<int> { 2, 8, 8, 2 },
                Color = Colors.Gray,
                Electronegativity = 1.00,
                Reactivity = 7.0
            };

            // Manganese
            _atomicTable["Mn"] = new Atom("Mn", "Manganese", 25, 54.938)
            {
                ValenceElectrons = 7,
                MaxBonds = 7,
                ElectronShells = new List<int> { 2, 8, 13, 2 },
                Color = Colors.Purple,
                Electronegativity = 1.55,
                Reactivity = 6.0
            };

            // Iron
            _atomicTable["Fe"] = new Atom("Fe", "Iron", 26, 55.845)
            {
                ValenceElectrons = 2,
                MaxBonds = 6,
                ElectronShells = new List<int> { 2, 8, 14, 2 },
                Color = Colors.Brown,
                Electronegativity = 1.83,
                Reactivity = 5.0
            };
        }

        private void InitializeKnownMolecules()
        {
            // Water (H2O)
            var water = new Molecule
            {
                Name = "Water",
                Formula = "H2O",
                Stability = MoleculeStability.Stable,
                MoleculeColor = Colors.LightBlue
            };
            _knownMolecules.Add(water);

            // Oxygen gas (O2)
            var oxygen = new Molecule
            {
                Name = "Oxygen Gas",
                Formula = "O2",
                Stability = MoleculeStability.Stable,
                IsFlammable = true,
                MoleculeColor = Colors.LightCyan
            };
            _knownMolecules.Add(oxygen);

            // Hydrogen gas (H2)
            var hydrogen = new Molecule
            {
                Name = "Hydrogen Gas",
                Formula = "H2",
                Stability = MoleculeStability.HighlyReactive,
                IsFlammable = true,
                IsExplosive = true,
                MoleculeColor = Colors.White
            };
            _knownMolecules.Add(hydrogen);

            // Carbon dioxide (CO2)
            var co2 = new Molecule
            {
                Name = "Carbon Dioxide",
                Formula = "CO2",
                Stability = MoleculeStability.Stable,
                MoleculeColor = Colors.Gray
            };
            _knownMolecules.Add(co2);

            // Methane (CH4)
            var methane = new Molecule
            {
                Name = "Methane",
                Formula = "CH4",
                Stability = MoleculeStability.Stable,
                IsFlammable = true,
                MoleculeColor = Colors.LightGray
            };
            _knownMolecules.Add(methane);

            // Ammonia (NH3)
            var ammonia = new Molecule
            {
                Name = "Ammonia",
                Formula = "NH3",
                Stability = MoleculeStability.Stable,
                IsToxic = true,
                MoleculeColor = Colors.LightYellow
            };
            _knownMolecules.Add(ammonia);

            // Hydrogen Peroxide (H2O2)
            var peroxide = new Molecule
            {
                Name = "Hydrogen Peroxide",
                Formula = "H2O2",
                Stability = MoleculeStability.Metastable,
                IsExplosive = true,
                MoleculeColor = Colors.PaleTurquoise
            };
            _knownMolecules.Add(peroxide);

            // Sodium Chloride (NaCl)
            var salt = new Molecule
            {
                Name = "Sodium Chloride",
                Formula = "NaCl",
                Stability = MoleculeStability.Stable,
                MoleculeColor = Colors.White
            };
            _knownMolecules.Add(salt);
        }

        public Atom GetAtom(string symbol)
        {
            return _atomicTable.ContainsKey(symbol) ? _atomicTable[symbol].Clone() : null!;
        }

        public List<Atom> GetBasicAtoms()
        {
            return new List<Atom>
            {
                GetAtom("H"),
                GetAtom("C"),
                GetAtom("N"),
                GetAtom("O")
            };
        }

        public List<Atom> GetAllAtoms()
        {
            return _atomicTable.Values.Select(a => a.Clone()).ToList();
        }

        public bool CanBond(Atom atom1, Atom atom2)
        {
            return atom1.CanBond() && atom2.CanBond();
        }

        public Bond? CreateBond(Atom atom1, Atom atom2, BondType bondType)
        {
            if (!CanBond(atom1, atom2))
                return null;

            // Calculate bond properties
            double bondEnergy = CalculateBondEnergy(atom1, atom2, bondType);
            double bondLength = CalculateBondLength(atom1, atom2);

            var bond = new Bond(atom1, atom2, bondType, bondEnergy, bondLength);

            // Check stability
            bond.IsStable = IsBondStable(atom1, atom2, bondType);

            atom1.Bonds.Add(bond);
            atom2.Bonds.Add(bond);

            return bond;
        }

        private double CalculateBondEnergy(Atom atom1, Atom atom2, BondType bondType)
        {
            // Simplified bond energy calculation
            double baseEnergy = 0;

            string bondPair = $"{atom1.Symbol}-{atom2.Symbol}";

            switch (bondPair)
            {
                case "H-H": baseEnergy = 436; break;
                case "O-O": baseEnergy = 498; break;
                case "N-N": baseEnergy = 945; break;
                case "C-C": baseEnergy = 348; break;
                case "C-H": baseEnergy = 413; break;
                case "O-H": baseEnergy = 463; break;
                case "N-H": baseEnergy = 391; break;
                case "C-O": baseEnergy = 358; break;
                case "C-N": baseEnergy = 305; break;
                default: baseEnergy = 300; break;
            }

            // Multiply for double/triple bonds
            switch (bondType)
            {
                case BondType.Double: baseEnergy *= 2; break;
                case BondType.Triple: baseEnergy *= 3; break;
            }

            return baseEnergy;
        }

        private double CalculateBondLength(Atom atom1, Atom atom2)
        {
            // Simplified bond length (in angstroms)
            return 1.0 + (_random.NextDouble() * 0.5);
        }

        private bool IsBondStable(Atom atom1, Atom atom2, BondType bondType)
        {
            // Check electronegativity difference for ionic vs covalent
            double electronegativityDiff = Math.Abs(atom1.Electronegativity - atom2.Electronegativity);

            if (bondType == BondType.Ionic && electronegativityDiff < 1.7)
                return false;

            if (bondType != BondType.Ionic && electronegativityDiff > 2.0)
                return false;

            return true;
        }

        public Molecule CreateMolecule(List<Atom> atoms, List<Bond> bonds)
        {
            var molecule = new Molecule
            {
                Atoms = atoms,
                Bonds = bonds
            };

            molecule.CalculateFormula();
            molecule.CheckStability();

            // Try to identify the molecule
            var known = _knownMolecules.FirstOrDefault(m => m.Formula == molecule.Formula);
            if (known != null)
            {
                molecule.Name = known.Name;
                molecule.IsExplosive = known.IsExplosive;
                molecule.IsFlammable = known.IsFlammable;
                molecule.IsToxic = known.IsToxic;
                molecule.MoleculeColor = known.MoleculeColor;
            }

            return molecule;
        }

        public MoleculeReactionResult TryReact(Molecule mol1, Molecule mol2)
        {
            var result = new MoleculeReactionResult
            {
                Success = false,
                Products = new List<Molecule>()
            };

            // H2 + O2 -> H2O (explosive!)
            if ((mol1.Formula == "H2" && mol2.Formula == "O2") ||
                (mol1.Formula == "O2" && mol2.Formula == "H2"))
            {
                result.Success = true;
                result.IsExplosive = true;
                result.EnergyReleased = 572; // kJ/mol
                result.Description = "EXPLOSIVE REACTION! Hydrogen and oxygen combine to form water with a massive release of energy!";
                result.VisualEffect = ReactionVisualEffect.Explosion;

                // Create 2 water molecules
                result.Products.Add(_knownMolecules.First(m => m.Formula == "H2O"));
                result.Products.Add(_knownMolecules.First(m => m.Formula == "H2O"));
            }
            // Na + Cl -> NaCl (vigorous)
            else if ((mol1.Formula == "Na" && mol2.Formula == "Cl") ||
                     (mol1.Formula == "Cl" && mol2.Formula == "Na"))
            {
                result.Success = true;
                result.IsVigorous = true;
                result.EnergyReleased = 411;
                result.Description = "Vigorous reaction! Sodium and chlorine form table salt with heat and light.";
                result.VisualEffect = ReactionVisualEffect.Flash;
                result.Products.Add(_knownMolecules.First(m => m.Formula == "NaCl"));
            }
            // C + O2 -> CO2 (combustion)
            else if ((mol1.Formula == "C" && mol2.Formula == "O2") ||
                     (mol1.Formula == "O2" && mol2.Formula == "C"))
            {
                result.Success = true;
                result.IsCombustion = true;
                result.EnergyReleased = 393;
                result.Description = "Combustion! Carbon burns in oxygen to form carbon dioxide.";
                result.VisualEffect = ReactionVisualEffect.Fire;
                result.Products.Add(_knownMolecules.First(m => m.Formula == "CO2"));
            }
            // H2O2 decomposition (unstable)
            else if (mol1.Formula == "H2O2" || mol2.Formula == "H2O2")
            {
                result.Success = true;
                result.IsDecomposition = true;
                result.EnergyReleased = 98;
                result.Description = "Hydrogen peroxide decomposes into water and oxygen gas!";
                result.VisualEffect = ReactionVisualEffect.Bubbling;
                result.Products.Add(_knownMolecules.First(m => m.Formula == "H2O"));
                result.Products.Add(_knownMolecules.First(m => m.Formula == "O2"));
            }

            return result;
        }

        public ReactionHazard AnalyzeHazard(List<Molecule> molecules)
        {
            var hazard = new ReactionHazard
            {
                Level = HazardLevel.Safe
            };

            bool hasExplosive = molecules.Any(m => m.IsExplosive);
            bool hasFlammable = molecules.Any(m => m.IsFlammable);
            bool hasOxidizer = molecules.Any(m => m.Formula == "O2");
            bool hasUnstable = molecules.Any(m => m.Stability == MoleculeStability.Unstable);

            if (hasExplosive && hasFlammable)
            {
                hazard.Level = HazardLevel.Extreme;
                hazard.Warnings.Add("EXTREME DANGER: Explosive and flammable materials in proximity!");
            }
            else if (hasFlammable && hasOxidizer)
            {
                hazard.Level = HazardLevel.High;
                hazard.Warnings.Add("HIGH DANGER: Flammable material with oxidizer - fire/explosion risk!");
            }
            else if (hasExplosive)
            {
                hazard.Level = HazardLevel.Moderate;
                hazard.Warnings.Add("CAUTION: Explosive material present. Handle carefully.");
            }
            else if (hasUnstable)
            {
                hazard.Level = HazardLevel.Low;
                hazard.Warnings.Add("Warning: Unstable molecule detected.");
            }

            return hazard;
        }
    }

    public class MoleculeReactionResult
    {
        public bool Success { get; set; }
        public List<Molecule> Products { get; set; } = new List<Molecule>();
        public double EnergyReleased { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsExplosive { get; set; }
        public bool IsVigorous { get; set; }
        public bool IsCombustion { get; set; }
        public bool IsDecomposition { get; set; }
        public ReactionVisualEffect VisualEffect { get; set; }
    }

    public class ReactionHazard
    {
        public HazardLevel Level { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public enum HazardLevel
    {
        Safe,
        Low,
        Moderate,
        High,
        Extreme
    }

    public enum ReactionVisualEffect
    {
        None,
        Bubbling,
        Flash,
        Fire,
        Explosion,
        Spark,
        ColorChange
    }
}
