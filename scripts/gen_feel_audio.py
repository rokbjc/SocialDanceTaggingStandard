#!/usr/bin/env python3
"""Generate a MIDI + rendered MP3 per Rhythmic Feel, voiced with real (General MIDI) instruments.

Each feel gets a short, looping arrangement built from the instruments that actually define it — a
solo piano for stride, an upright bass + ride for walking swing, a gypsy guitar 'la pompe' for
manouche, clave + congas + tumbao for Latin, and a distinct drum-kit + lead per blues/jazz shuffle so
they no longer all sound the same. The MIDI is the editable source (open it in any DAW / soundfont);
the MP3 is the in-doc preview, rendered through a GM soundfont via fluidsynth.

    python scripts/gen_feel_audio.py            # all feels
    python scripts/gen_feel_audio.py shuffle    # one feel
    python scripts/gen_feel_audio.py --retrim   # trim existing MP3s to their loop length (ffmpeg only)

Toolchain (not committed — a build dependency, like ffmpeg): fluidsynth on PATH + a GM soundfont at
$SDT_SOUNDFONT (default ~/.sdt-audio-tools/gm.sf3). Outputs: docs/audio/rhythmic_feel/<id>.mid + .mp3.
"""
from __future__ import annotations
import os
import pathlib
import subprocess
import sys
import tempfile

try:
    import mido
    import yaml
except ImportError:
    sys.exit("requires mido + pyyaml: pip install mido pyyaml")

ROOT = pathlib.Path(__file__).resolve().parent.parent
FEELS = ROOT / "data" / "vocabulary" / "core" / "rhythmic_feel.yaml"
OUT = ROOT / "docs" / "audio" / "rhythmic_feel"
SOUNDFONT = pathlib.Path(os.environ.get("SDT_SOUNDFONT", str(pathlib.Path.home() / ".sdt-audio-tools" / "gm.sf3")))
TPB = 480          # MIDI ticks per beat
BARS = 2

# General MIDI program numbers (0-based) and drum (channel 9) note numbers.
PIANO, EPIANO, ORGAN, VIBES, ACCORDION = 0, 4, 17, 11, 21
NYLON, STEEL, JAZZGT, CLEANGT, ODGT, BANJO = 24, 25, 26, 27, 29, 105
UPBASS, FINGBASS = 32, 33
VIOLIN, STRINGS = 40, 48
TRUMPET, MUTETRP, ASAX, TSAX, HARMONICA = 56, 59, 65, 66, 22
KICK, RIM, SNARE, CLAP, ESNARE = 36, 37, 38, 39, 40
CHH, OHH, LTOM, RIDE, RIDEBELL, CRASH = 42, 46, 45, 51, 53, 49
HCONGA, LCONGA, HBONGO, LBONGO, CLAVES, MARACAS, COWBELL, GUIRO, CABASA = 63, 64, 60, 61, 75, 70, 56, 74, 69
DRUMS = 9

WALK = [43, 47, 50, 48, 45, 43, 38, 41]   # a simple G-blues walking line (2 bars of quarters)


def build_midi(bpm: float, events: list, programs: dict) -> mido.MidiFile:
    """events: (channel, note, start_beat, dur_beat, velocity). programs: {channel: gm_program}."""
    mf = mido.MidiFile(ticks_per_beat=TPB)
    tr = mido.MidiTrack()
    mf.tracks.append(tr)
    tr.append(mido.MetaMessage("set_tempo", tempo=mido.bpm2tempo(round(bpm)), time=0))
    for ch, prog in sorted(programs.items()):
        tr.append(mido.Message("program_change", channel=ch, program=prog, time=0))
    seq = []
    for ch, note, sb, db, vel in events:
        on, off = int(round(sb * TPB)), int(round((sb + db) * TPB))
        seq.append((on, 1, ch, note, max(1, min(127, int(vel)))))
        seq.append((max(on + 1, off), 0, ch, note, 0))
    seq.sort(key=lambda x: (x[0], x[1]))
    last = 0
    for t, typ, ch, note, vel in seq:
        dt, last = t - last, t
        kind = "note_on" if typ else "note_off"
        tr.append(mido.Message(kind, channel=ch, note=note, velocity=vel, time=dt))
    return mf


