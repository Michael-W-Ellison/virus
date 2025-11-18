using BiochemSimulator.Engine;
using BiochemSimulator.Models;
using System.Linq;
using Xunit;

namespace BiochemSimulator.Tests.Engine
{
    public class AtomicEngineTests
    {
        [Fact]
        public void Constructor_ShouldInitializePeriodicTable()
        {
            // Act
            var engine = new AtomicEngine();

            // Assert - Should be able to get basic atoms
            var hydrogen = engine.GetAtom("H");
            var carbon = engine.GetAtom("C");
            var nitrogen = engine.GetAtom("N");
            var oxygen = engine.GetAtom("O");

            Assert.NotNull(hydrogen);
            Assert.NotNull(carbon);
            Assert.NotNull(nitrogen);
            Assert.NotNull(oxygen);
        }

        [Fact]
        public void GetAtom_ValidSymbol_ShouldReturnAtomClone()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var hydrogen1 = engine.GetAtom("H");
            var hydrogen2 = engine.GetAtom("H");

            // Assert
            Assert.NotNull(hydrogen1);
            Assert.NotNull(hydrogen2);
            Assert.NotSame(hydrogen1, hydrogen2); // Should be different instances (clones)
            Assert.Equal("H", hydrogen1.Symbol);
            Assert.Equal("Hydrogen", hydrogen1.Name);
            Assert.Equal(1, hydrogen1.AtomicNumber);
        }

        [Fact]
        public void GetAtom_InvalidSymbol_ShouldReturnNull()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var atom = engine.GetAtom("Xx"); // Non-existent element

