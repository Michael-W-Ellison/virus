using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System.Linq;
using System.Windows.Media;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class ChemistryEngineTests
    {
        [Fact]
        public void Constructor_ShouldInitializeChemicals()
        {
            // Act
            var engine = new ChemistryEngine();

            // Assert
            var chemicals = engine.GetAvailableChemicals();
            Assert.NotEmpty(chemicals);
        }

        [Fact]
        public void GetChemical_ValidName_ShouldReturnChemical()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var water = engine.GetChemical("Water");

            // Assert
            Assert.NotNull(water);
            Assert.Equal("Water", water.Name);
            Assert.Equal("H₂O", water.Formula);
        }

        [Fact]
        public void GetChemical_InvalidName_ShouldReturnNull()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemical = engine.GetChemical("NonExistentChemical");

            // Assert
            Assert.Null(chemical);
        }

        [Fact]
        public void GetAvailableChemicals_ShouldReturnList()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetAvailableChemicals();

            // Assert
            Assert.NotEmpty(chemicals);
            Assert.Contains(chemicals, c => c.Name == "Water");
            Assert.Contains(chemicals, c => c.Name == "Bleach");
        }

        [Fact]
        public void GetChemicalsForPhase_AminoAcids_ShouldReturnAminoAcids()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetChemicalsForPhase(ExperimentPhase.AminoAcids);

            // Assert
            Assert.NotEmpty(chemicals);
            Assert.Contains(chemicals, c => c.Name == "Glycine");
            Assert.Contains(chemicals, c => c.Name == "Alanine");
            Assert.Contains(chemicals, c => c.Name == "Cysteine");
            Assert.Contains(chemicals, c => c.Name == "Water");
        }

        [Fact]
        public void GetChemicalsForPhase_Proteins_ShouldReturnAminoAcids()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetChemicalsForPhase(ExperimentPhase.Proteins);

            // Assert
            Assert.NotEmpty(chemicals);
            Assert.Contains(chemicals, c => c.Name == "Glycine");
            Assert.Contains(chemicals, c => c.Name == "Alanine");
            Assert.Contains(chemicals, c => c.Name == "Cysteine");
        }

        [Fact]
        public void GetChemicalsForPhase_RNA_ShouldReturnRNABases()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetChemicalsForPhase(ExperimentPhase.RNA);

            // Assert
            Assert.Equal(4, chemicals.Count);
            Assert.Contains(chemicals, c => c.Name == "Adenine");
            Assert.Contains(chemicals, c => c.Name == "Uracil");
            Assert.Contains(chemicals, c => c.Name == "Cytosine");
            Assert.Contains(chemicals, c => c.Name == "Guanine");
        }

        [Fact]
        public void GetChemicalsForPhase_DNA_ShouldReturnDNABases()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetChemicalsForPhase(ExperimentPhase.DNA);

            // Assert
            Assert.Equal(4, chemicals.Count);
            Assert.Contains(chemicals, c => c.Name == "Adenine");
            Assert.Contains(chemicals, c => c.Name == "Thymine");
            Assert.Contains(chemicals, c => c.Name == "Cytosine");
            Assert.Contains(chemicals, c => c.Name == "Guanine");
            // Should NOT contain Uracil (RNA only)
            Assert.DoesNotContain(chemicals, c => c.Name == "Uracil");
        }

        [Fact]
        public void GetChemicalsForPhase_Lipids_ShouldReturnLipids()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var chemicals = engine.GetChemicalsForPhase(ExperimentPhase.Lipids);

            // Assert
            Assert.NotEmpty(chemicals);
            Assert.Contains(chemicals, c => c.Name == "Phospholipid");
            Assert.Contains(chemicals, c => c.Name == "Cholesterol");
        }

        [Fact]
        public void GetDisinfectants_ShouldReturnOnlyDisinfectants()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var disinfectants = engine.GetDisinfectants();

            // Assert
            Assert.NotEmpty(disinfectants);
            Assert.Contains(disinfectants, c => c.Name == "Bleach");
            Assert.Contains(disinfectants, c => c.Name == "Ethanol");
            Assert.Contains(disinfectants, c => c.Name == "Penicillin");
            // Should not contain amino acids or water
            Assert.DoesNotContain(disinfectants, c => c.Name == "Glycine");
            Assert.DoesNotContain(disinfectants, c => c.Name == "Water");
        }

        [Fact]
        public void TryReact_ValidNeutralization_ShouldReturnReaction()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var acid = engine.GetChemical("HydrochloricAcid")!;
            var base_ = engine.GetChemical("SodiumHydroxide")!;

            // Act
            var reaction = engine.TryReact(new List<Chemical> { acid, base_ });

            // Assert
            Assert.NotNull(reaction);
            Assert.Contains(reaction.Products, p => p.Name == "Sodium Chloride");
            Assert.Contains(reaction.Products, p => p.Name == "Water");
            Assert.True(reaction.IsExothermic);
        }

        [Fact]
        public void TryReact_InvalidCombination_ShouldReturnNull()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var water = engine.GetChemical("Water")!;
            var glucose = engine.GetChemical("Glucose")!;

            // Act
            var reaction = engine.TryReact(new List<Chemical> { water, glucose });

            // Assert
            Assert.Null(reaction);
        }

        [Fact]
        public void TryReact_ProteinSynthesis_ShouldReturnReaction()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var glycine = engine.GetChemical("Glycine")!;
            var alanine = engine.GetChemical("Alanine")!;
            var cysteine = engine.GetChemical("Cysteine")!;

            // Act
            var reaction = engine.TryReact(new List<Chemical> { glycine, alanine, cysteine });

            // Assert
            Assert.NotNull(reaction);
            Assert.Contains(reaction.Products, p => p.Name == "Protein");
        }

        [Fact]
        public void CalculateDamageToOrganism_BasicToxicity_ShouldCalculate()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var chemical = new Chemical("Test", "T", "Test", ChemicalType.Other)
            {
                Toxicity = 5.0
            };
            var organism = new Organism();

            // Act
            var damage = engine.CalculateDamageToOrganism(chemical, organism);

            // Assert
            Assert.Equal(50.0, damage); // 5.0 * 10
        }

        [Fact]
        public void CalculateDamageToOrganism_CausticChemical_ShouldIncreaseDamage()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var caustic = new Chemical("Caustic", "C", "Test", ChemicalType.Acid)
            {
                Toxicity = 5.0,
                IsCaustic = true
            };
            var organism = new Organism();

            // Act
            var damage = engine.CalculateDamageToOrganism(caustic, organism);

            // Assert
            Assert.Equal(75.0, damage); // 5.0 * 10 * 1.5
        }

        [Fact]
        public void CalculateDamageToOrganism_OxidizerChemical_ShouldIncreaseDamage()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var oxidizer = new Chemical("Oxidizer", "O", "Test", ChemicalType.Oxidizer)
            {
                Toxicity = 5.0,
                IsOxidizer = true
            };
            var organism = new Organism();

            // Act
            var damage = engine.CalculateDamageToOrganism(oxidizer, organism);

            // Assert
            Assert.Equal(65.0, damage); // 5.0 * 10 * 1.3
        }

        [Fact]
        public void CalculateDamageToOrganism_WithResistance_ShouldReduceDamage()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var chemical = new Chemical("Toxin", "T", "Test", ChemicalType.Other)
            {
                Toxicity = 5.0
            };
            var organism = new Organism();
            organism.Resistances["Toxin"] = 0.5; // 50% resistance

            // Act
            var damage = engine.CalculateDamageToOrganism(chemical, organism);

            // Assert
            Assert.Equal(25.0, damage); // 50 * (1 - 0.5)
        }

        [Fact]
        public void CalculateDamageToOrganism_FullResistance_ShouldNoDamage()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var chemical = new Chemical("Toxin", "T", "Test", ChemicalType.Other)
            {
                Toxicity = 8.0
            };
            var organism = new Organism();
            organism.Resistances["Toxin"] = 1.0; // 100% resistance

            // Act
            var damage = engine.CalculateDamageToOrganism(chemical, organism);

            // Assert
            Assert.Equal(0.0, damage);
        }

        [Fact]
        public void WillReactDangerously_AcidAndBleach_ShouldReturnTrue()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var acid = engine.GetChemical("HydrochloricAcid")!;
            var bleach = engine.GetChemical("Bleach")!;

            // Act
            var isDangerous = engine.WillReactDangerously(acid, bleach);

            // Assert
            Assert.True(isDangerous);
        }

        [Fact]
        public void WillReactDangerously_BleachAndAcid_ShouldReturnTrue()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var acid = engine.GetChemical("HydrochloricAcid")!;
            var bleach = engine.GetChemical("Bleach")!;

            // Act - Order reversed
            var isDangerous = engine.WillReactDangerously(bleach, acid);

            // Assert
            Assert.True(isDangerous);
        }

        [Fact]
        public void WillReactDangerously_StrongAcidAndBase_ShouldReturnTrue()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var acid = engine.GetChemical("SulfuricAcid")!;
            var base_ = engine.GetChemical("SodiumHydroxide")!;

            // Act
            var isDangerous = engine.WillReactDangerously(acid, base_);

            // Assert
            Assert.True(isDangerous);
        }

        [Fact]
        public void WillReactDangerously_SafeCombination_ShouldReturnFalse()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var water = engine.GetChemical("Water")!;
            var glucose = engine.GetChemical("Glucose")!;

            // Act
            var isDangerous = engine.WillReactDangerously(water, glucose);

            // Assert
            Assert.False(isDangerous);
        }

        [Fact]
        public void GetReactionColor_ValidReaction_ShouldReturnColor()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var acid = engine.GetChemical("HydrochloricAcid")!;
            var base_ = engine.GetChemical("SodiumHydroxide")!;

            // Act
            var color = engine.GetReactionColor(acid, base_);

            // Assert
            Assert.NotNull(color);
        }

        [Fact]
        public void GetReactionColor_NoReaction_ShouldReturnNull()
        {
            // Arrange
            var engine = new ChemistryEngine();
            var water = engine.GetChemical("Water")!;
            var glucose = engine.GetChemical("Glucose")!;

            // Act
            var color = engine.GetReactionColor(water, glucose);

            // Assert
            Assert.Null(color);
        }

        [Fact]
        public void GetChemical_Bleach_ShouldHaveHighToxicity()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var bleach = engine.GetChemical("Bleach")!;

            // Assert
            Assert.NotNull(bleach);
            Assert.True(bleach.Toxicity >= 8.0);
            Assert.True(bleach.IsCaustic);
            Assert.True(bleach.IsOxidizer);
        }

        [Fact]
        public void GetChemical_Water_ShouldBeNeutral()
        {
            // Arrange
            var engine = new ChemistryEngine();

            // Act
            var water = engine.GetChemical("Water")!;

            // Assert
            Assert.NotNull(water);
            Assert.Equal(7.0, water.pH);
        }
    }
}
