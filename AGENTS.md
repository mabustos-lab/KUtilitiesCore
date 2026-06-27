# AGENTS.md — KUtilitiesCore

## Build & Test

```bash
dotnet build KUtilitiesCore.sln
dotnet test KUtilitiesCore.sln
```

Run a single test project:
```bash
dotnet test KUtilitiesCore.DataAccessTests
```

Run a single test by name:
```bash
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~WithResultDelegate_RecordStruct_MapsCorrectly"
```

No lint, formatter, or typecheck commands exist. Build warnings = the verification gate.

## Project Map

Core libraries (multi-target `net48;net8.0` unless noted):

| Project | Dir | TFMs | Role |
|---|---|---|---|
| KUtilitiesCore | `KUtilitiesCore/` | net48;net8.0 | Base utilities (validation, LINQ ext, telemetry) |
| KUtilitiesCore.Encryption | `KUtiitiesCore.Encryption/` | net48;net8.0 | AES, Base64, DPAPI crypto |
| KUtilitiesCore.MVVM | `KUtilitiesCore.MVVM/` | net48;net8.0 | RelayCommands, ViewModel helpers |
| KUtilitiesCore.MVVM.Messaging | `KUtilitiesCore.MVVM.Messaging/` | net48;net8.0 | Decoupled messaging |
| KUtilitiesCore.Data | `KUtilitiesCore.Data/` | net48;net8.0 | CSV/Excel import-export (ClosedXML) |
| KUtilitiesCore.Data.Win | `KUtilitiesCore.Data.Win/` | net48;net8.0-windows | WinForms UI components |
| KUtilitiesCore.DataAccess | `KUtilitiesCore.DataAccess/` | **netstandard2.1** | Abstract interfaces (UoW, Specification, Paging) |
| KUtilitiesCore.Dal | `KUtilitiesCore.Dal/` | **net8.0 only** | SQL Server DAL (DaoContext, DataReaderConverter, BulkInsert) |
| KUtilitiesCore.DataAccess.EfCore | `KUtilitiesCore.DataAccess.EfCore/` | net8.0 | EF Core implementation of DataAccess |
| KUtilitiesCore.DataAccess.Http | `KUtilitiesCore.DataAccess.Http/` | net8.0 | HTTP API data access (Polly resilience) |
| KUtilitiesCore.Logger | `KUtilities.Logger/` | **netstandard2.0** | Logging (file, SQL, console providers) |
| KUtilitiesCore.GitHubUpdater | `KUtilitiesCore.GitHubUpdater/` | net48;net8.0 | GitHub API auto-updater |

Dependency chain (simplified): `DataAccess` (abstractions) → `Dal` (SQL Server impl), `EfCore`, `Http`. `Encryption` → `Core` → `MVVM` → `MVVM.Messaging`. `Core` + `Logger` → `Dal`.

## Development Approach

- **SOLID first**: Apply SOLID principles for extensibility and maintainability. New features should fit existing abstraction layers (e.g. `DataAccess` interfaces → `Dal`/`EfCore`/`Http` implementations).
- **Specify before coding**: Define the intent and interface contract before implementing. Document key decisions and rejected alternatives in XML doc comments or commit messages.
- **TDD**: Write tests before implementation code. Validate edge cases — concurrency, error paths, empty/large data sets.
- **Small, reusable modules**: Prefer small focused types over monolithic classes. Follow existing patterns (e.g. `IMappingStrategy` strategy pattern, `DataReaderConverter` fluent builder).

## Conventions

- **Language**: Code documentation and XML doc comments are in **Spanish**.
- **Nullable**: Inconsistent across projects — `KUtilitiesCore.Dal` and `KUtilitiesCore.Data` use `Nullable disable`, most others use `Nullable enable`. Match the existing project setting.
- **No `InternalsVisibleTo`**: Internal types in `Dal` (e.g. `IMappingStrategy`) are consumed via `DaoContext.ExecuteReaderCore` which is public. Tests cannot reference internals directly.
- **NuGet packages**: All use `GenerateDocumentationFile=True`. No `Directory.Build.props` or `Directory.Packages.props` — each `.csproj` manages its own package versions.
- **Encryption project dir typo**: The Encryption project folder is `KUtiitiesCore.Encryption` (triple 'i'). All references must use this exact path.
- **XML doc comments**: Explain *purpose* and *why*, not *what* the code does. Include usage examples for public APIs (`<example>` tag) when the usage pattern is non-obvious.
- **Naming**: Follow standard C# conventions. Interface prefix `I`, `PascalCase` for public members, `_camelCase` for private fields.

## Testing

- **Framework**: MSTest 4.2.3 everywhere except `KUtilitiesCore.MVVMTests.EventCommandBinder` which uses xUnit 2.9.0.
- **Mocking**: Moq 4.20.72 (only in `KUtilitiesCore.DataAccessTests`).
- **No .editorconfig or analyzer rules** — no enforced style beyond compiler warnings.
- **Edge cases to cover**: concurrency, error/exception paths, empty collections, large data sets.

### Dal/DataAccess testing gotchas

- `DaoContext` requires a real `SecureConnectionBuilder` even with a mocked `DbDataReader`. Tests use `localhost` + `IntegratedSecurity=true`. No actual SQL Server connection is made when passing `dbDataReader` to `ExecuteReaderCore`.
- Mocking `DbDataReader` for multi-result-set scenarios requires `CallBase = true`, `Read()` must return `false` after finite rows (infinite loop otherwise), and `NextResult()` must return `false` after the last result set.
- Mock may throw `ObjectDisposedException` on Dispose — handle via try/catch with `Assert.Inconclusive`.
- `KUtilitiesCore.GitHubUpdaterTests` targets **net48 only** and uses `DotNetEnv` for env loading.
- `KUtilitiesCore.Data.WinTests` targets **net8.0-windows** (WinForms required).

## Architecture notes

- `IMappingStrategy.Map(DataTable)` is internal to `KUtilitiesCore.Dal`. Strategy implementations: `ObjectMappingStrategy<TResult>` (reflection, requires `class, new()`), `DelegateMappingStrategy<TResult>` (delegate, no constraints), `DataTableMappingStrategy` (raw DataTable).
- `IReaderResultSet.GetResult<TResult>()` has `where TResult : class, new()` constraint. `GetResultUnsafe<TResult>()` has no constraints — use for structs/records without parameterless ctor.
- `DataReaderConverter` is a fluent builder: `.Create().WithResult<T>().WithResult<T>(mapper).WithDefaultDataTable()`.
- `KUtilitiesCore.DataAccess` is the abstraction layer (netstandard2.1). Concrete implementations (`Dal`, `EfCore`, `Http`) are separate projects targeting net8.0+.
- Offline `SPHelper/` and `Paging/` folders in `Dal`/`DataAccess` are excluded from compile (`<Compile Remove>`).