            // Assert
            Assert.Null(atom);
        }

        [Fact]
        public void GetBasicAtoms_ShouldReturnHCNO()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var basicAtoms = engine.GetBasicAtoms();

            // Assert
            Assert.Equal(4, basicAtoms.Count);
            Assert.Contains(basicAtoms, a => a.Symbol == "H");
            Assert.Contains(basicAtoms, a => a.Symbol == "C");
            Assert.Contains(basicAtoms, a => a.Symbol == "N");
            Assert.Contains(basicAtoms, a => a.Symbol == "O");
        }

        [Fact]
        public void GetAtom_Hydrogen_ShouldHaveCorrectProperties()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var hydrogen = engine.GetAtom("H");

            // Assert
            Assert.NotNull(hydrogen);
            Assert.Equal(1, hydrogen.ValenceElectrons);
            Assert.Equal(1, hydrogen.MaxBonds);
            Assert.Equal(MaterialPhase.Gas, hydrogen.PhaseAtRoomTemp);
            Assert.False(hydrogen.IsRadioactive);
        }

        [Fact]
        public void GetAtom_Carbon_ShouldHaveCorrectProperties()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var carbon = engine.GetAtom("C");

            // Assert
            Assert.NotNull(carbon);
            Assert.Equal(4, carbon.ValenceElectrons);
            Assert.Equal(4, carbon.MaxBonds);
            Assert.False(carbon.IsRadioactive);
        }

        [Fact]
        public void GetAtom_Oxygen_ShouldHaveCorrectProperties()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var oxygen = engine.GetAtom("O");

            // Assert
            Assert.NotNull(oxygen);
            Assert.Equal(6, oxygen.ValenceElectrons);
            Assert.Equal(2, oxygen.MaxBonds);
            Assert.Equal(MaterialPhase.Gas, oxygen.PhaseAtRoomTemp);
        }

        [Fact]
        public void CanBond_BothAtomsAvailable_ShouldReturnTrue()
        {
            // Arrange
            var engine = new AtomicEngine();
            var hydrogen = engine.GetAtom("H")!;
            var oxygen = engine.GetAtom("O")!;
            hydrogen.MaxBonds = 1;
            oxygen.MaxBonds = 2;

            // Act
            var canBond = engine.CanBond(hydrogen, oxygen);

            // Assert
            Assert.True(canBond);
        }

        [Fact]
        public void CanBond_OneAtomFull_ShouldReturnFalse()
        {
            // Arrange
            var engine = new AtomicEngine();
            var hydrogen1 = engine.GetAtom("H")!;
            var hydrogen2 = engine.GetAtom("H")!;
            var oxygen = engine.GetAtom("O")!;

            hydrogen1.MaxBonds = 1;
            hydrogen2.MaxBonds = 1;

            // Fill hydrogen1's bonds
            var bond = new Bond(hydrogen1, oxygen, BondType.Single, 463, 0.96);
            hydrogen1.Bonds.Add(bond);

            // Act
            var canBond = engine.CanBond(hydrogen1, hydrogen2);

            // Assert
            Assert.False(canBond);
        }

        [Fact]
        public void CreateBond_ValidAtoms_ShouldCreateBond()
        {
            // Arrange
            var engine = new AtomicEngine();
            var hydrogen = engine.GetAtom("H")!;
            var oxygen = engine.GetAtom("O")!;
            hydrogen.MaxBonds = 1;
            oxygen.MaxBonds = 2;

            // Act
            var bond = engine.CreateBond(hydrogen, oxygen, BondType.Single);

            // Assert
            Assert.NotNull(bond);
            Assert.Equal(hydrogen, bond.Atom1);
            Assert.Equal(oxygen, bond.Atom2);
            Assert.Equal(BondType.Single, bond.Type);
            Assert.True(bond.BondEnergy > 0);
            Assert.Single(hydrogen.Bonds);
            Assert.Single(oxygen.Bonds);
        }

        [Fact]
        public void CreateBond_NoAvailableBonds_ShouldReturnNull()
        {
            // Arrange
            var engine = new AtomicEngine();
            var hydrogen = engine.GetAtom("H")!;
            var oxygen = engine.GetAtom("O")!;
            hydrogen.MaxBonds = 1;

            // Fill hydrogen's bond capacity
            var existingBond = new Bond(hydrogen, oxygen, BondType.Single, 463, 0.96);
            hydrogen.Bonds.Add(existingBond);

            var carbon = engine.GetAtom("C")!;

            // Act
            var bond = engine.CreateBond(hydrogen, carbon, BondType.Single);

            // Assert
            Assert.Null(bond);
        }

        [Fact]
        public void CreateMolecule_Water_ShouldIdentifyCorrectly()
        {
            // Arrange
            var engine = new AtomicEngine();
            var h1 = engine.GetAtom("H")!;
            var h2 = engine.GetAtom("H")!;
            var o = engine.GetAtom("O")!;

            h1.MaxBonds = 1;
            h2.MaxBonds = 1;
            o.MaxBonds = 2;

            var bond1 = engine.CreateBond(h1, o, BondType.Single)!;
            var bond2 = engine.CreateBond(h2, o, BondType.Single)!;

            var atoms = new List<Atom> { h1, h2, o };
            var bonds = new List<Bond> { bond1, bond2 };

            // Act
            var molecule = engine.CreateMolecule(atoms, bonds);

            // Assert
            Assert.Equal("H2O", molecule.Formula);
            Assert.Equal("Water", molecule.Name);
        }

        [Fact]
        public void CreateMolecule_Methane_ShouldHaveCorrectFormula()
        {
            // Arrange
            var engine = new AtomicEngine();
            var carbon = engine.GetAtom("C")!;
            var h1 = engine.GetAtom("H")!;
            var h2 = engine.GetAtom("H")!;
            var h3 = engine.GetAtom("H")!;
            var h4 = engine.GetAtom("H")!;

            carbon.MaxBonds = 4;
            h1.MaxBonds = 1;
            h2.MaxBonds = 1;
            h3.MaxBonds = 1;
            h4.MaxBonds = 1;

            var bond1 = engine.CreateBond(carbon, h1, BondType.Single)!;
            var bond2 = engine.CreateBond(carbon, h2, BondType.Single)!;
            var bond3 = engine.CreateBond(carbon, h3, BondType.Single)!;
            var bond4 = engine.CreateBond(carbon, h4, BondType.Single)!;

            var atoms = new List<Atom> { carbon, h1, h2, h3, h4 };
            var bonds = new List<Bond> { bond1, bond2, bond3, bond4 };

            // Act
            var molecule = engine.CreateMolecule(atoms, bonds);

            // Assert
            Assert.Equal("CH4", molecule.Formula);
        }

        [Fact]
        public void CreateMolecule_CarbonDioxide_ShouldHaveCorrectFormula()
        {
            // Arrange
            var engine = new AtomicEngine();
            var carbon = engine.GetAtom("C")!;
            var o1 = engine.GetAtom("O")!;
            var o2 = engine.GetAtom("O")!;

            carbon.MaxBonds = 4;
            o1.MaxBonds = 2;
            o2.MaxBonds = 2;

            // C=O=O (double bonds)
            var bond1 = engine.CreateBond(carbon, o1, BondType.Double)!;
            var bond2 = engine.CreateBond(carbon, o2, BondType.Double)!;

            var atoms = new List<Atom> { carbon, o1, o2 };
            var bonds = new List<Bond> { bond1, bond2 };

            // Act
            var molecule = engine.CreateMolecule(atoms, bonds);

            // Assert
            Assert.Equal("CO2", molecule.Formula);
        }

        [Fact]
        public void CreateMolecule_ShouldCheckStability()
        {
            // Arrange
            var engine = new AtomicEngine();
            var h1 = engine.GetAtom("H")!;
            var h2 = engine.GetAtom("H")!;

            h1.MaxBonds = 1;
            h2.MaxBonds = 1;

            var bond = engine.CreateBond(h1, h2, BondType.Single)!;

            var atoms = new List<Atom> { h1, h2 };
            var bonds = new List<Bond> { bond };

            // Act
            var molecule = engine.CreateMolecule(atoms, bonds);

            // Assert
            Assert.Equal("H2", molecule.Formula);
            // Stability should be checked (either Stable or HighlyReactive for H2)
            Assert.NotEqual(MoleculeStability.Unstable, molecule.Stability);
        }

        [Fact]
        public void GetAllAtoms_ShouldReturnMultipleElements()
        {
            // Arrange
            var engine = new AtomicEngine();

            // Act
            var allAtoms = engine.GetAllAtoms();

            // Assert
            Assert.NotEmpty(allAtoms);
            Assert.True(allAtoms.Count >= 4); // At least H, C, N, O
            Assert.Contains(allAtoms, a => a.Symbol == "H");
            Assert.Contains(allAtoms, a => a.Symbol == "C");
            Assert.Contains(allAtoms, a => a.Symbol == "N");
            Assert.Contains(allAtoms, a => a.Symbol == "O");
        }

        [Fact]
        public void CreateBond_DoubleBond_ShouldHaveHigherEnergy()
        {
            // Arrange
            var engine = new AtomicEngine();
            var c1 = engine.GetAtom("C")!;
            var c2 = engine.GetAtom("C")!;
            var c3 = engine.GetAtom("C")!;
            var c4 = engine.GetAtom("C")!;

            c1.MaxBonds = 4;
            c2.MaxBonds = 4;
            c3.MaxBonds = 4;
            c4.MaxBonds = 4;

            // Act
            var singleBond = engine.CreateBond(c1, c2, BondType.Single)!;
            var doubleBond = engine.CreateBond(c3, c4, BondType.Double)!;

            // Assert
            Assert.True(doubleBond.BondEnergy > singleBond.BondEnergy);
        }

        [Fact]
        public void CreateBond_TripleBond_ShouldHaveHighestEnergy()
        {
            // Arrange
            var engine = new AtomicEngine();
            var n1 = engine.GetAtom("N")!;
            var n2 = engine.GetAtom("N")!;
            var n3 = engine.GetAtom("N")!;
            var n4 = engine.GetAtom("N")!;

            n1.MaxBonds = 3;
            n2.MaxBonds = 3;
            n3.MaxBonds = 3;
            n4.MaxBonds = 3;

            // Act
            var singleBond = engine.CreateBond(n1, n2, BondType.Single)!;
            var tripleBond = engine.CreateBond(n3, n4, BondType.Triple)!;

            // Assert
            Assert.NotNull(tripleBond);
            Assert.True(tripleBond.BondEnergy > singleBond.BondEnergy * 2);
        }
    }
}
