using System.Collections.Generic;
using Dalamud.Plugin.Services;

namespace FF14Accessibility;

/// <summary>Where an answer the game normally gives comes from now.</summary>
internal enum CompatSource
{
    /// <summary>The game's own function, resolved by ClientStructs.</summary>
    GameFunction,

    /// <summary>The game's own function, found by a Korean-only signature after
    /// ClientStructs failed. Same answer, different way of locating it.</summary>
    KoreanSignature,

    /// <summary>Rebuilt inside the mod. The answer can differ from the game's.</summary>
    Emulated,
}

/// <summary>
/// One place that says out loud which compatibility paths are live.
///
/// The mod is used by people who cannot see the screen, so a fallback that
/// quietly changes an answer is worse than one that fails loudly: there is no
/// way to notice it from the outside. Every path therefore reports itself.
///
/// - the log always gets a line per path, including "the game answers this one"
/// - speech gets ONE line at startup, and only for paths whose answer can
///   differ from the game's. A Korean signature that found the game's own
///   function changes nothing for the user, so it stays in the log.
/// - "/acc compat" says the full state on demand, so nothing has to be
///   remembered from the startup announcement
///
/// Strings live here rather than in AccessibilityStrings because this file is
/// Korean-overlay only. Upstream rewrites that file constantly and every line we
/// add there is a merge conflict later.
/// </summary>
internal static class CompatReport
{
    /// <summary>Runs the probes. Call before the first service is built - the
    /// node visibility path has to be in place before anything reads a node.</summary>
    internal static void Install(ISigScanner scanner, IPluginLog log)
    {
        NodeVisibilityCompat.Install(scanner, log);
        GearsetMarkCompat.Probe(log);

        // The probes log their own findings; this is the summary line, and it is
        // written even when nothing was replaced, so the log always answers
        // "was compatibility involved?".
        log.Information($"[Compat] node visibility: {NodeVisibilityCompat.Source} "
                        + $"(0x{NodeVisibilityCompat.Address:X}), gearset mark: "
                        + $"{(GearsetMarkCompat.AnswersByItemId ? "item id" : "game function")}.");
    }

    /// <summary>What to say once at startup, or null when every answer is the
    /// game's own.</summary>
    internal static string? StartupNotice
    {
        get
        {
            var notes = new List<string>();
            if (NodeVisibilityCompat.Source == CompatSource.Emulated)
                notes.Add(Loc.IsGerman
                    ? "Sichtbarkeit von Elementen wird nachgebildet."
                    : "Element visibility is emulated.");
            if (GearsetMarkCompat.AnswersByItemId)
                notes.Add(Loc.IsGerman
                    ? "Ausrüstungsset-Markierung geht nach Gegenstands-ID."
                    : "Gearset marks go by item ID.");

            if (notes.Count == 0) return null;
            var prefix = Loc.IsGerman ? "Kompatibilitätshinweis: " : "Compatibility note: ";
            return prefix + string.Join(" ", notes);
        }
    }

    /// <summary>Full state for "/acc compat" - always says something.</summary>
    internal static string OnDemand
    {
        get
        {
            var visibility = NodeVisibilityCompat.Source switch
            {
                CompatSource.GameFunction => Loc.IsGerman ? "Spielfunktion" : "the game's own function",
                CompatSource.KoreanSignature => Loc.IsGerman
                    ? "Spielfunktion über koreanische Signatur"
                    : "the game's own function via the Korean signature",
                _ => Loc.IsGerman ? "im Mod nachgebildet" : "emulated inside the mod",
            };
            var gearset = GearsetMarkCompat.AnswersByItemId
                ? (Loc.IsGerman ? "nach Gegenstands-ID" : "by item ID")
                : (Loc.IsGerman ? "Spielfunktion" : "the game's own function");

            return Loc.IsGerman
                ? $"Sichtbarkeit: {visibility}. Ausrüstungsset-Markierung: {gearset}."
                : $"Visibility: {visibility}. Gearset marks: {gearset}.";
        }
    }
}
