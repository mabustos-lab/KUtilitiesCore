# AGENTS.md — KUtilitiesCore

## Build & Test

The SDK is pinned by `global.json` to the 10.x band (`rollForward: latestFeature`) with the new `dotnet test` MTP experience (`"test": { "runner": "Microsoft.Testing.Platform" }`). All 7 MSTest test projects set `<EnableMSTestRunner>true</EnableMSTestRunner>`; `EventCommandBinder` uses `UseMicrosoftTestingPlatform`.

```bash
dotnet build KUtilitiesCore.sln
dotnet test --solution KUtilitiesCore.sln
```

Run a single test project:
```bash
dotnet test KUtilitiesCore.DataAccessTests
```

Run a single test by name:
```bash
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~WithResultDelegate_RecordStruct_MapsCorrectly"
```

Note: under the .NET 10 SDK, VSTest mode of `dotnet test` no longer supports Microsoft.Testing.Platform v2 — use the `--solution` syntax above (aka.ms/dotnet-test-mtp-error).

No lint, formatter, or typecheck commands exist. Build warnings = the verification gate.

## Project Map

Core libraries (net10.0 unless noted; net48/net8.0 support was removed in the .NET 10 jump):

| Project | Dir | TFMs | Role |
|---|---|---|---|
| KUtilitiesCore | `KUtilitiesCore/` | net10.0 | Base utilities (validation, LINQ ext, telemetry) |
| KUtilitiesCore.Encryption | `KUtiitiesCore.Encryption/` | net10.0 | AES, Base64, DPAPI crypto (DPAPI requires Windows) |
| KUtilitiesCore.MVVM | `KUtilitiesCore.MVVM/` | net10.0 | RelayCommands, ViewModel helpers |
| KUtilitiesCore.MVVM.Messaging | `KUtilitiesCore.MVVM.Messaging/` | net10.0 | Decoupled messaging |
| KUtilitiesCore.Data | `KUtilitiesCore.Data/` | net10.0 | CSV/Excel import-export (ClosedXML) |
| KUtilitiesCore.Data.Win | `KUtilitiesCore.Data.Win/` | **net10.0-windows** | WinForms UI components |
| KUtilitiesCore.DataAccess | `KUtilitiesCore.DataAccess/` | **netstandard2.1** | Abstract interfaces (UoW, Specification, Paging) |
| KUtilitiesCore.Dal | `KUtilitiesCore.Dal/` | net10.0 | SQL Server DAL (DaoContext, DataReaderConverter, BulkInsert) |
| KUtilitiesCore.DataAccess.EfCore | `KUtilitiesCore.DataAccess.EfCore/` | net10.0 | EF Core 10 implementation of DataAccess |
| KUtilitiesCore.DataAccess.Http | `KUtilitiesCore.DataAccess.Http/` | net10.0 | HTTP API data access (Polly resilience) |
| KUtilitiesCore.Logger | `KUtilities.Logger/` | **netstandard2.0** | Logging (file, SQL, console providers) |
| KUtilitiesCore.GitHubUpdater | `KUtilitiesCore.GitHubUpdater/` | net10.0 | GitHub API auto-updater |

Dependency chain (simplified): `DataAccess` (abstractions) → `Dal` (SQL Server impl), `EfCore`, `Http`. `Encryption` → `Core` → `MVVM` → `MVVM.Messaging`. `Core` + `Logger` → `Dal`.

## Development Approach

- **SOLID first**: Apply SOLID principles for extensibility and maintainability. New features should fit existing abstraction layers (e.g. `DataAccess` interfaces → `Dal`/`EfCore`/`Http` implementations).
- **Specify before coding**: Define the intent and interface contract before implementing. Document key decisions and rejected alternatives in XML doc comments or commit messages.
- **TDD**: Write tests before implementation code. Validate edge cases — concurrency, error paths, empty/large data sets.
- **Small, reusable modules**: Prefer small focused types over monolithic classes. Follow existing patterns (e.g. `IMappingStrategy` strategy pattern, `DataReaderConverter` fluent builder).
- **Type safety** → All code must be fully typed.
- **Gradual changes** → Opt for incremental changes rather than major refactorings.
- **Questioning assumptions** → Always check implicit inferences.
- **Pattern detection** → Identify and highlight repeated code.

