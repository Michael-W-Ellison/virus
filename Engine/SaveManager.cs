using BiochemSimulator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BiochemSimulator.Engine
{
    public class SaveManager
    {
        private readonly string _saveFolderPath;
        private readonly string _profilesFolderPath;
        private readonly string _achievementsFolderPath;

        public SaveManager()
        {
            // Create save directories in user's documents
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _saveFolderPath = Path.Combine(documentsPath, "BiochemSimulator", "Saves");
            _profilesFolderPath = Path.Combine(documentsPath, "BiochemSimulator", "Profiles");
            _achievementsFolderPath = Path.Combine(documentsPath, "BiochemSimulator", "Achievements");

            Directory.CreateDirectory(_saveFolderPath);
            Directory.CreateDirectory(_profilesFolderPath);
            Directory.CreateDirectory(_achievementsFolderPath);
        }

        #region Profile Management

        public void SaveProfile(PlayerProfile profile)
        {
            try
            {
                string fileName = GetSafeFileName(profile.PlayerName) + ".json";
                string filePath = Path.Combine(_profilesFolderPath, fileName);

                string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save profile: {ex.Message}");
            }
        }

        public PlayerProfile? LoadProfile(string playerName)
        {
            try
            {
                string fileName = GetSafeFileName(playerName) + ".json";
                string filePath = Path.Combine(_profilesFolderPath, fileName);

                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<PlayerProfile>(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load profile: {ex.Message}");
            }
        }

        public List<PlayerProfile> GetAllProfiles()
        {
            var profiles = new List<PlayerProfile>();

            try
            {
                var files = Directory.GetFiles(_profilesFolderPath, "*.json");

                foreach (var file in files)
                {
                    string json = File.ReadAllText(file);
                    var profile = JsonConvert.DeserializeObject<PlayerProfile>(json);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }

                // Sort by last played date
                return profiles.OrderByDescending(p => p.LastPlayedDate).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load profiles: {ex.Message}");
            }
        }

        public bool ProfileExists(string playerName)
        {
            string fileName = GetSafeFileName(playerName) + ".json";
            string filePath = Path.Combine(_profilesFolderPath, fileName);
            return File.Exists(filePath);
        }

        public void DeleteProfile(string playerName)
        {
            try
            {
                string fileName = GetSafeFileName(playerName) + ".json";
                string filePath = Path.Combine(_profilesFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // Also delete associated saves
                    DeleteAllSavesForProfile(playerName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete profile: {ex.Message}");
            }
        }

        #endregion

        #region Game Save Management

        public void SaveGame(GameSave gameSave, string playerName)
        {
            try
            {
                string safePlayerName = GetSafeFileName(playerName);
                string fileName = $"{safePlayerName}_{gameSave.SaveName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(_saveFolderPath, fileName);

                string json = JsonConvert.SerializeObject(gameSave, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save game: {ex.Message}");
            }
        }

        public GameSave? LoadGame(string saveFilePath)
        {
            try
            {
                if (!File.Exists(saveFilePath))
                    return null;

                string json = File.ReadAllText(saveFilePath);
                return JsonConvert.DeserializeObject<GameSave>(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load game: {ex.Message}");
            }
        }

        public List<GameSave> GetSavesForProfile(string playerName)
        {
            var saves = new List<GameSave>();

            try
            {
                string safePlayerName = GetSafeFileName(playerName);
                var files = Directory.GetFiles(_saveFolderPath, $"{safePlayerName}_*.json");

                foreach (var file in files)
                {
                    string json = File.ReadAllText(file);
                    var save = JsonConvert.DeserializeObject<GameSave>(json);
                    if (save != null)
                    {
                        saves.Add(save);
                    }
                }

                return saves.OrderByDescending(s => s.SaveDate).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load saves: {ex.Message}");
            }
        }

        public void DeleteSave(string saveFilePath)
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete save: {ex.Message}");
            }
        }

        public void DeleteAllSavesForProfile(string playerName)
        {
            try
            {
                string safePlayerName = GetSafeFileName(playerName);
                var files = Directory.GetFiles(_saveFolderPath, $"{safePlayerName}_*.json");

                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Silent fail for cleanup
            }
        }

        #endregion

        #region Achievement Management

        public void SaveAchievementProgress(string playerName, HashSet<string> unlockedAchievements)
        {
            try
            {
                string fileName = GetSafeFileName(playerName) + "_achievements.json";
                string filePath = Path.Combine(_achievementsFolderPath, fileName);

                var achievementData = new Dictionary<string, DateTime>();
                foreach (var achId in unlockedAchievements)
                {
                    achievementData[achId] = DateTime.Now;
                }

                string json = JsonConvert.SerializeObject(achievementData, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save achievements: {ex.Message}");
            }
        }

        public HashSet<string> LoadAchievementProgress(string playerName)
        {
            try
            {
                string fileName = GetSafeFileName(playerName) + "_achievements.json";
                string filePath = Path.Combine(_achievementsFolderPath, fileName);

                if (!File.Exists(filePath))
                    return new HashSet<string>();

                string json = File.ReadAllText(filePath);
                var achievementData = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(json);

                return achievementData != null ? new HashSet<string>(achievementData.Keys) : new HashSet<string>();
            }
            catch
            {
                return new HashSet<string>();
            }
        }

        #endregion

        #region Helper Methods

        private string GetSafeFileName(string fileName)
        {
            // Remove invalid characters from file name
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        public string GetSaveFolderPath() => _saveFolderPath;
        public string GetProfilesFolderPath() => _profilesFolderPath;

        #endregion
    }
}
