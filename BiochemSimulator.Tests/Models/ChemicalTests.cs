using System.Windows.Media;
using BiochemSimulator.Models;
using Xunit;

namespace BiochemSimulator.Tests.Models
{
    public class ChemicalTests
    {
        [Fact]
        public void Constructor_ShouldInitializeBasicProperties()
        {
            // Act
            var acid = new Chemical("Hydrochloric Acid", "HCl", "Strong acid", ChemicalType.Acid, ph: 1.0);

            // Assert
            Assert.Equal("Hydrochloric Acid", acid.Name);
            Assert.Equal("HCl", acid.Formula);
            Assert.Equal("Strong acid", acid.Description);
            Assert.Equal(ChemicalType.Acid, acid.Type);
            Assert.Equal(1.0, acid.pH);
        }

        [Fact]
        public void Constructor_WithColor_ShouldSetColor()
        {
            // Act
            var chemical = new Chemical("Test", "T", "Test chemical", ChemicalType.Other, color: Colors.Blue);

            // Assert
            Assert.Equal(Colors.Blue, chemical.Color);
        }

        [Fact]
        public void Constructor_WithoutColor_ShouldUseTransparent()
        {
            // Act
            var chemical = new Chemical("Test", "T", "Test chemical", ChemicalType.Other);

            // Assert
            Assert.Equal(Colors.Transparent, chemical.Color);
        }

        [Fact]
        public void Constructor_DefaultPH_ShouldBeNeutral()
        {
            // Act
            var chemical = new Chemical("Water", "H2O", "Neutral", ChemicalType.Other);

            // Assert
            Assert.Equal(7.0, chemical.pH);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultConcentration()
        {
            // Act
            var chemical = new Chemical("Test", "T", "Test", ChemicalType.Other);

            // Assert
            Assert.Equal(1.0, chemical.Concentration);
        }

        [Fact]
        public void Chemical_AcidProperties_ShouldBeSettable()
        {
            // Arrange & Act
            var acid = new Chemical("Sulfuric Acid", "H2SO4", "Strong acid", ChemicalType.Acid, ph: 0.5)
            {
                IsCaustic = true,
                Toxicity = 8.0,
                Reactivity = 9.0
            };

            // Assert
            Assert.True(acid.IsCaustic);
            Assert.Equal(8.0, acid.Toxicity);
            Assert.Equal(9.0, acid.Reactivity);
        }

        [Fact]
        public void Chemical_BaseProperties_ShouldBeSettable()
        {
            // Arrange & Act
            var base_ = new Chemical("Sodium Hydroxide", "NaOH", "Strong base", ChemicalType.Base, ph: 14.0)
            {
                IsCaustic = true,
                Toxicity = 7.0,
                Reactivity = 8.0
            };

            // Assert
            Assert.Equal(ChemicalType.Base, base_.Type);
            Assert.True(base_.IsCaustic);
            Assert.Equal(14.0, base_.pH);
        }

        [Fact]
        public void Chemical_OxidizerAndReducer_ShouldBeIndependent()
        {
            // Arrange & Act
            var chemical = new Chemical("Test", "T", "Test", ChemicalType.Other)
            {
                IsOxidizer = true,
                IsReducer = false
            };

            // Assert
            Assert.True(chemical.IsOxidizer);
            Assert.False(chemical.IsReducer);
        }
    }

    public class ChemicalReactionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmptyLists()
        {
            // Act
            var reaction = new ChemicalReaction();

            // Assert
            Assert.NotNull(reaction.Reactants);
            Assert.Empty(reaction.Reactants);
            Assert.NotNull(reaction.Products);
            Assert.Empty(reaction.Products);
            Assert.Equal(string.Empty, reaction.Description);
            Assert.Equal(string.Empty, reaction.VisualDescription);
        }

        [Fact]
        public void ChemicalReaction_ShouldAllowReactantsAndProducts()
        {
            // Arrange
            var reaction = new ChemicalReaction();
            var acid = new Chemical("HCl", "HCl", "Acid", ChemicalType.Acid, ph: 1.0);
            var base_ = new Chemical("NaOH", "NaOH", "Base", ChemicalType.Base, ph: 14.0);
            var salt = new Chemical("NaCl", "NaCl", "Salt", ChemicalType.Salt);
            var water = new Chemical("Water", "H2O", "Water", ChemicalType.Other);

            // Act
            reaction.Reactants.Add(acid);
            reaction.Reactants.Add(base_);
            reaction.Products.Add(salt);
            reaction.Products.Add(water);
            reaction.Description = "Neutralization";
            reaction.IsExothermic = true;

            // Assert
            Assert.Equal(2, reaction.Reactants.Count);
            Assert.Equal(2, reaction.Products.Count);
            Assert.Equal("Neutralization", reaction.Description);
            Assert.True(reaction.IsExothermic);
        }

        [Fact]
        public void ChemicalReaction_EnergyChange_ShouldBeSettable()
        {
            // Arrange & Act
            var reaction = new ChemicalReaction
            {
                EnergyChange = -57.3, // Exothermic
                IsExothermic = true
            };

            // Assert
            Assert.Equal(-57.3, reaction.EnergyChange);
            Assert.True(reaction.IsExothermic);
        }

        [Fact]
        public void ChemicalReaction_VisualEffect_ShouldBeSettable()
        {
            // Arrange & Act
            var reaction = new ChemicalReaction
            {
                VisualEffect = Colors.Red,
                VisualDescription = "Bright red glow"
            };

            // Assert
            Assert.Equal(Colors.Red, reaction.VisualEffect);
            Assert.Equal("Bright red glow", reaction.VisualDescription);
        }
    }
}
