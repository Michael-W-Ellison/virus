using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class AchievementManagerTests
    {
        private readonly PlayerProfile _profile;
        private readonly AchievementManager _manager;

        public AchievementManagerTests()
        {
            _profile = new PlayerProfile { PlayerName = "TestPlayer" };
            _manager = new AchievementManager(_profile);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeWithProfile()
        {
            // Assert
            Assert.NotNull(_manager);
            Assert.NotNull(_manager.GetAllAchievements());
        }

        [Fact]
        public void Constructor_ShouldLoadAchievementDefinitions()
        {
            // Act
            var achievements = _manager.GetAllAchievements();

            // Assert
            Assert.NotEmpty(achievements);
        }

        #endregion

        #region Molecule Achievement Tests

        [Fact]
        public void CheckAchievements_MoleculeCreated_ShouldUnlockFirstMolecule()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;
            Achievement? unlockedAchievement = null;
            _manager.AchievementUnlocked += (s, e) => unlockedAchievement = e.Achievement;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2");

            // Assert
            Assert.Contains("first_molecule", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_MoleculeCreated_ShouldUnlockWaterCreation()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2O");

            // Assert
            Assert.Contains("water_creation", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_MoleculeCreated_ShouldUnlockRNACreation()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "RNA");

            // Assert
            Assert.Contains("rna_creation", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_MoleculeCreated_ShouldUnlockDNACreation()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "DNA");

            // Assert
            Assert.Contains("dna_creation", _profile.UnlockedAchievements);
        }

        #endregion

        #region Atom Achievement Tests

        [Fact]
        public void CheckAchievements_AtomUsed_ShouldUnlockAllBasicAtoms()
        {
            // Arrange
            _profile.DiscoverAtom("H");
            _profile.DiscoverAtom("C");
            _profile.DiscoverAtom("N");
            _profile.DiscoverAtom("O");

            // Act
            _manager.CheckAchievements(GameEvent.AtomUsed, "O");

            // Assert
            Assert.Contains("all_basic_atoms", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_AtomUsed_ShouldNotUnlockWithMissingAtoms()
        {
            // Arrange
            _profile.DiscoverAtom("H");
            _profile.DiscoverAtom("C");

            // Act
            _manager.CheckAchievements(GameEvent.AtomUsed, "C");

            // Assert
            Assert.DoesNotContain("all_basic_atoms", _profile.UnlockedAchievements);
        }

        #endregion

        #region Radioactive Achievement Tests

        [Fact]
        public void CheckAchievements_RadioactiveUsed_ShouldUnlockRadioactiveUser()
        {
            // Act
            _manager.CheckAchievements(GameEvent.RadioactiveUsed, "Rn");

            // Assert
            Assert.Contains("radioactive_user", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_RadioactiveUsed_ShouldUnlockUraniumUser()
        {
            // Act
            _manager.CheckAchievements(GameEvent.RadioactiveUsed, "U");

            // Assert
            Assert.Contains("uranium_user", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_RadioactiveUsed_ShouldUnlockPlutoniumUser()
        {
            // Act
            _manager.CheckAchievements(GameEvent.RadioactiveUsed, "Pu");

            // Assert
            Assert.Contains("plutonium_user", _profile.UnlockedAchievements);
        }

        #endregion

        #region Combat Achievement Tests

        [Fact]
        public void CheckAchievements_OrganismDefeated_ShouldUnlockFirstKill()
        {
            // Arrange
            _profile.TotalOrganismsDefeated = 1;

            // Act
            _manager.CheckAchievements(GameEvent.OrganismDefeated);

            // Assert
            Assert.Contains("first_kill", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_OrganismDefeated_ShouldUnlockDefeat100()
        {
            // Arrange
            _profile.TotalOrganismsDefeated = 100;

            // Act
            _manager.CheckAchievements(GameEvent.OrganismDefeated);

            // Assert
            Assert.Contains("defeat_100", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_OrganismDefeated_ShouldUnlockDefeat500()
        {
            // Arrange
            _profile.TotalOrganismsDefeated = 500;

            // Act
            _manager.CheckAchievements(GameEvent.OrganismDefeated);

            // Assert
            Assert.Contains("defeat_500", _profile.UnlockedAchievements);
        }

        #endregion

        #region Life Creation Tests

        [Fact]
        public void CheckAchievements_LifeCreated_ShouldUnlockFirstLife()
        {
            // Act
            _manager.CheckAchievements(GameEvent.LifeCreated);

            // Assert
            Assert.Contains("first_life", _profile.UnlockedAchievements);
        }

        #endregion

        #region Explosion Achievement Tests

        [Fact]
        public void CheckAchievements_ExplosionCaused_ShouldUnlockExplosiveReaction()
        {
            // Act
            _manager.CheckAchievements(GameEvent.ExplosionCaused);

            // Assert
            Assert.Contains("explosive_reaction", _profile.UnlockedAchievements);
        }

        #endregion

        #region Victory Achievement Tests

        [Fact]
        public void CheckAchievements_GameWon_ShouldUnlockSpeedRun_WhenUnder5Minutes()
        {
            // Arrange
            var victoryData = new VictoryData
            {
                TimeTaken = 250, // Under 5 minutes
                IconsLost = 0,
                WasAfter3Losses = false
            };

            // Act
            _manager.CheckAchievements(GameEvent.GameWon, victoryData);

            // Assert
            Assert.Contains("speed_run", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_GameWon_ShouldUnlockPerfectGame_WhenNoIconsLost()
        {
            // Arrange
            var victoryData = new VictoryData
            {
                TimeTaken = 600,
                IconsLost = 0,
                WasAfter3Losses = false
            };

            // Act
            _manager.CheckAchievements(GameEvent.GameWon, victoryData);

            // Assert
            Assert.Contains("perfect_game", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_GameWon_ShouldUnlockComebackKid()
        {
            // Arrange
            var victoryData = new VictoryData
            {
                TimeTaken = 600,
                IconsLost = 5,
                WasAfter3Losses = true
            };

            // Act
            _manager.CheckAchievements(GameEvent.GameWon, victoryData);

            // Assert
            Assert.Contains("comeback_kid", _profile.UnlockedAchievements);
        }

        #endregion

        #region Loss Achievement Tests

        [Fact]
        public void CheckAchievements_GameLost_ThreeTimes_ShouldUnlockThreeStrikes()
        {
            // Act - Lose 3 times
            _manager.CheckAchievements(GameEvent.GameLost);
            _manager.CheckAchievements(GameEvent.GameLost);
            _manager.CheckAchievements(GameEvent.GameLost);

            // Assert
            Assert.Contains("three_strikes", _profile.UnlockedAchievements);
        }

        #endregion

        #region Trash Disposal Tests

        [Fact]
        public void CheckAchievements_TrashDisposal_ShouldUnlockTrashDisposal()
        {
            // Act
            _manager.CheckAchievements(GameEvent.TrashDisposal);

            // Assert
            Assert.Contains("trash_disposal", _profile.UnlockedAchievements);
        }

        #endregion

        #region Outbreak Survived Tests

        [Fact]
        public void CheckAchievements_OutbreakSurvived_ShouldUnlockOutbreakSurvivor()
        {
            // Act
            _manager.CheckAchievements(GameEvent.OutbreakSurvived);

            // Assert
            Assert.Contains("outbreak_survivor", _profile.UnlockedAchievements);
        }

        #endregion

        #region Resistance Achievement Tests

        [Fact]
        public void CheckAchievements_ResistanceDeveloped_ShouldUnlockFirstResistance()
        {
            // Arrange
            _profile.ResistanceEncounters = 1;

            // Act
            _manager.CheckAchievements(GameEvent.ResistanceDeveloped);

            // Assert
            Assert.Contains("first_resistance", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_ResistanceDeveloped_ShouldUnlockResistanceExpert()
        {
            // Arrange
            _profile.ResistanceEncounters = 10;

            // Act
            _manager.CheckAchievements(GameEvent.ResistanceDeveloped);

            // Assert
            Assert.Contains("resistance_expert", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_ResistanceDeveloped_ShouldUnlockSuperResistance()
        {
            // Arrange
            _profile.HighestResistanceLevel = 0.85;

            // Act
            _manager.CheckAchievements(GameEvent.ResistanceDeveloped);

            // Assert
            Assert.Contains("super_resistance", _profile.UnlockedAchievements);
        }

        #endregion

        #region Reaction Achievement Tests

        [Fact]
        public void CheckAchievements_ReactionDiscovered_ShouldUnlockDiscover10Reactions()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                _profile.DiscoverReaction($"Reaction{i}");
            }

            // Act
            _manager.CheckAchievements(GameEvent.ReactionDiscovered);

            // Assert
            Assert.Contains("discover_10_reactions", _profile.UnlockedAchievements);
        }

        #endregion

        #region Play Time Achievement Tests

        [Fact]
        public void CheckAchievements_ShouldUnlockPlay1Hour()
        {
            // Arrange
            _profile.TotalPlayTime = 60;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated);

            // Assert
            Assert.Contains("play_1hour", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_ShouldUnlockPlay10Hours()
        {
            // Arrange
            _profile.TotalPlayTime = 600;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated);

            // Assert
            Assert.Contains("play_10hours", _profile.UnlockedAchievements);
        }

        #endregion

        #region Collection Achievement Tests

        [Fact]
        public void CheckAchievements_ShouldUnlockDiscover5Atoms()
        {
            // Arrange
            _profile.DiscoverAtom("H");
            _profile.DiscoverAtom("C");
            _profile.DiscoverAtom("N");
            _profile.DiscoverAtom("O");
            _profile.DiscoverAtom("S");

            // Act
            _manager.CheckAchievements(GameEvent.AtomUsed);

            // Assert
            Assert.Contains("discover_5_atoms", _profile.UnlockedAchievements);
        }

        #endregion

        #region Mastery Achievement Tests

        [Fact]
        public void CheckAchievements_ShouldUnlockWin10Games()
        {
            // Arrange
            _profile.GamesWon = 10;

            // Act
            _manager.CheckAchievements(GameEvent.GameWon);

            // Assert
            Assert.Contains("win_10_games", _profile.UnlockedAchievements);
        }

        [Fact]
        public void CheckAchievements_ShouldUnlockHighGeneration()
        {
            // Arrange
            _profile.HighestGeneration = 10;

            // Act
            _manager.CheckAchievements(GameEvent.GameWon);

            // Assert
            Assert.Contains("high_generation", _profile.UnlockedAchievements);
        }

        #endregion

        #region Event Tests

        [Fact]
        public void AchievementUnlocked_ShouldFireEvent_WhenAchievementUnlocked()
        {
            // Arrange
            Achievement? receivedAchievement = null;
            _manager.AchievementUnlocked += (s, e) => receivedAchievement = e.Achievement;
            _profile.TotalMoleculesCreated = 1;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2");

            // Assert
            Assert.NotNull(receivedAchievement);
            Assert.Equal("first_molecule", receivedAchievement.Id);
        }

        [Fact]
        public void AchievementUnlocked_ShouldNotFireEvent_WhenAlreadyUnlocked()
        {
            // Arrange
            _profile.UnlockedAchievements.Add("first_molecule");
            _profile.TotalMoleculesCreated = 1;
            int eventCount = 0;
            _manager.AchievementUnlocked += (s, e) => eventCount++;

            // Act
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2");

            // Assert
            Assert.Equal(0, eventCount);
        }

        #endregion

        #region Utility Methods Tests

        [Fact]
        public void GetUnlockedAchievements_ShouldReturnCorrectList()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2O");

            // Act
            var unlocked = _manager.GetUnlockedAchievements();

            // Assert
            Assert.NotEmpty(unlocked);
            Assert.All(unlocked, a => Assert.Contains(a.Id, _profile.UnlockedAchievements));
        }

        [Fact]
        public void GetLockedAchievements_ShouldExcludeUnlocked()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2O");

            // Act
            var locked = _manager.GetLockedAchievements();

            // Assert
            Assert.DoesNotContain(locked, a => a.Id == "water_creation");
        }

        [Fact]
        public void GetCompletionPercentage_ShouldCalculateCorrectly()
        {
            // Arrange
            var totalAchievements = _manager.GetAllAchievements().Count;

            // Act
            var percentage = _manager.GetCompletionPercentage();

            // Assert
            Assert.Equal(0, percentage); // No achievements unlocked yet
        }

        [Fact]
        public void GetCompletionPercentage_ShouldIncreaseWithUnlockedAchievements()
        {
            // Arrange
            _profile.TotalMoleculesCreated = 1;
            _manager.CheckAchievements(GameEvent.MoleculeCreated, "H2O");
            var totalAchievements = _manager.GetAllAchievements().Count;

            // Act
            var percentage = _manager.GetCompletionPercentage();

            // Assert
            Assert.True(percentage > 0);
        }

        #endregion
    }
}
