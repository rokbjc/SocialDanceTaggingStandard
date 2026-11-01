<!-- Thanks for contributing! Keep PRs small and focused. -->

## What & why

<!-- One or two sentences. Link the issue if there is one. -->

## Checklist

- [ ] I edited **source** files, not generated ones (see CONTRIBUTING → source vs. derived).
- [ ] If I changed `data/fields.yaml` / `data/field-invalid-examples.yaml`, I ran
      `python scripts/gen_field_docs.py` and committed the regenerated `docs/standard/fields/**`.
- [ ] If I changed library behaviour, I updated tests and the `conformance/` vectors.
- [ ] `python scripts/validate_data.py` passes.
- [ ] `cd dotnet && dotnet build SocialDanceTaggingStandard.slnx && dotnet test` passes.
- [ ] Commits are signed off (`git commit -s`).
