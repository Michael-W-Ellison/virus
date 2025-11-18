using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using BiochemSimulator.Models;
using Xunit;

namespace BiochemSimulator.Tests.Models
{
    public class AtomTests
    {
        [Fact]
        public void Constructor_ShouldInitializeBasicProperties()
        {
            // Act
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008);

            // Assert
            Assert.Equal("H", hydrogen.Symbol);
            Assert.Equal("Hydrogen", hydrogen.Name);
            Assert.Equal(1, hydrogen.AtomicNumber);
            Assert.Equal(1.008, hydrogen.AtomicMass);
            Assert.Equal(1, hydrogen.Protons);
            Assert.Equal(1, hydrogen.Electrons);
            Assert.Equal(0, hydrogen.Neutrons); // 1.008 - 1 = ~0
        }

        [Fact]
        public void Constructor_ShouldCalculateNeutronsCorrectly()
        {
            // Act
            var carbon = new Atom("C", "Carbon", 6, 12.011);

            // Assert
            Assert.Equal(6, carbon.Protons);
            Assert.Equal(6, carbon.Electrons);
            Assert.Equal(6, carbon.Neutrons); // 12.011 - 6 = ~6
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptyBondsList()
        {
            // Act
            var atom = new Atom("O", "Oxygen", 8, 16.0);

            // Assert
            Assert.NotNull(atom.Bonds);
            Assert.Empty(atom.Bonds);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultState()
        {
            // Act
            var atom = new Atom("N", "Nitrogen", 7, 14.007);

            // Assert
            Assert.Equal(AtomState.Stable, atom.State);
            Assert.Equal(RadioactiveDecayType.None, atom.DecayType);
            Assert.Equal(MaterialPhase.Solid, atom.PhaseAtRoomTemp);
        }

        [Fact]
        public void CanBond_WithAvailableBonds_ShouldReturnTrue()
        {
            // Arrange
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };

            // Act & Assert
            Assert.True(carbon.CanBond());
        }

        [Fact]
        public void CanBond_WithMaxBondsReached_ShouldReturnFalse()
        {
            // Arrange
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };
            var hydrogen1 = new Atom("H", "Hydrogen", 1, 1.008);
            var hydrogen2 = new Atom("H", "Hydrogen", 1, 1.008);
            var hydrogen3 = new Atom("H", "Hydrogen", 1, 1.008);
            var hydrogen4 = new Atom("H", "Hydrogen", 1, 1.008);

            carbon.Bonds.Add(new Bond(carbon, hydrogen1, BondType.Single, 413, 1.09));
            carbon.Bonds.Add(new Bond(carbon, hydrogen2, BondType.Single, 413, 1.09));
            carbon.Bonds.Add(new Bond(carbon, hydrogen3, BondType.Single, 413, 1.09));
            carbon.Bonds.Add(new Bond(carbon, hydrogen4, BondType.Single, 413, 1.09));

            // Act & Assert
            Assert.False(carbon.CanBond());
        }

        [Fact]
        public void AvailableBondingSites_WithNoBonds_ShouldReturnMaxBonds()
        {
            // Arrange
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };

            // Act
            var available = carbon.AvailableBondingSites();

