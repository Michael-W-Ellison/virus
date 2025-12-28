using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class SaveManagerTests : IDisposable
    {
        private readonly SaveManager _saveManager;
        private readonly string _testPlayerName;
        private readonly List<string> _createdFiles;

        public SaveManagerTests()
        {
            _saveManager = new SaveManager();
            _testPlayerName = $"TestPlayer_{Guid.NewGuid():N}";
            _createdFiles = new List<string>();
        }

        public void Dispose()
        {
            // Cleanup test data
            try
            {
                _saveManager.DeleteProfile(_testPlayerName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateSaveDirectories()
        {
            // Assert
            Assert.True(Directory.Exists(_saveManager.GetSaveFolderPath()));
            Assert.True(Directory.Exists(_saveManager.GetProfilesFolderPath()));
        }

        #endregion

        #region Profile Management Tests

        [Fact]
        public void SaveProfile_ShouldCreateProfileFile()
        {
            // Arrange
            var profile = new PlayerProfile { PlayerName = _testPlayerName };

            // Act
            _saveManager.SaveProfile(profile);

            // Assert
            Assert.True(_saveManager.ProfileExists(_testPlayerName));
        }

        [Fact]
        public void LoadProfile_ShouldReturnSavedProfile()
        {
            // Arrange
            var profile = new PlayerProfile
            {
                PlayerName = _testPlayerName,
                TotalPlayTime = 100,
                ExperimentsCompleted = 5
            };
            _saveManager.SaveProfile(profile);

            // Act
            var loadedProfile = _saveManager.LoadProfile(_testPlayerName);

            // Assert
            Assert.NotNull(loadedProfile);
            Assert.Equal(_testPlayerName, loadedProfile.PlayerName);
            Assert.Equal(100, loadedProfile.TotalPlayTime);
            Assert.Equal(5, loadedProfile.ExperimentsCompleted);
        }

        [Fact]
        public void LoadProfile_ShouldReturnNull_WhenProfileDoesNotExist()
        {
            // Act
            var profile = _saveManager.LoadProfile("NonExistentPlayer_" + Guid.NewGuid());

            // Assert
            Assert.Null(profile);
        }

        [Fact]
        public void ProfileExists_ShouldReturnTrue_WhenProfileExists()
        {
            // Arrange
            var profile = new PlayerProfile { PlayerName = _testPlayerName };
            _saveManager.SaveProfile(profile);

            // Act & Assert
            Assert.True(_saveManager.ProfileExists(_testPlayerName));
        }

        [Fact]
        public void ProfileExists_ShouldReturnFalse_WhenProfileDoesNotExist()
        {
            // Act & Assert
            Assert.False(_saveManager.ProfileExists("NonExistentPlayer_" + Guid.NewGuid()));
        }

        [Fact]
        public void DeleteProfile_ShouldRemoveProfile()
        {
            // Arrange
            var profile = new PlayerProfile { PlayerName = _testPlayerName };
            _saveManager.SaveProfile(profile);
            Assert.True(_saveManager.ProfileExists(_testPlayerName));

            // Act
            _saveManager.DeleteProfile(_testPlayerName);

            // Assert
            Assert.False(_saveManager.ProfileExists(_testPlayerName));
        }

        [Fact]
        public void GetAllProfiles_ShouldReturnAllProfiles()
        {
            // Arrange
            var profile = new PlayerProfile { PlayerName = _testPlayerName };
            _saveManager.SaveProfile(profile);

            // Act
            var profiles = _saveManager.GetAllProfiles();

            // Assert
            Assert.NotNull(profiles);
            Assert.Contains(profiles, p => p.PlayerName == _testPlayerName);
        }

        #endregion

        #region Game Save Tests

        [Fact]
        public void SaveGame_ShouldCreateSaveFile()
        {
            // Arrange
            var gameSave = new GameSave
            {
                SaveName = "TestSave",
                PlayerName = _testPlayerName,
                SaveDate = DateTime.Now,
                CurrentState = GameState.AtomicChemistry
            };

            // Act
            _saveManager.SaveGame(gameSave, _testPlayerName);

            // Assert
            var saves = _saveManager.GetSavesForProfile(_testPlayerName);
            Assert.NotEmpty(saves);
        }

        [Fact]
        public void GetSavesForProfile_ShouldReturnSavesForPlayer()
        {
            // Arrange
            var gameSave = new GameSave
            {
                SaveName = "TestSave",
                PlayerName = _testPlayerName,
                SaveDate = DateTime.Now,
                CurrentState = GameState.BiochemSimulator
            };
            _saveManager.SaveGame(gameSave, _testPlayerName);

            // Act
            var saves = _saveManager.GetSavesForProfile(_testPlayerName);

            // Assert
            Assert.NotEmpty(saves);
            Assert.All(saves, s => Assert.Equal(_testPlayerName, s.PlayerName));
        }

        [Fact]
        public void GetSavesForProfile_ShouldReturnEmptyList_WhenNoSaves()
        {
            // Act
            var saves = _saveManager.GetSavesForProfile("NonExistentPlayer_" + Guid.NewGuid());

            // Assert
            Assert.Empty(saves);
        }

        [Fact]
        public void GetSavesWithPaths_ShouldReturnSavesWithFilePaths()
        {
            // Arrange
            var gameSave = new GameSave
            {
                SaveName = "TestSaveWithPath",
                PlayerName = _testPlayerName,
                SaveDate = DateTime.Now,
                CurrentState = GameState.AtomicChemistry
            };
            _saveManager.SaveGame(gameSave, _testPlayerName);

            // Act
            var savesWithPaths = _saveManager.GetSavesWithPaths(_testPlayerName);

            // Assert
            Assert.NotEmpty(savesWithPaths);
            foreach (var (save, path) in savesWithPaths)
            {
                Assert.NotNull(save);
                Assert.NotEmpty(path);
                Assert.True(File.Exists(path));
            }
        }

        [Fact]
        public void LoadGame_ShouldReturnSavedGame()
        {
            // Arrange
            var gameSave = new GameSave
            {
                SaveName = "LoadTest",
                PlayerName = _testPlayerName,
                SaveDate = DateTime.Now,
                CurrentState = GameState.DNA,
                CurrentPhase = ExperimentPhase.DNA
            };
            _saveManager.SaveGame(gameSave, _testPlayerName);

            var savesWithPaths = _saveManager.GetSavesWithPaths(_testPlayerName);
            Assert.NotEmpty(savesWithPaths);
            var (_, filePath) = savesWithPaths[0];

            // Act
            var loadedSave = _saveManager.LoadGame(filePath);

            // Assert
            Assert.NotNull(loadedSave);
            Assert.Equal("LoadTest", loadedSave.SaveName);
            Assert.Equal(GameState.DNA, loadedSave.CurrentState);
        }

        [Fact]
        public void LoadGame_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Act
            var save = _saveManager.LoadGame("/nonexistent/path/save.json");

            // Assert
            Assert.Null(save);
        }

        [Fact]
        public void DeleteAllSavesForProfile_ShouldRemoveAllSaves()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                var gameSave = new GameSave
                {
                    SaveName = $"Save{i}",
                    PlayerName = _testPlayerName,
                    SaveDate = DateTime.Now
                };
                _saveManager.SaveGame(gameSave, _testPlayerName);
            }
            Assert.NotEmpty(_saveManager.GetSavesForProfile(_testPlayerName));

            // Act
            _saveManager.DeleteAllSavesForProfile(_testPlayerName);

            // Assert
            Assert.Empty(_saveManager.GetSavesForProfile(_testPlayerName));
        }

        #endregion

        #region Achievement Tests

        [Fact]
        public void SaveAchievementProgress_ShouldPersistAchievements()
        {
            // Arrange
            var achievements = new HashSet<string> { "first_molecule", "mad_scientist", "life_creator" };

            // Act
            _saveManager.SaveAchievementProgress(_testPlayerName, achievements);
            var loaded = _saveManager.LoadAchievementProgress(_testPlayerName);

            // Assert
            Assert.Equal(achievements.Count, loaded.Count);
            Assert.Contains("first_molecule", loaded);
            Assert.Contains("mad_scientist", loaded);
            Assert.Contains("life_creator", loaded);
        }

        [Fact]
        public void LoadAchievementProgress_ShouldReturnEmptySet_WhenNoAchievements()
        {
            // Act
            var achievements = _saveManager.LoadAchievementProgress("NonExistentPlayer_" + Guid.NewGuid());

            // Assert
            Assert.NotNull(achievements);
            Assert.Empty(achievements);
        }

        #endregion

        #region Helper Method Tests

        [Fact]
        public void GetSaveFolderPath_ShouldReturnValidPath()
        {
            // Act
            var path = _saveManager.GetSaveFolderPath();

            // Assert
            Assert.NotEmpty(path);
            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void GetProfilesFolderPath_ShouldReturnValidPath()
        {
            // Act
            var path = _saveManager.GetProfilesFolderPath();

            // Assert
            Assert.NotEmpty(path);
            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void SaveProfile_ShouldHandleSpecialCharactersInName()
        {
            // Arrange
            var specialName = _testPlayerName + "_Special<>:/";
            var profile = new PlayerProfile { PlayerName = specialName };

            // Act
            _saveManager.SaveProfile(profile);

            // Assert
            Assert.True(_saveManager.ProfileExists(specialName));

            // Cleanup
            _saveManager.DeleteProfile(specialName);
        }

        #endregion
    }
}
