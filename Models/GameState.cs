namespace BiochemSimulator.Models
{
    public enum GameState
    {
        Introduction,
        AtomicChemistry,        // NEW: Build molecules from atoms
        MolecularChemistry,     // NEW: Combine molecules
        BiochemSimulator,
        CreatingLife,
        ObservingLife,
        PostExperimentTasks,
        BiohazardAlarm,
        VirusOutbreak,
        ChemicalWarfare,
        GameOver,
        Victory
    }

    public enum ExperimentPhase
    {
        // Atomic Phase
        BuildingAtoms,
        SimpleMolecules,        // H2, O2, H2O
        ComplexMolecules,       // CH4, CO2, NH3

        // Biochemistry Phase
        AminoAcids,
        Proteins,
        Lipids,
        RNA,
        DNA,
        CellMembrane,
        PrimitiveCell,
        LivingOrganism
    }
}
