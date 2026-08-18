using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace FF14AccessibilityInstaller;

/// <summary>
/// Barrier-free installer/updater for the FF14 Accessibility plugin (and the
/// vnavmesh pathfinding plugin it uses for auto-walk). Extracted from the
/// original console version, now driven from a WinForms GUI.
///
/// Why the DevPlugin route: Dalamud's own plugin installer is an ImGui overlay
/// that screen readers cannot read, so a blind user cannot use it. This tool
/// instead copies the plugin DLLs into Dalamud's devPlugins folder and enables
/// them directly in dalamudConfig.json, so Dalamud auto-loads them on the next
/// game start - no ImGui click required.
///
/// All status text goes through <see cref="LogMessage"/>, which the GUI writes
/// into a focusable, read-only, multi-line log textbox (screen-reader friendly).
/// Yes/No questions go through <see cref="AskYesNo"/>, which the GUI answers via
/// a standard MessageBox (also read aloud automatically by screen readers).
/// </summary>
public sealed partial class InstallerService
{
    private const string AccessibilityInternalName = "FF14Accessibility";
    private const string VnavmeshInternalName = "vnavmesh";

    private const string AccessibilityRepoOwner = "derbruedi";
    private const string AccessibilityRepoName = "ff14-accessibility";
    private const string VnavmeshRepositoryJsonUrl = "https://puni.sh/api/repository/veyn";

    // Selbst-Update: im KR-Build inaktiv (RunAsync ruft es nicht auf), weil es
    // noch keinen Release-Kanal gibt. Der Apparat bleibt aber stehen - ihn zu
    // loeschen macht jedes Rebase auf einen neuen Upstream-Stand teurer, und
    // sobald ein Kanal existiert, ist es ein Einzeiler in RunAsync.
    private const string InstallerManifestAssetName = "installer.json";
    private const string InstallerExeAssetName = "FF14AccessibilityInstaller.exe";

