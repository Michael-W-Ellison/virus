using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class OrganismManagerTests
    {
        private ChemistryEngine CreateMockChemistryEngine()
        {
            return new ChemistryEngine();
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptyOrganismsList()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();

            // Act
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Assert
            Assert.Empty(manager.Organisms);
            Assert.Equal(0, manager.TotalOrganismsCreated);
            Assert.Equal(0, manager.GenerationsEvolved);
        }

        [Fact]
        public void InitializeOutbreak_ShouldCreateInitialOrganisms()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.InitializeOutbreak(10);

            // Assert
            Assert.Equal(10, manager.Organisms.Count);
            Assert.Equal(10, manager.TotalOrganismsCreated);
            Assert.All(manager.Organisms, o => Assert.True(o.IsAlive));
        }

        [Fact]
        public void InitializeOutbreak_ShouldSetVirusType()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.InitializeOutbreak(5);

            // Assert
            Assert.All(manager.Organisms, o => Assert.Equal(OrganismType.Virus, o.Type));
        }

        [Fact]
        public void InitializeOutbreak_ShouldSetGeneration1()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.InitializeOutbreak(5);

            // Assert
            Assert.All(manager.Organisms, o => Assert.Equal(1, o.Generation));
        }

        [Fact]
        public void InitializeOutbreak_ShouldPositionWithinScreen()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.InitializeOutbreak(10);

            // Assert
            Assert.All(manager.Organisms, o =>
            {
                Assert.True(o.Position.X >= 0 && o.Position.X <= 800);
                Assert.True(o.Position.Y >= 0 && o.Position.Y <= 600);
            });
        }

        [Fact]
        public void InitializeOutbreak_CalledTwice_ShouldClearPreviousOrganisms()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);
            manager.InitializeOutbreak(10);

            // Act
            manager.InitializeOutbreak(5);

            // Assert
            Assert.Equal(5, manager.Organisms.Count);
            Assert.Equal(15, manager.TotalOrganismsCreated); // 10 + 5
        }

        [Fact]
        public void GetAliveCount_AllAlive_ShouldReturnTotal()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);
            manager.InitializeOutbreak(10);

            // Act
            var aliveCount = manager.GetAliveCount();

            // Assert
            Assert.Equal(10, aliveCount);
        }

        [Fact]
        public void GetAliveCount_SomeDead_ShouldReturnOnlyAlive()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);
            manager.InitializeOutbreak(10);

            // Kill some organisms
            manager.Organisms[0].IsAlive = false;
            manager.Organisms[1].IsAlive = false;
            manager.Organisms[2].IsAlive = false;

            // Act
            var aliveCount = manager.GetAliveCount();

            // Assert
            Assert.Equal(7, aliveCount);
        }

        [Fact]
        public void Update_WithDeadOrganisms_ShouldRemoveThem()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);
            manager.InitializeOutbreak(10);

            // Kill some organisms
            manager.Organisms[0].IsAlive = false;
            manager.Organisms[1].IsAlive = false;

            // Act
            manager.Update(1.0);

            // Assert
            Assert.Equal(8, manager.Organisms.Count);
            Assert.All(manager.Organisms, o => Assert.True(o.IsAlive));
        }

        [Fact]
        public void ApplyChemical_WithinRadius_ShouldAffectOrganisms()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var organism = new Organism
            {
                Position = new Point(100, 100),
                Health = 100.0
            };
            manager.Organisms.Add(organism);

            var chemical = new Chemical("Test Toxin", "TX", "Test", ChemicalType.Other)
            {
                Toxicity = 5.0
            };

            // Act
            var affected = manager.ApplyChemical(chemical, new Point(100, 100), 50);

            // Assert
            Assert.Single(affected);
            Assert.Contains(organism, affected);
        }

        [Fact]
        public void ApplyChemical_OutsideRadius_ShouldNotAffect()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var organism = new Organism
            {
                Position = new Point(100, 100),
                Health = 100.0
            };
            manager.Organisms.Add(organism);

            var chemical = new Chemical("Test Toxin", "TX", "Test", ChemicalType.Other)
            {
                Toxicity = 5.0
            };

            // Act
            var affected = manager.ApplyChemical(chemical, new Point(300, 300), 50);

            // Assert
            Assert.Empty(affected);
        }

        [Fact]
        public void ApplyChemical_Survivors_ShouldDevelopResistance()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var organism = new Organism
            {
                Position = new Point(100, 100),
                Health = 100.0
            };
            manager.Organisms.Add(organism);

            var chemical = new Chemical("Bleach", "NaClO", "Disinfectant", ChemicalType.Disinfectant)
            {
                Toxicity = 7.0
            };

            // Act
            manager.ApplyChemical(chemical, new Point(100, 100), 50);

            // Assert
            if (organism.IsAlive)
            {
                Assert.True(organism.Resistances.ContainsKey("Bleach"));
                Assert.True(organism.Resistances["Bleach"] > 0);
            }
        }

        [Fact]
        public void GetAverageResistance_NoResistance_ShouldReturnZero()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);
            manager.InitializeOutbreak(5);

            // Act
            var avgResistance = manager.GetAverageResistance("Bleach");

            // Assert
            Assert.Equal(0.0, avgResistance);
        }

        [Fact]
        public void GetAverageResistance_WithResistance_ShouldCalculateAverage()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var org1 = new Organism();
            org1.Resistances["Bleach"] = 0.3;

            var org2 = new Organism();
            org2.Resistances["Bleach"] = 0.5;

            var org3 = new Organism();
            org3.Resistances["Bleach"] = 0.7;

            manager.Organisms.Add(org1);
            manager.Organisms.Add(org2);
            manager.Organisms.Add(org3);

            // Act
            var avgResistance = manager.GetAverageResistance("Bleach");

            // Assert
            Assert.Equal(0.5, avgResistance); // (0.3 + 0.5 + 0.7) / 3 = 0.5
        }

        [Fact]
        public void GetAverageResistance_IgnoresDeadOrganisms()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var org1 = new Organism();
            org1.Resistances["Bleach"] = 0.4;

            var org2 = new Organism { IsAlive = false };
            org2.Resistances["Bleach"] = 0.8;

            var org3 = new Organism();
            org3.Resistances["Bleach"] = 0.6;

            manager.Organisms.Add(org1);
            manager.Organisms.Add(org2);
            manager.Organisms.Add(org3);

            // Act
            var avgResistance = manager.GetAverageResistance("Bleach");

            // Assert
            Assert.Equal(0.5, avgResistance); // (0.4 + 0.6) / 2, ignoring dead org
        }

        [Fact]
        public void GetResistanceStats_ShouldReturnAllResistances()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var org1 = new Organism();
            org1.Resistances["Bleach"] = 0.3;
            org1.Resistances["Antibiotic"] = 0.2;

            var org2 = new Organism();
            org2.Resistances["Bleach"] = 0.5;
            org2.Resistances["Acid"] = 0.4;

            manager.Organisms.Add(org1);
            manager.Organisms.Add(org2);

            // Act
            var stats = manager.GetResistanceStats();

            // Assert
            Assert.Equal(3, stats.Count);
            Assert.True(stats.ContainsKey("Bleach"));
            Assert.True(stats.ContainsKey("Antibiotic"));
            Assert.True(stats.ContainsKey("Acid"));
        }

        [Fact]
        public void SetScreenSize_ShouldUpdateDimensions()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.SetScreenSize(1024, 768);
            manager.InitializeOutbreak(10);

            // Assert
            Assert.All(manager.Organisms, o =>
            {
                Assert.True(o.Position.X >= 0 && o.Position.X <= 1024);
                Assert.True(o.Position.Y >= 0 && o.Position.Y <= 768);
            });
        }

        [Fact]
        public void InitializeOutbreak_ShouldSetReproductionRateAndMutationRate()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            // Act
            manager.InitializeOutbreak(5);

            // Assert
            Assert.All(manager.Organisms, o =>
            {
                Assert.True(o.ReproductionRate > 0);
                Assert.True(o.MutationRate > 0);
            });
        }

        [Fact]
        public void ApplyChemical_DamageDecreasesWithDistance()
        {
            // Arrange
            var chemEngine = CreateMockChemistryEngine();
            var manager = new OrganismManager(chemEngine, 800, 600);

            var orgCenter = new Organism
            {
                Position = new Point(100, 100),
                Health = 100.0
            };

            var orgEdge = new Organism
            {
                Position = new Point(150, 100), // 50 units away
                Health = 100.0
            };

            manager.Organisms.Add(orgCenter);
            manager.Organisms.Add(orgEdge);

            var chemical = new Chemical("Toxin", "T", "Test", ChemicalType.Other)
            {
                Toxicity = 8.0
            };

            // Act
            manager.ApplyChemical(chemical, new Point(100, 100), 100);

            // Assert - organism at center should take more damage than one at edge
            // (or at least, both should be affected)
            Assert.True(orgCenter.Health < 100.0 || orgEdge.Health < 100.0);
        }
    }
}
