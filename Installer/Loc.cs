using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace FF14AccessibilityInstaller;

/// <summary>
/// Minimal static localization helper (DE/EN). No resx needed - just two
/// dictionaries and a Get(key) lookup. Keeps the current UI language in
/// <see cref="Current"/> and persists the user's choice to
/// %APPDATA%\FF14AccessibilityInstaller\installer-settings.json so it can be
/// pre-selected (not auto-applied) the next time the language dialog shows.
/// </summary>
public static class Loc
{
    public const string German = "de";
    public const string English = "en";
    public const string Korean = "ko";

    /// <summary>Currently active UI language. The Korean build defaults to Korean.</summary>
    public static string Current { get; set; } = Korean;

    public static string Get(string key)
    {
        if (Texts.TryGetValue(Current, out var dict) && dict.TryGetValue(key, out var value))
            return value;
        // Fallback auf Englisch, nicht Deutsch: was hier durchfaellt, liest sonst
        // ein koreanischer Nutzer als deutschen Satz vor.
        if (Texts[English].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }

    public static string Get(string key, params object?[] args) => string.Format(Get(key), args);

    // ── Sprache erkennen/merken ─────────────────────────────────────────────

    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FF14AccessibilityInstaller");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "installer-settings.json");

    /// <summary>Reads the previously saved language, or null if none was saved yet.</summary>
    public static string? LoadSavedLanguage()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return null;
            var json = File.ReadAllText(SettingsPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("language", out var langProp))
            {
                var lang = langProp.GetString();
                if (lang == German || lang == English || lang == Korean) return lang;
            }
        }
        catch
        {
            // Beschädigte/fehlende Settings-Datei ist kein Fehlerfall - einfach neu fragen.
        }
        return null;
    }

    public static void SaveLanguage(string language)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(new { language });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Nicht kritisch - beim naechsten Start wird einfach wieder gefragt.
        }
    }

    /// <summary>
    /// Detects whether the system UI language is German, via a raw Win32 call
    /// (GetUserDefaultLocaleName) rather than CultureInfo. The project sets
    /// InvariantGlobalization=true (see csproj), which makes .NET's own
    /// CultureInfo.CurrentUICulture always report the invariant culture - so
    /// we bypass that and ask Windows directly.
    /// </summary>
    public static bool SystemLanguageIsGerman()
    {
        try
        {
            var sb = new StringBuilder(85);
            if (GetUserDefaultLocaleName(sb, sb.Capacity) > 0)
                return sb.ToString().StartsWith("de", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // Falls der Aufruf scheitert, bleibt es bei Englisch als Fallback.
        }
        return false;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetUserDefaultLocaleName(StringBuilder lpLocaleName, int cchLocaleName);

    // ── Texte ────────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Dictionary<string, string>> Texts = new()
    {
        [German] = new Dictionary<string, string>
        {
            ["WarnPrefix"] = "Achtung: ",
            ["ErrorPrefix"] = "Fehler: ",
            ["UnknownVersion"] = "unbekannt",

            ["InstallerHeader"] = "FF14 Accessibility – Installer und Updater (Version {0}).",
            ["CheckingXivLauncher"] = "Prüfe XIVLauncher ...",
            ["XivLauncherFound"] = "XIVLauncher gefunden.",
            ["SummaryHeader"] = "=== Zusammenfassung ===",
            ["SummaryAccessibility"] = "FF14 Accessibility: {0}",
            ["SummaryVnavmesh"] = "vnavmesh (Auto-Lauf): {0}",
            ["SummaryDungeonPaths"] = "Dungeon-Wege: {0}",

            // Wegdateien für die Kategorie "Dungeon"
            ["CheckingDungeonPaths"] = "Lade die Wegdateien für die Kategorie „Dungeon\" ...",
            ["DungeonPathsWritten"] = "{0} Wegdateien geschrieben nach {1}",
            ["DungeonPathsSummary"] = "{0} Wege geladen",
            ["DungeonPathsUnreachable"] = "Die Wegdateien sind gerade nicht erreichbar ({0}). Das Plugin holt sie beim ersten Start selbst nach.",
            ["DungeonPathsTimeout"] = "Zeitüberschreitung beim Laden der Wegdateien. Das Plugin holt sie beim ersten Start selbst nach.",
            ["DungeonPathsNothingInArchive"] = "Im geladenen Archiv steckt keine einzige Wegdatei - die Quelle hat ihren Aufbau geändert.",
            ["DungeonPathsArchiveTooBig"] = "Das Archiv ist unerwartet groß und wurde nicht entpackt.",
            ["DungeonPathsUnexpectedError"] = "Die Wegdateien konnten nicht geschrieben werden: {0}",
            ["UnexpectedError"] = "Unerwarteter Fehler: {0}",
            ["NoPartialWrite"] = "Es wurde nichts Unvollständiges geschrieben, das dein System beschädigt.",

            ["XivLauncherNotInstalled1"] = "XIVLauncher ist nicht installiert (Ordner nicht gefunden:",
            ["XivLauncherNotInstalled2"] = "  {0}).",
            ["XivLauncherNeeded"] = "XIVLauncher wird gebraucht, weil es Dalamud lädt – die Grundlage für das Plugin.",
            ["DownloadingXivLauncherAuto"] = "Lade die neueste XIVLauncher-Version herunter und installiere sie automatisch ...",
            ["GitHubUnreachable"] = "GitHub nicht erreichbar ({0}). Bitte Internetverbindung prüfen.",
            ["InstallXivLauncherManually1"] = "Installiere XIVLauncher alternativ manuell von https://goatcorp.github.io/ und",
            ["RunProgramAgain"] = "führe dieses Programm danach erneut aus.",
            ["TimeoutFetchXivLauncher"] = "Zeitüberschreitung beim Abruf der XIVLauncher-Version. Bitte Internetverbindung prüfen.",
            ["NoXivLauncherSetupFound"] = "Kein XIVLauncher-Setup im neuesten Release gefunden.",
            ["InstallXivLauncherManually2"] = "Bitte installiere XIVLauncher manuell von https://goatcorp.github.io/ .",
            ["DownloadingXivLauncherVersion"] = "Lade XIVLauncher {0} herunter ({1}) ...",
            ["DownloadFailedInstallManually"] = "Download fehlgeschlagen ({0}). Bitte installiere XIVLauncher manuell von",
            ["UrlAndRunAgain"] = "https://goatcorp.github.io/ und führe dieses Programm danach erneut aus.",
            ["TimeoutDownloadRetry"] = "Zeitüberschreitung beim Download. Bitte Internetverbindung prüfen und erneut versuchen.",
            ["XivLauncherSaveFailed"] = "XIVLauncher-Setup konnte nicht gespeichert werden: {0}",
            ["InstallingXivLauncherSilent"] = "Installiere XIVLauncher automatisch im Hintergrund (--silent) ...",
            ["XivLauncherInstallStarted"] = "XIVLauncher-Installation wurde gestartet und sollte inzwischen abgeschlossen sein.",
            ["AutoInstallNotConfirmed"] = "Die automatische Installation konnte nicht bestätigt werden ({0}).",
            ["RunSetupManuallyHint"] = "Falls XIVLauncher nicht gestartet ist, führe die Datei manuell aus:",
            ["LoginHint1"] = "Bitte melde dich jetzt im XIVLauncher an, aktiviere in den Einstellungen",
            ["LoginHint2"] = "Dalamud und starte das Spiel EINMAL. Führe diesen Installer danach erneut",
            ["LoginHint3"] = "aus, um das Barrierefreiheits-Plugin einzurichten.",

            // Selbst-Update des Installers
            ["CheckingInstallerVersion"] = "Prüfe, ob es eine neuere Installer-Version gibt ...",
            ["NoInstallerManifest"] = "Keine Installer-Versionsangabe im Release gefunden - überspringe die Prüfung.",
            ["InstallerCheckFailed"] = "Installer-Version konnte nicht geprüft werden ({0}). Der Installer arbeitet normal weiter.",
            ["InstallerManifestUnreadable"] = "Installer-Versionsangabe war nicht lesbar - überspringe die Prüfung.",
            ["InstallerUpToDate"] = "Der Installer ist aktuell (Version {0}).",
            ["InstallerAssetMissing"] = "Die neue Installer-Datei ({0}) fehlt im Release - überspringe das Update.",
            ["InstallerOwnPathUnknown"] = "Eigener Programmpfad nicht ermittelbar - überspringe das Installer-Update.",
            ["InstallerUpdateAvailable"] = "Neue Installer-Version verfügbar: {0} (installiert ist {1}).",
            ["InstallerUpdateQuestion"] =
                "Es gibt eine neuere Version des Installers ({0}).\n\n" +
                "Soll sie jetzt heruntergeladen und gestartet werden? Der Download ist etwa {1} Megabyte groß.\n\n" +
                "Der Installer schließt sich dabei kurz und öffnet sich automatisch neu. " +
                "Danach läuft die Installation von selbst weiter.\n\n" +
                "Ja = jetzt aktualisieren, Nein = mit der aktuellen Version weiterarbeiten.",
            ["InstallerUpdateDeclined"] = "Installer-Update übersprungen. Es geht mit der vorhandenen Version weiter.",
            ["DownloadingInstaller"] = "Lade Installer-Version {0} herunter ...",
            ["InstallerDownloadLabel"] = "Installer",
            ["InstallerDownloadFailed"] = "Installer-Update konnte nicht geladen werden ({0}). Es geht mit der vorhandenen Version weiter.",
            ["InstallerHashOk"] = "Prüfsumme der neuen Installer-Datei stimmt.",
            ["InstallerNoHash"] = "Keine Prüfsumme im Release hinterlegt - Download wird ungeprüft übernommen.",
            ["InstallerHashMismatch"] = "Die Prüfsumme der geladenen Installer-Datei stimmt NICHT. Update abgebrochen, es geht mit der vorhandenen Version weiter.",
            ["InstallerStartFailed"] = "Der neue Installer konnte nicht gestartet werden ({0}). Es geht mit der vorhandenen Version weiter.",
            ["InstallerRestarting"] = "Installer wird auf Version {0} aktualisiert. Das Fenster schließt sich jetzt und öffnet sich gleich automatisch neu ...",
            ["InstallerUpdatedTo"] = "Installer wurde auf Version {0} aktualisiert. Die Installation läuft automatisch weiter.",
            ["InstallerUpdatedMessage"] =
                "Der Installer wurde auf Version {0} aktualisiert und neu gestartet.\n\n" +
                "Die Installation läuft nach dem Bestätigen automatisch weiter - du musst nichts weiter tun.",
            ["SelfUpdateNoOwnPath"] =
                "Der eigene Programmpfad war nicht ermittelbar. Die alte Installer-Datei bleibt bestehen; " +
                "dieses Fenster arbeitet aus dem temporären Ordner weiter.",
            ["SelfUpdateReplaceFailed"] =
                "Die vorhandene Installer-Datei konnte nicht ersetzt werden:\n{0}\n\nGrund: {1}\n\n" +
                "Die Installation kann trotzdem weiterlaufen. Wenn du die neue Version dauerhaft behalten willst, " +
                "lade FF14AccessibilityInstaller.exe einmal von Hand aus dem neuesten Release herunter.",
            ["SelfUpdateRestartFailed"] =
                "Die Installer-Datei wurde aktualisiert, konnte aber nicht automatisch gestartet werden:\n{0}\n\nGrund: {1}\n\n" +
                "Bitte starte sie von Hand.",

            ["CheckingAccessibilityVersion"] = "Prüfe neueste Version von FF14 Accessibility ...",
            ["NoAccessibilityAssetFound"] = "Kein passendes Release-Paket für FF14 Accessibility gefunden.",
            ["ErrorNoReleaseAsset"] = "Fehler (kein Release-Asset gefunden)",
            ["AccessibilityUpToDate"] = "FF14 Accessibility ist aktuell (Version {0}).",
            ["UpToDateShort"] = "aktuell (Version {0})",
            ["DownloadingAccessibility"] = "Lade FF14 Accessibility {0} herunter ...",
            ["AccessibilityUpdated"] = "FF14 Accessibility aktualisiert auf Version {0}.",
            ["AccessibilityInstalled"] = "FF14 Accessibility installiert (Version {0}).",
            ["UpdatedToShort"] = "aktualisiert auf {0}",
            ["NewlyInstalledShort"] = "neu installiert, Version {0}",
            ["CouldNotWritePluginFiles"] = "Konnte Plugin-Dateien nicht schreiben: {0}",
            ["CloseGameAndLauncher"] = "Bitte schließe FINAL FANTASY XIV und den XIVLauncher vollständig und versuche es erneut.",
            ["ErrorFilesLocked"] = "Fehler (Dateien gesperrt – bitte Spiel/Launcher schließen)",
            ["AccessibilityGitHubUnreachable"] = "FF14 Accessibility: GitHub nicht erreichbar ({0}).",
            ["ErrorNoNetworkGitHub"] = "Fehler (kein Netzwerk/GitHub nicht erreichbar)",
            ["AccessibilityDownloadTimeout"] = "FF14 Accessibility: Zeitüberschreitung beim Download.",
            ["ErrorTimeout"] = "Fehler (Zeitüberschreitung)",
            ["AccessibilityUnexpectedError"] = "FF14 Accessibility: Unerwarteter Fehler ({0}).",
            ["ErrorGeneric"] = "Fehler",

            ["CheckingVnavmeshVersion"] = "Prüfe neueste Version von vnavmesh (Auto-Lauf) ...",
            ["VnavmeshPunishUnreachable"] = "vnavmesh: puni.sh nicht erreichbar ({0}). Auto-Lauf bleibt unverändert.",
            ["ErrorNoNetworkPunish"] = "Fehler (kein Netzwerk/puni.sh nicht erreichbar)",
            ["VnavmeshPunishTimeout"] = "vnavmesh: Zeitüberschreitung bei puni.sh.",
            ["VnavmeshNotFound"] = "vnavmesh nicht im puni.sh-Repository gefunden.",
            ["ErrorNotFound"] = "Fehler (nicht gefunden)",
            ["VnavmeshNoDownloadLink"] = "vnavmesh: kein Download-Link im Repository gefunden.",
            ["ErrorNoDownloadLink"] = "Fehler (kein Download-Link)",
            ["VnavmeshUpToDate"] = "vnavmesh ist aktuell (Version {0}).",
            ["AutoWalkNeedsVnav1"] = "Das Auto-Lauf-Feature (automatisch zu Zielen laufen) braucht das separate",
            ["AutoWalkNeedsVnav2"] = "Plugin vnavmesh. Es stammt von einem anderen Autor (veyn) und wird vom",
            ["AutoWalkNeedsVnav3"] = "Original geladen, nicht von uns weitergegeben.",
            ["AskSetupVnavmesh"] = "Soll vnavmesh jetzt für den Auto-Lauf eingerichtet werden?",
            ["VnavmeshSkipped"] = "vnavmesh übersprungen. Alles außer dem Auto-Lauf funktioniert trotzdem.",
            ["SkippedShort"] = "übersprungen",
            ["DownloadingVnavmesh"] = "Lade vnavmesh {0} herunter ...",
            ["VnavmeshUpdated"] = "vnavmesh aktualisiert auf Version {0}.",
            ["VnavmeshSetup"] = "vnavmesh eingerichtet (Version {0}).",
            ["NewlySetupShort"] = "neu eingerichtet, Version {0}",
            ["VnavmeshCouldNotWriteFiles"] = "vnavmesh: Konnte Dateien nicht schreiben: {0}",
            ["VnavmeshDownloadFailed"] = "vnavmesh: Download fehlgeschlagen ({0}).",
            ["ErrorNoNetwork"] = "Fehler (kein Netzwerk)",
            ["VnavmeshDownloadTimeout"] = "vnavmesh: Zeitüberschreitung beim Download.",
            ["VnavmeshUnexpectedError"] = "vnavmesh: Unerwarteter Fehler ({0}).",

            ["ConfigNotExist1"] = "dalamudConfig.json existiert noch nicht – Dalamud legt sie erst beim",
            ["ConfigNotExist2"] = "ersten Spielstart an. Starte das Spiel EINMAL über XIVLauncher (mit",
            ["ConfigNotExist3"] = "aktiviertem Dalamud) und führe diesen Installer danach erneut aus, damit",
            ["ConfigNotExist4"] = "die Plugins aktiviert werden können.",
            ["ConfigMissingReturn"] = "dalamudConfig.json fehlt noch – bitte Spiel einmal starten und Installer erneut ausführen.",
            ["ConfigReadFailed"] = "dalamudConfig.json konnte nicht gelesen werden: {0}",
            ["ConfigReadFailedReturn"] = "Fehler beim Lesen von dalamudConfig.json (Datei gesperrt?).",
            ["ConfigParseFailed"] = "dalamudConfig.json ließ sich nicht lesen ({0}).",
            ["ConfigNotTouching"] = "Ich fasse sie nicht an. Bitte melde dich – hier ist Handarbeit sicherer.",
            ["ConfigInvalidReturn"] = "Fehler: dalamudConfig.json ungültig, nicht verändert.",
            ["ConfigUnexpectedStructure"] = "Unerwarteter Aufbau der Konfiguration (DevPluginLoadLocations fehlt).",
            ["ConfigSafetyNoChange1"] = "Zur Sicherheit wird nichts geändert. Bitte starte das Spiel einmal und",
            ["ConfigSafetyNoChange2"] = "versuche es erneut.",
            ["ConfigUnexpectedStructureReturn"] = "Fehler: unerwarteter Aufbau von dalamudConfig.json, nicht verändert.",
            ["ConfigUpdated"] = "Konfiguration aktualisiert (Sicherung: {0}).",
            ["ProfileStructureUnexpected"] = "Unerwarteter Aufbau der Konfiguration (DefaultProfile fehlt) – Plugins konnten nicht aktiviert werden. Bitte melde dich.",
            ["ProfileStructureUnexpectedReturn"] = "Fehler: Plugins eingetragen, aber Aktivierung nicht möglich (DefaultProfile fehlt).",
            ["PluginsRegisteredEnabledReturn"] = "Plugins eingetragen und aktiviert.",
            ["ConfigWriteFailed"] = "dalamudConfig.json konnte nicht geschrieben werden: {0}",
            ["ConfigWriteFailedReturn"] = "Fehler beim Schreiben von dalamudConfig.json (Datei gesperrt?).",

            ["DownloadProgress"] = "{0}: {1} % ...",

            ["WindowTitle"] = "FF14 Accessibility – Installer",
            ["MainTitleWithVersion"] = "FF14 Accessibility – Installer und Updater (Version {0})",
            ["MainTitleAccessibleName"] = "FF14 Accessibility Installer, Version {0}",
            ["LogBoxAccessibleName"] = "Statusmeldungen",
            ["LogBoxAccessibleDescription"] = "Fortschritts- und Ergebnismeldungen des Installers, mit Pfeiltasten durchgehbar.",
            ["InstallButtonText"] = "Installieren / Aktualisieren",
            ["InstallButtonAccessibleName"] = "Installieren oder Aktualisieren",
            ["ExitButtonText"] = "Beenden",
            ["ExitButtonAccessibleName"] = "Beenden",
            ["OperationCompleted"] = "Vorgang abgeschlossen. Details siehe Log-Bereich im Fenster.",

            ["LanguageDialogTitle"] = "Sprache wählen / Choose language",
            ["LanguageGermanButton"] = "Deutsch",
            ["LanguageEnglishButton"] = "English",
            ["KrCheckingProfile"] = "Pruefe das koreanische Profil ...",
            ["KrProfileRoot"] = "Profilordner: {0}  (entschieden durch: {1})",
            ["KrProfileFound"] = "Profil vorhanden.",
            ["KrProfileCreated"] = "Fehlende Teile angelegt: {0}",
            ["KrRuntimeVariableSet"] = "DALAMUD_RUNTIME gesetzt: {0}",
            ["KrRuntimeNeedsRestart"] = "Das Spiel muss danach neu gestartet werden - Prozesse erben die Umgebung beim Start.",
            ["KrDalamudMissing"] = "Koreanisches Dalamud ist noch nicht installiert.",
            ["KrDalamudGetIt"] = "Bitte zuerst den KR-Dalamud-Updater ausfuehren und \"Check Update\" druecken:",
            ["KrDalamudThenCheckUpdate"] = "Danach dieses Programm erneut starten.",
            ["KrUpdaterWhatItIs1"] = "Der KR-Dalamud-Updater fehlt. Er bringt Dalamud in den koreanischen Client;",
            ["KrUpdaterWhatItIs2"] = "auf der globalen Seite macht das der XIVLauncher. Er stammt von einem anderen",
            ["KrUpdaterWhatItIs3"] = "Autor (MiqoKR) und wird aus dessen Release geladen, nicht von uns weitergegeben.",
            ["AskSetupKrUpdater"] = "Soll der KR-Dalamud-Updater jetzt heruntergeladen werden?",
            ["KrUpdaterSkipped"] = "KR-Dalamud-Updater uebersprungen.",
            ["KrUpdaterCheckingRelease"] = "Suche neuestes Release des KR-Dalamud-Updaters ...",
            ["KrUpdaterDownloading"] = "Lade den KR-Dalamud-Updater herunter ...",
            ["KrUpdaterDownloadLabel"] = "KR-Dalamud-Updater",
            ["KrUpdaterInstalledAt"] = "KR-Dalamud-Updater entpackt nach: {0}",
            ["KrUpdaterLaunched"] = "Der Updater wurde geoeffnet.",
            ["KrUpdaterUnreachable"] = "GitHub nicht erreichbar ({0}). Der Updater muss von Hand geholt werden.",
            ["KrUpdaterTimeout"] = "Zeitueberschreitung bei GitHub. Der Updater muss von Hand geholt werden.",
            ["KrUpdaterNoAsset"] = "Im Release ist kein \"{0}\"-Archiv. Der Updater muss von Hand geholt werden.",
            ["KrUpdaterDownloadFailed"] = "Download des Updaters fehlgeschlagen ({0}).",
            ["KrUpdaterCouldNotWrite"] = "Updater konnte nicht geschrieben werden ({0}).",
            ["KrUpdaterExeMissing"] = "Archiv entpackt, aber es liegt keine EXE hier: {0}",
            ["KrUpdaterUnexpectedError"] = "Unerwarteter Fehler beim Updater ({0}).",
            ["KrLookingForLocalBuild"] = "Suche den lokalen Build des Plugins ...",
            ["KrNoLocalBuild"] = "Kein gebautes Plugin gefunden.",
            ["KrBuildHint"] = "Bitte zuerst run\build.bat ausfuehren.",
            ["KrUsingLocalBuild"] = "Verwende lokalen Build {0} aus {1}",
            ["KrUsingRelease"] = "Verwende Release {0}",
            ["KrReleaseUnreachable"] = "Release konnte nicht abgefragt werden ({0}). Es geht mit dem lokalen Build weiter.",
            ["KrLocalBuildWins"] = "Lokaler Build {0} ist nicht aelter als Release {1} - er wird verwendet.",
            ["KrReleaseDownloadFailed"] = "Download des Releases fehlgeschlagen ({0}).",
            ["KrErrorNoLocalBuild"] = "Fehler: kein lokaler Build vorhanden.",
            ["KrBuildVersionUnreadable"] = "Das gebaute Manifest nennt keine brauchbare Version ({0}). Abbruch - der Ordnername muss eine Version sein.",
            ["KrInstalledAt"] = "Installiert nach: {0}",
            ["KrOldVersionRemoved"] = "Alte Version entfernt: {0}",
            ["KrOldVersionKept"] = "Alte Version blieb liegen: {0} ({1}). Dalamud raeumt sie beim naechsten Start auf.",
            ["KrProfileEntrySeeded"] = "Im Profil eingetragen und aktiviert: {0} ({1})",
            ["KrDevInstallRemoved"] = "Die fruehere Entwickler-Installation wurde entfernt.",
            ["KrDevInstallStuck"] = "Die fruehere Entwickler-Installation liess sich nicht entfernen: {0} ({1}). Spiel schliessen und erneut ausfuehren - sonst laedt dasselbe Plugin zweimal.",
            ["LanguageKoreanButton"] = "한국어",
        },
        [English] = new Dictionary<string, string>
        {
            ["WarnPrefix"] = "Warning: ",
            ["ErrorPrefix"] = "Error: ",
            ["UnknownVersion"] = "unknown",

            ["InstallerHeader"] = "FF14 Accessibility – Installer and Updater (version {0}).",
            ["CheckingXivLauncher"] = "Checking for XIVLauncher ...",
            ["XivLauncherFound"] = "XIVLauncher found.",
            ["SummaryHeader"] = "=== Summary ===",
            ["SummaryAccessibility"] = "FF14 Accessibility: {0}",
            ["SummaryVnavmesh"] = "vnavmesh (auto-walk): {0}",
            ["SummaryDungeonPaths"] = "Dungeon routes: {0}",

            // Route files for the "dungeon" category
            ["CheckingDungeonPaths"] = "Downloading the route files for the \"dungeon\" category ...",
            ["DungeonPathsWritten"] = "Wrote {0} route files to {1}",
            ["DungeonPathsSummary"] = "{0} routes loaded",
            ["DungeonPathsUnreachable"] = "The route files cannot be reached right now ({0}). The plugin will fetch them itself on first start.",
            ["DungeonPathsTimeout"] = "Timed out while downloading the route files. The plugin will fetch them itself on first start.",
            ["DungeonPathsNothingInArchive"] = "The downloaded archive contains no route file at all - the source has changed its layout.",
            ["DungeonPathsArchiveTooBig"] = "The archive is unexpectedly large and was not unpacked.",
            ["DungeonPathsUnexpectedError"] = "The route files could not be written: {0}",
            ["UnexpectedError"] = "Unexpected error: {0}",
            ["NoPartialWrite"] = "Nothing incomplete was written that could damage your system.",

            ["XivLauncherNotInstalled1"] = "XIVLauncher is not installed (folder not found:",
            ["XivLauncherNotInstalled2"] = "  {0}).",
            ["XivLauncherNeeded"] = "XIVLauncher is required because it loads Dalamud, the foundation the plugin runs on.",
            ["DownloadingXivLauncherAuto"] = "Downloading the latest XIVLauncher version and installing it automatically ...",
            ["GitHubUnreachable"] = "GitHub is unreachable ({0}). Please check your internet connection.",
            ["InstallXivLauncherManually1"] = "Alternatively, install XIVLauncher manually from https://goatcorp.github.io/ and",
            ["RunProgramAgain"] = "run this program again afterwards.",
            ["TimeoutFetchXivLauncher"] = "Timed out while checking the XIVLauncher version. Please check your internet connection.",
            ["NoXivLauncherSetupFound"] = "No XIVLauncher setup found in the latest release.",
            ["InstallXivLauncherManually2"] = "Please install XIVLauncher manually from https://goatcorp.github.io/ .",
            ["DownloadingXivLauncherVersion"] = "Downloading XIVLauncher {0} ({1}) ...",
            ["DownloadFailedInstallManually"] = "Download failed ({0}). Please install XIVLauncher manually from",
            ["UrlAndRunAgain"] = "https://goatcorp.github.io/ and run this program again afterwards.",
            ["TimeoutDownloadRetry"] = "Timed out during download. Please check your internet connection and try again.",
            ["XivLauncherSaveFailed"] = "Could not save the XIVLauncher setup file: {0}",
            ["InstallingXivLauncherSilent"] = "Installing XIVLauncher automatically in the background (--silent) ...",
            ["XivLauncherInstallStarted"] = "The XIVLauncher installation was started and should be finished by now.",
            ["AutoInstallNotConfirmed"] = "The automatic installation could not be confirmed ({0}).",
            ["RunSetupManuallyHint"] = "If XIVLauncher did not start, run the file manually:",
            ["LoginHint1"] = "Please log in to XIVLauncher now, enable Dalamud in the settings,",
            ["LoginHint2"] = "and start the game ONCE. Then run this installer again",
            ["LoginHint3"] = "to set up the accessibility plugin.",

            // Installer self-update
            ["CheckingInstallerVersion"] = "Checking whether a newer installer version exists ...",
            ["NoInstallerManifest"] = "No installer version info found in the release - skipping the check.",
            ["InstallerCheckFailed"] = "Could not check the installer version ({0}). Carrying on as usual.",
            ["InstallerManifestUnreadable"] = "The installer version info could not be read - skipping the check.",
            ["InstallerUpToDate"] = "The installer is up to date (version {0}).",
            ["InstallerAssetMissing"] = "The new installer file ({0}) is missing from the release - skipping the update.",
            ["InstallerOwnPathUnknown"] = "Could not determine this program's own path - skipping the installer update.",
            ["InstallerUpdateAvailable"] = "A newer installer version is available: {0} (you have {1}).",
            ["InstallerUpdateQuestion"] =
                "A newer version of the installer is available ({0}).\n\n" +
                "Download and start it now? The download is about {1} megabytes.\n\n" +
                "The installer will close briefly and reopen automatically. " +
                "The installation then continues on its own.\n\n" +
                "Yes = update now, No = keep using the current version.",
            ["InstallerUpdateDeclined"] = "Installer update skipped. Continuing with the current version.",
            ["DownloadingInstaller"] = "Downloading installer version {0} ...",
            ["InstallerDownloadLabel"] = "Installer",
            ["InstallerDownloadFailed"] = "Could not download the installer update ({0}). Continuing with the current version.",
            ["InstallerHashOk"] = "Checksum of the new installer file matches.",
            ["InstallerNoHash"] = "No checksum published in the release - the download is accepted unverified.",
            ["InstallerHashMismatch"] = "The checksum of the downloaded installer does NOT match. Update aborted, continuing with the current version.",
            ["InstallerStartFailed"] = "The new installer could not be started ({0}). Continuing with the current version.",
            ["InstallerRestarting"] = "Updating the installer to version {0}. This window closes now and reopens automatically in a moment ...",
            ["InstallerUpdatedTo"] = "The installer was updated to version {0}. The installation continues automatically.",
            ["InstallerUpdatedMessage"] =
                "The installer was updated to version {0} and restarted.\n\n" +
                "After you confirm, the installation continues automatically - there is nothing else you need to do.",
            ["SelfUpdateNoOwnPath"] =
                "Could not determine this program's own path. The old installer file stays as it is; " +
                "this window continues from the temporary folder.",
            ["SelfUpdateReplaceFailed"] =
                "Could not replace the existing installer file:\n{0}\n\nReason: {1}\n\n" +
                "The installation can still continue. If you want to keep the new version permanently, " +
                "download FF14AccessibilityInstaller.exe manually from the latest release once.",
            ["SelfUpdateRestartFailed"] =
                "The installer file was updated but could not be started automatically:\n{0}\n\nReason: {1}\n\n" +
                "Please start it manually.",

            ["CheckingAccessibilityVersion"] = "Checking for the latest version of FF14 Accessibility ...",
            ["NoAccessibilityAssetFound"] = "No matching release package found for FF14 Accessibility.",
            ["ErrorNoReleaseAsset"] = "Error (no release asset found)",
            ["AccessibilityUpToDate"] = "FF14 Accessibility is up to date (version {0}).",
            ["UpToDateShort"] = "up to date (version {0})",
            ["DownloadingAccessibility"] = "Downloading FF14 Accessibility {0} ...",
            ["AccessibilityUpdated"] = "FF14 Accessibility updated to version {0}.",
            ["AccessibilityInstalled"] = "FF14 Accessibility installed (version {0}).",
            ["UpdatedToShort"] = "updated to {0}",
            ["NewlyInstalledShort"] = "newly installed, version {0}",
            ["CouldNotWritePluginFiles"] = "Could not write plugin files: {0}",
            ["CloseGameAndLauncher"] = "Please close FINAL FANTASY XIV and XIVLauncher completely, then try again.",
            ["ErrorFilesLocked"] = "Error (files locked – please close the game/launcher)",
            ["AccessibilityGitHubUnreachable"] = "FF14 Accessibility: GitHub is unreachable ({0}).",
            ["ErrorNoNetworkGitHub"] = "Error (no network / GitHub unreachable)",
            ["AccessibilityDownloadTimeout"] = "FF14 Accessibility: Timed out during download.",
            ["ErrorTimeout"] = "Error (timed out)",
            ["AccessibilityUnexpectedError"] = "FF14 Accessibility: Unexpected error ({0}).",
            ["ErrorGeneric"] = "Error",

            ["CheckingVnavmeshVersion"] = "Checking for the latest version of vnavmesh (auto-walk) ...",
            ["VnavmeshPunishUnreachable"] = "vnavmesh: puni.sh is unreachable ({0}). Auto-walk remains unchanged.",
            ["ErrorNoNetworkPunish"] = "Error (no network / puni.sh unreachable)",
            ["VnavmeshPunishTimeout"] = "vnavmesh: Timed out contacting puni.sh.",
            ["VnavmeshNotFound"] = "vnavmesh not found in the puni.sh repository.",
            ["ErrorNotFound"] = "Error (not found)",
            ["VnavmeshNoDownloadLink"] = "vnavmesh: no download link found in the repository.",
            ["ErrorNoDownloadLink"] = "Error (no download link)",
            ["VnavmeshUpToDate"] = "vnavmesh is up to date (version {0}).",
            ["AutoWalkNeedsVnav1"] = "The auto-walk feature (walking to targets automatically) needs the separate",
            ["AutoWalkNeedsVnav2"] = "vnavmesh plugin. It comes from a different author (veyn) and is loaded from",
            ["AutoWalkNeedsVnav3"] = "the original source, not redistributed by us.",
            ["AskSetupVnavmesh"] = "Set up vnavmesh now for auto-walk?",
            ["VnavmeshSkipped"] = "vnavmesh skipped. Everything except auto-walk still works.",
            ["SkippedShort"] = "skipped",
            ["DownloadingVnavmesh"] = "Downloading vnavmesh {0} ...",
            ["VnavmeshUpdated"] = "vnavmesh updated to version {0}.",
            ["VnavmeshSetup"] = "vnavmesh set up (version {0}).",
            ["NewlySetupShort"] = "newly set up, version {0}",
            ["VnavmeshCouldNotWriteFiles"] = "vnavmesh: Could not write files: {0}",
            ["VnavmeshDownloadFailed"] = "vnavmesh: Download failed ({0}).",
            ["ErrorNoNetwork"] = "Error (no network)",
            ["VnavmeshDownloadTimeout"] = "vnavmesh: Timed out during download.",
            ["VnavmeshUnexpectedError"] = "vnavmesh: Unexpected error ({0}).",

            ["ConfigNotExist1"] = "dalamudConfig.json does not exist yet – Dalamud only creates it on the",
            ["ConfigNotExist2"] = "first game start. Start the game ONCE via XIVLauncher (with",
            ["ConfigNotExist3"] = "Dalamud enabled) and run this installer again afterwards so",
            ["ConfigNotExist4"] = "the plugins can be enabled.",
            ["ConfigMissingReturn"] = "dalamudConfig.json is still missing – please start the game once and run the installer again.",
            ["ConfigReadFailed"] = "dalamudConfig.json could not be read: {0}",
            ["ConfigReadFailedReturn"] = "Error reading dalamudConfig.json (file locked?).",
            ["ConfigParseFailed"] = "dalamudConfig.json could not be parsed ({0}).",
            ["ConfigNotTouching"] = "I will not touch it. Please get in touch – manual editing is safer here.",
            ["ConfigInvalidReturn"] = "Error: dalamudConfig.json is invalid, not modified.",
            ["ConfigUnexpectedStructure"] = "Unexpected configuration structure (DevPluginLoadLocations missing).",
            ["ConfigSafetyNoChange1"] = "For safety, nothing will be changed. Please start the game once and",
            ["ConfigSafetyNoChange2"] = "try again.",
            ["ConfigUnexpectedStructureReturn"] = "Error: unexpected dalamudConfig.json structure, not modified.",
            ["ConfigUpdated"] = "Configuration updated (backup: {0}).",
            ["ProfileStructureUnexpected"] = "Unexpected configuration structure (DefaultProfile missing) – plugins could not be enabled. Please get in touch.",
            ["ProfileStructureUnexpectedReturn"] = "Error: plugins registered, but enabling was not possible (DefaultProfile missing).",
            ["PluginsRegisteredEnabledReturn"] = "Plugins registered and enabled.",
            ["ConfigWriteFailed"] = "dalamudConfig.json could not be written: {0}",
            ["ConfigWriteFailedReturn"] = "Error writing dalamudConfig.json (file locked?).",

            ["DownloadProgress"] = "{0}: {1} % ...",

            ["WindowTitle"] = "FF14 Accessibility – Installer",
            ["MainTitleWithVersion"] = "FF14 Accessibility – Installer and Updater (version {0})",
            ["MainTitleAccessibleName"] = "FF14 Accessibility Installer, version {0}",
            ["LogBoxAccessibleName"] = "Status messages",
            ["LogBoxAccessibleDescription"] = "Progress and result messages from the installer, navigable with arrow keys.",
            ["InstallButtonText"] = "Install / Update",
            ["InstallButtonAccessibleName"] = "Install or update",
            ["ExitButtonText"] = "Exit",
            ["ExitButtonAccessibleName"] = "Exit",
            ["OperationCompleted"] = "Operation completed. See the log area in the window for details.",

            ["LanguageDialogTitle"] = "Sprache wählen / Choose language",
            ["LanguageGermanButton"] = "Deutsch",
            ["LanguageEnglishButton"] = "English",
            ["KrCheckingProfile"] = "Checking the Korean profile ...",
            ["KrProfileRoot"] = "Profile root: {0}  (decided by: {1})",
            ["KrProfileFound"] = "Profile found.",
            ["KrProfileCreated"] = "Created the missing pieces: {0}",
            ["KrRuntimeVariableSet"] = "DALAMUD_RUNTIME set to: {0}",
            ["KrRuntimeNeedsRestart"] = "The game has to be started fresh after this - a process inherits its environment at launch.",
            ["KrDalamudMissing"] = "Korean Dalamud is not installed yet.",
            ["KrDalamudGetIt"] = "Run the KR Dalamud updater first and press \"Check Update\":",
            ["KrDalamudThenCheckUpdate"] = "Then start this program again.",
            ["KrUpdaterWhatItIs1"] = "The KR Dalamud updater is missing. It puts Dalamud into the Korean client;",
            ["KrUpdaterWhatItIs2"] = "on the global side XIVLauncher does that. It comes from a different author",
            ["KrUpdaterWhatItIs3"] = "(MiqoKR) and is loaded from their release, not redistributed by us.",
            ["AskSetupKrUpdater"] = "Download the KR Dalamud updater now?",
            ["KrUpdaterSkipped"] = "Skipped the KR Dalamud updater.",
            ["KrUpdaterCheckingRelease"] = "Looking for the latest KR Dalamud updater release ...",
            ["KrUpdaterDownloading"] = "Downloading the KR Dalamud updater ...",
            ["KrUpdaterDownloadLabel"] = "KR Dalamud updater",
            ["KrUpdaterInstalledAt"] = "KR Dalamud updater unpacked to: {0}",
            ["KrUpdaterLaunched"] = "The updater has been opened.",
            ["KrUpdaterUnreachable"] = "GitHub is unreachable ({0}). The updater has to be fetched by hand.",
            ["KrUpdaterTimeout"] = "GitHub timed out. The updater has to be fetched by hand.",
            ["KrUpdaterNoAsset"] = "The release carries no \"{0}\" archive. The updater has to be fetched by hand.",
            ["KrUpdaterDownloadFailed"] = "Downloading the updater failed ({0}).",
            ["KrUpdaterCouldNotWrite"] = "Could not write the updater ({0}).",
            ["KrUpdaterExeMissing"] = "The archive extracted, but there is no EXE here: {0}",
            ["KrUpdaterUnexpectedError"] = "Unexpected error while fetching the updater ({0}).",
            ["KrLookingForLocalBuild"] = "Looking for the locally built plugin ...",
            ["KrNoLocalBuild"] = "No built plugin found.",
            ["KrBuildHint"] = "Run run\build.bat first.",
            ["KrUsingLocalBuild"] = "Using local build {0} from {1}",
            ["KrUsingRelease"] = "Using release {0}",
            ["KrReleaseUnreachable"] = "Could not check the release ({0}). Carrying on with the local build.",
            ["KrLocalBuildWins"] = "Local build {0} is not older than release {1} - using the local build.",
            ["KrReleaseDownloadFailed"] = "Downloading the release failed ({0}).",
            ["KrErrorNoLocalBuild"] = "Error: no local build available.",
            ["KrBuildVersionUnreadable"] = "The built manifest carries no usable version ({0}). Stopping - the folder name has to be a version.",
            ["KrInstalledAt"] = "Installed to: {0}",
            ["KrOldVersionRemoved"] = "Removed the old version: {0}",
            ["KrOldVersionKept"] = "Could not remove the old version: {0} ({1}). Dalamud cleans it up on the next start.",
            ["KrProfileEntrySeeded"] = "Registered in the profile and enabled: {0} ({1})",
            ["KrDevInstallRemoved"] = "Removed the earlier dev-plugin installation.",
            ["KrDevInstallStuck"] = "Could not remove the earlier dev-plugin installation: {0} ({1}). Close the game and run this again - otherwise the same plugin loads twice.",
            ["LanguageKoreanButton"] = "한국어",
        },

        [Korean] = new Dictionary<string, string>
        {
            // 한국어는 KR 흐름이 실제로 보여주는 키만 갖는다. 나머지는 Get()이
            // 영어로 떨어뜨린다 - 안 쓰는 문장 130개를 번역해 두면 그게 낡는다.
            ["WarnPrefix"] = "경고: ",
            ["ErrorPrefix"] = "실패: ",
            ["UnknownVersion"] = "알 수 없음",
            ["InstallerHeader"] = "FF14 접근성 모드 설치 프로그램 (한국 서버용, 버전 {0})",

            // 설치 프로그램 자기 업데이트
            ["CheckingInstallerVersion"] = "설치 프로그램의 새 버전이 있는지 확인한다 ...",
            ["NoInstallerManifest"] = "릴리스에 설치 프로그램 버전 정보가 없다 - 확인을 건너뛴다.",
            ["InstallerCheckFailed"] = "설치 프로그램 버전을 확인하지 못했다: {0}. 설치는 그대로 이어진다.",
            ["InstallerManifestUnreadable"] = "설치 프로그램 버전 정보를 읽지 못했다 - 확인을 건너뛴다.",
            ["InstallerUpToDate"] = "설치 프로그램 {0} - 이미 최신이다.",
            ["InstallerAssetMissing"] = "새 설치 프로그램 파일({0})이 릴리스에 없다 - 업데이트를 건너뛴다.",
            ["InstallerOwnPathUnknown"] = "이 프로그램 자신의 경로를 알아내지 못했다 - 설치 프로그램 업데이트를 건너뛴다.",
            ["InstallerUpdateAvailable"] = "새 설치 프로그램 버전이 있다: {0} (설치된 것은 {1}).",
            ["InstallerUpdateQuestion"] =
                "설치 프로그램의 새 버전이 있다({0}).\n\n" +
                "지금 내려받아 실행할까? 내려받는 크기는 약 {1}메가바이트다.\n\n" +
                "설치 프로그램이 잠깐 닫혔다가 자동으로 다시 실행된다. " +
                "그 다음 설치가 알아서 이어진다.\n\n" +
                "예 = 지금 업데이트, 아니오 = 지금 버전 그대로 진행.",
            ["InstallerUpdateDeclined"] = "설치 프로그램 업데이트를 건너뛰었다. 지금 버전 그대로 이어서 한다.",
            ["DownloadingInstaller"] = "설치 프로그램 {0}을 내려받는다 ...",
            ["InstallerDownloadLabel"] = "설치 프로그램",
            ["InstallerDownloadFailed"] = "설치 프로그램 업데이트를 내려받지 못했다: {0}. 지금 버전 그대로 이어서 한다.",
            ["InstallerHashOk"] = "새 설치 프로그램 파일의 체크섬이 맞다.",
            ["InstallerNoHash"] = "릴리스에 체크섬이 없다 - 내려받은 파일을 검사 없이 쓴다.",
            ["InstallerHashMismatch"] = "내려받은 설치 프로그램의 체크섬이 맞지 않는다. 업데이트를 중단하고 지금 버전 그대로 이어서 한다.",
            ["InstallerStartFailed"] = "새 설치 프로그램을 실행하지 못했다: {0}. 지금 버전 그대로 이어서 한다.",
            ["InstallerRestarting"] = "설치 프로그램을 {0}으로 업데이트한다. 이 창은 지금 닫히고 잠시 뒤 자동으로 다시 실행된다 ...",
            ["InstallerUpdatedTo"] = "설치 프로그램을 {0}으로 업데이트했다. 설치는 자동으로 이어진다.",
            ["InstallerUpdatedMessage"] =
                "설치 프로그램을 {0}으로 업데이트하고 다시 실행했다.\n\n" +
                "확인을 누르면 설치가 자동으로 이어진다 - 더 할 일은 없다.",
            ["SelfUpdateNoOwnPath"] =
                "이 프로그램 자신의 경로를 알아내지 못했다. 옛 설치 프로그램 파일은 그대로 남고, " +
                "이 창은 임시 폴더에서 이어서 동작한다.",
            ["SelfUpdateReplaceFailed"] =
                "기존 설치 프로그램 파일을 바꾸지 못했다:\n{0}\n\n이유: {1}\n\n" +
                "설치는 그대로 이어서 할 수 있다. 새 버전을 계속 쓰려면 " +
                "최신 릴리스에서 FF14AccessibilityInstaller-KR.exe를 한 번 직접 받는다.",
            ["SelfUpdateRestartFailed"] =
                "설치 프로그램 파일은 업데이트했는데 자동으로 실행하지 못했다:\n{0}\n\n이유: {1}\n\n" +
                "직접 실행한다.",

            ["KrCheckingProfile"] = "한국 서버 프로필을 확인한다 ...",
            ["KrProfileRoot"] = "프로필 루트: {0}  (정한 곳: {1})",
            ["KrProfileFound"] = "프로필이 있다.",
            ["KrProfileCreated"] = "없던 것을 만들었다: {0}",
            ["KrRuntimeVariableSet"] = "DALAMUD_RUNTIME을 걸었다: {0}",
            ["KrRuntimeNeedsRestart"] = "이 값은 게임을 새로 켜야 반영된다. 프로세스는 시작할 때 환경을 물고 간다.",
            ["KrDalamudMissing"] = "한국 서버용 Dalamud가 아직 없다.",
            ["KrDalamudGetIt"] = "먼저 KR Dalamud 업데이터를 실행해 \"Check Update\"를 누른다:",
            ["KrDalamudThenCheckUpdate"] = "그 다음 이 프로그램을 다시 실행한다.",
            ["KrUpdaterWhatItIs1"] = "KR Dalamud 업데이터가 없다. 게임에 Dalamud를 붙여 주는 프로그램이다.",
            ["KrUpdaterWhatItIs2"] = "이 프로그램은 우리가 만든 것이 아니고 재배포하지도 않는다 - 원 저장소에서 직접 받는다.",
            ["KrUpdaterWhatItIs3"] = "이것이 없으면 모드를 설치해도 게임에 올라오지 않는다.",
            ["AskSetupKrUpdater"] = "KR Dalamud 업데이터를 지금 받을까?",
            ["KrUpdaterSkipped"] = "KR Dalamud 업데이터를 건너뛰었다.",
            ["KrUpdaterCheckingRelease"] = "KR Dalamud 업데이터의 최신 판을 확인한다 ...",
            ["KrUpdaterDownloading"] = "KR Dalamud 업데이터를 내려받는다 ...",
            ["KrUpdaterDownloadLabel"] = "KR Dalamud 업데이터",
            ["KrUpdaterInstalledAt"] = "KR Dalamud 업데이터를 풀었다: {0}",
            ["KrUpdaterLaunched"] = "업데이터를 열었다.",
            ["KrUpdaterUnreachable"] = "GitHub에 연결하지 못했다: {0}. 업데이터는 직접 받아야 한다.",
            ["KrUpdaterTimeout"] = "GitHub 응답이 없다. 업데이터는 직접 받아야 한다.",
            ["KrUpdaterNoAsset"] = "최신 판에 \"{0}\" 압축이 없다. 업데이터는 직접 받아야 한다.",
            ["KrUpdaterDownloadFailed"] = "업데이터를 내려받지 못했다: {0}",
            ["KrUpdaterCouldNotWrite"] = "업데이터를 쓰지 못했다: {0}",
            ["KrUpdaterExeMissing"] = "압축은 풀렸는데 실행 파일이 여기 없다: {0}",
            ["KrUpdaterUnexpectedError"] = "업데이터를 받다가 예상 못 한 오류가 났다: {0}",

            ["KrLookingForLocalBuild"] = "빌드해 둔 플러그인을 찾는다 ...",
            ["KrNoLocalBuild"] = "빌드된 플러그인이 없다.",
            ["KrBuildHint"] = "먼저 run\build.bat을 실행한다.",
            ["KrUsingLocalBuild"] = "로컬 빌드 {0}을 쓴다 ({1})",
            ["KrUsingRelease"] = "릴리스 {0}을 쓴다",
            ["KrReleaseUnreachable"] = "릴리스를 확인하지 못했다: {0}. 로컬 빌드로 이어서 한다.",
            ["KrLocalBuildWins"] = "로컬 빌드 {0}이 릴리스 {1}보다 낮지 않다 - 로컬 빌드를 쓴다.",
            ["KrReleaseDownloadFailed"] = "릴리스를 내려받지 못했다: {0}",
            ["KrErrorNoLocalBuild"] = "실패: 빌드된 플러그인이 없다.",
            ["KrBuildVersionUnreadable"] = "빌드된 매니페스트에서 버전을 읽지 못했다({0}). 폴더 이름이 버전이어야 해서 여기서 멈춘다.",
            ["KrInstalledAt"] = "설치한 곳: {0}",
            ["KrOldVersionRemoved"] = "옛 판을 지웠다: {0}",
            ["KrOldVersionKept"] = "옛 판을 못 지웠다: {0} ({1}). Dalamud가 다음 기동에 치운다.",
            ["KrProfileEntrySeeded"] = "프로필에 등록하고 켰다: {0} ({1})",
            ["KrDevInstallRemoved"] = "예전 개발용 설치를 걷어냈다.",
            ["KrDevInstallStuck"] = "예전 개발용 설치를 못 지웠다: {0} ({1}). 게임을 끄고 다시 실행한다. 그대로 두면 같은 모드가 두 번 적재된다.",

            ["CheckingAccessibilityVersion"] = "접근성 모드 버전을 확인한다 ...",
            ["NoAccessibilityAssetFound"] = "릴리스에 접근성 모드 압축이 없다.",
            ["AccessibilityUpToDate"] = "접근성 모드 {0} - 이미 최신이다.",
            ["DownloadingAccessibility"] = "접근성 모드 {0}을 내려받는다 ...",
            ["AccessibilityUpdated"] = "접근성 모드를 {0}으로 갱신했다.",
            ["AccessibilityInstalled"] = "접근성 모드 {0}을 설치했다.",
            ["UpdatedToShort"] = "{0}으로 갱신",
            ["NewlyInstalledShort"] = "{0} 새로 설치",
            ["UpToDateShort"] = "{0} - 이미 최신",
            ["CouldNotWritePluginFiles"] = "플러그인 파일을 쓰지 못했다: {0}",
            ["CloseGameAndLauncher"] = "FINAL FANTASY XIV와 업데이터를 완전히 끄고 다시 시도한다.",
            ["ErrorFilesLocked"] = "실패: 파일이 잠겨 있다.",
            ["AccessibilityUnexpectedError"] = "예상 못 한 오류: {0}",
            ["ErrorGeneric"] = "실패",
            ["ErrorTimeout"] = "실패: 시간 초과",
            ["ErrorNotFound"] = "실패: 찾지 못함",

            ["CheckingVnavmeshVersion"] = "vnavmesh 버전을 확인한다 ...",
            ["VnavmeshPunishUnreachable"] = "puni.sh 저장소에 닿지 못했다: {0}",
            ["ErrorNoNetworkPunish"] = "실패: puni.sh에 연결하지 못함",
            ["VnavmeshPunishTimeout"] = "puni.sh 응답이 시간 초과됐다.",
            ["VnavmeshNotFound"] = "저장소 목록에 vnavmesh가 없다.",
            ["VnavmeshNoDownloadLink"] = "vnavmesh 내려받기 주소가 없다.",
            ["ErrorNoDownloadLink"] = "실패: 내려받기 주소 없음",
            ["VnavmeshUpToDate"] = "vnavmesh {0} - 이미 최신이다.",
            ["AutoWalkNeedsVnav1"] = "자동 이동은 vnavmesh 플러그인이 있어야 동작한다.",
            ["AutoWalkNeedsVnav2"] = "이 플러그인은 우리가 만든 것이 아니고 재배포하지도 않는다 - 원 저장소에서 직접 받는다.",
            ["AutoWalkNeedsVnav3"] = "설치하지 않으면 자동 이동만 빠지고 나머지는 그대로 동작한다.",
            ["AskSetupVnavmesh"] = "vnavmesh를 설치할까?",
            ["VnavmeshSkipped"] = "vnavmesh를 건너뛰었다.",
            ["SkippedShort"] = "건너뜀",
            ["DownloadingVnavmesh"] = "vnavmesh {0}을 내려받는다 ...",
            ["VnavmeshUpdated"] = "vnavmesh를 {0}으로 갱신했다.",
            ["VnavmeshSetup"] = "vnavmesh {0}을 설치했다.",
            ["NewlySetupShort"] = "{0} 새로 설치",
            ["VnavmeshCouldNotWriteFiles"] = "vnavmesh 파일을 쓰지 못했다: {0}",
            ["VnavmeshDownloadFailed"] = "vnavmesh를 내려받지 못했다: {0}",
            ["ErrorNoNetwork"] = "실패: 연결하지 못함",
            ["VnavmeshDownloadTimeout"] = "vnavmesh 내려받기가 시간 초과됐다.",
            ["VnavmeshUnexpectedError"] = "vnavmesh 처리 중 예상 못 한 오류: {0}",

            ["ConfigNotExist1"] = "dalamudConfig.json이 없다.",
            ["ConfigNotExist2"] = "게임을 한 번 실행해 Dalamud를 붙이면 만들어진다.",
            ["ConfigNotExist3"] = "그 다음 이 프로그램을 다시 실행한다.",
            ["ConfigNotExist4"] = "",
            ["ConfigMissingReturn"] = "실패: dalamudConfig.json이 없다.",
            ["ConfigReadFailed"] = "dalamudConfig.json을 읽지 못했다: {0}",
            ["ConfigReadFailedReturn"] = "실패: dalamudConfig.json을 읽지 못함",
            ["ConfigParseFailed"] = "dalamudConfig.json을 해석하지 못했다: {0}",
            ["ConfigNotTouching"] = "손대지 않는다 - 망가뜨리는 쪽이 더 비싸다.",
            ["ConfigInvalidReturn"] = "실패: dalamudConfig.json이 깨져 있다.",
            ["ConfigUnexpectedStructure"] = "설정 구조가 예상과 다르다.",
            ["ConfigSafetyNoChange1"] = "안전을 위해 아무것도 바꾸지 않았다.",
            ["ConfigSafetyNoChange2"] = "게임을 한 번 실행한 뒤 다시 시도한다.",
            ["ConfigUnexpectedStructureReturn"] = "실패: 설정 구조가 예상과 다름",
            ["ConfigUpdated"] = "설정을 갱신했다. 백업: {0}",
            ["ProfileStructureUnexpected"] = "DefaultProfile이 없어 플러그인을 켜지 못했다.",
            ["ProfileStructureUnexpectedReturn"] = "실패: 등록은 했으나 켜지 못함 (DefaultProfile 없음)",
            ["PluginsRegisteredEnabledReturn"] = "플러그인을 등록하고 켰다.",
            ["ConfigWriteFailed"] = "dalamudConfig.json을 쓰지 못했다: {0}",
            ["ConfigWriteFailedReturn"] = "실패: dalamudConfig.json을 쓰지 못함 (파일이 잠겼나?)",

            ["SummaryHeader"] = "== 결과 ==",
            ["SummaryAccessibility"] = "접근성 모드: {0}",
            ["SummaryVnavmesh"] = "vnavmesh: {0}",
            ["UnexpectedError"] = "예상 못 한 오류: {0}",
            ["NoPartialWrite"] = "중간까지 쓰다 만 것은 없다.",
            ["DownloadProgress"] = "{0}: {1} 퍼센트 ...",

            ["WindowTitle"] = "FF14 접근성 모드 설치 프로그램 (한국 서버)",
            ["MainTitleWithVersion"] = "FF14 접근성 모드 설치 프로그램, 한국 서버용 (버전 {0})",
            ["MainTitleAccessibleName"] = "FF14 접근성 모드 설치 프로그램, 한국 서버용, 버전 {0}",
            ["LogBoxAccessibleName"] = "진행 상황",
            ["LogBoxAccessibleDescription"] = "설치 프로그램의 진행과 결과 메시지. 화살표 키로 읽는다.",
            ["InstallButtonText"] = "설치 / 갱신",
            ["InstallButtonAccessibleName"] = "설치하거나 갱신한다",
            ["ExitButtonText"] = "끝내기",
            ["ExitButtonAccessibleName"] = "끝내기",
            ["OperationCompleted"] = "끝났다. 자세한 것은 창의 진행 상황 영역에서 읽는다.",

            ["LanguageDialogTitle"] = "언어 선택 / Choose language",
            ["LanguageGermanButton"] = "Deutsch",
            ["LanguageEnglishButton"] = "English",
            ["LanguageKoreanButton"] = "한국어",
        },
    };
}
