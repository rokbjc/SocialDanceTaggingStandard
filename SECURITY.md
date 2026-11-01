# Security policy

## Supported versions

The project is pre-1.0. While the version is `0.x`, **only the latest published release** of each stream
(the spec version in [`data/fields.yaml`](data/fields.yaml) and the NuGet packages built from `dotnet/`) receives
security fixes. There is no back-porting to older `0.x` releases — upgrade to the latest.

## What is in scope

SDTS is a tag **standard** plus a .NET **reference library** that reads and writes audio-file metadata. The
security surface worth reporting:

- A crafted tag or audio file that makes a reader crash, hang, exhaust memory, or read/write outside the file
  (the ID3 path goes through TagLibSharp; the vocabulary loader parses YAML via YamlDotNet).
- Unsafe deserialization or path handling in the Python tooling under `scripts/` or in the .NET loaders.
- A supply-chain problem in a declared dependency (see [`NOTICE.md`](NOTICE.md)).

**Out of scope:** the *content* of the vocabulary or spec text (propose those changes normally, see
[`CONTRIBUTING.md`](CONTRIBUTING.md)); issues that require a machine already fully compromised; and anything in
anything outside this repository.

## Reporting a vulnerability

**Do not open a public issue for a security problem.** Use GitHub's private reporting:

1. Go to the repository's **Security** tab → **Report a vulnerability** (GitHub private vulnerability
   reporting), or open a **draft security advisory**.
2. Include the affected version, a minimal reproduction (a crafted file is ideal), and the impact.

If private reporting is unavailable to you, contact the maintainer (`@rokbjc`) through GitHub and ask for a
private channel — do **not** post details publicly first.

## What to expect

This is a small, single-maintainer project — response is best-effort, not contractual:

- **Acknowledgement:** within ~7 days.
- **Assessment + fix plan:** shared with you as it develops; we coordinate a disclosure date.
- **Credit:** reporters are credited in the advisory and CHANGELOG unless they ask to stay anonymous.

Please give a reasonable window to ship a fix before any public disclosure.
