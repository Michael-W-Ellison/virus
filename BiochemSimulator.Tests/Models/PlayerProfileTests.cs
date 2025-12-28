using BiochemSimulator.Models;
using System;
using Xunit;

namespace BiochemSimulator.Tests.Models
{
    public class PlayerProfileTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Act
            var profile = new PlayerProfile();

            // Assert
            Assert.Equal("Player", profile.PlayerName);
            Assert.Equal(0, profile.TotalPlayTime);
            Assert.Equal(0, profile.GamesPlayed);
            Assert.Equal(0, profile.GamesWon);
            Assert.Equal(0, profile.GamesLost);
            Assert.Empty(profile.DiscoveredAtoms);
            Assert.Empty(profile.DiscoveredMolecules);
            Assert.Empty(profile.UnlockedAchievements);
        }

        [Fact]
        public void Constructor_ShouldSetCreatedDate()
        {
            // Act
            var before = DateTime.Now;
            var profile = new PlayerProfile();
            var after = DateTime.Now;

            // Assert
            Assert.True(profile.CreatedDate >= before);
            Assert.True(profile.CreatedDate <= after);
        }

        [Fact]
        public void Constructor_ShouldSetLastPlayedDate()
        {
            // Act
            var before = DateTime.Now;
            var profile = new PlayerProfile();
            var after = DateTime.Now;

            // Assert
            Assert.True(profile.LastPlayedDate >= before);
            Assert.True(profile.LastPlayedDate <= after);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void PlayerName_CanBeSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.PlayerName = "TestPlayer";

            // Assert
            Assert.Equal("TestPlayer", profile.PlayerName);
        }

        [Fact]
        public void TotalPlayTime_CanBeSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.TotalPlayTime = 120;

            // Assert
            Assert.Equal(120, profile.TotalPlayTime);
        }

        #endregion

        #region UpdateLastPlayed Tests

        [Fact]
        public void UpdateLastPlayed_ShouldUpdateLastPlayedDate()
        {
            // Arrange
            var profile = new PlayerProfile();
            var originalDate = profile.LastPlayedDate;
            System.Threading.Thread.Sleep(10); // Small delay

            // Act
            profile.UpdateLastPlayed();

            // Assert
            Assert.True(profile.LastPlayedDate >= originalDate);
        }

        #endregion

        #region AddPlayTime Tests

        [Fact]
        public void AddPlayTime_ShouldAccumulateTime()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.AddPlayTime(30);
            profile.AddPlayTime(45);

            // Assert
            Assert.Equal(75, profile.TotalPlayTime);
        }

        [Fact]
        public void AddPlayTime_ShouldHandleZero()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalPlayTime = 10;

            // Act
            profile.AddPlayTime(0);

            // Assert
            Assert.Equal(10, profile.TotalPlayTime);
        }

        #endregion

        #region RecordGameStart Tests

        [Fact]
        public void RecordGameStart_ShouldIncrementGamesPlayed()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameStart();
            profile.RecordGameStart();

            // Assert
            Assert.Equal(2, profile.GamesPlayed);
        }

        [Fact]
        public void RecordGameStart_ShouldUpdateLastPlayed()
        {
            // Arrange
            var profile = new PlayerProfile();
            var originalDate = profile.LastPlayedDate;
            System.Threading.Thread.Sleep(10);

            // Act
            profile.RecordGameStart();

            // Assert
            Assert.True(profile.LastPlayedDate >= originalDate);
        }

        #endregion

        #region RecordGameWon Tests

        [Fact]
        public void RecordGameWon_ParameterlessOverload_ShouldIncrementGamesWon()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameWon();
            profile.RecordGameWon();

            // Assert
            Assert.Equal(2, profile.GamesWon);
        }

        [Fact]
        public void RecordGameWon_ShouldIncrementGamesWon()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(1, profile.GamesWon);
        }

        [Fact]
        public void RecordGameWon_ShouldAddToOrganismsDefeated()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalOrganismsDefeated = 10;

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(60, profile.TotalOrganismsDefeated);
        }

        [Fact]
        public void RecordGameWon_ShouldUpdateFastestVictoryTime_WhenFirstWin()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(300, profile.FastestVictoryTime);
        }

        [Fact]
        public void RecordGameWon_ShouldUpdateFastestVictoryTime_WhenFaster()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.FastestVictoryTime = 400;

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(300, profile.FastestVictoryTime);
        }

        [Fact]
        public void RecordGameWon_ShouldNotUpdateFastestVictoryTime_WhenSlower()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.FastestVictoryTime = 200;

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(200, profile.FastestVictoryTime);
        }

        [Fact]
        public void RecordGameWon_ShouldUpdateMostOrganismsDefeated()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.MostOrganismsDefeatedInOneGame = 30;

            // Act
            profile.RecordGameWon(300, 50);

            // Assert
            Assert.Equal(50, profile.MostOrganismsDefeatedInOneGame);
        }

        #endregion

        #region RecordGameLost Tests

        [Fact]
        public void RecordGameLost_ParameterlessOverload_ShouldIncrementGamesLost()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameLost();
            profile.RecordGameLost();

            // Assert
            Assert.Equal(2, profile.GamesLost);
        }

        [Fact]
        public void RecordGameLost_ShouldIncrementGamesLost()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordGameLost(600, 20);

            // Assert
            Assert.Equal(1, profile.GamesLost);
        }

        [Fact]
        public void RecordGameLost_ShouldAddToOrganismsDefeated()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalOrganismsDefeated = 10;

            // Act
            profile.RecordGameLost(600, 20);

            // Assert
            Assert.Equal(30, profile.TotalOrganismsDefeated);
        }

        [Fact]
        public void RecordGameLost_ShouldUpdateLongestSurvivalTime()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.LongestSurvivalTime = 400;

            // Act
            profile.RecordGameLost(600, 20);

            // Assert
            Assert.Equal(600, profile.LongestSurvivalTime);
        }

        #endregion

        #region RecordResistanceEncounter Tests

        [Fact]
        public void RecordResistanceEncounter_ShouldIncrementEncounters()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.RecordResistanceEncounter(0.5);
            profile.RecordResistanceEncounter(0.6);

            // Assert
            Assert.Equal(2, profile.ResistanceEncounters);
        }

        [Fact]
        public void RecordResistanceEncounter_ShouldUpdateHighestLevel()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.HighestResistanceLevel = 0.3;

            // Act
            profile.RecordResistanceEncounter(0.8);

            // Assert
            Assert.Equal(0.8, profile.HighestResistanceLevel);
        }

        [Fact]
        public void RecordResistanceEncounter_ShouldNotUpdateHighestLevel_WhenLower()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.HighestResistanceLevel = 0.9;

            // Act
            profile.RecordResistanceEncounter(0.5);

            // Assert
            Assert.Equal(0.9, profile.HighestResistanceLevel);
        }

        #endregion

        #region Discovery Tests

        [Fact]
        public void DiscoverAtom_ShouldAddToSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.DiscoverAtom("H");
            profile.DiscoverAtom("C");

            // Assert
            Assert.Equal(2, profile.DiscoveredAtoms.Count);
            Assert.Contains("H", profile.DiscoveredAtoms);
            Assert.Contains("C", profile.DiscoveredAtoms);
        }

        [Fact]
        public void DiscoverAtom_ShouldNotDuplicateAtoms()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.DiscoverAtom("H");
            profile.DiscoverAtom("H");

            // Assert
            Assert.Single(profile.DiscoveredAtoms);
        }

        [Fact]
        public void DiscoverMolecule_ShouldAddToSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.DiscoverMolecule("H2O");
            profile.DiscoverMolecule("CO2");

            // Assert
            Assert.Equal(2, profile.DiscoveredMolecules.Count);
            Assert.Contains("H2O", profile.DiscoveredMolecules);
            Assert.Contains("CO2", profile.DiscoveredMolecules);
        }

        [Fact]
        public void DiscoverReaction_ShouldAddToSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.DiscoverReaction("Combustion");
            profile.DiscoverReaction("Hydrolysis");

            // Assert
            Assert.Equal(2, profile.DiscoveredReactions.Count);
        }

        #endregion

        #region UnlockAchievement Tests

        [Fact]
        public void UnlockAchievement_ShouldAddToSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.UnlockAchievement("first_molecule", 10);

            // Assert
            Assert.Contains("first_molecule", profile.UnlockedAchievements);
        }

        [Fact]
        public void UnlockAchievement_ShouldAddPoints()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.UnlockAchievement("first_molecule", 10);
            profile.UnlockAchievement("water_creation", 25);

            // Assert
            Assert.Equal(35, profile.TotalAchievementPoints);
        }

        [Fact]
        public void UnlockAchievement_ShouldNotDuplicateOrAddPointsTwice()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.UnlockAchievement("first_molecule", 10);
            profile.UnlockAchievement("first_molecule", 10);

            // Assert
            Assert.Single(profile.UnlockedAchievements);
            Assert.Equal(10, profile.TotalAchievementPoints);
        }

        #endregion

        #region GetWinRate Tests

        [Fact]
        public void GetWinRate_ShouldReturnZero_WhenNoGamesPlayed()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            var rate = profile.GetWinRate();

            // Assert
            Assert.Equal(0, rate);
        }

        [Fact]
        public void GetWinRate_ShouldCalculateCorrectly()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.GamesPlayed = 10;
            profile.GamesWon = 7;

            // Act
            var rate = profile.GetWinRate();

            // Assert
            Assert.Equal(70, rate);
        }

        [Fact]
        public void GetWinRate_ShouldReturnHundred_WhenAllWins()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.GamesPlayed = 5;
            profile.GamesWon = 5;

            // Act
            var rate = profile.GetWinRate();

            // Assert
            Assert.Equal(100, rate);
        }

        #endregion

        #region GetPlayTimeFormatted Tests

        [Fact]
        public void GetPlayTimeFormatted_ShouldFormatMinutes()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalPlayTime = 45;

            // Act
            var formatted = profile.GetPlayTimeFormatted();

            // Assert
            Assert.Equal("45 minutes", formatted);
        }

        [Fact]
        public void GetPlayTimeFormatted_ShouldFormatHours()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalPlayTime = 125; // 2 hours, 5 minutes

            // Act
            var formatted = profile.GetPlayTimeFormatted();

            // Assert
            Assert.Equal("2 hours, 5 minutes", formatted);
        }

        [Fact]
        public void GetPlayTimeFormatted_ShouldFormatDays()
        {
            // Arrange
            var profile = new PlayerProfile();
            profile.TotalPlayTime = 1500; // 1 day, 1 hour

            // Act
            var formatted = profile.GetPlayTimeFormatted();

            // Assert
            Assert.Equal("1 days, 1 hours", formatted);
        }

        #endregion

        #region Save File Tests

        [Fact]
        public void CurrentSaveFile_ShouldDefaultToEmptyString()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Assert
            Assert.Equal(string.Empty, profile.CurrentSaveFile);
        }

        [Fact]
        public void HasActiveSave_ShouldDefaultToFalse()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Assert
            Assert.False(profile.HasActiveSave);
        }

        [Fact]
        public void SaveFileProperties_CanBeSet()
        {
            // Arrange
            var profile = new PlayerProfile();

            // Act
            profile.CurrentSaveFile = "MySave1";
            profile.HasActiveSave = true;

            // Assert
            Assert.Equal("MySave1", profile.CurrentSaveFile);
            Assert.True(profile.HasActiveSave);
        }

        #endregion
    }
}
