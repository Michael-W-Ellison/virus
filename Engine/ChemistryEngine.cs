using BiochemSimulator.Models;
using System.Windows.Media;

namespace BiochemSimulator.Engine
{
    public class ChemistryEngine
    {
        private Dictionary<string, Chemical> _chemicals;
        private List<ChemicalReaction> _reactions;

        public ChemistryEngine()
        {
            _chemicals = new Dictionary<string, Chemical>();
            _reactions = new List<ChemicalReaction>();
            InitializeChemicals();
            InitializeReactions();
        }

        private void InitializeChemicals()
        {
            // Basic chemicals for early experiments
            _chemicals["Water"] = new Chemical("Water", "H₂O", "Universal solvent", ChemicalType.Other, 7.0, Colors.LightBlue);
            _chemicals["HydrochloricAcid"] = new Chemical("Hydrochloric Acid", "HCl", "Strong acid", ChemicalType.Acid, 1.0, Colors.Yellow) { IsCaustic = true, Toxicity = 8.0 };
            _chemicals["SodiumHydroxide"] = new Chemical("Sodium Hydroxide", "NaOH", "Strong base", ChemicalType.Base, 14.0, Colors.White) { IsCaustic = true, Toxicity = 8.0 };
            _chemicals["SodiumChloride"] = new Chemical("Sodium Chloride", "NaCl", "Table salt", ChemicalType.Salt, 7.0, Colors.White);

            // Amino Acids
            _chemicals["Glycine"] = new Chemical("Glycine", "C₂H₅NO₂", "Simplest amino acid", ChemicalType.AminoAcid, 6.0, Colors.LightGray);
            _chemicals["Alanine"] = new Chemical("Alanine", "C₃H₇NO₂", "Non-polar amino acid", ChemicalType.AminoAcid, 6.0, Colors.LightGray);
            _chemicals["Cysteine"] = new Chemical("Cysteine", "C₃H₇NO₂S", "Contains sulfur", ChemicalType.AminoAcid, 5.0, Colors.LightYellow);

            // Nucleotides
            _chemicals["Adenine"] = new Chemical("Adenine", "C₅H₅N₅", "DNA/RNA base", ChemicalType.Nucleotide, 7.0, Colors.LightGreen);
            _chemicals["Thymine"] = new Chemical("Thymine", "C₅H₆N₂O₂", "DNA base", ChemicalType.Nucleotide, 7.0, Colors.LightGreen);
            _chemicals["Cytosine"] = new Chemical("Cytosine", "C₄H₅N₃O", "DNA/RNA base", ChemicalType.Nucleotide, 7.0, Colors.LightGreen);
            _chemicals["Guanine"] = new Chemical("Guanine", "C₅H₅N₅O", "DNA/RNA base", ChemicalType.Nucleotide, 7.0, Colors.LightGreen);
            _chemicals["Uracil"] = new Chemical("Uracil", "C₄H₄N₂O₂", "RNA base", ChemicalType.Nucleotide, 7.0, Colors.LightGreen);

            // Lipids
            _chemicals["Phospholipid"] = new Chemical("Phospholipid", "C₄₂H₈₂NO₈P", "Cell membrane component", ChemicalType.Lipid, 7.0, Colors.Pink);
            _chemicals["Cholesterol"] = new Chemical("Cholesterol", "C₂₇H₄₆O", "Membrane stabilizer", ChemicalType.Lipid, 7.0, Colors.LightPink);

            // Sugars and Energy
            _chemicals["Glucose"] = new Chemical("Glucose", "C₆H₁₂O₆", "Simple sugar, energy source", ChemicalType.Sugar, 7.0, Colors.LightGoldenrodYellow);
            _chemicals["ATP"] = new Chemical("ATP", "C₁₀H₁₆N₅O₁₃P₃", "Cellular energy currency", ChemicalType.OrganicCompound, 7.0, Colors.Orange);

            // Complex molecules
            _chemicals["Protein"] = new Chemical("Protein", "Variable", "Polymer of amino acids", ChemicalType.Protein, 7.0, Colors.Beige);
            _chemicals["RNA"] = new Chemical("RNA", "Variable", "Ribonucleic acid", ChemicalType.Nucleotide, 7.0, Colors.Purple);
            _chemicals["DNA"] = new Chemical("DNA", "Variable", "Deoxyribonucleic acid", ChemicalType.Nucleotide, 7.0, Colors.Blue);
            _chemicals["CellMembrane"] = new Chemical("Cell Membrane", "Variable", "Lipid bilayer", ChemicalType.Lipid, 7.0, Colors.Pink);

            // Disinfectants and antibiotics
            _chemicals["Bleach"] = new Chemical("Bleach", "NaClO", "Sodium hypochlorite", ChemicalType.Disinfectant, 12.0, Colors.PaleGoldenrod)
                { IsCaustic = true, IsOxidizer = true, Toxicity = 9.0, Reactivity = 8.0 };
            _chemicals["Ethanol"] = new Chemical("Ethanol", "C₂H₅OH", "Alcohol disinfectant", ChemicalType.Disinfectant, 7.0, Colors.Transparent)
                { Toxicity = 5.0, Reactivity = 4.0 };
            _chemicals["HydrogenPeroxide"] = new Chemical("Hydrogen Peroxide", "H₂O₂", "Oxidizing agent", ChemicalType.Oxidizer, 6.2, Colors.LightCyan)
                { IsOxidizer = true, Toxicity = 6.0, Reactivity = 7.0 };
            _chemicals["Penicillin"] = new Chemical("Penicillin", "C₁₆H₁₈N₂O₄S", "Antibiotic", ChemicalType.Antibiotic, 7.0, Colors.White)
                { Toxicity = 2.0, Reactivity = 3.0 };
            _chemicals["Lysozyme"] = new Chemical("Lysozyme", "C₆₁₃H₉₅₉N₁₉₃O₁₉₅S₁₀", "Enzyme that breaks cell walls", ChemicalType.Enzyme, 7.0, Colors.LightYellow)
                { Toxicity = 1.0, Reactivity = 5.0 };

            // Strong oxidizers and reducers
            _chemicals["PotassiumPermanganate"] = new Chemical("Potassium Permanganate", "KMnO₄", "Strong oxidizer", ChemicalType.Oxidizer, 7.0, Colors.Purple)
                { IsOxidizer = true, Toxicity = 7.0, Reactivity = 9.0 };
            _chemicals["SulfuricAcid"] = new Chemical("Sulfuric Acid", "H₂SO₄", "Very strong acid", ChemicalType.Acid, 0.3, Colors.Yellow)
                { IsCaustic = true, IsOxidizer = true, Toxicity = 10.0, Reactivity = 9.0 };
        }

