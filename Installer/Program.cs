namespace FF14AccessibilityInstaller;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var args = Environment.GetCommandLineArgs();

        // --check sagt nur, was der Installer vorfindet; --bootstrap legt zusaetzlich
        // die fehlenden Profilteile an. Beide beenden sich danach. Die GUI kann man
        // nicht ohne Maus/Bildschirm pruefen, diese Aufloesung schon - und sie ist
        // genau das, was bei "es passiert nichts" fehlt.
        var check = Array.IndexOf(args, "--check") >= 0;
        var bootstrap = Array.IndexOf(args, "--bootstrap") >= 0;
        if (check || bootstrap)
        {
            KrCheck.Run(bootstrap);
            return;
        }

        // --install laeuft denselben Ablauf wie die GUI, nur ohne Fenster, und
        // setzt einen Exit-Code. Damit ist der Installationsweg automatisch
        // pruefbar (tools/pack-check) - die GUI selbst ist es nicht.
        if (Array.IndexOf(args, "--install") >= 0)
        {
            Loc.Current = Loc.LoadSavedLanguage() ?? DefaultLanguage();
            Environment.ExitCode = KrCheck.RunInstall(Array.IndexOf(args, "--skip-vnavmesh") >= 0);
            return;
        }

        ApplicationConfiguration.Initialize();

        // Phase 2 des Selbst-Updates: aus %TEMP% gestartet, ersetzt die
        // Original-EXE und startet sie neu (siehe SelfUpdate). Läuft ohne GUI
        // und ohne Sprachdialog durch - die Sprache ist bereits gespeichert.
        var applyIndex = Array.IndexOf(args, SelfUpdate.ApplyUpdateArg);
        if (applyIndex >= 0 && applyIndex + 2 < args.Length)
        {
            Loc.Current = Loc.LoadSavedLanguage() ?? DefaultLanguage();
            if (SelfUpdate.ApplyUpdate(args[applyIndex + 1], args[applyIndex + 2]))
                return;   // Original-EXE läuft jetzt - dieser Temp-Prozess ist fertig.

            // Ersetzen fehlgeschlagen: Nutzer wurde informiert, wir zeigen die
            // GUI aus dem temporären Ordner, damit er trotzdem installieren kann.
            Application.Run(new MainForm(justUpdated: true));
            return;
        }

        // Reste früherer Updates aufräumen (je ~160 MB in %TEMP%). Nicht im
        // --apply-update-Zweig, denn dort läuft eine dieser Dateien gerade selbst.
        SelfUpdate.CleanupLeftovers();

        // Frisch aktualisiert gestartet: Sprachdialog überspringen (sonst müsste
        // der Nutzer ihn wegen des Updates ein zweites Mal beantworten) und die
        // Installation direkt weiterlaufen lassen.
        if (Array.IndexOf(args, SelfUpdate.UpdatedArg) >= 0)
        {
            Loc.Current = Loc.LoadSavedLanguage() ?? DefaultLanguage();
            Application.Run(new MainForm(justUpdated: true));
            return;
        }

        var saved = Loc.LoadSavedLanguage();
        var preselect = saved ?? DefaultLanguage();

        using var languageDialog = new LanguageDialog(preselect);
        languageDialog.ShowDialog();
        Loc.Current = languageDialog.SelectedLanguage;
        Loc.SaveLanguage(Loc.Current);

        Application.Run(new MainForm());
    }

    /// <summary>
    /// Vorauswahl im Sprachdialog. Der KR-Build faellt auf Koreanisch zurueck,
    /// nicht auf Englisch - wer ihn benutzt, spielt auf dem koreanischen Client.
    /// </summary>
    private static string DefaultLanguage()
        => Loc.SystemLanguageIsGerman() ? Loc.German : Loc.Korean;
}
