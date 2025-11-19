# BiochemSimulator Developer Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Architecture Overview](#architecture-overview)
3. [Development Environment Setup](#development-environment-setup)
4. [Project Structure](#project-structure)
5. [Core Systems](#core-systems)
6. [Testing](#testing)
7. [Building & Debugging](#building--debugging)
8. [Code Conventions](#code-conventions)
9. [Performance Considerations](#performance-considerations)
10. [Common Development Tasks](#common-development-tasks)

---

## Getting Started

### Prerequisites

**Required:**
- .NET 8.0 SDK or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Windows 10 or later (for WPF)
- Git (for version control)

**Recommended:**
- Visual Studio 2022 (Community Edition or higher)
  - Workloads: ".NET desktop development"
- Visual Studio Code with C# extensions (alternative)
- Windows Terminal or PowerShell 7+

### Quick Start

```bash
# Clone the repository
git clone <repository-url>
cd virus

# Restore dependencies
dotnet restore BiochemSimulator.sln

# Build the solution
dotnet build BiochemSimulator.sln --configuration Debug

# Run tests
dotnet test BiochemSimulator.Tests/BiochemSimulator.Tests.csproj

# Run the application
dotnet run --project BiochemSimulator.csproj
```

---

## Architecture Overview

### Application Architecture

BiochemSimulator follows a **Model-Engine-View** architecture pattern:

```
┌─────────────────────────────────────────────────┐
│                   View Layer                    │
│  (WPF XAML + Code-Behind: MainWindow.xaml.cs)  │
└─────────────────┬───────────────────────────────┘
                  │ Events & Data Binding
┌─────────────────▼───────────────────────────────┐
│                 Engine Layer                    │
│  GameManager │ ChemistryEngine │ OrganismMgr   │
│  AtomicEngine │ SaveManager │ AchievementMgr   │
└─────────────────┬───────────────────────────────┘
                  │ Business Logic
┌─────────────────▼───────────────────────────────┐
│                 Model Layer                     │
│  Atom │ Chemical │ Molecule │ Organism          │
│  GameState │ PlayerProfile │ GameSave           │
└─────────────────────────────────────────────────┘
```

### Key Design Patterns

1. **Event-Driven Architecture**
   - GameManager raises events: `StateChanged`, `PhaseChanged`
   - UI subscribes to events for reactive updates

2. **Singleton Pattern**
   - ChemistryEngine (chemical database)
   - AtomicEngine (periodic table)

3. **Manager Pattern**
   - GameManager: Central game coordinator
   - OrganismManager: Organism lifecycle
   - SaveManager: Persistence layer
   - AchievementManager: Achievement tracking

4. **Immutable Data**
   - Chemical definitions (initialized once)
   - Periodic table elements (read-only after init)

---

## Development Environment Setup

### Visual Studio 2022

1. **Install Visual Studio 2022**
   - Download from: https://visualstudio.microsoft.com/
   - Select ".NET desktop development" workload

2. **Open Solution**
   ```
   File > Open > Project/Solution
   Navigate to BiochemSimulator.sln
   ```

3. **Configure Startup Project**
   - Right-click `BiochemSimulator` in Solution Explorer
   - Set as Startup Project

4. **Enable Test Explorer**
   ```
   Test > Test Explorer
   Build solution to discover tests
   ```

### Visual Studio Code

1. **Install Extensions**
   - C# (ms-dotnettools.csharp)
   - C# Dev Kit (ms-dotnettools.csdevkit)
   - .NET Install Tool (ms-dotnettools.vscode-dotnet-runtime)

2. **Open Folder**
   ```
   File > Open Folder > Select virus/ directory
   ```

3. **Configure Launch**
   - `.vscode/launch.json` (auto-generated)
   - `.vscode/tasks.json` (auto-generated)

4. **Run Tests**
   ```bash
   # In integrated terminal
   dotnet test
   ```

---

## Project Structure

```
virus/
├── BiochemSimulator.sln                    # Solution file (main + test projects)
├── BiochemSimulator.csproj                 # Main project configuration
├── README.md                               # User-facing documentation
├── DEVELOPER_GUIDE.md                      # This file
├── TROUBLESHOOTING.md                      # Common issues and fixes
├── TDD_IMPLEMENTATION_REPORT.md            # Test coverage report
│
├── App.xaml                                # Application resources & styles
├── App.xaml.cs                             # Application entry point
├── MainWindow.xaml                         # Main UI layout (WPF)
├── MainWindow.xaml.cs                      # UI logic & event handlers (~1900 lines)
├── ProfileSelectionWindow.xaml             # Profile selection UI
├── ProfileSelectionWindow.xaml.cs          # Profile selection logic
│
├── Models/                                 # Data models
│   ├── GameState.cs                        # GameState enum, GamePhase enum
│   ├── Chemical.cs                         # Chemical, ChemicalReaction, ChemicalType
│   ├── Organism.cs                         # Organism with evolution & resistance
│   ├── Atom.cs                             # Atom, Bond, BondType, Molecule
│   ├── PlayerProfile.cs                    # Player profile data
│   ├── GameSave.cs                         # Save game data structure
│   └── Achievement.cs                      # Achievement definitions
│
├── Engine/                                 # Business logic
│   ├── GameManager.cs                      # Central game coordinator (~800 lines)
│   ├── ChemistryEngine.cs                  # Chemical database & reactions (~900 lines)
│   ├── OrganismManager.cs                  # Organism lifecycle & evolution (~500 lines)
│   ├── AtomicEngine.cs                     # Periodic table & atomic bonding (~600 lines)
│   ├── SaveManager.cs                      # Save/load persistence (~300 lines)
│   └── AchievementManager.cs               # Achievement tracking (~200 lines)
│
└── BiochemSimulator.Tests/                 # Test project (130+ tests)
    ├── BiochemSimulator.Tests.csproj       # Test project configuration
    ├── README.md                           # Test documentation
    ├── Models/                             # Model tests (56+ tests)
    │   ├── OrganismTests.cs                # 17 tests
    │   ├── AtomTests.cs                    # 15 tests
    │   ├── MoleculeTests.cs                # 13 tests
    │   └── ChemicalTests.cs                # 11 tests
    └── Engine/                             # Engine tests (74+ tests)
        ├── AtomicEngineTests.cs            # 24 tests
        ├── ChemistryEngineTests.cs         # 26 tests
        ├── OrganismManagerTests.cs         # 20 tests
        └── GameManagerTests.cs             # 24 tests
```

### File Size Reference

| File | Lines | Complexity | Description |
|------|-------|------------|-------------|
| MainWindow.xaml.cs | ~1900 | High | UI event handlers, rendering, user interactions |
| GameManager.cs | ~800 | Medium-High | Game state machine, phase progression |
| ChemistryEngine.cs | ~900 | High | Chemical database initialization, reaction logic |
| AtomicEngine.cs | ~600 | Medium | Periodic table, molecular bonding rules |
| OrganismManager.cs | ~500 | Medium | Organism spawning, evolution, AI behavior |

---

## Core Systems

### 1. Game State Machine

**Location:** `Engine/GameManager.cs`

**States:**
```csharp
public enum GameState
{
    MainMenu,           // Initial screen
    BiochemistryLab,    // Phase 1-4: Chemistry gameplay
    MicroscopeView,     // Phase 5: Observing organisms
    VirusOutbreak,      // Phase 6: Desktop defense
    GameOver,           // Win/lose screen
    Paused              // Pause menu
}
```

**Phase Progression:**
```csharp
public enum GamePhase
{
    AminoAcids,         // Create amino acids
    Proteins,           // Synthesize proteins
    RNA,                // Build RNA
    DNA,                // Construct DNA
    CellMembrane,       // Form cell membrane
    MicroscopeView,     // Observe life
    VirusDefense        // Combat outbreak
}
```

**State Transitions:**
```
MainMenu → BiochemistryLab (Start Game)
BiochemistryLab → MicroscopeView (Complete Phase 4)
MicroscopeView → VirusOutbreak (Dispose organisms, wait 30s)
VirusOutbreak → GameOver (Win/lose condition)
```

### 2. Chemistry System

**Location:** `Engine/ChemistryEngine.cs`

**Chemical Database:**
- **70+ chemicals** defined in `InitializeChemicals()`
- Categories: Amino Acids, Nucleotides, Disinfectants, Lipids, etc.
- Properties: pH, Toxicity, Reactivity, Color, Formula

**Reaction System:**
- **30+ reactions** defined in `InitializeReactions()`
- Reactants → Products with visual effects
- Dangerous reaction detection (e.g., Bleach + Acid)
- Exothermic reactions with heat effects

**Key Methods:**
```csharp
ChemistryEngine.Instance.GetChemical(name)          // Retrieve chemical
ChemistryEngine.Instance.CalculateDamage(...)       // Organism damage
ChemistryEngine.Instance.FindReaction(chemicals)    // Match reaction
```

### 3. Atomic/Molecular System

**Location:** `Engine/AtomicEngine.cs`, `Models/Atom.cs`

**Periodic Table:**
- Basic elements: H, C, N, O, S, P, Cl, Na, K, Ca
- Properties: Atomic number, mass, electronegativity, bonding capacity

**Bonding Rules:**
```csharp
H: 1 bond   (e.g., H-H, H-O)
O: 2 bonds  (e.g., H-O-H, O=O)
N: 3 bonds  (e.g., N≡N)
C: 4 bonds  (e.g., CH₄, C=C)
```

**Molecule Validation:**
- All atoms must have satisfied valence
- Bond energy calculations
- Stability checking
- Formula generation (e.g., "H2O", "CH4")

### 4. Evolution & Resistance System

**Location:** `Engine/OrganismManager.cs`, `Models/Organism.cs`

**Organism Lifecycle:**
1. **Spawning:** Random positions on screen
2. **Movement:** Slow drift with random direction changes
3. **Reproduction:** Creates clones with mutations
4. **Death:** Health reaches 0

**Evolution Mechanics:**
```csharp
// Mutation rate: 10% per reproduction
if (random.NextDouble() < 0.1)
{
    // Random mutations:
    // - Color shift
    // - Size change (±20%)
    // - Reproduction rate (±10%)
}
```

**Resistance Development:**
```csharp
// Survivors of chemical attacks develop resistance
organism.Resistances[chemicalName] += 0.05;  // +5% per exposure
// Capped at 95% maximum
```

### 5. Save/Load System

**Location:** `Engine/SaveManager.cs`, `MainWindow.xaml.cs`

**Save Data Structure:**
```csharp
public class GameSave
{
    string SaveName;
    DateTime SaveDate;
    string ProfileName;
    GameState CurrentState;
    GamePhase CurrentPhase;
    int Score;
    List<string> UnlockedAchievements;
    // Note: Workspace not fully serialized yet
}
```

**File Organization:**
```
Documents/
└── BiochemSimulator/
    ├── Profiles/
    │   └── [PlayerName].json
    └── Saves/
        └── [PlayerName]/
            ├── [SaveName1].json
            ├── [SaveName2].json
            └── ...
```

**Auto-Save:**
- Triggers every 5 minutes (DispatcherTimer)
- Saves to "AutoSave_[timestamp].json"

---

## Testing

### Test Framework

- **Framework:** xUnit 2.6.2
- **Coverage:** 130+ tests
- **Pattern:** Arrange-Act-Assert (AAA)

### Running Tests

**Command Line:**
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test file
dotnet test --filter "FullyQualifiedName~OrganismTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~OrganismTests.TakeDamage_WithResistance_ShouldReduceDamage"

# Code coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

**Visual Studio:**
1. Open Test Explorer: `Test > Test Explorer`
2. Click "Run All" or right-click specific tests
3. View results inline with code coverage

### Test Coverage by Component

| Component | Tests | Coverage |
|-----------|-------|----------|
| Organism | 17 | ~90% |
| Atom | 15 | ~85% |
| Molecule | 13 | ~90% |
| Chemical | 11 | ~80% |
| AtomicEngine | 24 | ~85% |
| ChemistryEngine | 26 | ~85% |
| OrganismManager | 20 | ~85% |
| GameManager | 24 | ~90% |

### Writing New Tests

**Test Template:**
```csharp
using Xunit;
using BiochemSimulator.Models;
using BiochemSimulator.Engine;

namespace BiochemSimulator.Tests.Models
{
    public class YourComponentTests
    {
        [Fact]
        public void MethodName_Scenario_ExpectedBehavior()
        {
            // Arrange
            var component = new YourComponent();
            var input = "test data";

            // Act
            var result = component.MethodName(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
```

---

## Building & Debugging

### Build Configurations

**Debug Build:**
```bash
dotnet build --configuration Debug
# Output: bin/Debug/net8.0-windows/BiochemSimulator.exe
# - Includes debug symbols (.pdb)
# - No optimizations
# - Assertions enabled
```

**Release Build:**
```bash
dotnet build --configuration Release
# Output: bin/Release/net8.0-windows/BiochemSimulator.exe
# - Optimized code
# - No debug symbols (unless specified)
# - Smaller binary size
```

**Publish (Standalone Executable):**
```bash
dotnet publish BiochemSimulator.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true

# Output: bin/Release/net8.0-windows/win-x64/publish/BiochemSimulator.exe
# - Single .exe file (includes .NET runtime)
# - ~80MB size
# - No .NET installation required on target machine
```

### Debugging in Visual Studio

**Breakpoints:**
1. Click left margin in code editor (red dot appears)
2. Press F5 to start debugging
3. Execution pauses at breakpoint
4. Inspect variables in Locals/Watch windows

**Useful Windows:**
- **Locals:** Auto-displays local variables
- **Watch:** Custom expressions (e.g., `organism.Health`, `_gameManager.CurrentPhase`)
- **Call Stack:** See method call hierarchy
- **Output:** Debug.WriteLine() messages

**Debug Shortcuts:**
- `F5`: Start debugging
- `F9`: Toggle breakpoint
- `F10`: Step over (next line)
- `F11`: Step into (enter method)
- `Shift+F11`: Step out (exit method)
- `Ctrl+F5`: Run without debugging

### Common Build Issues

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for detailed solutions.

---

## Code Conventions

### Naming Conventions

```csharp
// Classes: PascalCase
public class GameManager { }

// Methods: PascalCase
public void StartGame() { }

// Private fields: _camelCase with underscore prefix
private GameState _currentState;

// Public properties: PascalCase
public int Score { get; set; }

// Local variables: camelCase
var chemicalName = "Water";

// Constants: PascalCase
public const int MaxOrganisms = 500;

// Enums: PascalCase (type and values)
public enum GameState { MainMenu, Playing }
```

### File Organization

```csharp
// 1. Using statements (alphabetical, System first)
using System;
using System.Collections.Generic;
using System.Linq;
using BiochemSimulator.Models;

// 2. Namespace
namespace BiochemSimulator.Engine
{
    // 3. Class documentation
    /// <summary>
    /// Manages game state transitions and phase progression.
    /// </summary>
    public class GameManager
    {
        // 4. Events
        public event EventHandler<GameState> StateChanged;

        // 5. Constants
        private const int MaxScore = 10000;

        // 6. Private fields
        private GameState _currentState;

        // 7. Public properties
        public GameState CurrentState { get; private set; }

        // 8. Constructor
        public GameManager() { }

        // 9. Public methods
        public void StartGame() { }

        // 10. Private methods
        private void UpdatePhase() { }
    }
}
```

### XML Documentation

```csharp
/// <summary>
/// Calculates damage dealt to an organism by a chemical.
/// </summary>
/// <param name="chemical">The chemical being applied</param>
/// <param name="organism">The target organism</param>
/// <param name="distance">Distance from spray point (0-1)</param>
/// <returns>Damage value (0-100)</returns>
public double CalculateDamage(Chemical chemical, Organism organism, double distance)
{
    // Implementation
}
```

### Error Handling

```csharp
// Use try-catch for expected failures
try
{
    var gameSave = SaveManager.LoadGame(filePath);
}
catch (FileNotFoundException ex)
{
    MessageBox.Show($"Save file not found: {ex.Message}");
}

// Use null checks for optional parameters
public void ApplyChemical(Chemical chemical, Point? position = null)
{
    if (chemical == null)
        throw new ArgumentNullException(nameof(chemical));

    var sprayPosition = position ?? new Point(0, 0);
}
```

---

## Performance Considerations

### Rendering Optimization

**WPF Canvas Performance:**
```csharp
// GOOD: Batch canvas updates
AtomicCanvas.Children.Clear();
foreach (var organism in organisms)
{
    var ellipse = CreateOrganismVisual(organism);
    AtomicCanvas.Children.Add(ellipse);
}

// BAD: Individual updates in loop
foreach (var organism in organisms)
{
    AtomicCanvas.Children.Clear();  // Don't clear repeatedly!
    AtomicCanvas.Children.Add(CreateOrganismVisual(organism));
}
```

**Frame Rate Management:**
```csharp
// Use DispatcherTimer for game loop (not while-true loops)
_gameTimer = new DispatcherTimer
{
    Interval = TimeSpan.FromMilliseconds(16)  // ~60 FPS
};
_gameTimer.Tick += GameLoop_Tick;
```

### Memory Management

**Dispose Patterns:**
```csharp
// Unsubscribe from events to prevent memory leaks
_gameManager.StateChanged -= OnStateChanged;
_gameTimer.Stop();
_gameTimer.Tick -= GameLoop_Tick;
```

**Collection Optimization:**
```csharp
// Use List<T> capacity when size is known
var organisms = new List<Organism>(capacity: 100);

// Remove dead organisms periodically (not every frame)
if (_frameCount % 60 == 0)  // Every 60 frames
{
    _organisms.RemoveAll(o => o.Health <= 0);
}
```

### Algorithm Complexity

**Distance Calculations:**
```csharp
// Optimize expensive calculations
// Use squared distance to avoid Math.Sqrt when only comparing
double distanceSquared = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
if (distanceSquared < radiusSquared)  // No sqrt needed!
{
    // Point is within radius
}
```

**LINQ Performance:**
```csharp
// GOOD: Single pass
var aliveOrganisms = _organisms.Where(o => o.Health > 0).ToList();
int count = aliveOrganisms.Count;

// BAD: Multiple passes
int count = _organisms.Count(o => o.Health > 0);
var aliveOrganisms = _organisms.Where(o => o.Health > 0).ToList();
```

---

## Common Development Tasks

### Adding a New Chemical

**File:** `Engine/ChemistryEngine.cs`

```csharp
private void InitializeChemicals()
{
    // ... existing chemicals ...

    // Add your chemical
    _chemicals["YourChemical"] = new Chemical(
        name: "Your Chemical Name",
        formula: "YourFormula",
        description: "What it does",
        type: ChemicalType.Disinfectant,  // or appropriate type
        pH: 7.0,
        color: Colors.YourColor
    )
    {
        IsCaustic = false,
        Toxicity = 5.0,      // 0-10 scale
        Reactivity = 3.0,    // 0-10 scale
        IsOxidizer = false,
        IsReducer = false
    };
}
```

### Adding a New Chemical Reaction

**File:** `Engine/ChemistryEngine.cs`

```csharp
private void InitializeReactions()
{
    // ... existing reactions ...

    var yourReaction = new ChemicalReaction
    {
        Reactants = new List<Chemical>
        {
            _chemicals["Chemical1"],
            _chemicals["Chemical2"]
        },
        Products = new List<Chemical>
        {
            _chemicals["Product"]
        },
        Description = "Chemical1 + Chemical2 → Product",
        IsExothermic = true,  // or false
        IsDangerous = false,
        VisualEffect = Colors.Orange,
        VisualDescription = "Orange bubbling"
    };

    _reactions.Add(yourReaction);
}
```

### Adding a New Atom to Periodic Table

**File:** `Engine/AtomicEngine.cs`

```csharp
private void InitializePeriodicTable()
{
    // ... existing atoms ...

    _periodicTable.Add(new Atom
    {
        Symbol = "Ne",
        Name = "Neon",
        AtomicNumber = 10,
        AtomicMass = 20.18,
        Electronegativity = 0,  // Noble gas
        BondingCapacity = 0,    // Doesn't bond
        Color = Color.FromRgb(179, 227, 238)
    });
}
```

### Adding a New Achievement

**File:** `Engine/AchievementManager.cs`

```csharp
private void InitializeAchievements()
{
    // ... existing achievements ...

    _achievements.Add(new Achievement
    {
        Id = "your_achievement_id",
        Name = "Achievement Name",
        Description = "How to earn this achievement",
        IconPath = "path/to/icon.png",  // Optional
        Points = 10
    });
}
```

**Trigger Achievement:**
```csharp
// In appropriate location (e.g., MainWindow.xaml.cs)
if (condition_met)
{
    _achievementManager.UnlockAchievement("your_achievement_id");
}
```

### Adding a New Game Phase

1. **Add to enum** (`Models/GameState.cs`):
```csharp
public enum GamePhase
{
    // ... existing phases ...
    YourNewPhase
}
```

2. **Add progression logic** (`Engine/GameManager.cs`):
```csharp
public void CheckPhaseCompletion()
{
    switch (_currentPhase)
    {
        // ... existing cases ...

        case GamePhase.YourPreviousPhase:
            if (/* completion condition */)
            {
                AdvancePhase(GamePhase.YourNewPhase);
            }
            break;
    }
}
```

3. **Update UI** (`MainWindow.xaml.cs`):
```csharp
private void OnPhaseChanged(object sender, GamePhase newPhase)
{
    switch (newPhase)
    {
        // ... existing cases ...

        case GamePhase.YourNewPhase:
            TutorialText.Text = "Instructions for your new phase";
            // Update available chemicals, UI elements, etc.
            break;
    }
}
```

### Modifying Organism Behavior

**File:** `Engine/OrganismManager.cs`

```csharp
public void UpdateOrganisms(double deltaTime)
{
    foreach (var organism in _organisms.Where(o => o.Health > 0))
    {
        // Modify movement
        organism.Position = new Point(
            organism.Position.X + organism.Velocity.X * deltaTime,
            organism.Position.Y + organism.Velocity.Y * deltaTime
        );

        // Add custom behavior
        if (organism.Generation > 5)
        {
            // Older generations move faster
            organism.Velocity *= 1.2;
        }
    }
}
```

### Adding Visual Effects

**File:** `MainWindow.xaml.cs`

```csharp
private void CreateExplosionEffect(Point position)
{
    var effect = new Ellipse
    {
        Width = 0,
        Height = 0,
        Fill = new RadialGradientBrush(Colors.Orange, Colors.Transparent),
        Opacity = 1.0
    };

    Canvas.SetLeft(effect, position.X);
    Canvas.SetTop(effect, position.Y);
    EffectCanvas.Children.Add(effect);

    // Animate expansion
    var animation = new DoubleAnimation
    {
        From = 0,
        To = 100,
        Duration = TimeSpan.FromSeconds(0.5)
    };

    effect.BeginAnimation(Ellipse.WidthProperty, animation);
    effect.BeginAnimation(Ellipse.HeightProperty, animation);

    // Remove after animation
    Task.Delay(500).ContinueWith(_ =>
    {
        Dispatcher.Invoke(() => EffectCanvas.Children.Remove(effect));
    });
}
```

---

## Advanced Topics

### Custom Save Data

To add custom data to save files:

1. **Extend GameSave** (`Models/GameSave.cs`):
```csharp
public class GameSave
{
    // ... existing properties ...

    public Dictionary<string, int> CustomStats { get; set; }
}
```

2. **Update SaveManager** (`Engine/SaveManager.cs`):
```csharp
public void SaveGame(string savePath, GameManager gameManager)
{
    var save = new GameSave
    {
        // ... existing assignments ...
        CustomStats = new Dictionary<string, int>
        {
            ["TotalClicks"] = gameManager.TotalClicks
        }
    };

    // ... rest of save logic ...
}
```

### Event System Best Practices

**Raising Events:**
```csharp
public event EventHandler<GamePhase> PhaseChanged;

protected virtual void OnPhaseChanged(GamePhase newPhase)
{
    PhaseChanged?.Invoke(this, newPhase);  // Null-conditional operator
}
```

**Subscribing to Events:**
```csharp
// Subscribe
_gameManager.PhaseChanged += OnPhaseChanged;

// Unsubscribe (important for cleanup!)
_gameManager.PhaseChanged -= OnPhaseChanged;
```

### Multithreading Considerations

**UI Thread Access:**
```csharp
// Wrong: Accessing UI from background thread
Task.Run(() =>
{
    StatusText.Text = "This will crash!";  // InvalidOperationException
});

// Correct: Use Dispatcher
Task.Run(() =>
{
    Dispatcher.Invoke(() =>
    {
        StatusText.Text = "This works!";
    });
});
```

---

## Additional Resources

### Documentation
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [.NET 8.0 API Reference](https://docs.microsoft.com/en-us/dotnet/api/)
- [xUnit Testing Patterns](https://xunit.net/docs/getting-started/netcore/cmdline)

### Project Files
- [TDD_IMPLEMENTATION_REPORT.md](TDD_IMPLEMENTATION_REPORT.md) - Test coverage details
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Common issues and solutions
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines

### External Tools
- [WPF Inspector](https://github.com/snoopwpf/snoopwpf) - Runtime UI inspection
- [dotMemory](https://www.jetbrains.com/dotmemory/) - Memory profiling
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Performance benchmarking

---

**Questions or need help?** Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) or open an issue in the repository.