        private void InitializeReactions()
        {
            // Acid-Base neutralization
            var neutralization = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["HydrochloricAcid"], _chemicals["SodiumHydroxide"] },
                Products = new List<Chemical> { _chemicals["SodiumChloride"], _chemicals["Water"] },
                Description = "Neutralization reaction produces salt and water",
                IsExothermic = true,
                EnergyChange = -57.3,
                VisualEffect = Colors.White,
                VisualDescription = "Solution becomes warm and clear"
            };
            _reactions.Add(neutralization);

            // Amino acids to protein
            var proteinSynthesis = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["Glycine"], _chemicals["Alanine"], _chemicals["Cysteine"] },
                Products = new List<Chemical> { _chemicals["Protein"] },
                Description = "Peptide bonds form between amino acids",
                IsExothermic = false,
                VisualEffect = Colors.Beige,
                VisualDescription = "Amino acids link together forming a chain"
            };
            _reactions.Add(proteinSynthesis);

            // Nucleotides to RNA
            var rnaSynthesis = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["Adenine"], _chemicals["Uracil"], _chemicals["Cytosine"], _chemicals["Guanine"] },
                Products = new List<Chemical> { _chemicals["RNA"] },
                Description = "Nucleotides polymerize into RNA strand",
                IsExothermic = false,
                VisualEffect = Colors.Purple,
                VisualDescription = "Nucleotides connect forming a twisted strand"
            };
            _reactions.Add(rnaSynthesis);

            // Nucleotides to DNA
            var dnaSynthesis = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["Adenine"], _chemicals["Thymine"], _chemicals["Cytosine"], _chemicals["Guanine"] },
                Products = new List<Chemical> { _chemicals["DNA"] },
                Description = "Nucleotides form double helix DNA",
                IsExothermic = false,
                VisualEffect = Colors.Blue,
                VisualDescription = "Double helix structure emerges"
            };
            _reactions.Add(dnaSynthesis);

            // Lipids to membrane
            var membraneSynthesis = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["Phospholipid"], _chemicals["Cholesterol"] },
                Products = new List<Chemical> { _chemicals["CellMembrane"] },
                Description = "Phospholipids self-assemble into bilayer",
                IsExothermic = false,
                VisualEffect = Colors.Pink,
                VisualDescription = "Lipids arrange into a flexible membrane"
            };
            _reactions.Add(membraneSynthesis);

            // Bleach + Acid = Toxic chlorine gas
            var toxicReaction = new ChemicalReaction
            {
                Reactants = new List<Chemical> { _chemicals["Bleach"], _chemicals["HydrochloricAcid"] },
                Products = new List<Chemical> { _chemicals["Water"], _chemicals["SodiumChloride"] },
                Description = "DANGEROUS: Releases toxic chlorine gas!",
                IsExothermic = true,
                VisualEffect = Colors.YellowGreen,
                VisualDescription = "Toxic yellow-green gas forms - HAZARDOUS!"
            };
            _reactions.Add(toxicReaction);
        }

        public Chemical? GetChemical(string name)
        {
            return _chemicals.ContainsKey(name) ? _chemicals[name] : null;
        }

        public List<Chemical> GetAvailableChemicals()
        {
            return _chemicals.Values.ToList();
        }

        public List<Chemical> GetChemicalsForPhase(ExperimentPhase phase)
        {
            switch (phase)
            {
                case ExperimentPhase.AminoAcids:
                    return new List<Chemical>
                    {
                        _chemicals["Water"],
                        _chemicals["Glycine"],
                        _chemicals["Alanine"],
                        _chemicals["Cysteine"]
                    };
                case ExperimentPhase.Proteins:
                    return new List<Chemical>
                    {
                        _chemicals["Glycine"],
                        _chemicals["Alanine"],
                        _chemicals["Cysteine"]
                    };
                case ExperimentPhase.RNA:
                    return new List<Chemical>
                    {
                        _chemicals["Adenine"],
                        _chemicals["Uracil"],
                        _chemicals["Cytosine"],
                        _chemicals["Guanine"]
                    };
                case ExperimentPhase.DNA:
                    return new List<Chemical>
                    {
                        _chemicals["Adenine"],
                        _chemicals["Thymine"],
                        _chemicals["Cytosine"],
                        _chemicals["Guanine"]
                    };
                case ExperimentPhase.Lipids:
                case ExperimentPhase.CellMembrane:
                    return new List<Chemical>
                    {
                        _chemicals["Phospholipid"],
                        _chemicals["Cholesterol"]
                    };
                default:
                    return new List<Chemical>();
            }
        }

        public List<Chemical> GetDisinfectants()
        {
            return _chemicals.Values
                .Where(c => c.Type == ChemicalType.Disinfectant ||
                           c.Type == ChemicalType.Antibiotic ||
                           c.Type == ChemicalType.Oxidizer ||
                           c.IsCaustic)
                .ToList();
        }

        public ChemicalReaction? TryReact(List<Chemical> chemicals)
        {
            foreach (var reaction in _reactions)
            {
                if (ReactantsMatch(chemicals, reaction.Reactants))
                {
                    return reaction;
                }
            }
            return null;
        }

        private bool ReactantsMatch(List<Chemical> provided, List<Chemical> required)
        {
            if (provided.Count != required.Count) return false;

            var providedNames = provided.Select(c => c.Name).OrderBy(n => n).ToList();
            var requiredNames = required.Select(c => c.Name).OrderBy(n => n).ToList();

            return providedNames.SequenceEqual(requiredNames);
        }

        public double CalculateDamageToOrganism(Chemical chemical, Organism organism)
        {
            double baseDamage = chemical.Toxicity * 10.0;

            // Modify based on chemical properties
            if (chemical.IsCaustic) baseDamage *= 1.5;
            if (chemical.IsOxidizer) baseDamage *= 1.3;

            // Apply organism resistance
            double resistance = organism.Resistances.ContainsKey(chemical.Name)
                ? organism.Resistances[chemical.Name]
                : 0.0;

            return baseDamage * (1.0 - resistance);
        }

        public Color? GetReactionColor(Chemical chem1, Chemical chem2)
        {
            var reaction = TryReact(new List<Chemical> { chem1, chem2 });
            return reaction?.VisualEffect;
        }

        public bool WillReactDangerously(Chemical chem1, Chemical chem2)
        {
            // Acid + Bleach = Dangerous
            if ((chem1.Type == ChemicalType.Acid && chem2.Name == "Bleach") ||
                (chem2.Type == ChemicalType.Acid && chem1.Name == "Bleach"))
                return true;

            // Strong acid + Strong base = Exothermic
            if ((chem1.IsCaustic && chem1.Type == ChemicalType.Acid &&
                 chem2.IsCaustic && chem2.Type == ChemicalType.Base) ||
                (chem2.IsCaustic && chem2.Type == ChemicalType.Acid &&
                 chem1.IsCaustic && chem1.Type == ChemicalType.Base))
                return true;

            return false;
        }
    }
}
