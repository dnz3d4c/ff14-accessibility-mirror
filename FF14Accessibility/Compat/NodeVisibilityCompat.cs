using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

// Root namespace on purpose: nothing has to reference this file for it to work.
namespace FF14Accessibility;

/// <summary>
/// Keeps node visibility working on the Korean client.
///
/// WORKAROUND: ClientStructs declares
/// <c>[MemberFunction("E8 ?? ?? ?? ?? 3C 01 75 7F")] AtkResNode.IsVisible()</c> -
/// a pure game call with no managed body. That call-site pattern does not exist
/// in the Korean binary (0 matches in ffxiv_dx11.exe 2026.08.05.0000.0000,
/// checked offline with tools/sig-probe), so the resolver leaves the address
/// null and EVERY one of the 60 call sites throws InvalidOperationException.
/// Measured in game: title menu navigation, whole-window reading and the
/// Ctrl+F5 node dump all died this way, while everything that does not touch
/// node visibility kept working.
///
/// The function itself IS in the Korean binary - only the call site upstream
/// matches on differs. It was found by locality (it sits inside the run of
/// AtkResNode accessors, at an address no other ClientStructs signature claims)
/// and identified by its body, which is 38 bytes long:
///
///   48 85 C9                          test  rcx, rcx            ; null -> false
///   74 1E                             je    false
///   F7 81 AC 00 00 00  00 00 10 00    test  [rcx+0xAC], 0x100000 ; NodeFlags.Visible
///   74 12                             je    false
///   F7 81 B0 00 00 00  00 00 04 00    test  [rcx+0xB0], 0x40000  ; DrawFlags bit 18
///   75 06                             jne   false
///   B8 01 00 00 00                    mov   eax, 1
///   C3                                ret
///   32 C0                             xor   al, al
///   C3                                ret
///
/// The two displacements are NodeFlags at 0xAE and DrawFlags at 0xB0 as
/// ClientStructs 7.51 declares them (the dword test at 0xAC reaches the flags
/// halfword at 0xAE, and 0x00100000 there is NodeFlags.Visible = 0x10).
///
/// So the mod does not have to reimplement anything: it hands ClientStructs the
/// address it failed to find. ClientStructs reads Addresses.IsVisible.Value on
/// every call and Address is a sealed class whose Value field is writable, so
/// one assignment fixes all 60 call sites at once.
///
/// SELF-GATING, three ways: the address is only replaced when ClientStructs left
/// it null, the Korean signature is only used when it matches EXACTLY once, and
/// the managed replica is only installed when that scan fails. On every
/// non-Korean client this file does nothing at all.
///
/// The replica (last resort) is a transcription of the bytes above, not a guess:
/// visible flag set AND draw flag 0x40000 clear. An earlier version walked the
/// parent chain instead, which the game does not do - the disassembly settles it.
/// </summary>
internal static unsafe class NodeVisibilityCompat
{
    /// <summary>
    /// The function body quoted above, with the three jump displacements
    /// wildcarded so a recompile that moves them still matches. Verified to hit
    /// exactly once in the Korean binary; the same bytes without the tail hit 15
    /// times, because callers inline the same flag test - hence the whole body.
    /// </summary>
    private const string KoreanBodySignature =
        "48 85 C9 74 ?? F7 81 AC 00 00 00 00 00 10 00 74 ?? " +
        "F7 81 B0 00 00 00 00 00 04 00 75 ?? B8 01 00 00 00 C3";

    /// <summary>DrawFlags bit the game's own check rejects a node for. Its
    /// meaning is not documented anywhere available - only that the game answers
    /// "not visible" when it is set (see the byte listing above).</summary>
    private const uint DrawFlagRejected = 0x40000;

    /// <summary>Where the answer comes from since <see cref="Install"/> ran.</summary>
    internal static CompatSource Source { get; private set; } = CompatSource.GameFunction;

    /// <summary>Address in use, for the log. Zero before Install.</summary>
    internal static nint Address { get; private set; }

    /// <summary>
    /// Gives ClientStructs an address for IsVisible if it has none. Called from
    /// the plugin constructor before any service exists, so the first node read
    /// already has it.
    /// </summary>
    internal static void Install(ISigScanner scanner, IPluginLog log)
    {
        var address = AtkResNode.Addresses.IsVisible;
        if (address.Value != nint.Zero)
        {
            Source = CompatSource.GameFunction;
            Address = address.Value;
            return;
        }

        var found = ScanUnique(scanner, log);
        if (found != nint.Zero)
        {
            address.Value = found;
            Source = CompatSource.KoreanSignature;
            Address = found;
            return;
        }

        address.Value = (nint)(delegate* unmanaged<AtkResNode*, int>)&IsVisibleFallback;
        Source = CompatSource.Emulated;
        Address = address.Value;
        log.Warning("[Compat] AtkResNode::IsVisible: no address and no Korean signature match - "
                    + "using the managed replica, which can answer differently than the game.");
    }

    /// <summary>The one address the Korean signature matches, or zero if it
    /// matches none or more than one. More than one is not a near miss, it is a
    /// wrong answer waiting to happen, so it is refused.</summary>
    private static nint ScanUnique(ISigScanner scanner, IPluginLog log)
    {
        nint[] hits;
        try
        {
            hits = scanner.ScanAllText(KoreanBodySignature);
        }
        catch (Exception ex)
        {
            // Dalamud throws rather than returning empty when nothing matches.
            log.Warning($"[Compat] Korean IsVisible signature did not resolve: {ex.Message}");
            return nint.Zero;
        }

        if (hits.Length == 1)
        {
            log.Information($"[Compat] AtkResNode::IsVisible resolved by the Korean signature "
                            + $"at 0x{hits[0]:X} (ClientStructs left it null).");
            return hits[0];
        }

        log.Warning($"[Compat] Korean IsVisible signature matched {hits.Length} times, expected 1 - refused.");
        return nint.Zero;
    }

    /// <summary>
    /// Transcription of the game's own check, used only when the scan fails.
    ///
    /// Returns int rather than bool: the caller's function pointer declares bool,
    /// and returning a full 32-bit 0/1 is correct whether the runtime reads AL or
    /// EAX. A byte return would leave the upper bits undefined.
    /// </summary>
    [UnmanagedCallersOnly]
    private static int IsVisibleFallback(AtkResNode* node)
    {
        if (node == null) return 0;
        if ((node->NodeFlags & NodeFlags.Visible) == 0) return 0;
        if ((node->DrawFlags & DrawFlagRejected) != 0) return 0;
        return 1;
    }
}
