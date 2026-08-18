using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FF14AccessibilityInstaller;

/// <summary>
/// Everything the Korean client needs that the global one does not. Kept in one
/// file so <see cref="InstallerService"/> keeps its shape and the Korean-only
/// facts stay findable.
///
/// Two things differ from the global setup, and both are structural rather than
/// cosmetic:
///
/// 1. The profile lives in %APPDATA%\XIVLauncherKR, not %APPDATA%\XIVLauncher.
///    The Korean launcher is a separate program that never touches the global
///    folder, so the two can coexist on one machine.
///
/// 2. Nothing creates that profile. On the global side XIVLauncher builds it on
///    first login; the Korean launcher does not, and the Korean Dalamud updater
///    states outright that it "is not a setup program for a brand new PC". So
///    three pieces have to be made by hand, and each one fails differently when
///    it is missing:
///
///      installedPlugins + devPlugins   -> updater spawns error windows forever
///      dalamudConfig.json              -> same
///      DALAMUD_RUNTIME                 -> updater reports success, but the CLR
///                                         never starts inside the game
///
///    The third is the nasty one: it looks like it worked.
/// </summary>
internal static class KrProfile
{
    /// <summary>
    /// Overrides <see cref="Root"/>. Two uses: a profile that was moved off the
    /// default location, and exercising the "nothing exists yet" path on a
    /// machine where everything already exists - that branch is the one a new
    /// user hits first, so it must not be the one nobody ever ran.
    ///
    /// Not read from the shell environment of the game, only of this process.
    /// </summary>
    public const string RootOverrideVariable = "FF14ACC_KR_PROFILE";

    /// <summary>
    /// Where the Korean Dalamud updater keeps its user settings. Documented in
    /// its own README-KR.txt, so this is a published location, not something
    /// read out of the binary.
    /// </summary>
    private const string UpdaterSettingsDir = "KrDalamudUpdater";
    private const string UpdaterSettingsName = "settings.json";
    private const string ProfileRootKey = "ProfileRoot";

    /// <summary>The updater's own default. We match it; we did not pick it.</summary>
    private const string DefaultFolder = "XIVLauncherKR";

    /// <summary>Korean profile root. Sibling of the global XIVLauncher folder.</summary>
    public static readonly string Root = ResolveRoot();

    /// <summary>
    /// The profile root, and it has to be the same folder the updater will look
    /// at afterwards.
    ///
    /// Getting this wrong does not raise anything. The updater reads its own
    /// setting, finds nothing there, and creates an empty profile it then
    /// injects - the game starts, Dalamud starts, and only the plugin is
    /// missing. So the setting is what we ask, not what we assume.
    ///
    /// Order:
    ///
    ///   1. FF14ACC_KR_PROFILE - our own escape hatch, so it wins outright
    ///   2. the updater's ProfileRoot, environment variables expanded
    ///   3. %APPDATA%\XIVLauncherKR - the same value the updater defaults to
    ///
    /// Step 2 is read-only. Writing into another program's settings file is what
    /// the vnavmesh rule forbids; reading a documented user setting is not, and
    /// hardcoding is in fact the more coupled of the two - it bets both that
    /// their default never moves and that the user never edits it.
    /// </summary>
    private static string ResolveRoot()
    {
        var overridden = Environment.GetEnvironmentVariable(RootOverrideVariable);
        if (!string.IsNullOrWhiteSpace(overridden))
            return overridden;

        return FromUpdaterSettings() ?? DefaultRoot;
    }

    // GetFolderPath, not %APPDATA%: the shell API is the authority, and the
    // variable can be missing or stale in a service/elevated context.
    private static string AppData =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static string DefaultRoot => Path.Combine(AppData, DefaultFolder);

    /// <summary>Path to the updater's settings file. It may not exist.</summary>
    public static string UpdaterSettingsPath =>
        Path.Combine(AppData, UpdaterSettingsDir, UpdaterSettingsName);

