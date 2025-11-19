# BiochemSimulator Troubleshooting Guide

This guide covers common issues, error messages, and solutions for developing and running BiochemSimulator.

## Table of Contents
1. [Build Errors](#build-errors)
2. [Runtime Errors](#runtime-errors)
3. [Visual/UI Issues](#visualui-issues)
4. [Gameplay Issues](#gameplay-issues)
5. [Test Failures](#test-failures)
6. [Performance Issues](#performance-issues)
7. [Save/Load Issues](#saveload-issues)
8. [Platform-Specific Issues](#platform-specific-issues)

---

## Build Errors

### Error: CS0246 - Type or namespace name 'xUnit' could not be found

**Symptoms:**
```
error CS0246: The type or namespace name 'Fact' could not be found
error CS0246: The type or namespace name 'Assert' could not be found
281 similar errors
```

**Cause:** Test files are being compiled as part of the main project.

**Solution:**

1. Open `BiochemSimulator.csproj`
2. Add this exclusion block:
```xml
<ItemGroup>
  <!-- Exclude test project files from main project build -->
  <Compile Remove="BiochemSimulator.Tests\**" />
  <EmbeddedResource Remove="BiochemSimulator.Tests\**" />
  <None Remove="BiochemSimulator.Tests\**" />
  <Page Remove="BiochemSimulator.Tests\**" />
</ItemGroup>
```
3. Rebuild solution: `dotnet build BiochemSimulator.sln`

**Prevention:** Always use separate project files for tests and main code.

---

### Error: SDK 'Microsoft.NET.Sdk.WindowsDesktop' not found

**Symptoms:**
```
error MSB4236: The SDK 'Microsoft.NET.Sdk.WindowsDesktop' specified could not be found.
```

**Cause:** Using outdated project SDK for .NET 8.0.

**Solution:**

1. In `BiochemSimulator.csproj`, change:
```xml
<!-- Old (pre-.NET 5) -->
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

<!-- New (.NET 5+) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
</Project>
```

2. Rebuild: `dotnet build`

---

### Error: The name 'InitializeComponent' does not exist

**Symptoms:**
```
error CS0103: The name 'InitializeComponent' does not exist in the current context
```

**Cause:** XAML files not being compiled or designer files not generated.

**Solution:**

1. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

2. **Verify XAML files are included:**
   ```xml
   <ItemGroup>
     <Page Include="MainWindow.xaml" />
     <Page Include="ProfileSelectionWindow.xaml" />
   </ItemGroup>
   ```

3. **In Visual Studio:**
   - Right-click XAML file → Properties
   - Build Action should be "Page"

---

### Error: Assets file project.assets.json not found

**Symptoms:**
```
error : Assets file 'obj\project.assets.json' not found.
```

**Cause:** NuGet packages not restored.

**Solution:**
```bash
# Restore packages
dotnet restore BiochemSimulator.sln

# If that fails, delete bin and obj folders
rm -rf bin obj BiochemSimulator.Tests/bin BiochemSimulator.Tests/obj
dotnet restore BiochemSimulator.sln
dotnet build BiochemSimulator.sln
```

---

## Runtime Errors

### Exception: System.Windows.Markup.XamlParseException

**Symptoms:**
```
System.Windows.Markup.XamlParseException: 'Cannot create instance of 'MainWindow''
Inner exception: NullReferenceException
```

**Cause:** XAML code-behind initialization failure, often due to null fields.

**Solution:**

1. **Check constructor order:**
```csharp
public MainWindow()
{
    InitializeComponent();  // MUST be first

    // Then initialize fields
    _gameManager = new GameManager();
    _chemistryEngine = ChemistryEngine.Instance;
}
```

2. **Check for null XAML elements:**
```csharp
// Bad - assumes element exists
StatusText.Text = "Ready";  // Crashes if StatusText is null

// Good - check first
if (StatusText != null)
    StatusText.Text = "Ready";
```

---

### Exception: InvalidOperationException - Cross-thread operation

**Symptoms:**
```
System.InvalidOperationException: 'The calling thread cannot access this object because a different thread owns it.'
```

**Cause:** Accessing UI elements from background thread.

**Solution:**

Use `Dispatcher.Invoke()`:
```csharp
// Bad - direct access from Task
Task.Run(() =>
{
    StatusText.Text = "Done";  // Will crash!
});

// Good - use Dispatcher
Task.Run(() =>
{
    Dispatcher.Invoke(() =>
    {
        StatusText.Text = "Done";
    });
});
```

**Alternative:** Use `Dispatcher.BeginInvoke()` for non-blocking updates:
```csharp
Dispatcher.BeginInvoke(() =>
{
    StatusText.Text = "Done";
});
```

---

### Exception: FileNotFoundException when loading saves

**Symptoms:**
```
System.IO.FileNotFoundException: Could not find file 'C:\Users\...\save.json'
```

**Cause:** Save file path incorrect or file deleted.

**Solution:**

See [Save/Load Issues](#saveload-issues) section below.

---

### Exception: OutOfMemoryException with many organisms

**Symptoms:**
```
System.OutOfMemoryException: 'Insufficient memory to continue the execution of the program.'
```

**Cause:** Too many visual elements (500+ organisms with effects).

**Solution:**

1. **Limit maximum organisms:**
```csharp
// In OrganismManager.cs
private const int MaxOrganisms = 300;  // Reduce from 500

if (_organisms.Count >= MaxOrganisms)
{
    // Stop reproduction
    return;
}
```

2. **Cleanup dead organisms more frequently:**
```csharp
if (_frameCount % 30 == 0)  // Every 30 frames (was 60)
{
    _organisms.RemoveAll(o => o.Health <= 0);
}
```

3. **Disable expensive effects:**
```csharp
// Disable glow effects for high organism counts
if (_organisms.Count < 100)
{
    ApplyGlowEffect(organism);
}
```

---

## Visual/UI Issues

### Issue: Atoms appear transparent or invisible

**Symptoms:** Atoms on canvas are barely visible or completely transparent.

**Cause:** Opacity set too low (alpha < 150) or brush colors not configured.

**Solution:**

In `MainWindow.xaml.cs`, update `CreateAtomVisual()`:
```csharp
var ellipse = new Ellipse
{
    Fill = new RadialGradientBrush(
        atom.Color,
        Color.FromArgb(255, atom.Color.R, atom.Color.G, atom.Color.B)  // Alpha = 255 (full opacity)
    ),
    Opacity = 1.0,  // Full opacity
    StrokeThickness = 3,  // Thicker border
    Stroke = Brushes.White
};
```

**Verification:** Rebuild and run. Atoms should now be fully opaque.

---

### Issue: Dropdown text invisible (poor contrast)

**Symptoms:** Text in chemical inventory or atom list dropdowns is hard to read.

**Cause:** Text color too dark for dark background.

**Solution:**

In `MainWindow.xaml`, update ComboBox styles:
```xml
<Style x:Key="ChemicalButton" TargetType="Button">
    <!-- Add foreground color -->
    <Setter Property="TextElement.Foreground" Value="#D5DBDB"/>
    <!-- ... other setters ... -->
</Style>
```

For list items:
```xml
<TextBlock Text="{Binding Name}" Foreground="#D5DBDB" />
```

---

### Issue: Bonds between atoms not visible

**Symptoms:** After building molecule, atoms don't show connecting lines.

**Cause:** Bond lines not drawn or have wrong Z-order (behind atoms).

**Solution:**

In `DrawMoleculeBonds()`:
```csharp
var line = new Line
{
    X1 = bond.Atom1.Position.X,
    Y1 = bond.Atom1.Position.Y,
    X2 = bond.Atom2.Position.X,
    Y2 = bond.Atom2.Position.Y,
    Stroke = bondColor,
    StrokeThickness = 3,  // Increase from 1
    Opacity = 0.9,        // Increase from 0.5
    StrokeStartLineCap = PenLineCap.Round,
    StrokeEndLineCap = PenLineCap.Round
};

// Add to back so atoms appear on top
AtomicCanvas.Children.Insert(0, line);
```

**Color-Coded Bonds:**
```csharp
var bondColor = bond.Type switch
{
    BondType.Triple => Brushes.Cyan,
    BondType.Double => Brushes.LightGreen,
    BondType.Single => Brushes.White,
    _ => Brushes.White
};
```

---

### Issue: Window appears blank or all black

**Symptoms:** Application window shows no content or is completely black.

**Possible Causes:**
1. XAML parsing error
2. Graphics driver issue
3. Invalid canvas coordinates

**Solutions:**

1. **Check Output window for XAML errors:**
   - In Visual Studio: View → Output
   - Look for binding errors or missing resources

2. **Update graphics drivers:**
   - NVIDIA: https://www.nvidia.com/drivers
   - AMD: https://www.amd.com/support
   - Intel: https://www.intel.com/content/www/us/en/download-center

3. **Verify canvas bounds:**
```csharp
// Ensure positions are within canvas
if (x < 0 || x > AtomicCanvas.ActualWidth ||
    y < 0 || y > AtomicCanvas.ActualHeight)
{
    // Clamp to bounds
    x = Math.Max(0, Math.Min(x, AtomicCanvas.ActualWidth));
    y = Math.Max(0, Math.Min(y, AtomicCanvas.ActualHeight));
}
```

---

## Gameplay Issues

### Issue: Progression stuck after building water molecule

**Symptoms:** After building H₂O and getting achievement, no new instructions appear.

**Cause:** Phase progression not triggering or workspace not clearing.

**Solution:**

1. **Verify phase advancement** in `GameManager.cs`:
```csharp
public void CheckPhaseCompletion()
{
    if (_currentPhase == GamePhase.AminoAcids)
    {
        // Check if correct molecule was created
        if (moleculeName == "Water")
        {
            // Advance phase
            AdvancePhase(GamePhase.Proteins);
        }
    }
}
```

2. **Add 3-second delay before clearing:**
```csharp
Task.Delay(3000).ContinueWith(_ =>
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        ClearBeaker();
        ClearAtomWorkspace();
    });
});
```

3. **Clear status instructions:**
```csharp
AtomicStatusText.Text = $"✓ Created {molecule.Name}!\n" +
                        "➤ Next: Build another molecule";
```

---

### Issue: Building second molecule combines atoms from first

**Symptoms:** Creating two molecules sequentially results in second molecule containing atoms from both.

**Cause:** Atom workspace not cleared after building first molecule.

**Solution:**

In `BuildMolecule_Click()`:
```csharp
// After successfully building molecule:
// Clear the workspace to prevent next molecule from including these atoms
_gameManager.ClearAtomWorkspace();

// Clear atom visuals but keep bond visuals
foreach (var atomVisual in _atomVisuals.ToList())
{
    AtomicCanvas.Children.Remove(atomVisual.Ellipse);
    AtomicCanvas.Children.Remove(atomVisual.Text);
}
_atomVisuals.Clear();

// Reset workspace display
WorkspaceAtomsList.ItemsSource = null;
WorkspaceInstructions.Visibility = Visibility.Visible;
```

**Location:** `MainWindow.xaml.cs:1073-1087`

---

### Issue: Combine Molecules button does nothing

**Symptoms:** Clicking "Combine Molecules" shows message but no reaction occurs.

**Cause:** Either no reaction defined for those molecules, or incorrect error handling.

**Solution:**

Rewrite `CombineMolecules_Click()` with helpful feedback:
```csharp
private void CombineMolecules_Click(object sender, RoutedEventArgs e)
{
    if (_createdMolecules.Count < 2)
    {
        MessageBox.Show("You need at least 2 molecules to combine.", "Not Enough Molecules");
        return;
    }

    var reaction = _chemistryEngine.FindReaction(_createdMolecules);

    if (reaction != null)
    {
        // Success - reaction exists
        MessageBox.Show($"Reaction: {reaction.Description}", "Success!");
        // Clear workspace and show product
    }
    else
    {
        // No reaction - provide hints
        string hint = GetPhaseHint(_gameManager.CurrentPhase);
        MessageBox.Show(
            $"These molecules don't react.\n\n{hint}",
            "No Reaction",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}
```

---

### Issue: Organisms not appearing during virus outbreak

**Symptoms:** Biohazard alarm triggers but no organisms appear on screen.

**Cause:** Organisms spawning outside visible canvas or initialization failing.

**Solution:**

1. **Check canvas size:**
```csharp
System.Diagnostics.Debug.WriteLine($"Canvas size: {OrganismCanvas.ActualWidth} x {OrganismCanvas.ActualHeight}");
```

2. **Verify organism initialization:**
```csharp
private void InitializeOutbreak()
{
    _organismManager.InitializeOutbreak(
        initialCount: 10,
        screenWidth: (int)OrganismCanvas.ActualWidth,
        screenHeight: (int)OrganismCanvas.ActualHeight
    );

    var organisms = _organismManager.GetOrganisms();
    System.Diagnostics.Debug.WriteLine($"Spawned {organisms.Count} organisms");
}
```

3. **Check Z-order (organisms behind other elements):**
```xml
<!-- In MainWindow.xaml, ensure OrganismCanvas is last (on top) -->
<Canvas Name="BackgroundCanvas" />
<Canvas Name="AtomicCanvas" />
<Canvas Name="OrganismCanvas" Panel.ZIndex="100" />
```

---

### Issue: Organisms invulnerable to all chemicals

**Symptoms:** Spraying chemicals has no effect on organisms.

**Cause:** Damage calculation failing or organisms not being hit.

**Solution:**

1. **Debug chemical application:**
```csharp
private void ApplyChemical(Chemical chemical, Point position)
{
    var affected = _organismManager.ApplyChemical(chemical, position, radius: 50);
    System.Diagnostics.Debug.WriteLine($"Hit {affected} organisms with {chemical.Name}");
}
```

2. **Check damage calculation:**
```csharp
double damage = _chemistryEngine.CalculateDamage(
    chemical,
    organism,
    distance: distanceFromSpray
);

System.Diagnostics.Debug.WriteLine($"Damage to organism: {damage} (Resistance: {organism.Resistances[chemical.Name]*100}%)");
```

3. **Verify distance calculation:**
```csharp
double distance = Math.Sqrt(
    Math.Pow(organism.Position.X - sprayPosition.X, 2) +
    Math.Pow(organism.Position.Y - sprayPosition.Y, 2)
);

if (distance < radius)
{
    // Organism is within range
}
```

---

## Test Failures

### Test Failure: Organism resistance not capping at 95%

**Error:**
```
Expected: 0.95
Actual:   1.0
```

**Cause:** Resistance cap not enforced in `DevelopResistance()`.

**Solution:**

In `Organism.cs`:
```csharp
public void DevelopResistance(string chemicalName, double amount)
{
    if (!Resistances.ContainsKey(chemicalName))
        Resistances[chemicalName] = 0;

    Resistances[chemicalName] += amount;

    // Cap at 95%
    if (Resistances[chemicalName] > 0.95)
        Resistances[chemicalName] = 0.95;
}
```

---

### Test Failure: Molecule formula incorrect

**Error:**
```
Expected: "H2O"
Actual:   "OH2"
```

**Cause:** Formula not following element priority order (C, H, O, N).

**Solution:**

In `Molecule.cs`:
```csharp
public string Formula
{
    get
    {
        var elementCounts = Atoms
            .GroupBy(a => a.Symbol)
            .OrderBy(g => GetElementPriority(g.Key))
            .Select(g => g.Count() > 1 ? $"{g.Key}{g.Count()}" : g.Key);

        return string.Join("", elementCounts);
    }
}

private int GetElementPriority(string symbol)
{
    return symbol switch
    {
        "C" => 0,
        "H" => 1,
        "O" => 2,
        "N" => 3,
        _ => 4
    };
}
```

---

### Test Failure: xUnit tests not discovered

**Symptoms:** Test Explorer shows "No tests found" or 0 tests.

**Solution:**

1. **Rebuild test project:**
   ```bash
   dotnet build BiochemSimulator.Tests/BiochemSimulator.Tests.csproj
   ```

2. **Verify xUnit package version:**
   ```xml
   <PackageReference Include="xunit" Version="2.6.2" />
   <PackageReference Include="xunit.runner.visualstudio" Version="2.5.5" />
   <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
   ```

3. **In Visual Studio:**
   - Test → Test Explorer → Click "Refresh" icon
   - Test → Test Settings → Select "Keep Test Execution Engine Running"

---

## Performance Issues

### Issue: Low frame rate (< 30 FPS) during virus phase

**Cause:** Too many visual elements or expensive rendering operations.

**Solutions:**

1. **Reduce visual effects:**
```csharp
// Disable glow effects when > 100 organisms
if (_organisms.Count < 100)
{
    ApplyGlowEffect(organism);
}
```

2. **Use object pooling for visual elements:**
```csharp
private Queue<Ellipse> _ellipsePool = new Queue<Ellipse>();

private Ellipse GetPooledEllipse()
{
    if (_ellipsePool.Count > 0)
        return _ellipsePool.Dequeue();

    return new Ellipse();
}

private void ReturnToPool(Ellipse ellipse)
{
    OrganismCanvas.Children.Remove(ellipse);
    _ellipsePool.Enqueue(ellipse);
}
```

3. **Batch canvas updates:**
```csharp
// Update all organisms, then refresh canvas once
OrganismCanvas.Children.Clear();
foreach (var organism in aliveOrganisms)
{
    var visual = CreateOrganismVisual(organism);
    OrganismCanvas.Children.Add(visual);
}
```

4. **Reduce timer frequency:**
```csharp
_gameTimer.Interval = TimeSpan.FromMilliseconds(33);  // 30 FPS instead of 60
```

---

### Issue: High memory usage (> 2GB)

**Cause:** Memory leaks from event handlers or visual elements not being disposed.

**Solutions:**

1. **Unsubscribe from events:**
```csharp
// In window closing handler
protected override void OnClosed(EventArgs e)
{
    _gameManager.StateChanged -= OnStateChanged;
    _gameManager.PhaseChanged -= OnPhaseChanged;
    _gameTimer.Stop();
    _gameTimer.Tick -= GameLoop_Tick;

    base.OnClosed(e);
}
```

2. **Clear visual collections:**
```csharp
private void ClearOrganisms()
{
    foreach (var child in OrganismCanvas.Children.OfType<UIElement>().ToList())
    {
        OrganismCanvas.Children.Remove(child);
    }

    _organisms.Clear();
    _organismVisuals.Clear();
}
```

3. **Use weak event patterns for long-lived subscriptions:**
```csharp
WeakEventManager<GameManager, GameState>.AddHandler(
    _gameManager,
    nameof(_gameManager.StateChanged),
    OnStateChanged
);
```

---

## Save/Load Issues

### Issue: Loading saved game fails with FileNotFoundException

**Symptoms:**
```
Save file not found: C:\Users\...\BiochemSimulator\Saves\Player1\MySave.json
```

**Cause:** File path incorrect or save file deleted.

**Solution:**

Enhanced error handling in `LoadGame_Click()`:
```csharp
private void LoadGame_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var saveFilePath = GetSaveFilePath();

        // Check file exists before loading
        if (!System.IO.File.Exists(saveFilePath))
        {
            MessageBox.Show(
                $"Save file not found:\n{saveFilePath}\n\n" +
                "The save may have been deleted or moved.",
                "File Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        var gameSave = _saveManager.LoadGame(saveFilePath);

        if (gameSave != null)
        {
            // Restore state
            _gameManager.ChangeState(gameSave.CurrentState);

            MessageBox.Show(
                $"Game loaded successfully!\n" +
                $"State: {gameSave.CurrentState}\n" +
                $"Phase: {gameSave.CurrentPhase}",
                "Success"
            );
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading save: {ex.Message}", "Load Failed");
    }
}
```

---

### Issue: Save file corrupted or invalid JSON

**Symptoms:**
```
Newtonsoft.Json.JsonReaderException: 'Unexpected character encountered while parsing value'
```

**Cause:** Save file manually edited or corrupted during write.

**Solution:**

1. **Add JSON validation:**
```csharp
public GameSave LoadGame(string filePath)
{
    try
    {
        string json = File.ReadAllText(filePath);
        var save = JsonConvert.DeserializeObject<GameSave>(json);

        // Validate required fields
        if (save.SaveName == null || save.ProfileName == null)
        {
            throw new InvalidDataException("Save file missing required fields");
        }

        return save;
    }
    catch (JsonReaderException ex)
    {
        throw new InvalidDataException($"Corrupted save file: {ex.Message}");
    }
}
```

2. **Implement save file backup:**
```csharp
public void SaveGame(string savePath, GameSave save)
{
    // Create backup of existing save
    if (File.Exists(savePath))
    {
        File.Copy(savePath, savePath + ".backup", overwrite: true);
    }

    try
    {
        string json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText(savePath, json);

        // Delete backup on success
        File.Delete(savePath + ".backup");
    }
    catch
    {
        // Restore backup on failure
        if (File.Exists(savePath + ".backup"))
        {
            File.Copy(savePath + ".backup", savePath, overwrite: true);
        }
        throw;
    }
}
```

---

### Issue: Workspace not restored after loading save

**Symptoms:** Game state loads but atoms, molecules, and organisms don't appear.

**Cause:** Workspace serialization not implemented yet (known limitation).

**Current Status:** Partial implementation - only game state and phase are saved.

**Workaround:** Display message to user:
```csharp
MessageBox.Show(
    "Game loaded successfully!\n\n" +
    "Note: Full workspace restoration (atoms, molecules, organisms) " +
    "is coming in a future update.",
    "Partial Load",
    MessageBoxButton.OK,
    MessageBoxImage.Information
);
```

**Future Implementation:** See `DEVELOPER_GUIDE.md` → "Custom Save Data" section.

---

## Platform-Specific Issues

### Windows 11: Transparency effects not working

**Symptoms:** Organism overlays appear solid instead of translucent.

**Cause:** Windows 11 transparency effects disabled.

**Solution:**

1. **Check Windows Settings:**
   - Settings → Personalization → Colors
   - Enable "Transparency effects"

2. **Programmatic check:**
```csharp
// In MainWindow constructor
if (!SystemParameters.IsGlassEnabled)
{
    MessageBox.Show(
        "Transparency effects are disabled. Some visuals may appear solid.\n\n" +
        "Enable in Settings → Personalization → Colors → Transparency effects",
        "Visual Effects Disabled",
        MessageBoxButton.OK,
        MessageBoxImage.Information
    );
}
```

---

### High DPI Displays: UI elements appear tiny

**Symptoms:** Text and buttons too small on 4K or high-DPI displays.

**Cause:** DPI awareness not configured.

**Solution:**

Add to `App.xaml.cs`:
```csharp
protected override void OnStartup(StartupEventArgs e)
{
    // Enable per-monitor DPI awareness
    SetProcessDPIAware();

    base.OnStartup(e);
}

[System.Runtime.InteropServices.DllImport("user32.dll")]
private static extern bool SetProcessDPIAware();
```

Or add `app.manifest`:
```xml
<application xmlns="urn:schemas-microsoft-com:asm.v3">
  <windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </windowsSettings>
</application>
```

---

### Windows Defender: Application blocked or slow to start

**Symptoms:** Application takes 10+ seconds to launch or Windows Defender quarantines it.

**Cause:** False positive from Windows Defender SmartScreen.

**Solution:**

1. **Add exclusion:**
   - Windows Security → Virus & threat protection → Manage settings
   - Exclusions → Add exclusion → Folder
   - Select `virus/bin/Debug` or `virus/bin/Release`

2. **Sign your application** (for distribution):
```bash
# Requires code signing certificate
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com BiochemSimulator.exe
```

---

## Still Having Issues?

### Diagnostic Checklist

Before reporting a bug, try these steps:

1. **Clean and rebuild:**
   ```bash
   dotnet clean
   rm -rf bin obj */bin */obj
   dotnet build BiochemSimulator.sln
   ```

2. **Update .NET SDK:**
   ```bash
   dotnet --version  # Should be 8.0.x
   ```
   Download latest: https://dotnet.microsoft.com/download/dotnet/8.0

3. **Check for Windows updates:**
   - Settings → Windows Update → Check for updates

4. **Verify files are not corrupted:**
   ```bash
   git status
   git diff  # Check for unexpected changes
   ```

5. **Enable detailed logging:**
```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
    {
        File.WriteAllText("crash.log", ex.ExceptionObject.ToString());
    };

    base.OnStartup(e);
}
```

### Reporting Bugs

If the issue persists, please report it with:

1. **Error message** (full stack trace if available)
2. **Steps to reproduce**
3. **Expected vs. actual behavior**
4. **Environment info:**
   - Windows version (e.g., Windows 11 23H2)
   - .NET SDK version (`dotnet --version`)
   - Visual Studio version (if applicable)
5. **Screenshots or video** (if visual issue)

---

## Additional Resources

- [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) - Development documentation
- [TDD_IMPLEMENTATION_REPORT.md](TDD_IMPLEMENTATION_REPORT.md) - Test coverage
- [README.md](README.md) - User documentation
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

---

**Last Updated:** 2025-11-19
