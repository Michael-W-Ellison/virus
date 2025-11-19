# Contributing to BiochemSimulator

Thank you for your interest in contributing to BiochemSimulator! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Contribution Guidelines](#contribution-guidelines)
5. [Testing Requirements](#testing-requirements)
6. [Code Style](#code-style)
7. [Commit Messages](#commit-messages)
8. [Pull Request Process](#pull-request-process)
9. [Areas for Contribution](#areas-for-contribution)

---

## Code of Conduct

### Our Standards

- **Be respectful** and constructive in all interactions
- **Welcome newcomers** and help them learn
- **Focus on educational value** - this is a learning tool
- **Provide constructive feedback** on code and ideas
- **Respect different viewpoints** and experiences

### Unacceptable Behavior

- Harassment or discriminatory language
- Trolling or insulting comments
- Personal attacks
- Publishing others' private information
- Malicious code or intentional bugs

---

## Getting Started

### Prerequisites

Before contributing, ensure you have:

1. **.NET 8.0 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
2. **Git** for version control
3. **Visual Studio 2022** (recommended) or VS Code with C# extensions
4. Basic knowledge of **C#** and **WPF** (for UI contributions)

### Setting Up Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR-USERNAME/virus.git
   cd virus
   ```

3. **Add upstream remote:**
   ```bash
   git remote add upstream https://github.com/ORIGINAL-OWNER/virus.git
   ```

4. **Restore dependencies:**
   ```bash
   dotnet restore BiochemSimulator.sln
   ```

5. **Build the solution:**
   ```bash
   dotnet build BiochemSimulator.sln --configuration Debug
   ```

6. **Run tests to verify setup:**
   ```bash
   dotnet test BiochemSimulator.Tests/BiochemSimulator.Tests.csproj
   ```

   All 130+ tests should pass.

7. **Read the documentation:**
   - [README.md](README.md) - Project overview
   - [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) - Development documentation
   - [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Common issues

---

## Development Workflow

### 1. Create a Feature Branch

```bash
# Update your fork
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/your-feature-name
```

**Branch naming conventions:**
- `feature/` - New features (e.g., `feature/new-chemical-reactions`)
- `fix/` - Bug fixes (e.g., `fix/organism-rendering-issue`)
- `docs/` - Documentation updates (e.g., `docs/update-readme`)
- `test/` - Test additions (e.g., `test/add-molecule-tests`)
- `refactor/` - Code refactoring (e.g., `refactor/organism-manager`)

### 2. Make Your Changes

- Write clean, readable code
- Follow existing code style and conventions
- Add comments for complex logic
- Update documentation if needed

### 3. Write/Update Tests

**For new features:**
```csharp
[Fact]
public void NewFeature_Scenario_ExpectedBehavior()
{
    // Arrange
    var component = new YourComponent();

    // Act
    var result = component.NewMethod();

    // Assert
    Assert.Equal(expected, result);
}
```

**For bug fixes:**
- Add a test that reproduces the bug
- Verify test fails before fix
- Verify test passes after fix

### 4. Test Your Changes

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~YourTests"

# Run the application
dotnet run --project BiochemSimulator.csproj

# Test manually:
# - Verify new feature works as expected
# - Check for visual issues
# - Test edge cases
```

### 5. Commit Your Changes

```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "Add new chemical reaction system

- Implemented acid-base neutralization
- Added pH calculation logic
- Updated chemical database with 5 new compounds
- Added 10 tests for reaction validation"
```

### 6. Push and Create Pull Request

```bash
# Push to your fork
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub (see [Pull Request Process](#pull-request-process) below).

---

## Contribution Guidelines

### What to Contribute

**We welcome contributions in these areas:**

1. **New Chemical Reactions**
   - Real-world chemical reactions
   - Accurate pH values and properties
   - Visual effects for reactions

2. **Additional Chemicals**
   - Expand the chemical database
   - Include educational descriptions
   - Use accurate formulas

3. **Enhanced Visual Effects**
   - Improve organism rendering
   - Add animations
   - Better particle effects

4. **Performance Optimizations**
   - Reduce memory usage
   - Improve frame rate
   - Optimize rendering

5. **Bug Fixes**
   - Fix reported issues
   - Improve error handling
   - Resolve edge cases

6. **Test Coverage**
   - Add missing tests
   - Improve test quality
   - Increase coverage percentage

7. **Documentation**
   - Improve existing docs
   - Add code comments
   - Create tutorials

8. **Educational Content**
   - More realistic chemistry
   - Better tutorial messages
   - In-game help system

### What NOT to Contribute

- **Breaking changes** without discussion
- **Malicious code** or intentional bugs
- **Copyrighted content** without permission
- **Large refactors** without prior approval
- **Features outside project scope** (discuss first)

---

## Testing Requirements

### Required Tests

**All contributions must include tests** for:

- New features
- Bug fixes (regression tests)
- Modified logic

**Exceptions:**
- Pure documentation changes
- Visual-only changes (add manual test checklist)
- Build configuration updates

### Test Quality Standards

```csharp
// GOOD: Descriptive test name, clear AAA pattern
[Fact]
public void CalculateDamage_WithMaxResistance_ShouldDealMinimalDamage()
{
    // Arrange
    var organism = new Organism { Health = 100 };
    organism.Resistances["Bleach"] = 0.95; // 95% resistance (max)
    var chemical = new Chemical("Bleach", toxicity: 10.0);

    // Act
    double damage = _chemistryEngine.CalculateDamage(chemical, organism, distance: 0);

    // Assert
    Assert.True(damage <= 0.5, "Max resistance should reduce damage to ≤0.5");
}

// BAD: Unclear name, no AAA pattern, magic numbers
[Fact]
public void Test1()
{
    var o = new Organism();
    o.Resistances["X"] = 0.95;
    var d = _engine.Calc(new Chemical("X", 10), o, 0);
    Assert.True(d <= 0.5);
}
```

### Running Tests Locally

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "Category=Models"

# Verify all tests pass before submitting PR
```

---

## Code Style

### C# Conventions

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

**Key rules:**

1. **Naming:**
```csharp
// Classes, Methods, Properties: PascalCase
public class GameManager
{
    public void StartGame() { }
    public int Score { get; set; }
}

// Private fields: _camelCase with underscore
private GameState _currentState;

// Local variables, parameters: camelCase
void ProcessChemical(Chemical chemical)
{
    var reactionResult = ...;
}

// Constants: PascalCase
private const int MaxOrganisms = 500;
```

2. **Indentation:**
   - 4 spaces (not tabs)
   - Braces on new line (Allman style)

```csharp
// Correct
if (condition)
{
    DoSomething();
}

// Incorrect
if (condition) {
    DoSomething();
}
```

3. **File Organization:**
```csharp
// 1. Using statements (alphabetical, System first)
using System;
using System.Collections.Generic;
using BiochemSimulator.Models;

// 2. Namespace
namespace BiochemSimulator.Engine
{
    // 3. Class documentation
    /// <summary>
    /// Manages game state and phase progression.
    /// </summary>
    public class GameManager
    {
        // 4. Events
        public event EventHandler<GameState> StateChanged;

        // 5. Constants
        private const int DefaultScore = 0;

        // 6. Fields
        private GameState _currentState;

        // 7. Properties
        public GameState CurrentState { get; private set; }

        // 8. Constructor
        public GameManager() { }

        // 9. Public methods
        public void StartGame() { }

        // 10. Private methods
        private void UpdateState() { }
    }
}
```

4. **Comments:**
```csharp
// Single-line comments for brief explanations
var resistance = 0.95; // Cap at 95%

/// <summary>
/// XML documentation for public APIs
/// </summary>
/// <param name="chemical">The chemical to apply</param>
/// <returns>Damage dealt to organism</returns>
public double ApplyChemical(Chemical chemical)
{
    /* Multi-line comments for complex logic explanations
       Use when single-line comments are insufficient */
}
```

5. **LINQ Usage:**
```csharp
// Prefer method syntax for simple queries
var alive = organisms.Where(o => o.Health > 0).ToList();

// Use query syntax for complex queries
var grouped = from o in organisms
              where o.Health > 0
              group o by o.Generation into g
              select new { Generation = g.Key, Count = g.Count() };
```

### XAML Conventions

```xml
<!-- Indentation: 2 spaces -->
<Window>
  <Grid>
    <StackPanel>
      <!-- Property order: Name, common properties, layout, styling -->
      <Button Name="StartButton"
              Content="Start Game"
              Width="100"
              Height="30"
              Margin="10"
              Background="#2ECC71"
              Click="StartButton_Click" />
    </StackPanel>
  </Grid>
</Window>
```

---

## Commit Messages

### Format

```
Short summary (50 chars or less)

More detailed explanation (wrap at 72 chars). Explain what and why,
not how (code shows how).

- Bullet points are okay
- Use present tense ("Add feature" not "Added feature")
- Reference issues: "Fixes #123" or "Related to #456"
```

### Examples

**Good:**
```
Add acid-base neutralization reactions

Implement pH-based reaction system for acids and bases:
- Added CalculateNeutralization() method to ChemistryEngine
- Updated chemical database with pH values
- Created visual effect for exothermic neutralization
- Added 8 tests for reaction validation

Fixes #42
```

**Bad:**
```
Fixed stuff
```

```
Updated files
```

### Commit Types

Use these prefixes for clarity:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation only
- `test:` - Adding or updating tests
- `refactor:` - Code change that neither fixes a bug nor adds a feature
- `perf:` - Performance improvement
- `style:` - Code style changes (formatting, missing semi-colons, etc.)
- `build:` - Build system changes
- `chore:` - Maintenance tasks

**Examples:**
```
feat: Add potassium permanganate oxidizer
fix: Resolve organism rendering overlap issue
docs: Update DEVELOPER_GUIDE.md with new API
test: Add 15 tests for AtomicEngine bonding
refactor: Simplify damage calculation logic
perf: Optimize organism rendering for >200 entities
```

---

## Pull Request Process

### Before Submitting

**Checklist:**
- [ ] All tests pass locally (`dotnet test`)
- [ ] New tests added for new features/fixes
- [ ] Code follows style guidelines
- [ ] Documentation updated if needed
- [ ] Commit messages are descriptive
- [ ] No merge conflicts with main branch
- [ ] Build succeeds without warnings

### Creating the Pull Request

1. **Push your branch** to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Open Pull Request** on GitHub

3. **Fill out the PR template:**

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
Describe how you tested your changes:
- [ ] Unit tests added/updated
- [ ] Manual testing performed
- [ ] All tests pass

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review performed
- [ ] Comments added to complex code
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests added that prove fix/feature works

## Screenshots (if applicable)
Add screenshots for UI changes

## Related Issues
Fixes #(issue number)
```

### Review Process

1. **Automated checks** will run (build, tests)
2. **Maintainer review** of code quality and design
3. **Feedback** may be provided - address it by:
   ```bash
   # Make changes based on feedback
   git add .
   git commit -m "Address PR feedback: improve error handling"
   git push origin feature/your-feature-name
   ```
4. **Approval** and merge by maintainer

### After Merge

1. **Update your fork:**
   ```bash
   git checkout main
   git pull upstream main
   git push origin main
   ```

2. **Delete your feature branch** (optional):
   ```bash
   git branch -d feature/your-feature-name
   git push origin --delete feature/your-feature-name
   ```

---

## Areas for Contribution

### High Priority

1. **Full Workspace Serialization**
   - Save/load atoms, molecules, organisms with positions
   - Required for complete save game functionality
   - **Difficulty:** Medium-Hard
   - **Files:** `Models/GameSave.cs`, `Engine/SaveManager.cs`

2. **Sound Effects & Music**
   - Chemical reaction sounds
   - Background music
   - UI feedback sounds
   - **Difficulty:** Medium
   - **New dependencies:** NAudio or similar

3. **Achievement System Expansion**
   - More achievements (currently basic)
   - Achievement UI improvements
   - Steam-like achievement notifications
   - **Difficulty:** Easy-Medium
   - **Files:** `Engine/AchievementManager.cs`, `MainWindow.xaml.cs`

### Medium Priority

4. **Additional Chemical Reactions**
   - More organic chemistry
   - Enzyme reactions
   - Complex biochemical pathways
   - **Difficulty:** Easy-Medium
   - **Files:** `Engine/ChemistryEngine.cs`
   - **Reference:** Chemistry textbooks, databases

5. **Performance Optimizations**
   - Object pooling for visual elements
   - Spatial partitioning for organism collisions
   - Reduce GC pressure
   - **Difficulty:** Medium-Hard
   - **Files:** `MainWindow.xaml.cs`, `Engine/OrganismManager.cs`

6. **Enhanced Organism AI**
   - Flocking behavior
   - Avoidance of chemicals
   - Attraction to resources
   - **Difficulty:** Medium
   - **Files:** `Engine/OrganismManager.cs`, `Models/Organism.cs`

### Low Priority

7. **Difficulty Levels**
   - Easy, Normal, Hard modes
   - Adjust organism spawn rate, resistance development
   - **Difficulty:** Easy
   - **Files:** `Engine/GameManager.cs`, `Models/GameState.cs`

8. **Localization/Translation**
   - Support for multiple languages
   - Resource files for text
   - **Difficulty:** Medium
   - **New files:** Resources/*.resx

9. **Tutorial System**
   - Interactive tooltips
   - Step-by-step guided mode
   - Chemistry explanations
   - **Difficulty:** Medium
   - **Files:** `MainWindow.xaml.cs`, new Tutorial classes

### Good First Issues

Perfect for newcomers:

- Add new chemicals to database
- Improve error messages
- Fix typos in documentation
- Add unit tests for uncovered code
- Improve code comments
- Add validation to input fields

---

## Getting Help

### Resources

- **[DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md)** - Comprehensive development documentation
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Common issues and solutions
- **[README.md](README.md)** - Project overview and features

### Ask Questions

- Open a **GitHub Discussion** for general questions
- Comment on **existing issues** for context-specific questions
- **Draft a PR** early and ask for feedback

### Code Review

Don't be afraid of code review feedback - it's a learning opportunity!

- Reviews help maintain code quality
- Everyone's code gets reviewed, regardless of experience
- Focus on learning and improving

---

## Recognition

Contributors will be:
- Listed in project documentation
- Credited in release notes
- Acknowledged in achievement system (future feature)

Significant contributions may earn you:
- Collaborator status
- Direct commit access (after demonstrating quality)

---

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

## Questions?

If you have questions not covered here:

1. Check [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md)
2. Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
3. Open a GitHub Discussion
4. Ask in your Pull Request

---

**Thank you for contributing to BiochemSimulator!**

Your contributions help make this a better educational tool for everyone.

Happy coding!
