using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

// Root namespace on purpose: nothing has to reference this file for it to work.
namespace FF14Accessibility;

/// <summary>
/// Keeps node visibility working on the Korean client.
///
/// WORKAROUND: ClientStructs declares
/// <c>[MemberFunction("E8 ?? ?? ?? ?? 3C 01 75 7F")] AtkResNode.IsVisible()</c> -
/// a pure game call with no managed body. That byte pattern does not exist in
/// the Korean game binary, so the resolver leaves the address null and EVERY
/// call throws InvalidOperationException. Measured on KR 2026.08.05.0000.0000:
/// title menu navigation, whole-window reading and the Ctrl+F5 node dump all
/// died this way, while everything that does not touch node visibility kept
/// working.
///
/// The clean path - call the game's own function - is unavailable at the
/// address level, not merely at the call level. Neither ClientStructs 7.51 nor
/// 7.55 offers a second way to ask; the signature is identical in both, so this
/// is a Korean-binary difference, not a version difference.
///
/// Rather than rewrite 60 call sites, this replaces the resolved address with a
/// managed implementation. ClientStructs reads Addresses.IsVisible.Value on
/// every call and Address is a sealed class whose Value field is writable, so
/// one assignment fixes all call sites at once.
///
/// SELF-GATING: the address is only replaced when it is null. On a client where
/// the signature resolves - every non-Korean one - this file does nothing and
/// the game's own function is used unchanged.
///
/// SEMANTICS ARE APPROXIMATE. What the game function does beyond reading flags
/// is not documented anywhere available. The fallback walks the parent chain
/// requiring NodeFlags.Visible on the node and every ancestor. Evidence for the
/// flag being the right bit: ClientStructs itself defined
/// <c>IsVisible => NodeFlags.HasFlag(NodeFlags.Visible)</c> until commit
/// acd5de8d (2024-06-11) replaced it with the game call. The ancestor walk is
/// added because a node under a hidden parent is not on screen; that part is
/// reasoned, not verified.
/// </summary>
internal static unsafe class NodeVisibilityCompat
{
    /// <summary>Whether the managed fallback took over. False on clients where
    /// the game's own function resolved.</summary>
    internal static bool FallbackInstalled { get; private set; }

    /// <summary>
    /// Runs when the plugin assembly loads, before any service is constructed,
    /// so no existing file has to call it.
    /// </summary>
    // CA2255 warns that ModuleInitializer belongs in application code. That is
    // exactly what this is - a Dalamud plugin assembly, not a library someone
    // else references. Running here is the point: the address must be usable
    // before the first service touches a node, and doing it from a constructor
    // would mean editing a file upstream changes constantly.
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Install()
    {
        var address = AtkResNode.Addresses.IsVisible;
        if (address.Value != nint.Zero) return;

        address.Value = (nint)(delegate* unmanaged<AtkResNode*, int>)&IsVisibleFallback;
        FallbackInstalled = true;
    }

    /// <summary>
    /// Returns int rather than bool: the caller's function pointer declares
    /// bool, and returning a full 32-bit 0/1 is correct whether the runtime
    /// reads AL or EAX. A byte return would leave the upper bits undefined.
    /// </summary>
    [UnmanagedCallersOnly]
    private static int IsVisibleFallback(AtkResNode* node)
    {
        if (node == null) return 0;

        for (var current = node; current != null; current = current->ParentNode)
        {
            if ((current->NodeFlags & NodeFlags.Visible) == 0) return 0;
        }

        return 1;
    }
}
