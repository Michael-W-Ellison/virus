using BiochemSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiochemSimulator.Engine
{
    public class AchievementManager
    {
        private readonly PlayerProfile _profile;
        private readonly List<Achievement> _allAchievements;
        private int _consecutiveLosses = 0;

        public event EventHandler<AchievementUnlockedEventArgs>? AchievementUnlocked;

        public AchievementManager(PlayerProfile profile)
        {
            _profile = profile;
            _allAchievements = AchievementDefinitions.GetAllAchievements();
        }

        public void CheckAchievements(GameEvent gameEvent, object? data = null)
        {
            switch (gameEvent)
            {
                case GameEvent.MoleculeCreated:
                    CheckMoleculeAchievements(data as string);
                    break;

                case GameEvent.AtomUsed:
                    CheckAtomAchievements(data as string);
                    break;

                case GameEvent.OrganismDefeated:
                    CheckCombatAchievements();
                    break;

                case GameEvent.LifeCreated:
                    UnlockAchievement("first_life");
                    break;

                case GameEvent.ExplosionCaused:
                    UnlockAchievement("explosive_reaction");
                    break;

                case GameEvent.RadioactiveUsed:
                    CheckRadioactiveAchievements(data as string);
                    break;

                case GameEvent.GameWon:
                    CheckVictoryAchievements(data);
                    break;

                case GameEvent.GameLost:
                    CheckLossAchievements();
                    break;

                case GameEvent.ReactionDiscovered:
                    CheckReactionAchievements();
                    break;

                case GameEvent.TrashDisposal:
                    UnlockAchievement("trash_disposal");
                    break;

                case GameEvent.OutbreakSurvived:
                    UnlockAchievement("outbreak_survivor");
                    break;

                case GameEvent.ResistanceDeveloped:
                    CheckResistanceAchievements();
                    break;
            }

            // Check time-based achievements
            CheckPlayTimeAchievements();

            // Check collection achievements
            CheckCollectionAchievements();

            // Check mastery achievements
            CheckMasteryAchievements();
        }

        private void CheckMoleculeAchievements(string? moleculeFormula)
        {
            if (_profile.TotalMoleculesCreated == 1)
            {
                UnlockAchievement("first_molecule");
            }

            if (moleculeFormula == "H2O")
            {
                UnlockAchievement("water_creation");
            }

            if (_profile.DiscoveredMolecules.Count >= 100)
            {
                UnlockAchievement("chemistry_100");
            }

            if (moleculeFormula == "RNA")
            {
                UnlockAchievement("rna_creation");
            }

            if (moleculeFormula == "DNA")
            {
                UnlockAchievement("dna_creation");
            }
        }

        private void CheckAtomAchievements(string? atomSymbol)
        {
            // Check if all basic atoms have been used
            if (_profile.DiscoveredAtoms.Contains("H") &&
                _profile.DiscoveredAtoms.Contains("C") &&
                _profile.DiscoveredAtoms.Contains("N") &&
                _profile.DiscoveredAtoms.Contains("O"))
            {
                UnlockAchievement("all_basic_atoms");
            }
        }

        private void CheckRadioactiveAchievements(string? atomSymbol)
        {
            UnlockAchievement("radioactive_user");

            if (atomSymbol == "U")
            {
                UnlockAchievement("uranium_user");
            }

            if (atomSymbol == "Pu")
            {
                UnlockAchievement("plutonium_user");
            }
        }

        private void CheckCombatAchievements()
        {
            if (_profile.TotalOrganismsDefeated == 1)
            {
                UnlockAchievement("first_kill");
            }

            if (_profile.TotalOrganismsDefeated >= 100)
            {
                UnlockAchievement("defeat_100");
            }

            if (_profile.TotalOrganismsDefeated >= 500)
            {
                UnlockAchievement("defeat_500");
            }
        }

        private void CheckVictoryAchievements(object? data)
        {
            _consecutiveLosses = 0; // Reset on win

            if (data is VictoryData victoryData)
            {
                if (victoryData.TimeTaken <= 300) // 5 minutes
                {
                    UnlockAchievement("speed_run");
                }

                if (victoryData.IconsLost == 0)
                {
                    UnlockAchievement("perfect_game");
                }

                if (victoryData.WasAfter3Losses)
                {
                    UnlockAchievement("comeback_kid");
                }
            }
        }

        private void CheckLossAchievements()
        {
            _consecutiveLosses++;

            if (_consecutiveLosses >= 3)
            {
                UnlockAchievement("three_strikes");
            }
        }

        private void CheckReactionAchievements()
        {
            if (_profile.DiscoveredReactions.Count >= 10)
            {
                UnlockAchievement("discover_10_reactions");
            }
        }

        private void CheckResistanceAchievements()
        {
            // This would need to track resistance encounters
            // For now, we'll assume the game manager tracks this
        }

        private void CheckPlayTimeAchievements()
        {
            if (_profile.TotalPlayTime >= 60) // 1 hour
            {
                UnlockAchievement("play_1hour");
            }

            if (_profile.TotalPlayTime >= 600) // 10 hours
            {
                UnlockAchievement("play_10hours");
            }
        }

        private void CheckCollectionAchievements()
        {
            if (_profile.DiscoveredAtoms.Count >= 5)
            {
                UnlockAchievement("discover_5_atoms");
            }

            if (_profile.DiscoveredAtoms.Count >= 17) // All atoms in the game
            {
                UnlockAchievement("discover_all_atoms");
            }
        }

        private void CheckMasteryAchievements()
        {
            if (_profile.GamesWon >= 10)
            {
                UnlockAchievement("win_10_games");
            }

            if (_profile.GamesWon >= 50)
            {
                UnlockAchievement("win_50_games");
            }

            if (_profile.HighestGeneration >= 10)
            {
                UnlockAchievement("high_generation");
            }

            // Check perfectionist achievement
            var nonSecretAchievements = _allAchievements.Where(a => !a.IsSecret).Select(a => a.Id);
            if (nonSecretAchievements.All(id => _profile.UnlockedAchievements.Contains(id)))
            {
                UnlockAchievement("perfectionist");
            }
        }

        private void UnlockAchievement(string achievementId)
        {
            if (_profile.UnlockedAchievements.Contains(achievementId))
                return; // Already unlocked

            var achievement = _allAchievements.FirstOrDefault(a => a.Id == achievementId);
            if (achievement == null)
                return;

            _profile.UnlockAchievement(achievementId, achievement.Points);
            achievement.UnlockedDate = DateTime.Now;

            // Fire event
            AchievementUnlocked?.Invoke(this, new AchievementUnlockedEventArgs(achievement));
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            return _allAchievements
                .Where(a => _profile.UnlockedAchievements.Contains(a.Id))
                .ToList();
        }

        public List<Achievement> GetLockedAchievements()
        {
            return _allAchievements
                .Where(a => !_profile.UnlockedAchievements.Contains(a.Id) && !a.IsSecret)
                .ToList();
        }

        public List<Achievement> GetAllAchievements()
        {
            return _allAchievements;
        }

        public double GetCompletionPercentage()
        {
            int total = _allAchievements.Count;
            int unlocked = _profile.UnlockedAchievements.Count;
            return (double)unlocked / total * 100;
        }
    }

    public enum GameEvent
    {
        MoleculeCreated,
        AtomUsed,
        OrganismDefeated,
        LifeCreated,
        ExplosionCaused,
        RadioactiveUsed,
        GameWon,
        GameLost,
        ReactionDiscovered,
        TrashDisposal,
        OutbreakSurvived,
        ResistanceDeveloped
    }

    public class AchievementUnlockedEventArgs : EventArgs
    {
        public Achievement Achievement { get; }

        public AchievementUnlockedEventArgs(Achievement achievement)
        {
            Achievement = achievement;
        }
    }

    public class VictoryData
    {
        public int TimeTaken { get; set; }
        public int IconsLost { get; set; }
        public bool WasAfter3Losses { get; set; }
    }
}
