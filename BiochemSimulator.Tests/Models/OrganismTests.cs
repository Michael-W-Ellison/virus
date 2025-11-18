using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using BiochemSimulator.Models;
using Xunit;

namespace BiochemSimulator.Tests.Models
{
    public class OrganismTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var organism = new Organism();

            // Assert
            Assert.NotEqual(Guid.Empty, organism.Id);
            Assert.Equal(100.0, organism.Health);
            Assert.Equal(1.0, organism.ReproductionRate);
            Assert.Equal(0.1, organism.MutationRate);
            Assert.True(organism.IsAlive);
            Assert.Equal(10.0, organism.Size);
            Assert.Equal(Colors.Green, organism.Color);
            Assert.Equal(OrganismType.SingleCell, organism.Type);
            Assert.NotNull(organism.Resistances);
            Assert.Empty(organism.Resistances);
        }

        [Fact]
        public void Clone_WithoutMutation_ShouldCreateExactCopy()
        {
            // Arrange
            var original = new Organism
            {
                Position = new Point(10, 20),
                Size = 15.0,
                Color = Colors.Red,
                Generation = 5,
                Health = 80.0,
                ReproductionRate = 1.5,
                MutationRate = 0.2,
                Type = OrganismType.Bacteria
            };
            original.Resistances["Bleach"] = 0.5;

            // Act
            var clone = original.Clone(mutate: false);

            // Assert
            Assert.NotEqual(original.Id, clone.Id); // New ID
            Assert.Equal(original.Position, clone.Position);
            Assert.Equal(original.Size, clone.Size);
            Assert.Equal(original.Color, clone.Color);
            Assert.Equal(original.Generation + 1, clone.Generation); // Incremented
            Assert.Equal(original.Health, clone.Health);
            Assert.Equal(original.ReproductionRate, clone.ReproductionRate);
            Assert.Equal(original.MutationRate, clone.MutationRate);
            Assert.Equal(original.Type, clone.Type);
            Assert.Equal(original.Resistances["Bleach"], clone.Resistances["Bleach"]);
        }

        [Fact]
        public void Clone_WithMutation_ShouldIncrementGeneration()
        {
            // Arrange
            var original = new Organism { Generation = 3 };

            // Act
            var clone = original.Clone(mutate: true);

            // Assert
            Assert.Equal(4, clone.Generation);
        }

        [Fact]
        public void TakeDamage_WithoutResistance_ShouldReduceHealthByFullAmount()
        {
            // Arrange
            var organism = new Organism { Health = 100.0 };

            // Act
            organism.TakeDamage(30.0, "Bleach");

            // Assert
            Assert.Equal(70.0, organism.Health);
            Assert.True(organism.IsAlive);
        }

        [Fact]
        public void TakeDamage_WithResistance_ShouldReduceDamage()
        {
            // Arrange
            var organism = new Organism { Health = 100.0 };
            organism.Resistances["Bleach"] = 0.5; // 50% resistance

            // Act
            organism.TakeDamage(40.0, "Bleach");

            // Assert
            // 40 damage with 50% resistance = 20 actual damage
            Assert.Equal(80.0, organism.Health);
            Assert.True(organism.IsAlive);
        }

        [Fact]
        public void TakeDamage_WithFullResistance_ShouldTakeNoDamage()
        {
            // Arrange
            var organism = new Organism { Health = 100.0 };
            organism.Resistances["Antibiotic"] = 1.0; // 100% resistance

            // Act
            organism.TakeDamage(50.0, "Antibiotic");

            // Assert
            Assert.Equal(100.0, organism.Health);
            Assert.True(organism.IsAlive);
        }

        [Fact]
        public void TakeDamage_FatalDamage_ShouldKillOrganism()
        {
            // Arrange
            var organism = new Organism { Health = 50.0 };

            // Act
            organism.TakeDamage(60.0, "Poison");

            // Assert
            Assert.True(organism.Health <= 0);
            Assert.False(organism.IsAlive);
        }

        [Fact]
        public void TakeDamage_ExactlyLethalDamage_ShouldKillOrganism()
        {
            // Arrange
            var organism = new Organism { Health = 50.0 };

            // Act
            organism.TakeDamage(50.0, "Toxin");

            // Assert
            Assert.Equal(0.0, organism.Health);
            Assert.False(organism.IsAlive);
        }

        [Fact]
        public void DevelopResistance_NewChemical_ShouldAddResistance()
        {
            // Arrange
            var organism = new Organism();

            // Act
            organism.DevelopResistance("Bleach", 0.2);

            // Assert
            Assert.True(organism.Resistances.ContainsKey("Bleach"));
            Assert.Equal(0.2, organism.Resistances["Bleach"]);
        }

        [Fact]
        public void DevelopResistance_ExistingChemical_ShouldIncreaseResistance()
        {
            // Arrange
            var organism = new Organism();
            organism.Resistances["Bleach"] = 0.3;

            // Act
            organism.DevelopResistance("Bleach", 0.4);

            // Assert
            Assert.Equal(0.7, organism.Resistances["Bleach"]);
        }

        [Fact]
        public void DevelopResistance_ShouldCapAt95Percent()
        {
            // Arrange
            var organism = new Organism();
            organism.Resistances["Antibiotic"] = 0.9;

            // Act
            organism.DevelopResistance("Antibiotic", 0.2);

            // Assert
            Assert.Equal(0.95, organism.Resistances["Antibiotic"]);
        }

        [Fact]
        public void DevelopResistance_MultipleIncrements_ShouldNotExceedCap()
        {
            // Arrange
            var organism = new Organism();

            // Act
            organism.DevelopResistance("Toxin", 0.5);
            organism.DevelopResistance("Toxin", 0.5);
            organism.DevelopResistance("Toxin", 0.5);

            // Assert
            Assert.Equal(0.95, organism.Resistances["Toxin"]);
        }

        [Fact]
        public void TakeDamage_CombinedWithResistanceDevelopment_ShouldWorkCorrectly()
        {
            // Arrange
            var organism = new Organism { Health = 100.0 };
            organism.DevelopResistance("Bleach", 0.4);

            // Act
            organism.TakeDamage(50.0, "Bleach"); // 50 * (1 - 0.4) = 30 damage

            // Assert
            Assert.Equal(70.0, organism.Health);
            Assert.True(organism.IsAlive);
        }

        [Fact]
        public void MultipleOrganisms_ShouldHaveUniqueIds()
        {
            // Act
            var organism1 = new Organism();
            var organism2 = new Organism();
            var organism3 = new Organism();

            // Assert
            Assert.NotEqual(organism1.Id, organism2.Id);
            Assert.NotEqual(organism1.Id, organism3.Id);
            Assert.NotEqual(organism2.Id, organism3.Id);
        }

        [Fact]
        public void Clone_ShouldCopyAllResistances()
        {
            // Arrange
            var original = new Organism();
            original.Resistances["Bleach"] = 0.3;
            original.Resistances["Antibiotic"] = 0.5;
            original.Resistances["Acid"] = 0.7;

            // Act
            var clone = original.Clone(mutate: false);

            // Assert
            Assert.Equal(3, clone.Resistances.Count);
            Assert.Equal(0.3, clone.Resistances["Bleach"]);
            Assert.Equal(0.5, clone.Resistances["Antibiotic"]);
            Assert.Equal(0.7, clone.Resistances["Acid"]);
        }
    }
}