            // Assert
            Assert.Equal(4, available);
        }

        [Fact]
        public void AvailableBondingSites_WithSomeBonds_ShouldReturnRemainingSlots()
        {
            // Arrange
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };
            var oxygen = new Atom("O", "Oxygen", 8, 16.0);
            carbon.Bonds.Add(new Bond(carbon, oxygen, BondType.Double, 799, 1.20));
            carbon.Bonds.Add(new Bond(carbon, oxygen, BondType.Double, 799, 1.20)); // Takes 2 slots

            // Act
            var available = carbon.AvailableBondingSites();

            // Assert
            Assert.Equal(2, available);
        }

        [Fact]
        public void AvailableBondingSites_WithMaxBonds_ShouldReturnZero()
        {
            // Arrange
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };
            var oxygen = new Atom("O", "Oxygen", 8, 16.0);
            hydrogen.Bonds.Add(new Bond(hydrogen, oxygen, BondType.Single, 463, 0.96));

            // Act
            var available = hydrogen.AvailableBondingSites();

            // Assert
            Assert.Equal(0, available);
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopy()
        {
            // Arrange
            var original = new Atom("N", "Nitrogen", 7, 14.007)
            {
                ValenceElectrons = 5,
                MaxBonds = 3,
                Electronegativity = 3.04,
                Reactivity = 5.0,
                IsRadioactive = false,
                IsConductor = false,
                MeltingPoint = -210,
                BoilingPoint = -196,
                Position = new Point(100, 200)
            };
            original.ElectronShells.Add(2);
            original.ElectronShells.Add(5);

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(original.Symbol, clone.Symbol);
            Assert.Equal(original.Name, clone.Name);
            Assert.Equal(original.AtomicNumber, clone.AtomicNumber);
            Assert.Equal(original.ValenceElectrons, clone.ValenceElectrons);
            Assert.Equal(original.MaxBonds, clone.MaxBonds);
            Assert.Equal(original.Electronegativity, clone.Electronegativity);
            Assert.Equal(original.Reactivity, clone.Reactivity);
            Assert.Equal(original.MeltingPoint, clone.MeltingPoint);
            Assert.Equal(original.BoilingPoint, clone.BoilingPoint);
            Assert.Equal(original.Position, clone.Position);

            // Verify electron shells are copied
            Assert.Equal(2, clone.ElectronShells.Count);
            Assert.Equal(2, clone.ElectronShells[0]);
            Assert.Equal(5, clone.ElectronShells[1]);

            // Verify it's a deep copy (different list instance)
            Assert.NotSame(original.ElectronShells, clone.ElectronShells);
        }

        [Fact]
        public void Clone_ShouldNotCopyBonds()
        {
            // Arrange
            var original = new Atom("C", "Carbon", 6, 12.011);
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008);
            original.Bonds.Add(new Bond(original, hydrogen, BondType.Single, 413, 1.09));

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Empty(clone.Bonds); // Clone should have fresh bonds list
        }
    }

    public class BondTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Arrange
            var carbon = new Atom("C", "Carbon", 6, 12.011);
            var oxygen = new Atom("O", "Oxygen", 8, 16.0);

            // Act
            var bond = new Bond(carbon, oxygen, BondType.Double, 799, 1.20);

            // Assert
            Assert.Equal(carbon, bond.Atom1);
            Assert.Equal(oxygen, bond.Atom2);
            Assert.Equal(BondType.Double, bond.Type);
            Assert.Equal(799, bond.BondEnergy);
            Assert.Equal(1.20, bond.BondLength);
            Assert.True(bond.IsStable);
        }

        [Fact]
        public void Bond_DifferentTypes_ShouldBeDistinguishable()
        {
            // Arrange
            var atom1 = new Atom("N", "Nitrogen", 7, 14.007);
            var atom2 = new Atom("N", "Nitrogen", 7, 14.007);

            // Act
            var singleBond = new Bond(atom1, atom2, BondType.Single, 160, 1.45);
            var doubleBond = new Bond(atom1, atom2, BondType.Double, 418, 1.25);
            var tripleBond = new Bond(atom1, atom2, BondType.Triple, 941, 1.10);

            // Assert
            Assert.Equal(BondType.Single, singleBond.Type);
            Assert.Equal(BondType.Double, doubleBond.Type);
            Assert.Equal(BondType.Triple, tripleBond.Type);
            Assert.True(tripleBond.BondEnergy > doubleBond.BondEnergy);
            Assert.True(doubleBond.BondEnergy > singleBond.BondEnergy);
        }
    }

    public class MoleculeTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var molecule = new Molecule();

            // Assert
            Assert.NotEqual(Guid.Empty, molecule.Id);
            Assert.Equal(string.Empty, molecule.Formula);
            Assert.Equal("Unknown", molecule.Name);
            Assert.NotNull(molecule.Atoms);
            Assert.Empty(molecule.Atoms);
            Assert.NotNull(molecule.Bonds);
            Assert.Empty(molecule.Bonds);
            Assert.Equal(MoleculeStability.Stable, molecule.Stability);
        }

        [Fact]
        public void CalculateFormula_Water_ShouldBeH2O()
        {
            // Arrange
            var molecule = new Molecule();
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            molecule.Atoms.Add(new Atom("O", "Oxygen", 8, 16.0));

            // Act
            molecule.CalculateFormula();

            // Assert
            Assert.Equal("H2O", molecule.Formula);
        }

        [Fact]
        public void CalculateFormula_Methane_ShouldBeCH4()
        {
            // Arrange
            var molecule = new Molecule();
            molecule.Atoms.Add(new Atom("C", "Carbon", 6, 12.011));
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));

            // Act
            molecule.CalculateFormula();

            // Assert
            Assert.Equal("CH4", molecule.Formula);
        }

        [Fact]
        public void CalculateFormula_CarbonDioxide_ShouldBeCO2()
        {
            // Arrange
            var molecule = new Molecule();
            molecule.Atoms.Add(new Atom("C", "Carbon", 6, 12.011));
            molecule.Atoms.Add(new Atom("O", "Oxygen", 8, 16.0));
            molecule.Atoms.Add(new Atom("O", "Oxygen", 8, 16.0));

            // Act
            molecule.CalculateFormula();

            // Assert
            Assert.Equal("CO2", molecule.Formula);
        }

        [Fact]
        public void CalculateFormula_Glucose_ShouldBeC6H12O6()
        {
            // Arrange
            var molecule = new Molecule();
            for (int i = 0; i < 6; i++)
                molecule.Atoms.Add(new Atom("C", "Carbon", 6, 12.011));
            for (int i = 0; i < 12; i++)
                molecule.Atoms.Add(new Atom("H", "Hydrogen", 1, 1.008));
            for (int i = 0; i < 6; i++)
                molecule.Atoms.Add(new Atom("O", "Oxygen", 8, 16.0));

            // Act
            molecule.CalculateFormula();

            // Assert
            Assert.Equal("C6H12O6", molecule.Formula);
        }

        [Fact]
        public void CalculateFormula_SingleAtom_ShouldBeElementSymbol()
        {
            // Arrange
            var molecule = new Molecule();
            molecule.Atoms.Add(new Atom("He", "Helium", 2, 4.003));

            // Act
            molecule.CalculateFormula();

            // Assert
            Assert.Equal("He", molecule.Formula);
        }

        [Fact]
        public void CheckStability_WellBondedMolecule_ShouldBeStable()
        {
            // Arrange
            var molecule = new Molecule();
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };
            var h1 = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };
            var h2 = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };
            var h3 = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };
            var h4 = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };

            molecule.Atoms.AddRange(new[] { carbon, h1, h2, h3, h4 });

            // Create bonds
            var bond1 = new Bond(carbon, h1, BondType.Single, 413, 1.09);
            var bond2 = new Bond(carbon, h2, BondType.Single, 413, 1.09);
            var bond3 = new Bond(carbon, h3, BondType.Single, 413, 1.09);
            var bond4 = new Bond(carbon, h4, BondType.Single, 413, 1.09);

            carbon.Bonds.AddRange(new[] { bond1, bond2, bond3, bond4 });
            h1.Bonds.Add(bond1);
            h2.Bonds.Add(bond2);
            h3.Bonds.Add(bond3);
            h4.Bonds.Add(bond4);

            molecule.Bonds.AddRange(new[] { bond1, bond2, bond3, bond4 });

            // Act
            molecule.CheckStability();

            // Assert
            Assert.Equal(MoleculeStability.Stable, molecule.Stability);
        }

        [Fact]
        public void CheckStability_UnbondedAtoms_ShouldBeMetastable()
        {
            // Arrange
            var molecule = new Molecule();
            var carbon = new Atom("C", "Carbon", 6, 12.011) { MaxBonds = 4 };
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };

            molecule.Atoms.AddRange(new[] { carbon, hydrogen });
            // No bonds created

            // Act
            molecule.CheckStability();

            // Assert
            Assert.Equal(MoleculeStability.Metastable, molecule.Stability);
        }

        [Fact]
        public void CheckStability_OverbondedAtom_ShouldBeUnstable()
        {
            // Arrange
            var molecule = new Molecule();
            var hydrogen = new Atom("H", "Hydrogen", 1, 1.008) { MaxBonds = 1 };
            var oxygen1 = new Atom("O", "Oxygen", 8, 16.0);
            var oxygen2 = new Atom("O", "Oxygen", 8, 16.0);

            molecule.Atoms.AddRange(new[] { hydrogen, oxygen1, oxygen2 });

            // Overbond hydrogen (max 1, but add 2)
            hydrogen.Bonds.Add(new Bond(hydrogen, oxygen1, BondType.Single, 463, 0.96));
            hydrogen.Bonds.Add(new Bond(hydrogen, oxygen2, BondType.Single, 463, 0.96));

            // Act
            molecule.CheckStability();

            // Assert
            Assert.Equal(MoleculeStability.Unstable, molecule.Stability);
        }

        [Fact]
        public void CheckStability_PeroxideBond_ShouldBeExplosive()
        {
            // Arrange
            var molecule = new Molecule();
            var oxygen1 = new Atom("O", "Oxygen", 8, 16.0);
            var oxygen2 = new Atom("O", "Oxygen", 8, 16.0);

            molecule.Atoms.AddRange(new[] { oxygen1, oxygen2 });

            // Create O-O bond (peroxide)
            var peroxideBond = new Bond(oxygen1, oxygen2, BondType.Single, 142, 1.48);
            oxygen1.Bonds.Add(peroxideBond);
            oxygen2.Bonds.Add(peroxideBond);

            // Act
            molecule.CheckStability();

            // Assert
            Assert.True(molecule.IsExplosive);
            Assert.Equal(MoleculeStability.Unstable, molecule.Stability);
        }

        [Fact]
        public void CheckStability_NitrogenTripleBond_ShouldBeExplosive()
        {
            // Arrange
            var molecule = new Molecule();
            var nitrogen1 = new Atom("N", "Nitrogen", 7, 14.007);
            var nitrogen2 = new Atom("N", "Nitrogen", 7, 14.007);

            molecule.Atoms.AddRange(new[] { nitrogen1, nitrogen2 });

            // Create N≡N bond
            var tripleBond = new Bond(nitrogen1, nitrogen2, BondType.Triple, 941, 1.10);
            nitrogen1.Bonds.Add(tripleBond);
            nitrogen2.Bonds.Add(tripleBond);

            // Act
            molecule.CheckStability();

            // Assert
            Assert.True(molecule.IsExplosive);
        }
    }
}
