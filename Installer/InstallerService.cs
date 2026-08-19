using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace FF14AccessibilityInstaller;

/// <summary>
/// Barrier-free installer/updater for the FF14 Accessibility plugin (and the
/// vnavmesh pathfinding plugin it uses for auto-walk). Extracted from the
/// original console version, now driven from a WinForms GUI.
///
/// Why an installer at all: Dalamud's own plugin installer is an ImGui overlay
/// that screen readers cannot read, so a blind user cannot use it. This tool
/// puts the files where Dalamud looks for them and writes the settings Dalamud
/// needs, so it auto-loads them on the next game start - no ImGui click required.
///
/// Two routes, and they are not the same:
///
///   FF14Accessibility -> installedPlugins\FF14Accessibility\&lt;version&gt;\
///                        i.e. the shape Dalamud gives a released plugin. It is
///                        then a normal plugin, not a "dev plugin".
///   vnavmesh          -> devPlugins\vnavmesh\
///                        someone else's plugin. Putting it in installedPlugins
///                        would mean registering their repository in the user's
///                        Dalamud settings, and this project does not write into
///                        other people's configuration (status.md 4-3).
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

    // ── Wegdateien für die Kategorie "Dungeon" ─────────────────────────────
    //
    // WARUM DER INSTALLER DAS AUCH TUT, obwohl das Plugin es selbst kann: hier
    // ist der Netzzugriff erwartbar - der Nutzer installiert gerade, hört jede
    // Zeile mit und wartet ohnehin. Beim ersten Spielstart ist damit alles da,
    // statt dass mitten im Einloggen eine Ansage aufschlägt.
    //
    // ES ERSETZT DEN PLUGIN-WEG NICHT. Wer das Plugin über das Dalamud-Repo
    // bezieht, sieht diesen Installer nie; und ein gelöschter Ordner füllt sich
    // nur wieder, wenn das Plugin selbst nachlädt. Zwei Wege für dieselbe Sache
    // sind hier kein Doppel, sondern zwei verschiedene Lücken.
    //
    // Ausgeliefert wird nichts: geholt wird auf dem Rechner des Nutzers, vom
    // Ursprungs-Repo - dieselbe Trennung wie bei vnavmesh darüber.
    private const string DungeonPathsZipUrl =
        "https://codeload.github.com/erdelf/AutoDuty/zip/refs/heads/master";

    /// <summary>Ordnerteil INNERHALB des Archivs. Als Teilstring geprüft, nie als
    /// Präfix: ein GitHub-Zip packt alles in "&lt;repo&gt;-&lt;branch&gt;/", und ein
    /// umbenannter Standardzweig würde ein Präfix still ins Leere laufen lassen.</summary>
    private const string DungeonPathsFolderInZip = "/AutoDuty/Paths/";

    /// <summary>Grenzen gegen ein Archiv, das nicht das ist, wofür es sich
    /// ausgibt. Eine Wegdatei ist wenige Kilobyte groß.</summary>
    private const int DungeonPathsMaxEntryBytes = 2 * 1024 * 1024;
    private const long DungeonPathsMaxTotalBytes = 64L * 1024 * 1024;

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
    private static readonly string InstalledPluginsRoot = KrProfile.InstalledPluginsRoot;
    private static readonly string DalamudConfigPath = KrProfile.ConfigPath;

    /// <summary>Wohin die Wegdateien gehören. MUSS mit
    /// <c>DungeonRouteService.PathFolder</c> im Plugin übereinstimmen - das
    /// Plugin bildet denselben Pfad über Dalamuds Konfigurationsordner.</summary>
    private static readonly string DungeonPathsDir = Path.Combine(
        XivLauncherRoot, "pluginConfigs", AccessibilityInternalName, "DungeonPaths");

    /// <summary>
    /// What goes into the installed manifest's InstalledFromUrl. Dalamud's own
    /// constant for "came from the official repository"
    /// (SpecialPluginSource.MainRepo). It decides one thing that matters here:
    /// LocalPlugin.IsOrphaned is true when no configured repository matches the
    /// manifest, and an orphaned plugin is NOT loaded. With this value the
    /// manifest is not third-party, so the main repository matches it and the
    /// plugin loads. Verified against Dalamud's LocalPlugin.GetSourceRepository.
    /// </summary>
    private const string OfficialSource = "OFFICIAL";

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

    /// <summary>
    /// Leaves the vnavmesh step out entirely, network call included. For the
    /// automated check of the install path: it runs against a throwaway profile
    /// root and must not depend on puni.sh being reachable, nor download 30 MB
    /// on every test run.
    /// </summary>
    public bool SkipVnavmesh { get; set; }

    /// <summary>
    /// The version folder of the installed copy, or null if there is none.
    /// Exists so --check and --install can report the result from outside the
    /// GUI: "the installer said it worked" and "the files are where Dalamud
    /// looks" are different claims.
    /// </summary>
    public static string? InstalledCopyPath()
    {
        var found = FindInstalledManifest(Path.Combine(InstalledPluginsRoot, AccessibilityInternalName));
        return found == null ? null : Path.GetDirectoryName(found.ManifestPath);
    }

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
            if (!await PrepareKrProfileAsync())
                return false; // Dalamud fehlt - der Nutzer muss erst den Updater laufen lassen.
            Info(string.Empty);

            var accResult = await UpdateAccessibilityPluginAsync(ownVersion);
            Info(string.Empty);
            var vnavResult = SkipVnavmesh ? Loc.Get("SkippedShort") : await UpdateVnavmeshAsync();
            Info(string.Empty);
            // Nach dem Plugin, vor dem Patchen der Konfiguration: die Dateien
            // gehören in den Konfigurationsordner des Plugins, und ein
            // Fehlschlag hier darf die Installation selbst nicht aufhalten.
            var pathsResult = await UpdateDungeonPathsAsync();
            Info(string.Empty);
            var patchResult = PatchDalamudConfig();

            Info(string.Empty);
            Info(Loc.Get("SummaryHeader"));
            Info(Loc.Get("SummaryAccessibility", accResult));
            Info(Loc.Get("SummaryVnavmesh", vnavResult));
            Info(Loc.Get("SummaryDungeonPaths", pathsResult));
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
    /// Der globale Installer laedt an dieser Stelle XIVLauncher herunter. Wir
    /// laden stattdessen den KR-Dalamud-Updater - nicht als Weiterverteilung,
    /// sondern aus seinem eigenen Release, genau wie beim vnavmesh-Schritt.
    /// Was danach uebrig bleibt, ist der eine Knopf in dessen Fenster.
    /// </summary>
    private async Task<bool> PrepareKrProfileAsync()
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

        // Two different situations wear the same face here, and telling them
        // apart is the whole point: the updater being absent is ours to fix,
        // Dalamud being absent needs a button pressed in the updater's window.
        if (!KrProfile.UpdaterInstalled && !await SetupKrUpdaterAsync())
            return false;

        Warn(Loc.Get("KrDalamudMissing"));
        Info(Loc.Get("KrDalamudGetIt"));
        Info("  " + KrProfile.UpdaterPath);
        Info(Loc.Get("KrDalamudThenCheckUpdate"));

        // Opening it saves the one step the user would otherwise have to find a
        // path for. Failing to open it changes nothing - the path is printed above.
        if (KrProfile.TryLaunchUpdater())
            Info(Loc.Get("KrUpdaterLaunched"));

        return false;
    }

    /// <summary>
    /// Fetches the Korean Dalamud updater from its own GitHub release and unpacks
    /// it. Returns false when the user declined or the fetch failed - the caller
    /// then falls back to printing the path and the manual instruction.
    ///
    /// This is a download, not a redistribution. The repository carries no
    /// license, so the archive must not travel inside ours; taking it from the
    /// upstream release on the user's behalf is the same shape as the vnavmesh
    /// step and touches nothing the license would cover.
    /// </summary>
    private async Task<bool> SetupKrUpdaterAsync()
    {
        Info(string.Empty);
        Info(Loc.Get("KrUpdaterWhatItIs1"));
        Info(Loc.Get("KrUpdaterWhatItIs2"));
        Info(Loc.Get("KrUpdaterWhatItIs3"));

        if (!(AskYesNo?.Invoke(Loc.Get("AskSetupKrUpdater")) ?? false))
        {
            Info(Loc.Get("KrUpdaterSkipped"));
            return false;
        }

        string? downloadUrl;
        try
        {
            Info(Loc.Get("KrUpdaterCheckingRelease"));
            var json = await _http.GetStringAsync(KrProfile.UpdaterReleaseApi);
            downloadUrl = PickUpdaterAsset(JsonNode.Parse(json)?["assets"]?.AsArray());
        }
        catch (HttpRequestException ex)
        {
            Warn(Loc.Get("KrUpdaterUnreachable", ex.Message));
            return false;
        }
        catch (TaskCanceledException)
        {
            Warn(Loc.Get("KrUpdaterTimeout"));
            return false;
        }
        catch (Exception ex)
        {
            Warn(Loc.Get("KrUpdaterUnexpectedError", ex.Message));
            return false;
        }

        if (downloadUrl == null)
        {
            Warn(Loc.Get("KrUpdaterNoAsset", KrProfile.UpdaterAssetMarker));
            return false;
        }

        var zipPath = Path.Combine(Path.GetTempPath(), "kr_dalamud_updater_" + Guid.NewGuid() + ".zip");
        try
        {
            Info(Loc.Get("KrUpdaterDownloading"));
            await DownloadFileAsync(downloadUrl, zipPath, Loc.Get("KrUpdaterDownloadLabel"));

            // The archive is flat, so it goes into the app folder whole. Overwrite
            // rather than merge: a half-updated set of the three files is worse
            // than either version of them.
            Directory.CreateDirectory(KrProfile.UpdaterExtractDir);
            ZipFile.ExtractToDirectory(zipPath, KrProfile.UpdaterExtractDir, overwriteFiles: true);
        }
        catch (IOException ex)
        {
            Error(Loc.Get("KrUpdaterCouldNotWrite", ex.Message));
            return false;
        }
        catch (Exception ex)
        {
            Error(Loc.Get("KrUpdaterDownloadFailed", ex.Message));
            return false;
        }
        finally
        {
            TryDeleteFile(zipPath);
        }

        // Unpacking into the right folder is not the same as having a working
        // updater. Say which, so "nothing happened" has a place to start.
        if (!KrProfile.UpdaterInstalled)
        {
            Error(Loc.Get("KrUpdaterExeMissing", KrProfile.UpdaterPath));
            return false;
        }

        Info(Loc.Get("KrUpdaterInstalledAt", KrProfile.UpdaterExtractDir));
        return true;
    }

    /// <summary>
    /// Picks the release asset to download. Mirrored in tools/kr-setup/kr_profile.py
    /// (<c>pick_updater_asset</c>), where the cases are tested.
    ///
    /// Only the "Portable" zip carries an executable. The release also publishes a
    /// "Payload" zip for the updater's own self-update; downloading that one
    /// extracts perfectly and leaves nothing to run.
    /// </summary>
    private static string? PickUpdaterAsset(JsonArray? assets)
    {
        if (assets == null) return null;

        foreach (var entry in assets)
        {
            var name = entry?["name"]?.GetValue<string>();
            var url = entry?["browser_download_url"]?.GetValue<string>();
            if (name == null || url == null) continue;

            if (name.Contains(KrProfile.UpdaterAssetMarker, StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return url;
        }
        return null;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception)
        {
            // Leaving a temp file behind must not fail the install.
        }
    }

    // ── Eigenes Plugin (FF14Accessibility) ────────────────────────────────

    /// <summary>
    /// Installiert das Plugin aus dem lokalen Build statt aus einem GitHub-Release.
    ///
    /// Der Grund ist nicht Bequemlichkeit: das Release-Binary des Upstreams ist
    /// gegen FFXIVClientStructs 7.55 gebunden, das koreanische Dalamud liefert
    /// 7.51. Es wuerde laden und beim ersten Gearset-Aufruf werfen. Was hier
    /// installiert wird, muss also aus diesem Checkout kommen.
    ///
    /// The layout is Dalamud's own, not ours (PluginManager.LoadAllPlugins):
    ///
    ///   installedPlugins\FF14Accessibility\&lt;version&gt;\FF14Accessibility.dll
    ///                                                 \FF14Accessibility.json
    ///
    /// Three things about it are load-or-not decisions, not cosmetics:
    ///
    ///   - the folder name MUST parse as a version. CleanupPlugins deletes every
    ///     version folder whose name does not, so an unreadable manifest has to
    ///     stop the install rather than fall back to a placeholder string.
    ///   - the DLL name MUST equal the plugin folder name.
    ///   - the manifest needs InstalledFromUrl (see <see cref="OfficialSource"/>).
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

        var pluginRoot = Path.Combine(InstalledPluginsRoot, AccessibilityInternalName);
        var previous = FindInstalledManifest(pluginRoot);
        var wasInstalled = previous != null;

        string extractDir = Path.Combine(Path.GetTempPath(), "FF14AccExtract_" + Guid.NewGuid());
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractDir);

            // Version aus dem frisch entpackten Manifest, nicht aus dem Dateinamen -
            // der Packager schreibt sie dort hinein und nur die stimmt.
            var builtVersion =
                ReadLocalManifestVersion(Path.Combine(extractDir, AccessibilityInternalName + ".json"));
            if (builtVersion == null || !Version.TryParse(builtVersion, out _))
            {
                Error(Loc.Get("KrBuildVersionUnreadable", builtVersion ?? Loc.Get("UnknownVersion")));
                return Task.FromResult(Loc.Get("ErrorGeneric"));
            }

            Info(Loc.Get("KrUsingLocalBuild", builtVersion, zipPath));

            var versionDir = Path.Combine(pluginRoot, builtVersion);
            // Old version folders are build output, and Dalamud loads the highest
            // version it finds - leaving one behind means a downgrade survives.
            RemoveOtherVersions(pluginRoot, versionDir);
            DeployPluginFiles(extractDir, versionDir);
            // Carry the identity across updates. The profile entry in
            // dalamudConfig.json is keyed by this GUID; a fresh one on every
            // update would leave a dead entry behind each time.
            WriteInstalledManifest(versionDir, previous?.WorkingPluginId);
            Info(Loc.Get("KrInstalledAt", versionDir));

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

    /// <summary>What an already installed copy tells us: its version folder and
    /// the identity Dalamud gave it.</summary>
    private sealed record InstalledCopy(string ManifestPath, string Version, Guid WorkingPluginId);

    /// <summary>
    /// Finds the installed copy under installedPlugins\&lt;name&gt;, if there is one.
    /// Takes the highest version, which is the one Dalamud would load.
    /// </summary>
    private static InstalledCopy? FindInstalledManifest(string pluginRoot)
    {
        if (!Directory.Exists(pluginRoot)) return null;

        InstalledCopy? best = null;
        Version? bestVersion = null;

        foreach (var versionDir in Directory.GetDirectories(pluginRoot))
        {
            var manifestPath = Path.Combine(versionDir, AccessibilityInternalName + ".json");
            var version = ReadLocalManifestVersion(manifestPath);
            if (version == null || !Version.TryParse(version, out var parsed)) continue;
            if (bestVersion != null && parsed <= bestVersion) continue;

            bestVersion = parsed;
            best = new InstalledCopy(manifestPath, version, ReadWorkingPluginId(manifestPath));
        }

        return best;
    }

    private static Guid ReadWorkingPluginId(string manifestPath)
    {
        try
        {
            var node = JsonNode.Parse(File.ReadAllText(manifestPath));
            var raw = node?["WorkingPluginId"]?.GetValue<string>();
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
        catch (Exception)
        {
            return Guid.Empty;
        }
    }

    /// <summary>Drops every version folder except the one just written.</summary>
    private void RemoveOtherVersions(string pluginRoot, string keep)
    {
        if (!Directory.Exists(pluginRoot)) return;

        foreach (var dir in Directory.GetDirectories(pluginRoot))
        {
            if (string.Equals(dir, keep, StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                Directory.Delete(dir, recursive: true);
                Info(Loc.Get("KrOldVersionRemoved", Path.GetFileName(dir)));
            }
            catch (Exception ex)
            {
                // Dalamud cleans these up itself on the next boot, so a locked
                // folder is not worth failing the install over.
                Warn(Loc.Get("KrOldVersionKept", Path.GetFileName(dir), ex.Message));
            }
        }
    }

    /// <summary>
    /// Adds the fields a manifest only has once it is installed. The packager
    /// writes the repository half (name, version, API level); these four are the
    /// local half, and Dalamud writes them itself when it installs a plugin.
    ///
    /// <paramref name="carriedId"/> is the identity of the copy being replaced.
    /// Empty (or absent) is fine: Dalamud then assigns one on first load and
    /// saves it back into this file.
    /// </summary>
    private static void WriteInstalledManifest(string versionDir, Guid? carriedId)
    {
        var manifestPath = Path.Combine(versionDir, AccessibilityInternalName + ".json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();

        manifest["InstalledFromUrl"] = OfficialSource;
        manifest["Disabled"] = false;
        manifest["Testing"] = false;
        manifest["ScheduledForDeletion"] = false;
        if (carriedId is { } id && id != Guid.Empty)
            manifest["WorkingPluginId"] = id.ToString();

        File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            // Otherwise every non-ASCII letter in the description turns into
            // \uXXXX. Valid either way, but this file is meant to be readable.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }), new UTF8Encoding(false));
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

    // ── Wegdateien für die Kategorie "Dungeon" ─────────────────────────────

    /// <summary>
    /// Lädt die Wegdateien und legt sie dorthin, wo das Plugin sie liest.
    ///
    /// <para>
    /// EIN ARCHIV STATT DREIHUNDERT EINZELABRUFE: die Momentaufnahme des Repos
    /// ist ein Download von rund 750 KB und trägt alle Wegdateien. Einzeln
    /// geholt wären es über 300 Abrufe gegen ein Ratelimit - und bei jedem
    /// Abbruch ein halb gefüllter Ordner.
    /// </para>
    ///
    /// <para>
    /// GESCHRIEBEN WIRD ERST, WENN ALLES GELESEN IST. Ein halber Ordner wäre
    /// schlimmer als ein leerer: die Kategorie erschiene dann in manchen
    /// Dungeons und in anderen nicht, ohne dass ein blinder Spieler den Grund
    /// erfahren könnte.
    /// </para>
    ///
    /// <para>
    /// EIN FEHLSCHLAG BRICHT DIE INSTALLATION NICHT AB. Ohne Wegdateien fehlt
    /// eine Kategorie; ohne Plugin fehlt alles. Deshalb meldet diese Methode
    /// ihren Fehler in die Zusammenfassung und wirft nicht.
    /// </para>
    /// </summary>
    private async Task<string> UpdateDungeonPathsAsync()
    {
        Info(Loc.Get("CheckingDungeonPaths"));

        byte[] archive;
        try
        {
            archive = await _http.GetByteArrayAsync(DungeonPathsZipUrl);
        }
        catch (HttpRequestException ex)
        {
            Warn(Loc.Get("DungeonPathsUnreachable", ex.Message));
            return Loc.Get("ErrorNoNetwork");
        }
        catch (TaskCanceledException)
        {
            Warn(Loc.Get("DungeonPathsTimeout"));
            return Loc.Get("ErrorTimeout");
        }

        try
        {
            var files = new List<(string Name, byte[] Content)>();
            long total = 0;

            using (var stream = new MemoryStream(archive, writable: false))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    var full = entry.FullName.Replace('\\', '/');
                    if (full.IndexOf(DungeonPathsFolderInZip, StringComparison.Ordinal) < 0) continue;
                    if (!full.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

                    // NUR der nackte Dateiname wird zum Ziel, nie der Pfad aus dem
                    // Archiv: ein Eintrag namens "..\..\irgendwas.json" ist der Weg,
                    // auf dem ein Archiv außerhalb seines Zielordners schreibt.
                    var name = Path.GetFileName(entry.Name);
                    if (string.IsNullOrEmpty(name) || name != entry.Name) continue;
                    if (entry.Length > DungeonPathsMaxEntryBytes) continue;

                    total += entry.Length;
                    if (total > DungeonPathsMaxTotalBytes)
                    {
                        Warn(Loc.Get("DungeonPathsArchiveTooBig"));
                        return Loc.Get("ErrorGeneric");
                    }

                    using var entryStream = entry.Open();
                    using var buffer = new MemoryStream();
                    await entryStream.CopyToAsync(buffer);
                    files.Add((name, buffer.ToArray()));
                }
            }

            if (files.Count == 0)
            {
                // Archiv erreicht, aber nichts darin gefunden: der Aufbau der
                // Quelle hat sich geändert. Das ist ein Fehler und darf sich nicht
                // wie "geladen, 0 Dateien" lesen.
                Warn(Loc.Get("DungeonPathsNothingInArchive"));
                return Loc.Get("ErrorNotFound");
            }

            Directory.CreateDirectory(DungeonPathsDir);
            foreach (var (name, content) in files)
                File.WriteAllBytes(Path.Combine(DungeonPathsDir, name), content);

            Info(Loc.Get("DungeonPathsWritten", files.Count, DungeonPathsDir));
            return Loc.Get("DungeonPathsSummary", files.Count);
        }
        catch (Exception ex)
        {
            Warn(Loc.Get("DungeonPathsUnexpectedError", ex.Message));
            return Loc.Get("ErrorGeneric");
        }
    }

    // ── dalamudConfig.json ─────────────────────────────────────────────────

    /// <summary>
    /// Seeds everything Dalamud needs to load both plugins on the next boot
    /// without any UI interaction. The two routes need different things:
    ///
    ///   FF14Accessibility (installed): an enabled DefaultProfile entry carrying
    ///     the same GUID as the manifest (see <see cref="EnableInstalledPlugin"/>).
    ///   vnavmesh (dev): DevMode=true, a load location, and a DevPluginSettings
    ///     entry with StartOnBoot (see <see cref="EnableDevPlugin"/>).
    ///
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

        var accDevDll = Path.Combine(DevPluginsRoot, AccessibilityInternalName, AccessibilityInternalName + ".dll");
        var vnavDll = Path.Combine(DevPluginsRoot, VnavmeshInternalName, VnavmeshInternalName + ".dll");
        var hasVnav = File.Exists(vnavDll);
        var installed = FindInstalledManifest(Path.Combine(InstalledPluginsRoot, AccessibilityInternalName));

        try
        {
            var backup = DalamudConfigPath + ".bak-installer";
            File.Copy(DalamudConfigPath, backup, overwrite: true);

            // Only for vnavmesh, which keeps the dev route: without DevMode
            // Dalamud never scans DevPluginLoadLocations at all (PluginManager
            // boot load is gated on configuration.DevMode). Our own plugin does
            // not need it any more, so an install without vnavmesh leaves the
            // setting alone instead of turning developer mode on for nothing.
            if (hasVnav)
            {
                config["DevMode"] = true;
                AddDevPluginLocation(loadLocations, vnavDll);
            }

            var enabled = true;
            if (hasVnav) enabled &= EnableDevPlugin(config, VnavmeshInternalName, vnavDll);
            if (installed != null) enabled &= EnableInstalledPlugin(config, AccessibilityInternalName, installed);

            // Whatever the dev route left behind would load a SECOND copy of the
            // same plugin beside the installed one - same commands, same hotkeys.
            RemoveDevInstall(config, loadLocations, accDevDll);

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

    /// <summary>
    /// Same job as <see cref="EnableDevPlugin"/> for a plugin that sits in
    /// installedPlugins. Simpler, because an installed plugin needs no DevMode
    /// and no DevPluginSettings - the only thing standing between it and loading
    /// is the profile: PluginManager loads it when a profile wants its
    /// WorkingPluginId, and the manifest is where that GUID lives.
    ///
    /// Dalamud would assign a GUID itself and add a default-enabled entry, but
    /// only after it has loaded once. Seeding both sides here means the very
    /// first boot after the install already has the plugin on, and it means the
    /// state can be checked from outside the game.
    ///
    /// Returns false if the profile structure is not what we expect.
    /// </summary>
    private bool EnableInstalledPlugin(JObject config, string internalName, InstalledCopy installed)
    {
        if (config["DefaultProfile"]?["Plugins"]?["$values"] is not JArray profilePlugins)
            return false;

        var workingId = installed.WorkingPluginId;
        if (workingId == Guid.Empty)
        {
            workingId = Guid.NewGuid();
            // The manifest is the authority Dalamud reads, so it has to carry the
            // same GUID as the profile entry below.
            var manifest = JsonNode.Parse(File.ReadAllText(installed.ManifestPath))!.AsObject();
            manifest["WorkingPluginId"] = workingId.ToString();
            File.WriteAllText(installed.ManifestPath, manifest.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }), new UTF8Encoding(false));
        }

        var existing = profilePlugins.FirstOrDefault(p =>
            string.Equals((string?)p["InternalName"], internalName, StringComparison.Ordinal));
        if (existing != null)
        {
            existing["IsEnabled"] = true;
            // Overwrites whatever the dev install left here - one entry per
            // plugin, pointing at the copy that is actually on disk.
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

        Info(Loc.Get("KrProfileEntrySeeded", internalName, workingId.ToString()));
        return true;
    }

    /// <summary>
    /// Removes every trace of the dev-route install of our own plugin: the load
    /// location, the DevPluginSettings entry, and the folder itself.
    ///
    /// This is not tidiness. Dalamud loads dev plugins AND installed plugins in
    /// the same pass, so leaving the old copy in place means two instances of
    /// the same plugin registering the same command and the same hotkeys.
    ///
    /// The profile entry is deliberately not touched here -
    /// <see cref="EnableInstalledPlugin"/> has already pointed it at the
    /// installed copy.
    /// </summary>
    private void RemoveDevInstall(JObject config, JArray loadLocations, string devDllPath)
    {
        var removed = false;

        for (var i = loadLocations.Count - 1; i >= 0; i--)
        {
            if (string.Equals((string?)loadLocations[i]["Path"], devDllPath, StringComparison.OrdinalIgnoreCase))
            {
                loadLocations.RemoveAt(i);
                removed = true;
            }
        }

        if (config["DevPluginSettings"] is JObject devSettings)
        {
            foreach (var key in devSettings.Properties().Select(p => p.Name).ToList())
            {
                if (string.Equals(key, devDllPath, StringComparison.OrdinalIgnoreCase))
                {
                    devSettings.Remove(key);
                    removed = true;
                }
            }
        }

        var devDir = Path.GetDirectoryName(devDllPath);
        if (devDir != null && Directory.Exists(devDir))
        {
            try
            {
                Directory.Delete(devDir, recursive: true);
                removed = true;
            }
            catch (Exception ex)
            {
                // Loud, because the consequence is two copies running at once.
                Warn(Loc.Get("KrDevInstallStuck", devDir, ex.Message));
            }
        }

        if (removed)
            Info(Loc.Get("KrDevInstallRemoved"));
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