# ── Arrangement archetypes (each returns events, programs) ───────────────────────────────────────
def _walk(bars: int, ch: int, vel: int = 78) -> list:
    return [(ch, WALK[(bar * 4 + beat) % len(WALK)], bar * 4 + beat, 0.92, vel)
            for bar in range(bars) for beat in range(4)]


def swing_kit(lead: int | None = None, energy: float = 1.0, bars: int = BARS) -> tuple:
    ev, prog = _walk(bars, 0), {0: UPBASS}
    if lead is not None:
        prog[1] = lead
    for bar in range(bars):
        for beat in range(4):
            b = bar * 4 + beat
            ev.append((DRUMS, KICK, b, 0.4, int(42 * energy)))                 # feathered four-on-floor
            ev += [(DRUMS, RIDE, b, 0.2, int(76 * energy)), (DRUMS, RIDE, b + 2 / 3, 0.2, int(58 * energy))]
            if beat in (1, 3):
                ev.append((DRUMS, CHH, b, 0.1, int(70 * energy)))              # hi-hat foot on 2 & 4
    if lead is not None:                                                        # a sparse swing riff
        for b, n in ((1.0, 67), (1.667, 70), (2.0, 69), (5.333, 67), (6.0, 65)):
            ev.append((1, n, b, 0.5, int(80 * energy)))
    return ev, prog


def shuffle_kit(lead: int | None = None, sparse: bool = False, energy: float = 1.0,
                drive: bool = False, bars: int = BARS) -> tuple:
    ev, prog = _walk(bars, 0, 70), {0: UPBASS}
    if lead is not None:
        prog[1] = lead
    for bar in range(bars):
        for beat in range(4):
            b = bar * 4 + beat
            if beat in (0, 2) or drive:
                ev.append((DRUMS, KICK, b, 0.4, int((90 if beat in (0, 2) else 60) * energy)))
            if beat in (1, 3):
                ev.append((DRUMS, SNARE, b, 0.4, int(104 * energy)))           # backbeat
            if not sparse:                                                      # triplet shuffle hats
                ev += [(DRUMS, CHH, b, 0.15, int(58 * energy)), (DRUMS, CHH, b + 2 / 3, 0.15, int(66 * energy))]
    if lead is not None:                                                        # a bluesy shuffle riff
        for b, n in ((0.0, 55), (0.667, 58), (1.0, 60), (2.0, 58), (4.0, 55), (4.667, 58), (6.0, 62)):
            ev.append((1, n, b, 0.6, int(86 * energy)))
    return ev, prog


def straight_kit(lead: int | None = None, energy: float = 1.0, bars: int = BARS) -> tuple:
    ev, prog = _walk(bars, 0, 74), {0: UPBASS}
    if lead is not None:
        prog[1] = lead
    for bar in range(bars):
        for beat in range(4):
            b = bar * 4 + beat
            if beat in (0, 2):
                ev.append((DRUMS, KICK, b, 0.4, int(92 * energy)))
            if beat in (1, 3):
                ev.append((DRUMS, SNARE, b, 0.4, int(102 * energy)))
            ev += [(DRUMS, CHH, b, 0.12, int(60 * energy)), (DRUMS, CHH, b + 0.5, 0.12, int(52 * energy))]
    return ev, prog


