using System;
using System.Collections.Generic;

namespace BiochemSimulator.Models
{
    public class GameSave
    {
        public string SaveName { get; set; } = "Autosave";
        public string PlayerName { get; set; } = string.Empty;
        public DateTime SaveDate { get; set; }
        public int PlayTimeSeconds { get; set; }

        // Game State
        public GameState CurrentState { get; set; }
        public ExperimentPhase CurrentPhase { get; set; }

        // Current Session Stats
        public int SessionOrganismsDefeated { get; set; }
        public int SessionMoleculesCreated { get; set; }
        public int SessionAtomsPlaced { get; set; }

        // Atomic Workspace
        public List<SerializableAtom> WorkspaceAtoms { get; set; } = new List<SerializableAtom>();
        public List<SerializableMolecule> WorkspaceMolecules { get; set; } = new List<SerializableMolecule>();

        // Organisms (if in outbreak phase)
        public List<SerializableOrganism> ActiveOrganisms { get; set; } = new List<SerializableOrganism>();

        // Beaker contents (chemical names)
        public List<string> BeakerChemicals { get; set; } = new List<string>();

        // Organism manager stats
        public int TotalOrganismsCreated { get; set; }
        public int GenerationsEvolved { get; set; }

        // Inventory
        public List<string> AvailableChemicals { get; set; } = new List<string>();
        public Dictionary<string, int> ChemicalQuantities { get; set; } = new Dictionary<string, int>();

        public GameSave()
        {
            SaveDate = DateTime.Now;
        }
    }

    // Serializable versions of game objects (simplified for JSON)
    public class SerializableAtom
    {
        public int Id { get; set; }  // Unique identifier for referencing in bonds
        public string Symbol { get; set; } = string.Empty;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public int CurrentBonds { get; set; }
    }

    public class SerializableBond
    {
        public int Atom1Id { get; set; }
        public int Atom2Id { get; set; }
        public BondType Type { get; set; }
        public double BondEnergy { get; set; }
        public double BondLength { get; set; }
    }

    public class SerializableMolecule
    {
        public string Id { get; set; } = string.Empty;  // Guid as string
        public string Name { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public List<SerializableAtom> Atoms { get; set; } = new List<SerializableAtom>();
        public List<SerializableBond> Bonds { get; set; } = new List<SerializableBond>();
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public int Stability { get; set; }  // MoleculeStability as int
        public bool IsExplosive { get; set; }
        public bool IsFlammable { get; set; }
        public bool IsToxic { get; set; }
    }

    public class SerializableOrganism
    {
        public string Id { get; set; } = string.Empty;  // Guid as string
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Health { get; set; }
        public double Size { get; set; }
        public int Generation { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public double ReproductionRate { get; set; }
        public double MutationRate { get; set; }
        public int Type { get; set; }  // OrganismType as int
        public Dictionary<string, double> Resistances { get; set; } = new Dictionary<string, double>();
    }
}
