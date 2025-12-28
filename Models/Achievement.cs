using System;
using System.Collections.Generic;

namespace BiochemSimulator.Models
{
    public class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🏆"; // Emoji icon
        public int Points { get; set; }
        public AchievementCategory Category { get; set; }
        public bool IsSecret { get; set; } // Hidden until unlocked
        public DateTime UnlockedDate { get; set; }

        public Achievement(string id, string name, string description, int points,
            AchievementCategory category, string icon = "🏆", bool isSecret = false)
        {
            Id = id;
            Name = name;
            Description = description;
            Points = points;
            Category = category;
            Icon = icon;
            IsSecret = isSecret;
        }
    }

    public enum AchievementCategory
    {
        Chemistry,
        Biology,
        Combat,
        Discovery,
        Mastery,
        Special
    }

    public static class AchievementDefinitions
    {
        public static List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>
            {
                // Chemistry Achievements
                new Achievement("first_molecule", "First Steps",
                    "Create your first molecule", 10, AchievementCategory.Chemistry, "⚗️"),
                new Achievement("water_creation", "H2O Master",
                    "Successfully create water from hydrogen and oxygen", 15, AchievementCategory.Chemistry, "💧"),
                new Achievement("explosive_reaction", "Mad Scientist",
                    "Cause your first explosive reaction", 20, AchievementCategory.Chemistry, "💥"),
                new Achievement("radioactive_user", "Nuclear Physicist",
                    "Use a radioactive element", 25, AchievementCategory.Chemistry, "☢️"),
                new Achievement("all_basic_atoms", "Periodic Master",
                    "Use all basic elements (H, C, N, O)", 30, AchievementCategory.Chemistry, "⚛️"),
                new Achievement("uranium_user", "Manhattan Project",
                    "Handle Uranium-235", 40, AchievementCategory.Chemistry, "☢️", true),
                new Achievement("plutonium_user", "Weapons Grade",
                    "Handle Plutonium-239", 50, AchievementCategory.Chemistry, "☣️", true),
                new Achievement("chemistry_100", "Chemistry Doctorate",
                    "Create 100 different molecules", 100, AchievementCategory.Chemistry, "🎓"),

                // Biology Achievements
                new Achievement("first_life", "Creator of Life",
                    "Create your first primitive life form", 50, AchievementCategory.Biology, "🧬"),
                new Achievement("rna_creation", "RNA World",
                    "Successfully synthesize RNA", 30, AchievementCategory.Biology, "🧬"),
                new Achievement("dna_creation", "Double Helix",
                    "Successfully synthesize DNA", 35, AchievementCategory.Biology, "🧬"),
                new Achievement("protein_synthesis", "Protein Factory",
                    "Create 10 different proteins", 25, AchievementCategory.Biology, "🧫"),
                new Achievement("cell_membrane", "Membrane Master",
                    "Form a complete cell membrane", 40, AchievementCategory.Biology, "🦠"),

                // Combat Achievements
                new Achievement("first_kill", "Exterminator",
                    "Defeat your first organism", 10, AchievementCategory.Combat, "🔫"),
                new Achievement("outbreak_survivor", "Outbreak Survivor",
                    "Survive the biohazard alarm", 50, AchievementCategory.Combat, "🚨"),
                new Achievement("defeat_100", "Plague Doctor",
                    "Defeat 100 organisms", 75, AchievementCategory.Combat, "⚕️"),
                new Achievement("defeat_500", "Viral Apocalypse",
                    "Defeat 500 organisms", 150, AchievementCategory.Combat, "☠️"),
                new Achievement("perfect_game", "Flawless Victory",
                    "Win without losing any desktop icons", 100, AchievementCategory.Combat, "👑", true),
                new Achievement("resistance_master", "Evolution Expert",
                    "Defeat organisms that developed resistance 3+ times", 60, AchievementCategory.Combat, "🧪"),
                new Achievement("first_resistance", "First Contact",
                    "Encounter organisms that developed chemical resistance", 20, AchievementCategory.Combat, "🦠"),
                new Achievement("resistance_expert", "Resistance Fighter",
                    "Encounter 10 instances of chemical resistance", 40, AchievementCategory.Combat, "💪"),
                new Achievement("super_resistance", "Unstoppable Force",
                    "Face organisms with 80%+ chemical resistance", 75, AchievementCategory.Combat, "🛡️", true),

                // Discovery Achievements
                new Achievement("discover_5_atoms", "Beginner Chemist",
                    "Discover 5 different atoms", 15, AchievementCategory.Discovery, "🔬"),
                new Achievement("discover_all_atoms", "Element Collector",
                    "Discover all available atoms", 100, AchievementCategory.Discovery, "⚛️"),
                new Achievement("discover_10_reactions", "Reaction Specialist",
                    "Discover 10 different chemical reactions", 40, AchievementCategory.Discovery, "🧪"),
                new Achievement("discover_all_hazards", "Hazard Expert",
                    "Experience all hazard levels", 50, AchievementCategory.Discovery, "⚠️"),

                // Mastery Achievements
                new Achievement("speed_run", "Speedrunner",
                    "Win a game in under 5 minutes", 75, AchievementCategory.Mastery, "⚡"),
                new Achievement("marathon_survivor", "Marathon Survivor",
                    "Survive for 30 minutes in outbreak mode", 80, AchievementCategory.Mastery, "⏱️"),
                new Achievement("win_10_games", "Veteran Scientist",
                    "Win 10 games", 50, AchievementCategory.Mastery, "🎖️"),
                new Achievement("win_50_games", "Master Scientist",
                    "Win 50 games", 200, AchievementCategory.Mastery, "👨‍🔬"),
                new Achievement("high_generation", "Evolution Observer",
                    "Survive organisms reaching generation 10+", 90, AchievementCategory.Mastery, "🧬", true),

                // Special Achievements
                new Achievement("trash_disposal", "Cleanup Crew",
                    "Dispose organisms in the trash can", 20, AchievementCategory.Special, "🗑️"),
                new Achievement("play_1hour", "Dedicated Researcher",
                    "Play for 1 hour total", 25, AchievementCategory.Special, "⏰"),
                new Achievement("play_10hours", "Lab Rat",
                    "Play for 10 hours total", 75, AchievementCategory.Special, "🐀"),
                new Achievement("three_strikes", "Trial and Error",
                    "Lose 3 games in a row", 15, AchievementCategory.Special, "❌", true),
                new Achievement("comeback_kid", "Phoenix",
                    "Win after losing 3 games in a row", 50, AchievementCategory.Special, "🔥", true),
                new Achievement("perfectionist", "100% Completion",
                    "Unlock all non-secret achievements", 500, AchievementCategory.Special, "💎", true)
            };
        }
    }
}
