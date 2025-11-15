namespace BiochemSimulator.Models
{
    public enum GameState
    {
        Introduction,
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
