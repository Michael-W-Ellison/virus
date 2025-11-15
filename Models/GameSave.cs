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
        public string Symbol { get; set; } = string.Empty;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public int CurrentBonds { get; set; }
    }

    public class SerializableMolecule
    {
        public string Name { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public List<SerializableAtom> Atoms { get; set; } = new List<SerializableAtom>();
        public double PositionX { get; set; }
        public double PositionY { get; set; }
    }

    public class SerializableOrganism
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Health { get; set; }
        public double Size { get; set; }
        public int Generation { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public Dictionary<string, double> Resistances { get; set; } = new Dictionary<string, double>();
    }
}
