using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Gates;
using SocialDanceTaggingStandard.Id3;
using SocialDanceTaggingStandard.Internal;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 8 — a consumer's audio pipeline derives measured fields (tempo_stability, intro_duration,
/// …) from the beat grid, and the DJ hand-confirms BPM (a half-time fix locking Ella's "Rug Cutter" at
/// 250). The confirmation_gates ledger is the trust boundary: a later re-analysis pass must REFUSE to
/// overwrite a gated field. SDTS round-trips the gate string (and FieldValidator can shape-check it),
/// but the enforcement + the 4-hex tamper CRC are CONSUMER policy — there is no SDTS enforcer today.
/// </summary>
internal static class GateTrustBoundary
{
    public static void Run()
    {
        // Audio-derived (proposed) fields, written by the analysis pass — typed, over an in-memory record.
        var file = new SdtsRecord(new InMemoryTagSource());
        file.TempoStability = TempoStability.Stable;
        file.IntroDuration = TimeSpan.FromMilliseconds(4200);

        // BPM lives in standard TBPM; the DJ confirmed 250. We stamp a gate with a compact content CRC
        // (first 4 hex of the StableHash — a consumer convention, illustrating the field's spec).
        string confirmedBpm = "250";
        string crc = StableHash.Of(confirmedBpm)[..4];
        file.Gates = ConfirmationGates.Empty.WithGate(FieldIds.Bpm, 1, crc).WithGate(new FieldId("beats"), 1);

        // A re-analysis mis-locks on the half-time subdivision and proposes 125.
        bool bpmGated = file.Gates.IsConfirmed(FieldIds.Bpm);
        Console.WriteLine($"  re-analysis proposes BPM 125; BPM gated={bpmGated} -> {(bpmGated ? "REFUSE overwrite (keep 250)" : "write")}");

        // Tamper check: does the stored confirmed value still match the gate's CRC?
        bool intact = file.Gates.GetCrc(FieldIds.Bpm) == StableHash.Of(confirmedBpm)[..4];
        Console.WriteLine($"  gate CRC intact for confirmed BPM={confirmedBpm}: {intact} (version {file.Gates.Get(FieldIds.Bpm)})");
    }
}