def piano_stride(bars: int = BARS) -> tuple:
    ev, prog = [], {0: PIANO}
    bass = [43, 38]                                                            # G2 / D2 alternation
    chord = [55, 59, 62]                                                       # G major triad (mid)
    for bar in range(bars):
        for beat in range(4):
            b = bar * 4 + beat
            if beat % 2 == 0:
                ev.append((0, bass[(beat // 2) % 2], b, 0.45, 88))             # low bass on 1 & 3
            else:
                ev += [(0, n, b, 0.4, 70) for n in chord]                      # chord 'pah' on 2 & 4
        for b, n in ((bar * 4 + 0.5, 74), (bar * 4 + 2.5, 79), (bar * 4 + 3, 76)):
            ev.append((0, n, b, 0.4, 64))                                      # light right-hand fill
    return ev, prog


def piano_boogie(bars: int = BARS) -> tuple:
    fig = [43, 47, 50, 52, 53, 52, 50, 47]                                     # eight-to-the-bar boogie bass
    ev, prog = [], {0: PIANO}
    for bar in range(bars):
        for i in range(8):
            ev.append((0, fig[i], bar * 4 + i * 0.5, 0.45, 82 if i % 2 == 0 else 68))
        ev += [(0, n, bar * 4 + 1, 0.5, 70) for n in (67, 70)]                 # right-hand blues stab
    ev.append((DRUMS, CHH, 0, 0.1, 0))  # (no kit — keep it pure piano; placeholder removed below)
    ev = [e for e in ev if e[0] != DRUMS]
    return ev, prog


def guitar_pompe(bars: int = BARS) -> tuple:
    ev, prog = _walk(bars, 0, 74), {0: UPBASS, 1: JAZZGT, 2: VIOLIN}
    chord = [55, 59, 62, 67]                                                   # Gm6-ish comp
    for bar in range(bars):
        for beat in range(4):                                                  # la pompe: chunk all 4
            ev += [(1, n, bar * 4 + beat, 0.18, 84 if beat in (1, 3) else 60) for n in chord]
    for b, n in ((1.0, 74), (1.667, 77), (2.0, 75), (5.0, 79), (6.0, 74)):     # violin lead
        ev.append((2, n, b, 0.5, 88))
    return ev, prog


def clave_band(brass: bool = True, bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: PIANO}
    if brass:
        prog[2] = TRUMPET
    son = [0.0, 0.75, 1.5, 2.5, 3.0]                                           # 2-3 son clave per bar(×2)
    for bar in range(bars):
        for pos in son:
            ev.append((DRUMS, CLAVES, bar * 4 + pos, 0.2, 92))
        for beat in range(4):
            b = bar * 4 + beat
            ev += [(DRUMS, HCONGA if beat in (1, 3) else LCONGA, b, 0.2, 70),
                   (DRUMS, HCONGA, b + 0.5, 0.15, 50)]
            if beat in (1, 3):                                                 # tumbao: anticipated bass
                ev.append((0, 43, b - 0.5, 0.5, 84))
                ev.append((0, 50, b, 0.5, 78))
            ev += [(1, n, b + 0.5, 0.3, 64) for n in (55, 59, 62)]             # piano montuno offbeats
    if brass:
        for b, n in ((1.5, 70), (2.0, 67), (5.5, 72), (6.0, 70)):
            ev.append((2, n, b, 0.5, 92))
    return ev, prog


def habanera(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: NYLON}
    for bar in range(bars):
        base = bar * 4
        for off, dur in ((0.0, 0.75), (0.75, 0.25), (1.0, 0.5), (1.5, 0.5)):   # habanera bass cell ×2 (2/4)
            ev.append((0, 43 if off == 0 else 38, base + off, dur, 82))
            ev.append((0, 43 if off == 0 else 38, base + 2 + off, dur, 80))
        for beat in range(4):
            ev.append((DRUMS, CLAVES, base + beat, 0.15, 70))
        ev += [(1, n, base + 1, 0.4, 60) for n in (55, 59, 62)]
    return ev, prog


def bossa(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: NYLON}
    pat = [0.0, 1.0, 1.5, 2.5, 3.0]                                            # bossa guitar comp
    for bar in range(bars):
        base = bar * 4
        ev += [(0, 43, base, 1.0, 70), (0, 38, base + 2, 1.0, 68)]             # root on 1 & 3
        for p in pat:
            ev += [(1, n, base + p, 0.3, 58) for n in (54, 58, 61, 65)]        # maj7 comp
        for beat in range(4):
            ev.append((DRUMS, RIM, base + beat + (0.0 if beat % 2 == 0 else 0.5), 0.1, 50))
    return ev, prog


def waltz3(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: PIANO}
    for bar in range(bars):
        base = bar * 3
        ev.append((0, 43, base, 0.9, 84))                                      # bass on 1
        for beat in (1, 2):
            ev += [(1, n, base + beat, 0.6, 62) for n in (55, 59, 62)]         # chords on 2 & 3
        ev.append((DRUMS, KICK, base, 0.3, 70))
        ev += [(DRUMS, CHH, base + b, 0.1, 48) for b in (1, 2)]
    return ev, prog


def two_beat(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: BANJO}
    for bar in range(bars):
        for beat in range(4):
            b = bar * 4 + beat
            if beat in (0, 2):
                ev.append((0, 43 if beat == 0 else 38, b, 0.5, 86))           # tuba/bass on 1 & 3
                ev.append((DRUMS, KICK, b, 0.3, 80))
            else:
                ev += [(1, n, b, 0.2, 70) for n in (55, 59, 62)]              # banjo chunk on 2 & 4
                ev.append((DRUMS, CHH, b, 0.1, 55))
    return ev, prog


def compound12(bars: int = BARS) -> tuple:                                     # slow 12/8 ballad
    ev, prog = [], {0: UPBASS, 1: PIANO}
    for bar in range(bars):
        base = bar * 4
        ev.append((0, 43, base, 1.8, 70))
        ev.append((0, 38, base + 2, 1.8, 68))
        for beat in range(4):
            for t in (0, 1 / 3, 2 / 3):
                ev.append((DRUMS, CHH, base + beat + t, 0.1, 46 + (16 if t == 0 else 0)))
            if beat in (1, 3):
                ev.append((DRUMS, SNARE, base + beat, 0.4, 92))
        ev += [(1, n, base + 0.0, 1.5, 56) for n in (55, 58, 62)]
    return ev, prog


# ── Scene arrangements: tango / ballroom / street-latin (the non-swing scenes) ───────────────────
def tango(sincopa: bool = False, bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: ACCORDION, 2: VIOLIN}
    chord = [55, 58, 62]                                                        # G minor (tango colour)
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((0, 43 if beat in (0, 2) else 38, b, 0.5, 98 if beat in (0, 2) else 84))  # marcato bass
            ev += [(1, n, b, 0.35, 76 if beat in (0, 2) else 60) for n in chord]                # bandoneón marcato
        if sincopa:                                                            # síncopa: weight to the off-beats
            for off in (1.5, 3.5):
                ev += [(1, n, base + off, 0.25, 100) for n in chord]
        for b, n in ((0.0, 67), (1.5, 70), (2.0, 69), (3.0, 67)):              # violin counter-line
            ev.append((2, n, base + b, 0.6, 86))
    return ev, prog


def tango_vals(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: ACCORDION, 2: VIOLIN}
    chord = [55, 58, 62]
    for bar in range(bars):
        base = bar * 3
        ev.append((0, 43, base, 0.9, 92))                                      # bass on 1
        for beat in (1, 2):
            ev += [(1, n, base + beat, 0.5, 64) for n in chord]                # accordion on 2 & 3
        for b, n in ((0.0, 67), (1.0, 70), (2.0, 74)):                         # circular violin lilt
            ev.append((2, n, base + b, 0.5, 84))
    return ev, prog


def milonga_traspie(bars: int = BARS) -> tuple:
    ev, prog = habanera(bars)                                                  # habanera bass frame
    for bar in range(bars):                                                    # traspié contratiempo lift
        base = bar * 4
        for off in (0.5, 1.5, 2.5, 3.5):
            ev.append((DRUMS, RIM, base + off, 0.1, 82))
    return ev, prog


def cha_cha(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: PIANO, 2: TRUMPET}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev += [(DRUMS, GUIRO, b, 0.1, 60), (DRUMS, GUIRO, b + 0.5, 0.1, 48)]
            ev.append((DRUMS, COWBELL, b, 0.12, 56))
            ev += [(1, n, b + 0.5, 0.3, 58) for n in (55, 59, 62)]            # piano montuno
            ev.append((0, 43 if beat in (0, 2) else 50, b, 0.5, 80))          # bass
        for t, n in ((3.0, HCONGA), (3.5, HCONGA), (3.75, LCONGA)):           # the cha-cha-cha triple
            ev.append((DRUMS, n, base + t, 0.12, 86))
        ev += [(2, n, base + 1, 0.5, 80) for n in (67, 71)]                   # brass stab
    return ev, prog


def bolero(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: NYLON, 2: STRINGS}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((0, 43 if beat in (0, 2) else 38, b, 0.6, 76))
            ev.append((DRUMS, HCONGA if beat in (1, 3) else LCONGA, b, 0.2, 54))
        for p in (0.0, 0.5, 1.0, 1.5, 2.0):                                   # cinquillo on claves
            ev.append((DRUMS, CLAVES, base + p, 0.1, 60))
        ev += [(1, n, base + 1, 0.5, 56) for n in (55, 59, 62)]              # nylon guitar
        ev += [(2, n, base, 2.4, 44) for n in (55, 62)]                       # string pad
    return ev, prog


def samba(bars: int = BARS) -> tuple:
    ev, prog = [], {0: FINGBASS}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((DRUMS, LTOM, b, 0.3, 104 if beat in (1, 3) else 56))   # surdo accent on the 'two'
            for t in (0.0, 0.25, 0.5, 0.75):                                  # tamborim / ganzá 16ths
                ev.append((DRUMS, CABASA, b + t, 0.06, 40 if t == 0 else 52))
            ev.append((DRUMS, COWBELL, b + 0.5, 0.08, 50))                    # agogô
        ev += [(0, 43, base + 1, 0.5, 86), (0, 43, base + 3, 0.5, 86)]        # surdo bass note
    return ev, prog


def merengue(bars: int = BARS) -> tuple:
    ev, prog = [], {0: FINGBASS, 1: PIANO}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((0, 43 if beat % 2 == 0 else 38, b, 0.4, 88))           # driving march bass
            ev += [(DRUMS, GUIRO, b, 0.1, 66), (DRUMS, GUIRO, b + 0.5, 0.1, 52)]  # güira
            ev.append((DRUMS, SNARE if beat in (1, 3) else LTOM, b, 0.2, 78))  # tambora
        ev += [(1, n, base + 1, 0.3, 58) for n in (55, 59, 62)]
    return ev, prog


def bachata(bars: int = BARS) -> tuple:
    ev, prog = [], {0: FINGBASS, 1: CLEANGT}
    arp = [60, 64, 67, 71]                                                     # requinto arpeggio
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((0, 43 if beat in (0, 2) else 38, b, 0.4, 80))
            ev += [(DRUMS, GUIRO, b, 0.1, 54), (DRUMS, GUIRO, b + 0.5, 0.1, 46)]
            ev.append((DRUMS, HBONGO, b, 0.1, 58))
        for i in range(8):                                                     # requinto 8ths
            ev.append((1, arp[i % 4], base + i * 0.5, 0.3, 68))
        ev.append((DRUMS, CLAP, base + 3, 0.12, 84))                          # the bachata tap on 4
    return ev, prog


def kizomba(bars: int = BARS) -> tuple:
    ev, prog = [], {0: FINGBASS, 1: EPIANO}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((DRUMS, KICK, b, 0.3, 92 if beat in (0, 2) else 48))
            if beat in (1, 3):
                ev.append((DRUMS, SNARE, b, 0.2, 68))
            ev += [(DRUMS, CHH, b, 0.1, 42), (DRUMS, CHH, b + 0.5, 0.1, 38)]
        ev += [(0, 43, base, 1.0, 80), (0, 46, base + 2.5, 0.8, 70)]          # smooth grounded bass
        ev += [(1, n, base, 2.2, 50) for n in (55, 58, 62, 65)]               # e-piano pad
    return ev, prog


def zouk(bars: int = BARS) -> tuple:
    ev, prog = [], {0: FINGBASS, 1: EPIANO}
    for bar in range(bars):
        base = bar * 4
        for beat in (0, 2):
            ev.append((DRUMS, KICK, base + beat, 0.3, 92))                    # tum-tum
        for p in (1.5, 3.5):
            ev.append((DRUMS, CLAP, base + p, 0.1, 82))                       # syncopated 'tchi'
        ev += [(DRUMS, CHH, base + i * 0.5, 0.08, 40) for i in range(8)]
        ev += [(0, 43, base, 1.0, 78), (0, 46, base + 2, 1.0, 70)]
        ev += [(1, n, base, 2.2, 48) for n in (54, 58, 61, 65)]
    return ev, prog


def paso_doble(bars: int = BARS) -> tuple:
    ev, prog = [], {0: UPBASS, 1: TRUMPET}
    for bar in range(bars):
        base = bar * 4
        for beat in range(4):
            b = base + beat
            ev.append((0, 43 if beat % 2 == 0 else 38, b, 0.4, 88))           # march bass
            ev.append((DRUMS, SNARE, b, 0.15, 66))                            # march snare
            if beat in (0, 2):
                ev.append((DRUMS, KICK, b, 0.3, 84))
        for b, n in ((0.0, 67), (0.5, 67), (1.0, 72), (2.0, 71), (3.0, 67)):  # España cañí fanfare
            ev.append((1, n, base + b, 0.4, 98))
    return ev, prog


# Per-feel arrangement: (builder, kwargs). Tempo comes from TEMPO; unlisted feels fall back by axes.
TEMPO = {
    "slow-drag": 60, "compound-triplet": 66, "habanera": 96, "second-line": 102, "swamp-shuffle": 88,
    "bossa-nova": 132, "shuffle": 120, "chicago-shuffle": 112, "blues-shuffle": 116, "delta-shuffle": 92,
    "piedmont-shuffle": 120, "soul-shuffle": 108, "retro-soul-shuffle": 108, "rb-shuffle": 122,
    "rock-shuffle": 136, "jump-shuffle": 172, "western-swing-shuffle": 150, "nola-shuffle": 108,
    "nola-rhumba": 108, "jazz-swing-8ths": 168, "manouche-swing": 184, "walking-4": 150, "stride": 176,
    "two-beat": 196, "straight-4": 176, "backbeat": 148, "boogie-shuffle": 152, "waltz": 150,
    "latin-clave": 188, "bebop": 220, "hard-bop": 188, "rockabilly": 168, "two-step": 168,
    "tango-marcato": 120, "tango-sincopa": 124, "tango-vals": 180, "milonga-traspie": 100,
    "viennese-waltz": 174, "cha-cha-cha": 120, "bolero-rumba": 76, "samba-batucada": 100,
    "paso-doble-march": 124, "bachata-derecho": 128, "merengue": 132, "kizomba": 92, "zouk-lambada": 90,
}
PROFILE = {
    "jazz-swing-8ths": (swing_kit, {}),
    "bebop": (swing_kit, {"lead": TSAX, "energy": 1.0}),
    "hard-bop": (swing_kit, {"lead": TRUMPET, "energy": 1.05}),
    "walking-4": (swing_kit, {"lead": None}),
    "stride": (piano_stride, {}),
    "boogie-shuffle": (piano_boogie, {}),
    "manouche-swing": (guitar_pompe, {}),
    "latin-clave": (clave_band, {"brass": True}),
    "nola-rhumba": (clave_band, {"brass": False}),
    "habanera": (habanera, {}),
    "bossa-nova": (bossa, {}),
    "waltz": (waltz3, {}),
    "two-beat": (two_beat, {}),
    "second-line": (two_beat, {}),
    "compound-triplet": (compound12, {}),
    "straight-4": (straight_kit, {"lead": TSAX}),
    "backbeat": (straight_kit, {"lead": ODGT}),
    "two-step": (straight_kit, {"lead": VIOLIN}),
    # blues/jazz shuffles — same groove, DISTINCT lead instrument so they no longer sound identical:
    "shuffle": (shuffle_kit, {}),
    "chicago-shuffle": (shuffle_kit, {"lead": HARMONICA}),
    "blues-shuffle": (shuffle_kit, {"lead": CLEANGT}),
    "delta-shuffle": (shuffle_kit, {"lead": STEEL, "sparse": True}),
    "piedmont-shuffle": (shuffle_kit, {"lead": NYLON, "sparse": True}),
    "swamp-shuffle": (shuffle_kit, {"lead": HARMONICA, "sparse": True}),
    "soul-shuffle": (shuffle_kit, {"lead": ORGAN}),
    "retro-soul-shuffle": (shuffle_kit, {"lead": TSAX}),
    "rb-shuffle": (shuffle_kit, {"lead": ASAX}),
    "rock-shuffle": (shuffle_kit, {"lead": ODGT, "drive": True}),
    "jump-shuffle": (shuffle_kit, {"lead": TSAX, "drive": True, "energy": 1.05}),
    "western-swing-shuffle": (shuffle_kit, {"lead": VIOLIN}),
    "rockabilly": (shuffle_kit, {"lead": CLEANGT, "drive": True}),
    # tango / ballroom / street-latin — voiced with each scene's signature instruments:
    "tango-marcato": (tango, {}),
    "tango-sincopa": (tango, {"sincopa": True}),
    "tango-vals": (tango_vals, {}),
    "milonga-traspie": (milonga_traspie, {}),
    "viennese-waltz": (waltz3, {}),
    "cha-cha-cha": (cha_cha, {}),
    "bolero-rumba": (bolero, {}),
    "samba-batucada": (samba, {}),
    "paso-doble-march": (paso_doble, {}),
    "bachata-derecho": (bachata, {}),
    "merengue": (merengue, {}),
    "kizomba": (kizomba, {}),
    "zouk-lambada": (zouk, {}),
}


def select_builder(vid: str, pulse: int, subdivision: str, accent: str) -> tuple:
    """The arrangement builder + kwargs for a feel: an explicit PROFILE entry, else inferred from axes."""
    if vid in PROFILE:
        return PROFILE[vid]
    if accent == "clave":
        return clave_band, {"brass": False}
    if pulse == 3:
        return waltz3, {}
    if subdivision in ("triplet", "mixed") and accent == "off-beat":
        return swing_kit, {}
    if subdivision in ("triplet", "mixed"):
        return shuffle_kit, {}
    return straight_kit, {}


def loop_seconds(vid: str, builder) -> float:
    """The exact length of the BARS-bar groove at the MIDI's quantized tempo — the seamless loop point.
    Cutting the render here drops the reverb/decode tail, so the looping player has wall-to-wall rhythm
    instead of music-then-silence. Meter is 3 for the waltz builders, 4 otherwise."""
    bpm = TEMPO.get(vid, 120)
    meter = 3 if builder in (waltz3, tango_vals) else 4
    return BARS * meter * mido.bpm2tempo(round(bpm)) / 1_000_000


def render(vid: str, pulse: int, subdivision: str, accent: str) -> None:
    bpm = TEMPO.get(vid, 120)
    builder, kwargs = select_builder(vid, pulse, subdivision, accent)
    events, programs = builder(**kwargs)
    midi = build_midi(bpm, events, programs)
    mid_path = OUT / f"{vid}.mid"
    midi.save(mid_path)

    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
        wav = pathlib.Path(tmp.name)
    try:
        subprocess.run(["fluidsynth", "-ni", "-g", "0.7", "-r", "44100", "-F", str(wav),
                        str(SOUNDFONT), str(mid_path)], check=True, capture_output=True)
        # -t cuts the render to exactly BARS bars: no dead tail, and the clip loops seamlessly.
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(wav),
                        "-t", f"{loop_seconds(vid, builder):.6f}", "-ac", "1", "-b:a", "112k",
                        str(OUT / f"{vid}.mp3")], check=True)
    finally:
        wav.unlink(missing_ok=True)


def retrim() -> int:
    """Trim the dead tail off the already-rendered MP3s in place, cutting each to its exact loop length.
    ffmpeg only — no fluidsynth/soundfont — so it fixes the committed clips without a full re-render
    (and keeps their existing timbre). Returns the number of clips retrimmed."""
    doc = yaml.safe_load(FEELS.read_text(encoding="utf-8"))
    n = 0
    for v in doc.get("values", []):
        vid = v.get("id")
        mp3 = OUT / f"{vid}.mp3"
        if not mp3.is_file():
            continue
        pulse = v.get("pulse")
        pulse = int(pulse) if str(pulse).isdigit() else 4
        builder, _ = select_builder(vid, pulse, v.get("subdivision") or "straight", v.get("accent") or "none")
        tmp = OUT / f"{vid}.retrim.mp3"
        try:
            subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(mp3),
                            "-t", f"{loop_seconds(vid, builder):.6f}", "-ac", "1", "-b:a", "112k",
                            str(tmp)], check=True)
            tmp.replace(mp3)
        finally:
            tmp.unlink(missing_ok=True)
        n += 1
    return n


def main() -> None:
    args = sys.argv[1:]
    if "--retrim" in args:
        print(f"retrimmed {retrim()} feel clips to their exact loop length in {OUT.relative_to(ROOT)}")
        return
    if not SOUNDFONT.is_file():
        sys.exit(f"GM soundfont not found at {SOUNDFONT} — set $SDT_SOUNDFONT (see module docstring).")
    doc = yaml.safe_load(FEELS.read_text(encoding="utf-8"))
    wanted = set(args)
    OUT.mkdir(parents=True, exist_ok=True)
    n = 0
    for v in doc.get("values", []):
        vid = v.get("id")
        if wanted and vid not in wanted:
            continue
        pulse = v.get("pulse")
        pulse = int(pulse) if str(pulse).isdigit() else 4
        render(vid, pulse, v.get("subdivision") or "straight", v.get("accent") or "none")
        n += 1
    print(f"wrote {n} feel clips (.mid + .mp3) to {OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
