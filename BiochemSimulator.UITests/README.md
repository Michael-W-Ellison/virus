# BiochemSimulator UI Tests

Automated UI tests for the BiochemSimulator WPF application using FlaUI.

## Overview

This test project provides automated UI testing capabilities using **FlaUI**, a .NET library that wraps the Windows UI Automation framework. These tests verify that the application's user interface works correctly from an end-user perspective.

## Requirements

- **Windows 10** or later (UI tests only run on Windows)
- **.NET 8.0 SDK**
- **Built application** (BiochemSimulator.exe must exist)

## Test Framework Stack

- **xUnit 2.6.2** - Test framework
- **FlaUI.UIA3 4.0.0** - UI Automation wrapper
- **Microsoft.NET.Test.Sdk** - Test platform

## Running the Tests

### Prerequisites

1. Build the main application first:
   ```bash
   dotnet build BiochemSimulator.csproj --configuration Debug
   ```

2. Ensure the application executable exists at:
   ```
   bin/Debug/net8.0-windows/BiochemSimulator.exe
   ```

### Running Tests

#### Command Line
```bash
# Run all UI tests
dotnet test BiochemSimulator.UITests/BiochemSimulator.UITests.csproj

# Run with detailed output
dotnet test BiochemSimulator.UITests/BiochemSimulator.UITests.csproj --logger "console;verbosity=detailed"

# Run specific test class
dotnet test BiochemSimulator.UITests/BiochemSimulator.UITests.csproj --filter "FullyQualifiedName~MainWindowTests"

# Run specific test
dotnet test BiochemSimulator.UITests/BiochemSimulator.UITests.csproj --filter "Application_ShouldLaunchSuccessfully"
```

#### Visual Studio
1. Open `BiochemSimulator.sln`
2. Open Test Explorer (Test > Test Explorer)
3. Run tests from the BiochemSimulator.UITests project

### Platform Detection

Tests are automatically skipped on non-Windows platforms. The custom attributes `[WindowsOnlyFact]` and `[WindowsOnlyTheory]` ensure tests only run on Windows:

```csharp
[WindowsOnlyFact]
public void MyTest()
{
    // This test will only run on Windows
}
```

## Test Categories

### MainWindowTests
- Application launch verification
- Menu bar existence
- File menu options (Save, Load, Exit)
- Window focus and responsiveness

### AtomicChemistryUITests
- Periodic table display
- Atom selection functionality
- Workspace area verification
- Build molecule button
- Hazard warnings accessibility

### SaveLoadUITests
- Save dialog functionality
- Save name input
- Load dialog
- Save file creation
- Saves list display

## Test Infrastructure

### UITestBase Class

The `UITestBase` class provides:

- **App lifecycle management**: `LaunchApplication()`, `Dispose()`
- **Window finding**: `WaitForWindow()`, `WaitForWindowToClose()`
- **Element finding**: `FindButton()`, `FindTextBox()`, `FindComboBox()`, `FindListBox()`
- **Profile handling**: `HandleProfileSelection()`
- **Utilities**: `TakeScreenshot()`, `CloseMessageBoxes()`

### Example Test

```csharp
public class MyUITests : UITestBase
{
    [WindowsOnlyFact]
    public void MyButton_ShouldBeClickable()
    {
        // Arrange
        LaunchApplication();
        HandleProfileSelection();

        // Act
        var button = FindButton("My Button");
        button?.Click();

        // Assert
        Assert.NotNull(button);
    }
}
```

## Debugging

### Screenshots
Failed tests can capture screenshots:
```csharp
TakeScreenshot("test_failure");
// Saved to: BiochemSimulator.UITests/bin/Debug/net8.0-windows/Screenshots/
```

### Timeouts
Default timeout is 10 seconds. Customize with:
```csharp
var element = WaitForElement(parent, cf => cf.ByName("Element"), TimeSpan.FromSeconds(30));
```

## Known Limitations

1. **Windows Only**: These tests cannot run on Linux or macOS
2. **Interactive Desktop**: Tests require an interactive Windows session (not headless CI)
3. **Screen Resolution**: Some tests may be sensitive to screen resolution
4. **Single Instance**: Only one instance of the application should run during tests

## CI/CD Integration

For Azure DevOps or GitHub Actions, ensure:
1. Windows runner/agent is used
2. Agent runs with interactive session (for UI automation)
3. Screen resolution is adequate (1024x768 minimum)

Example GitHub Actions workflow:
```yaml
jobs:
  ui-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Build Application
        run: dotnet build BiochemSimulator.csproj -c Debug
      - name: Run UI Tests
        run: dotnet test BiochemSimulator.UITests/BiochemSimulator.UITests.csproj
```

## Contributing

When adding new UI tests:
1. Inherit from `UITestBase`
2. Use `[WindowsOnlyFact]` or `[WindowsOnlyTheory]` attributes
3. Always call `LaunchApplication()` and `HandleProfileSelection()` in Arrange
4. Use helper methods from base class where possible
5. Clean up dialogs/message boxes after test actions
