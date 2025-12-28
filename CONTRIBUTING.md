# Contributing

Thanks for considering a contribution! Please follow the guidelines below to help us review and ship changes smoothly.

## Ground Rules
- Be respectful; follow the [Code of Conduct](CODE_OF_CONDUCT.md).
- For security issues, email **tomblanchard3@outlook.com** (do not open a public issue) — see [SECURITY.md](SECURITY.md).

## How to Contribute
1) **Open an issue** for bugs or feature requests before large changes.
2) **Fork & branch** from `main`.
3) **Run locally**:
```bash
dotnet restore
dotnet format NetCoreAzureBlobServiceAPI/NetCoreAzureBlobServiceAPI.csproj --verify-no-changes
dotnet test NetCoreAzureBlobServiceAPI/NetCoreAzureBlobServiceAPI.csproj --configuration Release
```
4) **Keep scope focused** and write clear commit messages.
5) **Open a PR** with:
   - What/why summary
   - Screenshots for UI/API surface changes if applicable

## Code Style & Quality
- .NET 8, nullable enabled.
- Use `dotnet format` (enforced in CI) and keep logging structured (`ILogger<T>`).
- Tests: add/adjust xUnit tests in `NetCoreAzureBlobServiceAPI.Tests` when behavior changes. Coverage is published via Codecov.

## CI
- CI runs build, format check, tests, and coverage on Ubuntu & Windows.
- CodeQL runs separately for security scanning.

## Documentation
- Update README or relevant docs when behavior/config changes.
- Note default file limits and allowed extensions if you alter them.

Thank you for contributing!
