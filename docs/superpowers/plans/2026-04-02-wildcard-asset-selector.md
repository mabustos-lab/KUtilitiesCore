# WildcardAssetSelector Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement `IAssetSelector` and `WildcardAssetSelector` to allow selecting GitHub assets using wildcard patterns (e.g., `*Setup*.exe`).

**Architecture:** Define an interface `IAssetSelector` for strategy pattern and implement it in `WildcardAssetSelector` using Regex for pattern matching.

**Tech Stack:** .NET 4.8 / .NET 8, MSTest, Regex.

---

### Task 1: Define IAssetSelector Interface

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Interface/IAssetSelector.cs`

- [ ] **Step 1: Create the IAssetSelector interface**

```csharp
using System.Collections.Generic;
using KUtilitiesCore.GitHubUpdater.Helpers;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Define una interfaz para seleccionar un asset de una lista basado en un patrón.
    /// </summary>
    public interface IAssetSelector
    {
        /// <summary>
        /// Selecciona el primer asset que coincida con el patrón proporcionado.
        /// </summary>
        /// <param name="assets">Lista de assets de GitHub.</param>
        /// <param name="pattern">Patrón de búsqueda (ej. comodines).</param>
        /// <returns>El asset encontrado o null si no hay coincidencias.</returns>
        GitHubAsset? Select(IEnumerable<GitHubAsset> assets, string pattern);
    }
}
```

- [ ] **Step 2: Verify compilation**
Run: `dotnet build KUtilitiesCore.GitHubUpdater/KUtilitiesCore.GitHubUpdater.csproj`
Expected: PASS

---

### Task 2: Implement WildcardAssetSelector (TDD)

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Helpers/WildcardAssetSelector.cs`
- Create: `KUtilitiesCore.GitHubUpdaterTests/WildcardAssetSelectorTests.cs`

- [ ] **Step 1: Write failing tests for WildcardAssetSelector**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;
using System.Collections.Generic;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public class WildcardAssetSelectorTests
    {
        private List<GitHubAsset> _assets;
        private IAssetSelector _selector;

        [TestInitialize]
        public void Setup()
        {
            _assets = new List<GitHubAsset>
            {
                new GitHubAsset { Name = "Setup-1.0.0.exe" },
                new GitHubAsset { Name = "MyApp.zip" },
                new GitHubAsset { Name = "Notes.txt" },
                new GitHubAsset { Name = "setup-v2.EXE" }
            };
            _selector = new WildcardAssetSelector();
        }

        [TestMethod]
        public void Select_WithExactExtension_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*.zip");
            Assert.IsNotNull(result);
            Assert.AreEqual("MyApp.zip", result.Name);
        }

        [TestMethod]
        public void Select_WithWildcardInMiddle_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*Setup*");
            Assert.IsNotNull(result);
            Assert.AreEqual("Setup-1.0.0.exe", result.Name);
        }

        [TestMethod]
        public void Select_IsCaseInsensitive_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*.exe");
            Assert.IsNotNull(result);
            // Should return the first one "Setup-1.0.0.exe" even if pattern is lowercase
            Assert.AreEqual("Setup-1.0.0.exe", result.Name);
            
            var result2 = _selector.Select(_assets, "SETUP*");
            Assert.IsNotNull(result2);
            Assert.AreEqual("Setup-1.0.0.exe", result2.Name);
        }

        [TestMethod]
        public void Select_WithNoMatch_ReturnsNull()
        {
            var result = _selector.Select(_assets, "*.msi");
            Assert.IsNull(result);
        }
    }
}
```

- [ ] **Step 2: Create skeleton implementation to allow test compilation**

```csharp
using System;
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
            throw new NotImplementedException();
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**
Run: `dotnet test KUtilitiesCore.GitHubUpdaterTests/KUtilitiesCore.GitHubUpdaterTests.csproj --filter WildcardAssetSelectorTests`
Expected: FAIL (NotImplementedException)

- [ ] **Step 4: Implement WildcardAssetSelector logic**

```csharp
using System;
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
            if (assets == null || string.IsNullOrEmpty(pattern))
                return null;

            // Convert wildcard to regex:
            // 1. Escape special regex characters
            // 2. Replace escaped wildcard (*) with (.*)
            // 3. Replace escaped wildcard (?) with (.)
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            return assets.FirstOrDefault(a => a.Name != null && regex.IsMatch(a.Name));
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**
Run: `dotnet test KUtilitiesCore.GitHubUpdaterTests/KUtilitiesCore.GitHubUpdaterTests.csproj --filter WildcardAssetSelectorTests`
Expected: PASS

- [ ] **Step 6: Commit changes**
Run: `git add KUtilitiesCore.GitHubUpdater/Interface/IAssetSelector.cs KUtilitiesCore.GitHubUpdater/Helpers/WildcardAssetSelector.cs KUtilitiesCore.GitHubUpdaterTests/WildcardAssetSelectorTests.cs`
Run: `git commit -m "feat: add WildcardAssetSelector for pattern matching"`
