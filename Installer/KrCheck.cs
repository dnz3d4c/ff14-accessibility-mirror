using System.Runtime.InteropServices;

namespace FF14AccessibilityInstaller;

/// <summary>
/// Diagnosis without the GUI. Prints every path the installer resolves and
/// whether it is there, then exits.
///
/// This exists because the installer is a window: when it does nothing useful,
/// the reason is always one of these paths, and there is no way to see them
/// from inside a WinForms dialog. Also the only way to verify the resolution
/// automatically.
/// </summary>
internal static class KrCheck
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    /// <summary>
    /// Runs the whole installation without the window, and says whether the
    /// files ended up where Dalamud looks for them.
    ///
    /// Two reasons this exists. It is the only way to exercise the install path
    /// automatically (tools/pack-check drives it against a throwaway profile via
    /// FF14ACC_KR_PROFILE), and it is the only way to install without a mouse.
    /// The GUI stays the path users take - this changes nothing about it.
    ///
    /// Returns a process exit code: 0 when an installed copy is on disk afterwards.
    /// </summary>
    public static int RunInstall(bool skipVnavmesh)
    {
        if (!AttachConsole(-1)) AllocConsole();

        var service = new InstallerService { SkipVnavmesh = skipVnavmesh, SkipSelfUpdate = true };
        service.LogMessage += Console.WriteLine;
        // The GUI asks this one in a MessageBox. Headless means yes - somebody
        // who typed --install is not waiting to be asked.
        service.AskYesNo = _ => true;

        service.RunAsync().GetAwaiter().GetResult();

        Console.WriteLine();
        var installed = InstallerService.InstalledCopyPath();
        Line("installed copy", installed ?? "(not found)", installed != null);
        return installed != null ? 0 : 1;
    }

    public static void Run(bool bootstrap)
    {
        // WinExe has no console of its own; borrow the caller's, or make one.
        if (!AttachConsole(-1)) AllocConsole();

        // Which of the three decided the root, always - not just when it was
        // overridden. A root that silently disagrees with the updater's own
        // setting is the one failure that produces no error anywhere.
        Console.WriteLine($"[   ] {"root decided by",-20} {KrProfile.RootSource()}");
        Line("updater settings", KrProfile.UpdaterSettingsPath,
             File.Exists(KrProfile.UpdaterSettingsPath));

        if (bootstrap)
        {
            var created = KrProfile.Bootstrap();
            Console.WriteLine(created.Count > 0
                ? "created: " + string.Join(", ", created)
                : "created: nothing was missing");

            var setRuntime = KrProfile.EnsureRuntimeVariable();
            Console.WriteLine(setRuntime != null
                ? "DALAMUD_RUNTIME set to " + setRuntime
                : "DALAMUD_RUNTIME left alone");
            Console.WriteLine();
        }

        Line("profile root", KrProfile.Root, Directory.Exists(KrProfile.Root));
        Line("devPlugins", KrProfile.DevPluginsRoot, Directory.Exists(KrProfile.DevPluginsRoot));
        Line("installedPlugins", KrProfile.InstalledPluginsRoot, Directory.Exists(KrProfile.InstalledPluginsRoot));
        Line("dalamudConfig.json", KrProfile.ConfigPath, File.Exists(KrProfile.ConfigPath));
        Line("dalamud hooks", Path.Combine(KrProfile.Root, "addon", "Hooks"), KrProfile.DalamudInstalled);
        Line("kr updater", KrProfile.UpdaterPath, File.Exists(KrProfile.UpdaterPath));

        var runtime = Environment.GetEnvironmentVariable("DALAMUD_RUNTIME", EnvironmentVariableTarget.User);
        Line("DALAMUD_RUNTIME", runtime ?? "(not set)", !string.IsNullOrWhiteSpace(runtime));

        var build = KrProfile.FindLocalBuild();
        Line("plugin build", build ?? "(not found)", build != null);

        // Where the plugin actually is, not where it should be. A dev leftover
        // beside the installed copy means the same plugin loads twice, and that
        // is invisible from inside the game until the commands collide.
        var installed = InstallerService.InstalledCopyPath();
        Line("installed copy", installed ?? "(not found)", installed != null);
        var devLeftover = Path.Combine(KrProfile.DevPluginsRoot, "FF14Accessibility");
        if (Directory.Exists(devLeftover))
            Console.WriteLine($"[!! ] {"dev leftover",-20} {devLeftover}  <- loads a second copy, remove it");

        Console.WriteLine();
        Console.WriteLine("base directory: " + AppContext.BaseDirectory);
    }

    private static void Line(string label, string path, bool present)
        => Console.WriteLine($"[{(present ? "OK " : "-- ")}] {label,-20} {path}");
}