    // Korean build: the profile is XIVLauncherKR and nothing creates it for us.
    // See KrProfile for why each piece matters.
    private static readonly string XivLauncherRoot = KrProfile.Root;
    private static readonly string DevPluginsRoot = KrProfile.DevPluginsRoot;
    private static readonly string DalamudConfigPath = KrProfile.ConfigPath;

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(10) };

    /// <summary>Raised for every status line. The GUI appends these to the log textbox.</summary>
    public event Action<string>? LogMessage;

    /// <summary>Ja/Nein-Rückfrage. Die GUI beantwortet das über eine MessageBox
    /// (synchron auf dem UI-Thread - RunAsync wird ohne ConfigureAwait(false)
    /// aufgerufen, daher laufen alle Fortsetzungen auf dem UI-Thread).</summary>
    public Func<string, bool>? AskYesNo { get; set; }

    /// <summary>Wird ausgelöst, wenn ein Selbst-Update läuft und sich dieser
    /// Prozess gleich beenden muss. Die GUI schließt daraufhin das Fenster.</summary>
    public event Action? RestartRequested;

    public InstallerService()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("FF14AccessibilityInstaller/1.0");
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    private void Log(string message) => LogMessage?.Invoke(message);
    private void Info(string message) => Log(message);
    private void Warn(string message) => Log(Loc.Get("WarnPrefix") + message);
    private void Error(string message) => Log(Loc.Get("ErrorPrefix") + message);

    /// <summary>Führt den kompletten Installations-/Update-Ablauf aus. Ein einziger
    /// Codepfad für Erstinstallation und Update (siehe Architektur-Doc Abschnitt 4.1).
    /// Gibt true zurück, wenn ein Selbst-Update eingeleitet wurde und sich der
    /// Prozess gleich beendet - der Aufrufer zeigt dann keine Abschlussmeldung.</summary>
    public async Task<bool> RunAsync()
    {
        var ownVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Loc.Get("UnknownVersion");
        Info(Loc.Get("InstallerHeader", ownVersion));
        Info(string.Empty);

        // Kein Selbst-Update im KR-Build: es gibt (noch) keinen Release-Kanal,
        // aus dem sich dieser Installer beziehen koennte. Der Aufruf bliebe sonst
        // bei jedem Start an GitHub haengen und meldete nichts Brauchbares.

        try
        {
            Info(Loc.Get("KrCheckingProfile"));
            if (!PrepareKrProfile())
                return false; // Dalamud fehlt - der Nutzer muss erst den Updater laufen lassen.
            Info(string.Empty);

            var accResult = await UpdateAccessibilityPluginAsync(ownVersion);
            Info(string.Empty);
            var vnavResult = await UpdateVnavmeshAsync();
            Info(string.Empty);
            var patchResult = PatchDalamudConfig();

            Info(string.Empty);
            Info(Loc.Get("SummaryHeader"));
            Info(Loc.Get("SummaryAccessibility", accResult));
            Info(Loc.Get("SummaryVnavmesh", vnavResult));
            Info(patchResult);
        }
        catch (Exception ex)
        {
            Error(Loc.Get("UnexpectedError", ex.Message));
            Error(Loc.Get("NoPartialWrite"));
        }
        return false;
    }

    // ── KR-Profil ──────────────────────────────────────────────────────────

    /// <summary>
    /// Baut die Teile des Profils, die der koreanische Launcher nicht baut, und
    /// prueft danach, ob Dalamud ueberhaupt da ist. Gibt false zurueck, wenn der
    /// Nutzer erst den KR-Updater laufen lassen muss.
    ///
    /// Der globale Installer laedt an dieser Stelle XIVLauncher herunter. Das
    /// geht hier nicht: den koreanischen Client bedient ein eigener Launcher,
    /// und Dalamud kommt aus einer fremden Patch-Pipeline, die wir bewusst nicht
    /// weiterverteilen.
    /// </summary>
    private bool PrepareKrProfile()
    {
        // Immer nennen, auch wenn alles stimmt. Landet das Plugin im falschen
        // Ordner, meldet nichts einen Fehler - der Updater legt sein eigenes
        // leeres Profil an und injiziert das. Dann ist diese Zeile die einzige
        // Spur, und die GUI koennen die Betroffenen nicht nachsehen.
        Info(Loc.Get("KrProfileRoot", KrProfile.Root, KrProfile.RootSource()));

        var created = KrProfile.Bootstrap();
        if (created.Count > 0)
            Info(Loc.Get("KrProfileCreated", string.Join(", ", created)));
        else
            Info(Loc.Get("KrProfileFound"));

        var runtime = KrProfile.EnsureRuntimeVariable();
        if (runtime != null)
        {
            Info(Loc.Get("KrRuntimeVariableSet", runtime));
            Info(Loc.Get("KrRuntimeNeedsRestart"));
        }

        if (KrProfile.DalamudInstalled)
            return true;

        Warn(Loc.Get("KrDalamudMissing"));
        Info(Loc.Get("KrDalamudGetIt"));
        Info("  " + KrProfile.UpdaterPath);
        Info(Loc.Get("KrDalamudThenCheckUpdate"));
        return false;
    }

    // ── Eigenes Plugin (FF14Accessibility) ────────────────────────────────

    /// <summary>
    /// Installiert das Plugin aus dem lokalen Build statt aus einem GitHub-Release.
    ///
    /// Der Grund ist nicht Bequemlichkeit: das Release-Binary des Upstreams ist
    /// gegen FFXIVClientStructs 7.55 gebunden, das koreanische Dalamud liefert
    /// 7.51. Es wuerde laden und beim ersten Gearset-Aufruf werfen. Was hier
    /// installiert wird, muss also aus diesem Checkout kommen.
    /// </summary>
    private Task<string> UpdateAccessibilityPluginAsync(string ownVersion)
    {
        Info(Loc.Get("KrLookingForLocalBuild"));

        var zipPath = KrProfile.FindLocalBuild();
        if (zipPath == null)
        {
            Warn(Loc.Get("KrNoLocalBuild"));
            Info(Loc.Get("KrBuildHint"));
            return Task.FromResult(Loc.Get("KrErrorNoLocalBuild"));
        }

        var targetDir = Path.Combine(DevPluginsRoot, AccessibilityInternalName);
        var manifestPath = Path.Combine(targetDir, AccessibilityInternalName + ".json");
        var localVersion = ReadLocalManifestVersion(manifestPath);
        var wasInstalled = localVersion != null;

        string extractDir = Path.Combine(Path.GetTempPath(), "FF14AccExtract_" + Guid.NewGuid());
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractDir);

            // Version aus dem frisch entpackten Manifest, nicht aus dem Dateinamen -
            // der Packager schreibt sie dort hinein und nur die stimmt.
            var builtVersion =
                ReadLocalManifestVersion(Path.Combine(extractDir, AccessibilityInternalName + ".json"))
                ?? Loc.Get("UnknownVersion");

            Info(Loc.Get("KrUsingLocalBuild", builtVersion, zipPath));
            DeployPluginFiles(extractDir, targetDir);

            Info(wasInstalled
                ? Loc.Get("AccessibilityUpdated", builtVersion)
                : Loc.Get("AccessibilityInstalled", builtVersion));
            return Task.FromResult(wasInstalled
                ? Loc.Get("UpdatedToShort", builtVersion)
                : Loc.Get("NewlyInstalledShort", builtVersion));
        }
        catch (IOException ex)
        {
            Error(Loc.Get("CouldNotWritePluginFiles", ex.Message));
            Error(Loc.Get("CloseGameAndLauncher"));
            return Task.FromResult(Loc.Get("ErrorFilesLocked"));
        }
        catch (Exception ex)
        {
            Error(Loc.Get("AccessibilityUnexpectedError", ex.Message));
            return Task.FromResult(Loc.Get("ErrorGeneric"));
        }
        finally
        {
            TryDeleteDirectory(extractDir);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch (Exception)
        {
            // Aufraeumen darf die Installation nicht scheitern lassen.
        }
    }

    // ── vnavmesh (Auto-Lauf) ───────────────────────────────────────────────

    private async Task<string> UpdateVnavmeshAsync()
    {
        Info(Loc.Get("CheckingVnavmeshVersion"));

        JsonNode? entry;
        try
        {
            var json = await _http.GetStringAsync(VnavmeshRepositoryJsonUrl);
            var arr = JsonNode.Parse(json)?.AsArray();
            entry = null;
            if (arr != null)
            {
                foreach (var e in arr)
                {
                    if (string.Equals(e?["InternalName"]?.GetValue<string>(), VnavmeshInternalName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        entry = e;
                        break;
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Warn(Loc.Get("VnavmeshPunishUnreachable", ex.Message));
            return Loc.Get("ErrorNoNetworkPunish");
        }
        catch (TaskCanceledException)
        {
            Warn(Loc.Get("VnavmeshPunishTimeout"));
            return Loc.Get("ErrorTimeout");
        }

        if (entry == null)
        {
            Warn(Loc.Get("VnavmeshNotFound"));
            return Loc.Get("ErrorNotFound");
        }

        var remoteVersion = entry["AssemblyVersion"]?.GetValue<string>() ?? Loc.Get("UnknownVersion");
        var downloadUrl = entry["DownloadLinkInstall"]?.GetValue<string>();
        if (string.IsNullOrEmpty(downloadUrl))
        {
            Warn(Loc.Get("VnavmeshNoDownloadLink"));
            return Loc.Get("ErrorNoDownloadLink");
        }

        var targetDir = Path.Combine(DevPluginsRoot, VnavmeshInternalName);
        var manifestPath = Path.Combine(targetDir, VnavmeshInternalName + ".json");
        var localVersion = ReadLocalManifestVersion(manifestPath);

        if (localVersion != null && !IsNewer(remoteVersion, localVersion))
        {
            Info(Loc.Get("VnavmeshUpToDate", localVersion));
            return Loc.Get("UpToDateShort", localVersion);
        }

        if (localVersion == null)
        {
            Info(string.Empty);
            Info(Loc.Get("AutoWalkNeedsVnav1"));
            Info(Loc.Get("AutoWalkNeedsVnav2"));
            Info(Loc.Get("AutoWalkNeedsVnav3"));
            var yes = AskYesNo?.Invoke(Loc.Get("AskSetupVnavmesh")) ?? false;
            if (!yes)
            {
                Info(Loc.Get("VnavmeshSkipped"));
                return Loc.Get("SkippedShort");
            }
        }

        try
        {
            Info(Loc.Get("DownloadingVnavmesh", remoteVersion));
            var zipPath = Path.Combine(Path.GetTempPath(), "vnavmesh_" + Guid.NewGuid() + ".zip");
            await DownloadFileAsync(downloadUrl, zipPath, "vnavmesh");

            var extractDir = Path.Combine(Path.GetTempPath(), "vnavmeshExtract_" + Guid.NewGuid());
            ZipFile.ExtractToDirectory(zipPath, extractDir);
            DeployPluginFiles(extractDir, targetDir);

            var wasInstalled = localVersion != null;
            Info(wasInstalled
                ? Loc.Get("VnavmeshUpdated", remoteVersion)
                : Loc.Get("VnavmeshSetup", remoteVersion));
            return wasInstalled
                ? Loc.Get("UpdatedToShort", remoteVersion)
                : Loc.Get("NewlySetupShort", remoteVersion);
        }
        catch (IOException ex)
        {
            Error(Loc.Get("VnavmeshCouldNotWriteFiles", ex.Message));
            Error(Loc.Get("CloseGameAndLauncher"));
            return Loc.Get("ErrorFilesLocked");
        }
        catch (HttpRequestException ex)
        {
            Error(Loc.Get("VnavmeshDownloadFailed", ex.Message));
            return Loc.Get("ErrorNoNetwork");
        }
        catch (TaskCanceledException)
        {
            Error(Loc.Get("VnavmeshDownloadTimeout"));
            return Loc.Get("ErrorTimeout");
        }
        catch (Exception ex)
        {
            Error(Loc.Get("VnavmeshUnexpectedError", ex.Message));
            return Loc.Get("ErrorGeneric");
        }
    }

    // ── dalamudConfig.json ─────────────────────────────────────────────────

    /// <summary>
    /// Registers the plugin DLLs as DevPlugin load locations and seeds everything
    /// Dalamud needs to load them on the next boot without any UI interaction:
    /// DevMode=true, a DevPluginSettings entry per DLL (StartOnBoot + WorkingPluginId)
    /// and a matching enabled DefaultProfile entry (see <see cref="EnableDevPlugin"/>).
    /// Conservative on purpose: makes a backup and writes BOM-free (Dalamud's
    /// ReliableFileStorage reads raw bytes; a UTF-8 BOM makes it silently fall
    /// back to an old SQLite copy - documented project trap).
    /// </summary>
    private string PatchDalamudConfig()
    {
        if (!File.Exists(DalamudConfigPath))
        {
            Warn(Loc.Get("ConfigNotExist1"));
            Warn(Loc.Get("ConfigNotExist2"));
            Warn(Loc.Get("ConfigNotExist3"));
            Warn(Loc.Get("ConfigNotExist4"));
            return Loc.Get("ConfigMissingReturn");
        }

        byte[] bytes;
        string text;
        try
        {
            bytes = File.ReadAllBytes(DalamudConfigPath);
            text = StripBom(bytes);
        }
        catch (IOException ex)
        {
            Error(Loc.Get("ConfigReadFailed", ex.Message));
            Error(Loc.Get("CloseGameAndLauncher"));
            return Loc.Get("ConfigReadFailedReturn");
        }

        JObject config;
        try
        {
            config = JObject.Parse(text);
        }
        catch (Exception ex)
        {
            Error(Loc.Get("ConfigParseFailed", ex.Message));
            Error(Loc.Get("ConfigNotTouching"));
            return Loc.Get("ConfigInvalidReturn");
        }

        var loadLocations = config["DevPluginLoadLocations"]?["$values"] as JArray;
        if (loadLocations == null)
        {
            Warn(Loc.Get("ConfigUnexpectedStructure"));
            Warn(Loc.Get("ConfigSafetyNoChange1"));
            Warn(Loc.Get("ConfigSafetyNoChange2"));
            return Loc.Get("ConfigUnexpectedStructureReturn");
        }

        var accDll = Path.Combine(DevPluginsRoot, AccessibilityInternalName, AccessibilityInternalName + ".dll");
        var vnavDll = Path.Combine(DevPluginsRoot, VnavmeshInternalName, VnavmeshInternalName + ".dll");
        var hasVnav = File.Exists(vnavDll);

        try
        {
            var backup = DalamudConfigPath + ".bak-installer";
            File.Copy(DalamudConfigPath, backup, overwrite: true);

            // Without DevMode Dalamud never scans DevPluginLoadLocations at all
            // (PluginManager boot load is gated on configuration.DevMode).
            config["DevMode"] = true;

            var hasAcc = File.Exists(accDll);
            if (hasAcc) AddDevPluginLocation(loadLocations, accDll);
            if (hasVnav) AddDevPluginLocation(loadLocations, vnavDll);

            var enabled = true;
            if (hasAcc) enabled &= EnableDevPlugin(config, AccessibilityInternalName, accDll);
            if (hasVnav) enabled &= EnableDevPlugin(config, VnavmeshInternalName, vnavDll);

            WriteAllTextNoBom(DalamudConfigPath, config.ToString());
            Info(Loc.Get("ConfigUpdated", Path.GetFileName(backup)));

            if (!enabled)
            {
                Warn(Loc.Get("ProfileStructureUnexpected"));
                return Loc.Get("ProfileStructureUnexpectedReturn");
            }

            return Loc.Get("PluginsRegisteredEnabledReturn");
        }
        catch (IOException ex)
        {
            Error(Loc.Get("ConfigWriteFailed", ex.Message));
            Error(Loc.Get("CloseGameAndLauncher"));
            return Loc.Get("ConfigWriteFailedReturn");
        }
    }

    private static void AddDevPluginLocation(JArray loadLocations, string dllPath)
    {
        var exists = loadLocations.Any(e =>
            string.Equals((string?)e["Path"], dllPath, StringComparison.OrdinalIgnoreCase));
        if (exists) return;

        loadLocations.Add(new JObject
        {
            ["$type"] = "Dalamud.Configuration.DevPluginLocationSettings, Dalamud",
            ["Path"] = dllPath,
            ["IsEnabled"] = true,
            ["Nickname"] = null,
        });
    }

    /// <summary>
    /// Seeds everything a dev plugin needs to load on boot. Verified against
    /// decompiled Dalamud 15.0.2.2 (PluginManager.LoadPluginAsync, LocalDevPlugin
    /// ctor, Profile.WantsPlugin): a dev plugin loads at boot only when its
    /// DevPluginSettings entry (dictionary keyed by full DLL path) has
    /// StartOnBoot=true AND the DefaultProfile contains an entry with the SAME
    /// WorkingPluginId and IsEnabled=true. Dalamud only generates a new GUID when
    /// the DevPluginSettings entry is missing, so pre-seeding both sides with one
    /// GUID is stable. Returns false if the profile structure is missing.
    /// </summary>
    private static bool EnableDevPlugin(JObject config, string internalName, string dllPath)
    {
        if (config["DevPluginSettings"] is not JObject devSettings)
        {
            devSettings = new JObject
            {
                ["$type"] = "System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[Dalamud.Configuration.Internal.DevPluginSettings, Dalamud]], System.Private.CoreLib",
            };
            config["DevPluginSettings"] = devSettings;
        }

        Guid workingId;
        if (devSettings[dllPath] is JObject entry)
        {
            entry["StartOnBoot"] = true;
            if (!Guid.TryParse((string?)entry["WorkingPluginId"], out workingId) || workingId == Guid.Empty)
            {
                workingId = Guid.NewGuid();
                entry["WorkingPluginId"] = workingId.ToString();
            }
        }
        else
        {
            workingId = Guid.NewGuid();
            devSettings[dllPath] = new JObject
            {
                ["$type"] = "Dalamud.Configuration.Internal.DevPluginSettings, Dalamud",
                ["StartOnBoot"] = true,
                ["NotifyForErrors"] = true,
                // Auto-reload on file change (user request 2026-07-16): a new
                // deploy is picked up without restarting the game.
                ["AutomaticReloading"] = true,
                ["WorkingPluginId"] = workingId.ToString(),
                ["DismissedValidationProblems"] = new JObject
                {
                    ["$type"] = "System.Collections.Generic.List`1[[System.String, System.Private.CoreLib]], System.Private.CoreLib",
                    ["$values"] = new JArray(),
                },
            };
        }

        if (config["DefaultProfile"]?["Plugins"]?["$values"] is not JArray profilePlugins)
            return false;

        var existing = profilePlugins.FirstOrDefault(p =>
            string.Equals((string?)p["InternalName"], internalName, StringComparison.Ordinal));
        if (existing != null)
        {
            existing["IsEnabled"] = true;
            // DevPluginSettings is the authority for the GUID (EffectiveWorkingPluginId).
            existing["WorkingPluginId"] = workingId.ToString();
        }
        else
        {
            profilePlugins.Add(new JObject
            {
                ["$type"] = "Dalamud.Plugin.Internal.Profiles.ProfileModelV1+ProfileModelV1Plugin, Dalamud",
                ["InternalName"] = internalName,
                ["WorkingPluginId"] = workingId.ToString(),
                ["IsEnabled"] = true,
            });
        }
        return true;
    }

    // ── GitHub-API-Helfer ──────────────────────────────────────────────────

    private async Task<JsonNode?> GetLatestReleaseAsync(string owner, string repo)
    {
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
        var json = await _http.GetStringAsync(apiUrl);
        return JsonNode.Parse(json);
    }

    /// <summary>Wie <see cref="PickAsset"/>, liefert aber den ganzen Asset-Knoten
    /// (für Felder wie "size", die das Tupel nicht trägt).</summary>
    private static JsonNode? PickAssetNode(JsonNode? release, Func<string, bool> pick)
    {
        var assets = release?["assets"]?.AsArray();
        if (assets == null) return null;

        foreach (var a in assets)
        {
            var name = a?["name"]?.GetValue<string>();
            if (name != null && pick(name)) return a;
        }
        return null;
    }

    private static (string tag, string url, string name)? PickAsset(JsonNode? release, Func<string, bool> pick)
    {
        var tag = release?["tag_name"]?.GetValue<string>() ?? "";
        var assets = release?["assets"]?.AsArray();
        if (assets != null)
        {
            foreach (var a in assets)
            {
                var name = a?["name"]?.GetValue<string>();
                if (name != null && pick(name))
                    return (tag, a!["browser_download_url"]!.GetValue<string>(), name);
            }
        }
        return null;
    }

    // ── Selbst-Update des Installers ───────────────────────────────────────

    /// <summary>
    /// Prüft, ob im neuesten Release eine neuere Installer-Version liegt, fragt
    /// den Nutzer, lädt sie und startet Phase 2 (siehe <see cref="SelfUpdate"/>).
    /// Gibt true zurück, wenn der Neustart eingeleitet wurde.
    ///
    /// Versionsquelle ist das Release-Asset "installer.json", NICHT der
    /// Dateiname der EXE: die heißt bewusst unveränderlich
    /// FF14AccessibilityInstaller.exe, damit der Download-Link und die
    /// Anleitung in der README stabil bleiben. (Der frühere Hinweis-Code las
    /// die Version per Regex aus dem Asset-Namen und konnte deshalb nie
    /// anschlagen.)
    ///
    /// Jeder Fehler ist hier unkritisch: das Selbst-Update entfällt dann still
    /// und die normale Plugin-Installation läuft weiter.
    /// </summary>
    private async Task<bool> TrySelfUpdateAsync(string ownVersion)
    {
        Info(Loc.Get("CheckingInstallerVersion"));

        JsonNode? manifest;
        JsonNode? release;
        try
        {
            release = await GetLatestReleaseAsync(AccessibilityRepoOwner, AccessibilityRepoName);
            var manifestAsset = PickAsset(release, n =>
                n.Equals(InstallerManifestAssetName, StringComparison.OrdinalIgnoreCase));
            if (manifestAsset == null)
            {
                // Releases vor Installer 1.1 haben kein installer.json.
                Info(Loc.Get("NoInstallerManifest"));
                return false;
            }

            var json = await _http.GetStringAsync(manifestAsset.Value.url);
            manifest = JsonNode.Parse(json);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or FormatException)
        {
            Warn(Loc.Get("InstallerCheckFailed", ex.Message));
            return false;
        }

        var remoteVersion = manifest?["InstallerVersion"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(remoteVersion))
        {
            Warn(Loc.Get("InstallerManifestUnreadable"));
            return false;
        }

        if (!IsNewer(remoteVersion, ownVersion))
        {
            Info(Loc.Get("InstallerUpToDate", ownVersion));
            return false;
        }

        var exeAssetName = manifest?["AssetName"]?.GetValue<string>() ?? InstallerExeAssetName;
        var exeAsset = PickAssetNode(release, n => n.Equals(exeAssetName, StringComparison.OrdinalIgnoreCase));
        if (exeAsset == null)
        {
            Warn(Loc.Get("InstallerAssetMissing", exeAssetName));
            return false;
        }

        var targetPath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(targetPath))
        {
            Warn(Loc.Get("InstallerOwnPathUnknown"));
            return false;
        }

        var sizeMb = (exeAsset["size"]?.GetValue<long>() ?? 0L) / (1024 * 1024);
        Info(Loc.Get("InstallerUpdateAvailable", remoteVersion, ownVersion));

        if (AskYesNo == null || !AskYesNo(Loc.Get("InstallerUpdateQuestion", remoteVersion, sizeMb)))
        {
            Info(Loc.Get("InstallerUpdateDeclined"));
            return false;
        }

        var downloadUrl = exeAsset["browser_download_url"]?.GetValue<string>();
        if (string.IsNullOrEmpty(downloadUrl))
        {
            Warn(Loc.Get("InstallerAssetMissing", exeAssetName));
            return false;
        }

        var newExePath = Path.Combine(Path.GetTempPath(), SelfUpdate.DownloadFilePrefix + Guid.NewGuid() + ".exe");
        try
        {
            Info(Loc.Get("DownloadingInstaller", remoteVersion));
            await DownloadFileAsync(downloadUrl, newExePath, Loc.Get("InstallerDownloadLabel"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            Warn(Loc.Get("InstallerDownloadFailed", ex.Message));
            TryDelete(newExePath);
            return false;
        }

        // Integritätsprüfung, damit niemals eine abgebrochene oder veränderte
        // Datei gestartet wird. Fehlt der Hash im Manifest, wird nur geloggt -
        // dann gilt dieselbe Vertrauensbasis wie beim manuellen Download.
        var expectedHash = manifest?["Sha256"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(expectedHash))
        {
            var actual = ComputeSha256(newExePath);
            if (!string.Equals(actual, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Error(Loc.Get("InstallerHashMismatch"));
                TryDelete(newExePath);
                return false;
            }
            Info(Loc.Get("InstallerHashOk"));
        }
        else
        {
            Info(Loc.Get("InstallerNoHash"));
        }

        try
        {
            Process.Start(new ProcessStartInfo(newExePath)
            {
                Arguments = $"{SelfUpdate.ApplyUpdateArg} \"{targetPath}\" {Environment.ProcessId}",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Warn(Loc.Get("InstallerStartFailed", ex.Message));
            TryDelete(newExePath);
            return false;
        }

        Info(Loc.Get("InstallerRestarting", remoteVersion));
        RestartRequested?.Invoke();
        return true;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { /* Temp-Datei bleibt liegen - unkritisch. */ }
        catch (UnauthorizedAccessException) { /* dito */ }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    // ── Download / Entpacken ───────────────────────────────────────────────

    private async Task DownloadFileAsync(string url, string destinationPath, string label)
    {
        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

        await using var httpStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(destinationPath);

        var buffer = new byte[81920];
        long readTotal = 0;
        var lastDecile = -1;
        int read;
        while ((read = await httpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, read);
            readTotal += read;
            if (totalBytes > 0)
            {
                var percent = (int)(readTotal * 100 / totalBytes);
                var decile = percent / 10;
                if (decile != lastDecile || percent == 100)
                {
                    Info(Loc.Get("DownloadProgress", label, percent));
                    lastDecile = decile;
                }
            }
        }
    }

    /// <summary>Kopiert die entpackten Plugin-Dateien (DLL, JSON-Manifest, PDB,
    /// Tolk.dll, nvdaControllerClient64.dll, NAudio*.dll) in den devPlugins-Zielordner.
    /// Kopiert alle Dateien im ZIP-Root (Struktur live verifiziert), notfalls
    /// rekursiv, falls die ZIP einen Unterordner enthält.</summary>
    private static void DeployPluginFiles(string extractDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        var files = Directory.GetFiles(extractDir);
        if (files.Length == 0)
            files = Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }
    }

    // ── Versions-/Sonstige Helfer ──────────────────────────────────────────

    private static string? ReadLocalManifestVersion(string manifestPath)
    {
        if (!File.Exists(manifestPath)) return null;
        try
        {
            var json = File.ReadAllText(manifestPath);
            var node = JsonNode.Parse(json);
            return node?["AssemblyVersion"]?.GetValue<string>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsNewer(string remote, string local)
    {
        var r = ParseVersionLoose(remote);
        var l = ParseVersionLoose(local);
        if (r != null && l != null) return r > l;
        return !string.Equals(remote, local, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parst eine Versionsangabe und füllt sie IMMER auf vier Stellen auf.
    /// Ohne das Auffüllen gilt "1.1.0" als KLEINER als "1.1.0.0" (nicht gesetzte
    /// Stellen zählen bei <see cref="Version"/> als -1) - eine dreistellige
    /// Angabe in installer.json würde das Selbst-Update also still nie auslösen.
    /// </summary>
    private static Version? ParseVersionLoose(string s)
    {
        s = s.TrimStart('v', 'V').Trim();
        var parts = s.Split('.');
        if (parts.Length > 4) return null;
        while (parts.Length < 4)
        {
            s += ".0";
            parts = s.Split('.');
        }
        return Version.TryParse(s, out var v) ? v : null;
    }

    private static string StripBom(byte[] bytes)
    {
        // UTF-8 BOM = EF BB BF.
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
        return new UTF8Encoding(false).GetString(bytes);
    }

    private static void WriteAllTextNoBom(string path, string content) =>
        File.WriteAllText(path, content, new UTF8Encoding(false));
}
