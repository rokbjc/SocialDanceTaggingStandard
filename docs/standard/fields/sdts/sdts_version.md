# SDTS schema version &nbsp;·&nbsp; `sdts_version`

> Skip files already at the current standard and migrate only older ones — fast, idempotent normalization.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `string` · single |
| **Values** | the SDTS schema/spec version this file was last normalized to (semver, e.g. 0.1.0) |
| **Tag** | `TXXX:SDTS_VERSION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Workflow & trust · SDTS-defined |

The SDTS schema version the file was last written / normalized under. On load the toolkit compares it to the current version: older -> run the relevant migrations and re-normalize; equal -> skip. Makes normalization cheap and idempotent. Distinct from SB-Migrations (one-shot per-migration tokens) — this is the single 'where is this file at' watermark.

**Values.** semver string (e.g. '0.1.0'); compared on load to decide whether to migrate / re-normalize

**Examples.** ✓ `0.1.0` · `0.2.0`

**Avoid.**
- `v0.1` — use a bare semver: `0.1.0`
- `latest` — must be a concrete version string

**Related.** [Confirmation gates (Custom10)](confirmation_gates.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
