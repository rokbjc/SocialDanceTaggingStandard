#!/usr/bin/env python3
"""Generate a beat-grid SVG per Rhythmic Feel — the rhythm made visible.

A small step-sequencer grid: lanes for the ride/hat, kick, snare/accent, bass or clave, across the
feel's subdivision steps (8ths for straight, triplets for swung, 16ths for clave) over a bar (two for
clave). Hits are dots, accents are the larger coloured dots — so a swing-triplet ride, a straight
backbeat, a clave and a waltz read as visibly different shapes. Pairs with the audio clip + the prose.

    python scripts/gen_feel_viz.py        # all feels

Writes docs/audio/rhythmic_feel/<id>.svg (self-contained, renders on GitHub and the site).
"""
from __future__ import annotations
import pathlib
import sys

try:
    import yaml
except ImportError:
    sys.exit("PyYAML required: pip install pyyaml")

ROOT = pathlib.Path(__file__).resolve().parent.parent
FEELS = ROOT / "data" / "vocabulary" / "core" / "rhythmic_feel.yaml"
OUT = ROOT / "docs" / "audio" / "rhythmic_feel"
SON_CLAVE = [0, 6, 12, 20, 24]   # 3-2 son clave on a 2-bar / 32-sixteenth grid


def pattern(pulse: int, subdivision: str, accent: str):
    """Return (cols, steps_per_beat, [(lane_label, {col: 'hit'|'accent'}), …])."""
    if accent == "clave":
        spb, bars = 4, 2
    elif subdivision in ("triplet", "mixed"):
        spb, bars = 3, 1
    else:
        spb, bars = 2, 1
    beats = pulse * bars
    cols = beats * spb
    lanes: list = []

    if subdivision != "bass-feel":
        ride = {c: "hit" for c in range(cols)}
        if subdivision in ("triplet", "mixed") and accent == "off-beat":
            for c in range(cols):
                if c % spb in (0, 2):
                    ride[c] = "accent"                       # swing ride emphasis
        lanes.append(("ride" if subdivision in ("triplet", "mixed") else "hat", ride))

    kick = {}
    for beat in range(beats):
        c = beat * spb
        if accent == "backbeat":
            if beat % pulse in (0, 2):
                kick[c] = "accent"
        else:
            kick[c] = "accent" if beat % pulse in (0, 2) else "hit"
    lanes.append(("kick", kick))

    if subdivision == "bass-feel":
        lanes.append(("bass", {beat * spb: ("accent" if beat % pulse in (0, 2) else "hit")
                               for beat in range(beats)}))

    if accent == "backbeat":
        lanes.append(("snare", {beat * spb: "accent" for beat in range(beats) if beat % pulse in (1, 3)}))
    elif accent == "off-beat":
        off = 2 if spb == 3 else 1
        lanes.append(("snare", {beat * spb + off: "accent" for beat in range(beats)
                                if beat * spb + off < cols}))
    elif accent == "on-beat" and subdivision != "bass-feel":
        lanes.append(("accent", {beat * spb: "hit" for beat in range(beats) if beat % pulse in (0, 2)}))
    elif accent == "clave":
        lanes.append(("clave", {p: "accent" for p in SON_CLAVE if p < cols}))

    return cols, spb, lanes


def svg(cols: int, spb: int, lanes: list, color: str) -> str:
    cw, lh, lw, hh, pad = 20, 24, 58, 22, 12
    w = lw + cols * cw + pad * 2
    h = hh + len(lanes) * lh + pad
    x0 = lw + pad
    p = [f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}" '
         f'font-family="-apple-system,Segoe UI,sans-serif" role="img" aria-label="beat grid">',
         f'<rect x="0.5" y="0.5" width="{w - 1}" height="{h - 1}" rx="8" fill="#f7f7f5" stroke="#e3e3df"/>']
    beats = cols // spb
    for beat in range(beats):
        bx = x0 + beat * spb * cw
        p.append(f'<line x1="{bx}" y1="{hh - 6}" x2="{bx}" y2="{h - pad}" stroke="#d4d4ce" '
                 f'stroke-width="{1.8 if beat % 4 == 0 else 1}"/>')
        p.append(f'<text x="{bx + spb * cw / 2:.0f}" y="{hh - 7}" fill="#9c9c94" font-size="11" '
                 f'text-anchor="middle">{beat % 4 + 1}</text>')
    p.append(f'<line x1="{x0 + cols * cw}" y1="{hh - 6}" x2="{x0 + cols * cw}" y2="{h - pad}" stroke="#d4d4ce"/>')
    for i, (label, hits) in enumerate(lanes):
        ly = hh + i * lh + lh / 2
        p.append(f'<text x="{lw}" y="{ly + 4:.0f}" fill="#7c7c74" font-size="11" text-anchor="end">{label}</text>')
        for c in range(cols):
            cx = x0 + c * cw + cw / 2
            kind = hits.get(c)
            if kind == "accent":
                p.append(f'<circle cx="{cx:.0f}" cy="{ly:.0f}" r="6.5" fill="{color}"/>')
            elif kind == "hit":
                p.append(f'<circle cx="{cx:.0f}" cy="{ly:.0f}" r="3.6" fill="#5a5a52"/>')
            else:
                p.append(f'<circle cx="{cx:.0f}" cy="{ly:.0f}" r="1.3" fill="#d6d6d0"/>')
    p.append("</svg>")
    return "\n".join(p)


def main() -> None:
    doc = yaml.safe_load(FEELS.read_text(encoding="utf-8"))
    wanted = set(sys.argv[1:])
    OUT.mkdir(parents=True, exist_ok=True)
    n = 0
    for v in doc.get("values", []):
        vid = v.get("id")
        if wanted and vid not in wanted:
            continue
        pulse = v.get("pulse")
        pulse = int(pulse) if str(pulse).isdigit() else 4
        color = (v.get("display") or {}).get("color") or "#C2185B"
        cols, spb, lanes = pattern(pulse, v.get("subdivision") or "straight", v.get("accent") or "none")
        (OUT / f"{vid}.svg").write_text(svg(cols, spb, lanes, color), encoding="utf-8", newline="\n")
        n += 1
    print(f"wrote {n} beat-grid SVGs to {OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
