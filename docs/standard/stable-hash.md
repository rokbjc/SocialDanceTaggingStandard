# StableHash (normative)

The mirror-sync baseline (how SDTS detects which side changed since the last sync — see
[`sync/sync-strategy.md`](../sync/sync-strategy.md)) stores a **content hash** per field, not the value.
Every language implementation **MUST** compute this hash identically, or 3-way reconcile silently breaks
across tools. This page is the normative definition; the conformance vectors are
[`../../conformance/vectors/hash.json`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/conformance/vectors/hash.json).
(MUST / SHOULD / MAY are used here as defined in [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119); see the
[spec](spec.md) for the full notational conventions.)

## Algorithm

`StableHash(value)` — **FNV-1a, 32-bit**, over the **UTF-8** bytes of `value`, rendered as **8 lowercase
hex digits**. Null and the empty string both map to the sentinel `00000000` (so "absent" round-trips and
add/remove is detectable).

```
function StableHash(value):
    if value is null or value == "":
        return "00000000"
    hash = 0x811C9DC5            # 2166136261  (FNV offset basis, 32-bit)
    for each byte b in UTF8(value):
        hash = hash XOR b
        hash = (hash * 0x01000193) mod 2^32   # 16777619 (FNV prime), wrap to 32 bits
    return lowercase 8-digit hex of hash
```

Notes that matter for interop:
- It is **FNV-1a** (XOR the byte, *then* multiply) — not FNV-1.
- Arithmetic is **unsigned 32-bit with wraparound**. In languages without native u32, mask with
  `& 0xFFFFFFFF` after each multiply.
- Hash the **UTF-8 bytes**, never UTF-16 code units or code points.
- Output is always exactly 8 hex chars, zero-padded, lowercase.

## Conformance vectors

| input | StableHash |
|---|---|
| (null / absent) | `00000000` |
| `""` (empty) | `00000000` |
| `Lindy Hop` | `41379818` |
| `dances` | `784f703d` |
| `Lindy Hop; Balboa` | `9e33e3a8` |
| `Café` (U+00E9 → UTF-8 `0xC3 0xA9`) | `cfc290a9` |
| `120` | `733864ae` |
| `original` | `a92b8b72` |

The reference implementation is `StableHash.Of` in `SocialDanceTaggingStandard` (public), pinned by
`StableHashTests` and by the shared `hash.json` vector file above.
