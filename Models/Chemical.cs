using System.Windows.Media;

namespace BiochemSimulator.Models
{
    public class Chemical
    {
        public string Name { get; set; }
        public string Formula { get; set; }
        public string Description { get; set; }
        public ChemicalType Type { get; set; }
        public double pH { get; set; }
        public double Concentration { get; set; }
        public Color Color { get; set; }
        public bool IsCaustic { get; set; }
        public bool IsOxidizer { get; set; }
        public bool IsReducer { get; set; }
        public double Toxicity { get; set; } // 0-10 scale
        public double Reactivity { get; set; } // 0-10 scale

        public Chemical(string name, string formula, string description, ChemicalType type,
            double ph = 7.0, Color? color = null)
        {
            Name = name;
            Formula = formula;
            Description = description;
            Type = type;
            pH = ph;
            Color = color ?? Colors.Transparent;
            Concentration = 1.0;
        }
    }

    public enum ChemicalType
    {
        Acid,
        Base,
        Salt,
        OrganicCompound,
        Enzyme,
        Nucleotide,
        AminoAcid,
        Lipid,
        Sugar,
        Protein,
        Disinfectant,
        Antibiotic,
        Oxidizer,
        Reducer,
        Other
    }

    public class ChemicalReaction
    {
        public List<Chemical> Reactants { get; set; }
        public List<Chemical> Products { get; set; }
        public string Description { get; set; }
        public bool IsExothermic { get; set; }
        public double EnergyChange { get; set; } // in kJ/mol
        public Color? VisualEffect { get; set; }
        public string VisualDescription { get; set; }

        public ChemicalReaction()
        {
            Reactants = new List<Chemical>();
            Products = new List<Chemical>();
            Description = string.Empty;
            VisualDescription = string.Empty;
        }
    }
}
