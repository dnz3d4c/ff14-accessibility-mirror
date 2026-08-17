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

    public static void Run(bool bootstrap)
    {
        // WinExe has no console of its own; borrow the caller's, or make one.
        if (!AttachConsole(-1)) AllocConsole();

        var overridden = Environment.GetEnvironmentVariable(KrProfile.RootOverrideVariable);
        if (!string.IsNullOrWhiteSpace(overridden))
            Console.WriteLine($"[!! ] {KrProfile.RootOverrideVariable} is set - using {overridden}");

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

        Console.WriteLine();
        Console.WriteLine("base directory: " + AppContext.BaseDirectory);
    }

    private static void Line(string label, string path, bool present)
        => Console.WriteLine($"[{(present ? "OK " : "-- ")}] {label,-20} {path}");
}