    /// <summary>
    /// ProfileRoot out of the updater's settings, or null when it is absent,
    /// unreadable or unusable. Someone else's broken file must not stop our
    /// installer, so every failure here is silent and falls through to the
    /// default.
    /// </summary>
    private static string? FromUpdaterSettings()
    {
        try
        {
            return ProfileRootFrom(File.ReadAllText(UpdaterSettingsPath), AppData);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// The parsing and validation, with nothing read from the machine. Split out
    /// because the branches that matter here are the ones that never happen on a
    /// working setup - no file, broken file, a value we must refuse - and a
    /// method that reaches for the real %APPDATA% cannot be driven into them.
    ///
    /// Returns null for anything unusable; the caller falls back.
    /// </summary>
    internal static string? ProfileRootFrom(string json, string appData)
    {
        string? value;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (parsed.RootElement.ValueKind != JsonValueKind.Object) return null;
            if (!parsed.RootElement.TryGetProperty(ProfileRootKey, out var property)) return null;
            if (property.ValueKind != JsonValueKind.String) return null;
            value = property.GetString();
        }
        catch (JsonException)
        {
            return null;
        }

        // The updater stores it literally as "%APPDATA%\XIVLauncherKR", and it
        // expands that against the environment - so we do too, even though our
        // own default comes from the shell API. Matching the updater is the
        // whole point; being independently correct is not.
        var expanded = Environment.ExpandEnvironmentVariables(value ?? string.Empty).Trim();
        return Usable(expanded, appData) ? expanded : null;
    }

    /// <summary>
    /// Values the updater itself rejects, so we reject them too: %APPDATA%
    /// itself and a bare drive root. Either one would treat somebody else's
    /// whole folder as the profile.
    /// </summary>
    internal static bool Usable(string candidate, string appData)
    {
        if (string.IsNullOrWhiteSpace(candidate)) return false;

        try
        {
            var full = Path.GetFullPath(candidate);
            if (Directory.GetParent(full) is null) return false;
            return !string.Equals(
                full.TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(appData).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Which of the three decided <see cref="Root"/>. Goes into the installer
    /// log: when the plugin ends up in the wrong folder, this is the first
    /// question, and the GUI cannot be inspected by the people who need it.
    ///
    /// Deliberately a path or a variable name rather than a sentence, so it
    /// reads the same in every language.
    /// </summary>
    public static string RootSource()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(RootOverrideVariable)))
            return RootOverrideVariable;
        return FromUpdaterSettings() is not null ? UpdaterSettingsPath : DefaultFolder;
    }

    public static readonly string DevPluginsRoot = Path.Combine(Root, "devPlugins");
    public static readonly string InstalledPluginsRoot = Path.Combine(Root, "installedPlugins");
    public static readonly string ConfigPath = Path.Combine(Root, "dalamudConfig.json");
    public static readonly string LogPath = Path.Combine(Root, "dalamud-kr-gui.log");

    /// <summary>Where the Korean Dalamud updater installs itself by default.</summary>
    public static readonly string UpdaterPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KR-Dalamud-Updater", "app", "Dalamud.Updater.exe");

    /// <summary>
    /// Minimal seed for dalamudConfig.json. Dalamud fills in every other default
    /// on its first run; this only has to be parseable and carry the $type so the
    /// deserializer accepts it.
    /// </summary>
    private const string ConfigSeed =
        "{\"$type\":\"Dalamud.Configuration.Internal.DalamudConfiguration, Dalamud\"}";

    /// <summary>True once the updater has produced a hook folder, i.e. Dalamud is present.</summary>
    public static bool DalamudInstalled => Directory.Exists(Path.Combine(Root, "addon", "Hooks"));

    /// <summary>
    /// Creates the pieces the Korean launcher never creates. Idempotent - every
    /// piece is checked before it is written, so running the installer twice is safe.
    /// Returns the list of things it actually had to create, for the log.
    /// </summary>
    public static List<string> Bootstrap()
    {
        var created = new List<string>();

        Directory.CreateDirectory(Root);

        if (!Directory.Exists(InstalledPluginsRoot))
        {
            Directory.CreateDirectory(InstalledPluginsRoot);
            created.Add("installedPlugins");
        }

        if (!Directory.Exists(DevPluginsRoot))
        {
            Directory.CreateDirectory(DevPluginsRoot);
            created.Add("devPlugins");
        }

        if (!File.Exists(ConfigPath))
        {
            // No BOM. Dalamud silently falls back to an old SQLite copy when it
            // finds one, which looks like "the settings did not stick".
            File.WriteAllText(ConfigPath, ConfigSeed, new UTF8Encoding(false));
            created.Add("dalamudConfig.json");
        }

        return created;
    }

    /// <summary>
    /// Points DALAMUD_RUNTIME at the system .NET install if it is not set yet.
    /// Dalamud.Boot reads this to find the runtime it loads into the game; the
    /// global side gets it from XIVLauncher's private runtime.
    ///
    /// Returns null when nothing had to change, otherwise the value that was set.
    /// The variable lands in the user environment, so the game has to be started
    /// fresh afterwards - a process inherits its environment at launch.
    /// </summary>
    public static string? EnsureRuntimeVariable()
    {
        var existing = Environment.GetEnvironmentVariable("DALAMUD_RUNTIME", EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(existing) && Directory.Exists(existing))
            return null;

        var dotnetRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
        if (!Directory.Exists(dotnetRoot))
            return null;

        Environment.SetEnvironmentVariable("DALAMUD_RUNTIME", dotnetRoot, EnvironmentVariableTarget.User);
        return dotnetRoot;
    }

    /// <summary>
    /// Finds the plugin archive built from this checkout. The global installer
    /// downloads a GitHub release instead, but that binary is compiled against
    /// FFXIVClientStructs 7.55 and the Korean Dalamud pins 7.51 - it would load
    /// and then throw on the first gearset call. So the Korean installer ships
    /// what was built here, or nothing.
    ///
    /// Two places, in this order:
    ///
    ///   1. next to the EXE - that is the shipping shape, two files in one folder
    ///   2. walking up to the build output - that is the developer shape
    ///
    /// Order matters. A shipped zip sitting next to the EXE is what the user
    /// chose to install; a stale build tree further up is not.
    /// </summary>
    public static string? FindLocalBuild()
    {
        foreach (var name in new[] { "FF14Accessibility.zip", "latest.zip" })
        {
            var beside = Path.Combine(AppContext.BaseDirectory, name);
            if (File.Exists(beside)) return beside;
        }

        var relative = Path.Combine(
            "FF14Accessibility", "bin", "Release", "net10.0-windows", "FF14Accessibility", "latest.zip");

        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 8 && dir != null; i++)
        {
            var candidate = Path.Combine(dir, relative);
            if (File.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
        }
        return null;
    }

    /// <summary>Opens the Korean Dalamud updater if it is installed. Returns false if it is not.</summary>
    public static bool TryLaunchUpdater()
    {
        if (!File.Exists(UpdaterPath)) return false;
        try
        {
            Process.Start(new ProcessStartInfo(UpdaterPath, "--no-elevate") { UseShellExecute = true });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
