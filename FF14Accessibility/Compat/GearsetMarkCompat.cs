using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

// Deliberately the ROOT namespace, not FF14Accessibility.Compat: an extension
// method in an enclosing namespace is in scope without a using directive, so
// InventoryService.cs needs no edit at all.
namespace FF14Accessibility;

/// <summary>
/// Keeps the gearset mark working when the plugin is built against an older
/// FFXIVClientStructs than upstream uses.
///
/// WORKAROUND: RaptureGearsetModule.IsItemRegisteredToGearset(InventoryItem*,
/// void*, int) exists in ClientStructs 7.55.1.8875 but NOT in 7.51.0.8667,
/// which is the version the Korean Dalamud distribution pins (verified by
/// dumping both assemblies with MetadataLoadContext, 2026-08-17). There is no
/// renamed or equivalent member in 7.51 - the whole method is absent, so the
/// clean path (call the game's own function) is unavailable at compile time,
/// not merely at run time.
///
/// This is an EXTENSION method, and that is the point: C# only considers
/// extension methods when no instance method applies. Built against 7.55 the
/// real game function still wins and this file is dead weight; built against
/// 7.51 it fills the hole. Neither build needs a compile-time switch and the
/// call site is untouched.
///
/// PRECISION DIFFERS FROM THE GAME'S ANSWER. The game answers per item
/// INSTANCE - of two identical pieces it can mark one and not the other.
/// Gearsets only store item ids (RaptureGearsetModule.GearsetEntry.GearsetItem
/// has ItemId, no container or slot), so the fallback can only answer per ID
/// and says yes for both copies. That is the same direction of error upstream
/// already accepts in IsAnyCopyRegisteredToGearset, and for the same reason:
/// a missing "do not sell" warning costs more than a surplus one.
/// </summary>
internal static unsafe class GearsetMarkCompat
{
    /// <summary>Gearsets store an HQ piece as id + 1000000; inventory items
    /// keep the plain id and carry HQ in ItemFlags. Comparing normalised ids
    /// lets an NQ piece match its HQ entry, which errs towards warning.</summary>
    private const uint HighQualityOffset = 1_000_000;

    /// <summary>RaptureGearsetModule.Entries is a fixed array of 100.</summary>
    private const int GearsetSlots = 100;

    /// <summary>
    /// Whether any saved gearset references this item's id.
    ///
    /// The extra parameters of the real 7.55 function (itemRow, equipSlotIndex)
    /// are not reproduced: the single caller passes neither, and inventing
    /// values for a function we are not calling would only invite confusion.
    /// </summary>
    public static bool IsItemRegisteredToGearset(
        this ref RaptureGearsetModule module, InventoryItem* item)
    {
        if (item == null) return false;

        var wanted = WithoutHighQuality(item->ItemId);
        if (wanted == 0) return false;

        var entries = module.Entries;
        for (var id = 0; id < GearsetSlots && id < entries.Length; id++)
        {
            // The game's own validity answer rather than a reimplemented one -
            // gearset ids are not dense, so counting to NumGearsets would skip
            // the wrong ones.
            if (!module.IsValidGearset(id)) continue;

            foreach (ref readonly var slot in entries[id].Items)
            {
                if (WithoutHighQuality(slot.ItemId) == wanted) return true;
            }
        }

        return false;
    }

    private static uint WithoutHighQuality(uint itemId)
        => itemId >= HighQualityOffset ? itemId - HighQualityOffset : itemId;
}
