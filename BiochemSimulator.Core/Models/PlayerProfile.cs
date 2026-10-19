using System;
using System.Collections.Generic;

namespace BiochemSimulator.Models
{
    public class PlayerProfile
    {
        public string PlayerName { get; set; } = "Player";
        public DateTime CreatedDate { get; set; }
        public DateTime LastPlayedDate { get; set; }
        public int TotalPlayTime { get; set; } // in minutes

        // Game Statistics
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int TotalOrganismsDefeated { get; set; }
        public int TotalMoleculesCreated { get; set; }
        public int TotalAtomsPlaced { get; set; }
        public int TotalChemicalsUsed { get; set; }
        public int LifeFormsCreated { get; set; }
        public int ExplosionsCaused { get; set; }
        public int RadioactiveElementsUsed { get; set; }
        public int HighestGeneration { get; set; } // Highest organism generation survived

        // Discovery Tracking
        public HashSet<string> DiscoveredAtoms { get; set; } = new HashSet<string>();
        public HashSet<string> DiscoveredMolecules { get; set; } = new HashSet<string>();
        public HashSet<string> DiscoveredReactions { get; set; } = new HashSet<string>();

        // Achievements
        public HashSet<string> UnlockedAchievements { get; set; } = new HashSet<string>();
        public int TotalAchievementPoints { get; set; }

        // Current Save
        public string CurrentSaveFile { get; set; } = string.Empty;
        public bool HasActiveSave { get; set; }

        // Best Records
        public int FastestVictoryTime { get; set; } // in seconds, 0 = not set
        public int MostOrganismsDefeatedInOneGame { get; set; }
        public int LongestSurvivalTime { get; set; } // in seconds

        public PlayerProfile()
        {
            CreatedDate = DateTime.Now;
            LastPlayedDate = DateTime.Now;
        }

        public void UpdateLastPlayed()
        {
            LastPlayedDate = DateTime.Now;
        }

        public void AddPlayTime(int minutes)
        {
            TotalPlayTime += minutes;
        }

        public void RecordGameStart()
        {
            GamesPlayed++;
            UpdateLastPlayed();
        }

        public void RecordGameWon(int timeTaken, int organismsDefeated)
        {
            GamesWon++;
            TotalOrganismsDefeated += organismsDefeated;

            if (FastestVictoryTime == 0 || timeTaken < FastestVictoryTime)
            {
                FastestVictoryTime = timeTaken;
            }

            if (organismsDefeated > MostOrganismsDefeatedInOneGame)
            {
                MostOrganismsDefeatedInOneGame = organismsDefeated;
            }
        }

        public void RecordGameLost(int survivalTime, int organismsDefeated)
        {
            GamesLost++;
            TotalOrganismsDefeated += organismsDefeated;

            if (survivalTime > LongestSurvivalTime)
            {
                LongestSurvivalTime = survivalTime;
            }
        }

        public void DiscoverAtom(string atomSymbol)
        {
            DiscoveredAtoms.Add(atomSymbol);
        }

        public void DiscoverMolecule(string moleculeFormula)
        {
            DiscoveredMolecules.Add(moleculeFormula);
        }

        public void DiscoverReaction(string reactionName)
        {
            DiscoveredReactions.Add(reactionName);
        }

        public void UnlockAchievement(string achievementId, int points)
        {
            if (UnlockedAchievements.Add(achievementId))
            {
                TotalAchievementPoints += points;
            }
        }

        public double GetWinRate()
        {
            if (GamesPlayed == 0) return 0;
            return (double)GamesWon / GamesPlayed * 100;
        }

        public string GetPlayTimeFormatted()
        {
            if (TotalPlayTime < 60)
                return $"{TotalPlayTime} minutes";
            else if (TotalPlayTime < 1440)
                return $"{TotalPlayTime / 60} hours, {TotalPlayTime % 60} minutes";
            else
                return $"{TotalPlayTime / 1440} days, {(TotalPlayTime % 1440) / 60} hours";
        }
    }
}
