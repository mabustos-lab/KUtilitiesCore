# GitHubUpdater Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform the updater into a robust, SOLID-compliant system that manages the full update lifecycle: Finding -> Comparing -> Selecting Assets -> Downloading with progress.

**Architecture:** 
- Introduces `IVersionParser` for reliable SemVer extraction via Regex.
- Introduces `IAssetSelector` for pattern-based (wildcard) file selection.
- Introduces `GitHubUpdateManager` as an orchestrator that coordinates `GitHubUpdateService` and `GitHubAssetDownloader`.
- Decouples components through interfaces to allow unit testing without real GitHub API calls.

**Tech Stack:** C# (.NET 4.8 / .NET 8), MSTest, Regex, HttpClient/WebClient.

---

### Task 1: Update Configuration Model

**Files:**
- Modify: `KUtilitiesCore.GitHubUpdater/Interface/IAppUpdateInfo.cs`
- Modify: `KUtilitiesCore.GitHubUpdater/AppUpdateInfo.cs`

- [ ] **Step 1: Add AssetPattern to IAppUpdateInfo**
Add the property to the interface.
```csharp
string AssetPattern { get; set; }
```

- [ ] **Step 2: Implement AssetPattern in AppUpdateInfo**
Update the class and its constructor to initialize the property.
```csharp
public string AssetPattern { get; set; } = "*";
```

- [ ] **Step 3: Update Save/Load logic if necessary**
Ensure `AssetPattern` is serialized.

- [ ] **Step 4: Commit**
`git add . && git commit -m "feat: add AssetPattern to AppUpdateInfo"`

---

### Task 2: Robust Version Parsing (IVersionParser)

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Interface/IVersionParser.cs`
- Create: `KUtilitiesCore.GitHubUpdater/Helpers/DefaultVersionParser.cs`
- Test: `KUtilitiesCore.GitHubUpdaterTests/DefaultVersionParserTests.cs`

- [ ] **Step 1: Define IVersionParser interface**
```csharp
namespace KUtilitiesCore.GitHubUpdater.Interface
{
    public interface IVersionParser
    {
        Version Parse(string tagName);
        string GetChannel(string tagName);
    }
}
```

- [ ] **Step 2: Create failing tests for version parsing**
Test cases: "v1.2.3-qa" -> 1.2.3, "2.0.0" -> 2.0.0, "release-1.5-dev" -> 1.5.0.

- [ ] **Step 3: Implement DefaultVersionParser using Regex**
```csharp
using System;
using System.Text.RegularExpressions;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    public class DefaultVersionParser : IVersionParser
    {
        public Version Parse(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return new Version(0, 0, 0);
            var match = Regex.Match(tagName, @"(\d+\.\d+\.\d+)");
            return match.Success ? Version.Parse(match.Value) : new Version(0, 0, 0);
        }

        public string GetChannel(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return string.Empty;
            var parts = tagName.Split('-');
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}
```

- [ ] **Step 4: Verify tests and Commit**
`git add . && git commit -m "feat: add DefaultVersionParser with Regex support"`

---

### Task 3: Pattern-based Asset Selection (IAssetSelector)

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Interface/IAssetSelector.cs`
- Create: `KUtilitiesCore.GitHubUpdater/Helpers/WildcardAssetSelector.cs`
- Test: `KUtilitiesCore.GitHubUpdaterTests/WildcardAssetSelectorTests.cs`

- [ ] **Step 1: Define IAssetSelector interface**
```csharp
using System.Collections.Generic;
using KUtilitiesCore.GitHubUpdater.Helpers;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    public interface IAssetSelector
    {
        GitHubAsset? Select(IEnumerable<GitHubAsset> assets, string pattern);
    }
}
```

- [ ] **Step 2: Implement WildcardAssetSelector**
Use a simple conversion from wildcard to Regex (e.g., `*` to `.*`).
```csharp
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    public class WildcardAssetSelector : IAssetSelector
    {
        public GitHubAsset? Select(IEnumerable<GitHubAsset> assets, string pattern)
        {
            if (assets == null || string.IsNullOrEmpty(pattern)) return null;
            string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return assets.FirstOrDefault(a => Regex.IsMatch(a.Name ?? "", regex, RegexOptions.IgnoreCase));
        }
    }
}
```

- [ ] **Step 3: Verify tests and Commit**
`git add . && git commit -m "feat: add WildcardAssetSelector for pattern matching"`

---

### Task 4: The Orchestrator (GitHubUpdateManager)

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Interface/IGitHubUpdateManager.cs`
- Create: `KUtilitiesCore.GitHubUpdater/GitHubUpdateManager.cs`

- [ ] **Step 1: Define IGitHubUpdateManager**
Include methods for checking and downloading.
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.AssetDownloader;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    public interface IGitHubUpdateManager
    {
        event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        Task<GitHubRelease?> CheckForUpdatesAsync();
        Task DownloadUpdateAsync(GitHubRelease release, string destinationPath, CancellationToken ct = default);
    }
}
```

- [ ] **Step 2: Implement GitHubUpdateManager**
Inject `GitHubUpdateService`, `GitHubAssetDownloader`, `IVersionParser`, and `IAssetSelector`.

- [ ] **Step 3: Connect Progress Reporting**
Subscribe to `GitHubAssetDownloader.DownloadProgress` and forward it.

- [ ] **Step 4: Commit**
`git add . && git commit -m "feat: add GitHubUpdateManager orchestrator"`

---

### Task 5: Final Integration & Cleanup

- [ ] **Step 1: Update GitHubUpdateService**
Optionally refactor to use `IVersionParser` for cleaner filtering.

- [ ] **Step 2: Create a full integration test**
A test that uses the `Manager` to run the whole flow.

- [ ] **Step 3: Final Documentation**
Ensure all public members have XML comments.

- [ ] **Step 4: Final Commit**
`git add . && git commit -m "refactor: final integration and cleanup of GitHubUpdater"`
