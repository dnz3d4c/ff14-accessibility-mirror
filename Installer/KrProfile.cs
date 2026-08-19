using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    // ── The Korean Dalamud updater ────────────────────────────────────────
    //
    // These five constants are mirrored in tools/kr-setup/kr_profile.py and a
    // test fails if the two drift apart. They decide where the updater is
    // fetched from and where it lands; if the installer unpacks it somewhere
    // other than UpdaterPath, it downloads the thing and then reports it
    // missing.

    /// <summary>Latest release of the Korean Dalamud updater. No pinned tag - we
    /// ask for whatever is current, the same way the vnavmesh step does.</summary>
    public const string UpdaterReleaseApi =
        "https://api.github.com/repos/MiqoKR/kr-dalamud-updater/releases/latest";

    /// <summary>Which of the release assets to take. The release also carries a
    /// "Payload" zip, which is what the updater uses to update itself - it has no
    /// executable in it, and taking it would extract cleanly and leave nothing
    /// runnable behind.</summary>
    public const string UpdaterAssetMarker = "Portable";

    /// <summary>Under LocalApplicationData: no elevation needed, and it satisfies
    /// the "ordinary writable folder" the updater's own README-KR.txt asks for.</summary>
    public const string UpdaterInstallFolder = "KR-Dalamud-Updater";
    public const string UpdaterAppFolder = "app";
    public const string UpdaterExeName = "Dalamud.Updater.exe";

    /// <summary>Where the release zip is unpacked. The archive is flat - the exe,
    /// README-KR.txt and UpdaterReleaseConfig.json all sit at its root - and
    /// UpdaterReleaseConfig.json has to stay next to the exe for the updater's
    /// self-update to work, so the three are never split apart.</summary>
    public static readonly string UpdaterExtractDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        UpdaterInstallFolder, UpdaterAppFolder);

    /// <summary>Where the Korean Dalamud updater installs itself by default.</summary>
    public static readonly string UpdaterPath = Path.Combine(UpdaterExtractDir, UpdaterExeName);

    /// <summary>
    /// The release page a person can open, as opposed to
    /// <see cref="UpdaterReleaseApi"/>, which only a program can use. Printed
    /// when fetching the updater failed: without a place to go, "it has to be
    /// fetched by hand" is not an instruction.
    /// </summary>
    public const string UpdaterReleasePage =
        "https://github.com/MiqoKR/kr-dalamud-updater/releases/latest";

    /// <summary>True once the updater's executable is on disk. Distinct from
    /// <see cref="DalamudInstalled"/>: having the updater is our problem to solve,
    /// having Dalamud needs the user to press Check Update in its window.</summary>
    public static bool UpdaterInstalled => File.Exists(UpdaterPath);

    /// <summary>
    /// Seed for dalamudConfig.json. Dalamud fills in every other default on its
    /// first run, but three containers have to be here from the start, because
    /// the installer runs against this file BEFORE Dalamud has ever written it.
    ///
    /// The order that makes this necessary is the documented one: the user
    /// presses Check Update in the updater, which puts addon\Hooks on disk, and
    /// then runs the installer again - without having started the game. At that
    /// moment Dalamud looks installed and this file is still whatever we wrote.
    /// A seed carrying only $type made PatchDalamudConfig refuse to touch it
    /// ("unexpected structure"), so the very first successful install reported a
    /// failure, and vnavmesh never got DevMode either.
    ///
    ///   DevPluginLoadLocations  -> PatchDalamudConfig bails without it
    ///   DefaultProfile          -> nothing can be enabled without it
    ///   ThirdRepoList           -> our repository has nowhere to be registered
    ///
    /// The shapes are not invented. They were read out of a dalamudConfig.json
    /// that Dalamud itself wrote, down to the type names and the short keys of
    /// ProfileModelV1 - "e" (enabled) is true and "n" is DEFAULT there, and a
    /// profile seeded without them would deserialize as a disabled profile.
    /// </summary>
    private const string ConfigSeed =
        "{\"$type\":\"Dalamud.Configuration.Internal.DalamudConfiguration, Dalamud\"," +
        "\"DevPluginLoadLocations\":{" +
            "\"$type\":\"System.Collections.Generic.List`1[[Dalamud.Configuration.DevPluginLocationSettings, Dalamud]], System.Private.CoreLib\"," +
            "\"$values\":[]}," +
        "\"ThirdRepoList\":{" +
            "\"$type\":\"System.Collections.Generic.List`1[[Dalamud.Configuration.ThirdPartyRepoSettings, Dalamud]], System.Private.CoreLib\"," +
            "\"$values\":[]}," +
        "\"DefaultProfile\":{" +
            "\"$type\":\"Dalamud.Plugin.Internal.Profiles.ProfileModelV1, Dalamud\"," +
            "\"p\":null,\"e4c\":false," +
            "\"pc\":{" +
                "\"$type\":\"System.Collections.Generic.List`1[[Dalamud.Plugin.Internal.Profiles.ProfileModelV1+ProfileModelV1Character, Dalamud]], System.Private.CoreLib\"," +
                "\"$values\":[]}," +
            "\"e\":true,\"c\":0," +
            "\"Plugins\":{" +
                "\"$type\":\"System.Collections.Generic.List`1[[Dalamud.Plugin.Internal.Profiles.ProfileModelV1+ProfileModelV1Plugin, Dalamud]], System.Private.CoreLib\"," +
                "\"$values\":[]}," +
            "\"id\":\"00000000-0000-0000-0000-000000000000\",\"n\":\"DEFAULT\"}}";

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
        else
        {
            created.AddRange(AddMissingContainers());
        }

        return created;
    }

    /// <summary>
    /// Adds the containers of <see cref="ConfigSeed"/> that an existing
    /// dalamudConfig.json does not have.
    ///
    /// Fixing the seed alone would not have been enough: a profile written by an
    /// earlier version of this installer already has the file, so the seed is
    /// never written again and the file stays without them. That is the same
    /// dead end, reached by upgrading instead of by installing.
    ///
    /// Only ever adds, and only what is missing. A container being absent means
    /// Dalamud has not written this file yet - it writes all of them, always - so
    /// there is no case where this overwrites somebody else's value.
    /// </summary>
    private static List<string> AddMissingContainers()
    {
        var added = new List<string>();
        try
        {
            var text = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            if (JsonNode.Parse(text) is not JsonObject config) return added;
            if (JsonNode.Parse(ConfigSeed) is not JsonObject seed) return added;

            foreach (var (key, value) in seed)
            {
                if (value is not JsonObject container || config.ContainsKey(key)) continue;
                config[key] = container.DeepClone();
                added.Add("dalamudConfig.json: " + key);
            }

            if (added.Count > 0)
                File.WriteAllText(ConfigPath, config.ToJsonString(), new UTF8Encoding(false));
        }
        catch (Exception)
        {
            // A config we cannot read is PatchDalamudConfig's problem to report,
            // with a message written for it. Failing the whole bootstrap here
            // would replace that message with a worse one.
            return added;
        }
        return added;
    }

    /// <summary>How <see cref="EnsureRuntimeVariable"/> left DALAMUD_RUNTIME.</summary>
    public enum RuntimeState
    {
        /// <summary>Already pointing at a folder that exists. Nothing to do.</summary>
        AlreadySet,

        /// <summary>Just pointed at the system .NET install.</summary>
        JustSet,

        /// <summary>There is no .NET install to point it at.</summary>
        DotnetMissing,
    }

    /// <summary>
    /// Points DALAMUD_RUNTIME at the system .NET install if it is not set yet.
    /// Dalamud.Boot reads this to find the runtime it loads into the game; the
    /// global side gets it from XIVLauncher's private runtime.
    ///
    /// Returns the state and the folder it concerns. The two failures used to be
    /// one: "already fine" and "no .NET to point at" both returned null, and the
    /// caller only had a message for the success. So the third failure this class
    /// warns about at the top - the one that looks like it worked - was also the
    /// one that produced no line anywhere.
    ///
    /// The variable lands in the user environment, so the game has to be started
    /// fresh afterwards - a process inherits its environment at launch.
    /// </summary>
    public static (RuntimeState State, string Folder) EnsureRuntimeVariable()
    {
        var dotnetRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");

        var existing = Environment.GetEnvironmentVariable("DALAMUD_RUNTIME", EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(existing) && Directory.Exists(existing))
            return (RuntimeState.AlreadySet, existing);

        if (!Directory.Exists(dotnetRoot))
            return (RuntimeState.DotnetMissing, dotnetRoot);

        Environment.SetEnvironmentVariable("DALAMUD_RUNTIME", dotnetRoot, EnvironmentVariableTarget.User);
        return (RuntimeState.JustSet, dotnetRoot);
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

    /// <summary>
    /// Opens the Korean Dalamud updater if it is installed. Returns false if it is not.
    ///
    /// Started without arguments, and that is the point. It used to be handed
    /// "--no-elevate", which pins the updater to normal rights. The Korean client
    /// runs elevated, so the injector's OpenProcess answered "access denied"
    /// (Win32 error 5) every single time and nothing upstream of that said why.
    /// The updater asks for elevation on its own when it needs it.
    /// </summary>
    public static bool TryLaunchUpdater()
    {
        if (!File.Exists(UpdaterPath)) return false;
        try
        {
            Process.Start(new ProcessStartInfo(UpdaterPath) { UseShellExecute = true });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
