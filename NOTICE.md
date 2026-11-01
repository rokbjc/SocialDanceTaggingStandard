# NOTICE

Copyright 2026 rokbjc.

Third-party attributions for the **Social-Dance Tagging Standard (SDTS)**.

This project's own licensing is declared elsewhere:
- **Source code** (`dotnet/`, `scripts/`) — Apache-2.0, see [`LICENSE`](LICENSE).
- **The standard text, schema, conformance vectors, and vocabulary data** (`docs/`, `data/`, `conformance/`,
  `schemas/`) — CC0-1.0, see [`LICENSE-SPEC`](LICENSE-SPEC). Vocabulary provenance: see
  [`data/vocabulary/README.md`](data/vocabulary/README.md).

The project's own content (vocabulary values, descriptions, the SDTS-authored dance packs) is original work of
the maintainer — drafted with AI assistance from general knowledge — and is CC0-dedicated; see
[`LICENSE-SPEC`](LICENSE-SPEC) for the provenance statement. The LGPL-2.1 TagLibSharp dependency is dynamically
linked (NuGet), so it imposes no copyleft obligation on downstream consumers of these packages.

The material below is the work of others, reused under the stated licenses.

## Runtime dependencies

| Package | Version | License | Upstream |
|---------|---------|---------|----------|
| TagLibSharp | 2.3.0 | LGPL-2.1-or-later | https://github.com/mono/taglib-sharp |
| YamlDotNet | 18.0.0 | MIT | https://github.com/aaubry/YamlDotNet |

TagLibSharp is referenced as a dynamically-linked NuGet `PackageReference` by `SocialDanceTaggingStandard.Id3`;
its LGPL-2.1 terms are satisfied by that dynamic linkage. YamlDotNet (MIT) is used by
`SocialDanceTaggingStandard.Taxonomy`.

## Icon sets (Iconify)

Each term's `display.icon` in [`data/vocabulary/`](data/vocabulary/) references an icon **by name** from the Iconify
sets below (e.g. `game-icons:clarinet`). The glyph artwork itself is **not redistributed in this repository** —
it is fetched and rendered at display time by an Iconify component. Each set used is credited here with its
author and license; set metadata was sourced from the Iconify collections API
(https://api.iconify.design/collections).

### Attribution required (CC-BY family)

| Set | Prefix | Author | License | Page |
|-----|--------|--------|---------|------|
| Game Icons | `game-icons` | GameIcons | CC-BY-3.0 | https://icon-sets.iconify.design/game-icons/ |
| Solar | `solar` | 480 Design | CC-BY-4.0 | https://icon-sets.iconify.design/solar/ |
| Font Awesome 6 Solid | `fa6-solid` | Dave Gandy / Fonticons | CC-BY-4.0 | https://icon-sets.iconify.design/fa6-solid/ |

### Permissive (MIT / Apache-2.0 / ISC)

| Set | Prefix | Author | License |
|-----|--------|--------|---------|
| Material Design Icons | `mdi` | Pictogrammers | Apache-2.0 |
| Material Symbols | `material-symbols` | Google | Apache-2.0 |
| MingCute | `mingcute` | MingCute Design | Apache-2.0 |
| IconPark Solid | `icon-park-solid` | ByteDance | Apache-2.0 |
| IconPark Outline | `icon-park-outline` | ByteDance | Apache-2.0 |
| Phosphor | `ph` | Phosphor Icons | MIT |
| Tabler Icons | `tabler` | Paweł Kuna | MIT |
| TDesign Icons | `tdesign` | TDesign | MIT |
| Qlementine Icons | `qlementine-icons` | Olivier Cléro | MIT |
| Nimbus | `nimbus` | Linkedstore S.A. | MIT |
| HeroIcons | `heroicons` | Refactoring UI Inc | MIT |
| Dinkie Icons | `dinkie-icons` | atelierAnchor | MIT |
| Bootstrap Icons | `bi` | The Bootstrap Authors | MIT |
| Lucide | `lucide` | Lucide Contributors | ISC |
| Lucide Lab | `lucide-lab` | Lucide Contributors | ISC |