## Conventions

- **Language**: Code documentation and XML doc comments are in **Spanish**.
- **Nullable**: All library projects use `Nullable enable` (Dal and Data were flipped in the `eliminate-build-warnings` change) except `KUtilitiesCore.DataAccess` and `KUtilitiesCore.DataAccess.Http`, which keep `Nullable disable` (oblivious contracts, netstandard2.1; their warnings were docs-only/zero). Test projects are mixed. Match the existing project setting.
- **No `InternalsVisibleTo`**: Internal types in `Dal` (e.g. `IMappingStrategy`) are consumed via `DaoContext.ExecuteReaderCore` which is public. Tests cannot reference internals directly.
- **NuGet packages**: All use `GenerateDocumentationFile=True`. No `Directory.Build.props` or `Directory.Packages.props` — each `.csproj` manages its own package versions.
- **Encryption project dir typo**: The Encryption project folder is `KUtiitiesCore.Encryption` (triple 'i'). All references must use this exact path.
- **XML doc comments**: Explain *purpose* and *why*, not *what* the code does. Include usage examples for public APIs (`<example>` tag) when the usage pattern is non-obvious.
- **Naming**: Variables, functions and classes in English, following Microsoft standards. Interface prefix `I`, `PascalCase` for public members, `_camelCase` for private fields.
- Ensure compliance with the **SOLID** principles and good engineering practice.

## Testing

- **Framework**: MSTest 4.4.1 (Microsoft.NET.Test.Sdk 18.10.1) everywhere except `KUtilitiesCore.MVVMTests.EventCommandBinder` which uses xunit.v3 4.0.1 with Microsoft.Testing.Platform 2.4.1.
- **Mocking**: Moq 4.20.72 (only in `KUtilitiesCore.DataAccessTests`).
- **No .editorconfig or analyzer rules** — no enforced style beyond compiler warnings.
- **Edge cases to cover**: concurrency, error/exception paths, empty collections, large data sets.

### Dal/DataAccess testing gotchas

- `DaoContext` requires a real `SecureConnectionBuilder` even with a mocked `DbDataReader`. Tests use `localhost` + `IntegratedSecurity=true`. No actual SQL Server connection is made when passing `dbDataReader` to `ExecuteReaderCore`.
- Mocking `DbDataReader` for multi-result-set scenarios requires `CallBase = true`, `Read()` must return `false` after finite rows (infinite loop otherwise), and `NextResult()` must return `false` after the last result set.
- Mock may throw `ObjectDisposedException` on Dispose — handle via try/catch with `Assert.Inconclusive`.
- `KUtilitiesCore.GitHubUpdaterTests` targets **net10.0** and uses `DotNetEnv` for env loading. `GitHubUpdateServiceTest` calls the live GitHub API and fails with 401 Unauthorized without a valid `GITHUB_TOKEN` in the environment — pre-existing, not a regression.
- `KUtilitiesCore.Data.WinTests` targets **net10.0-windows** (WinForms required).

## Architecture notes

- `IMappingStrategy.Map(DataTable)` is internal to `KUtilitiesCore.Dal`. Strategy implementations: `ObjectMappingStrategy<TResult>` (reflection, requires `class, new()`), `DelegateMappingStrategy<TResult>` (delegate, no constraints), `DataTableMappingStrategy` (raw DataTable).
- `IReaderResultSet.GetResult<TResult>()` has `where TResult : class, new()` constraint. `GetResultUnsafe<TResult>()` has no constraints — use for structs/records without parameterless ctor.
- `DataReaderConverter` is a fluent builder: `.Create().WithResult<T>().WithResult<T>(mapper).WithDefaultDataTable()`.
- `KUtilitiesCore.DataAccess` is the abstraction layer (netstandard2.1). Concrete implementations (`Dal`, `EfCore`, `Http`) are separate projects targeting net10.0.
- Offline `SPHelper/` and `Paging/` folders in `Dal`/`DataAccess` are excluded from compile (`<Compile Remove>`).

## Specific rules
For detailed rules and guidelines specific to the various areas of the project, see:

- [OpenSpec Tasks Mandatory Steps](./openspec-tasks-mandatory-steps.md) - Checklist and mandatory implementation guidelines for creating or updating OpenSpec `tasks.md` files
