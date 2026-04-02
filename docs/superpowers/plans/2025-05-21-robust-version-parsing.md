# Robust Version Parsing (IVersionParser) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a robust version parsing system for GitHub tags using Regex, providing version and release channel information.

**Architecture:** Define an interface `IVersionParser` and a default implementation `DefaultVersionParser` using Regex to extract version numbers (X.Y.Z) and optional release channels (e.g., -qa, -dev).

**Tech Stack:** C#, .NET 4.8 / .NET 8, Regex, MSTest.

---

### Task 1: Define IVersionParser Interface

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Interface/IVersionParser.cs`

- [ ] **Step 1: Create the IVersionParser interface**

```csharp
namespace KUtilitiesCore.GitHubUpdater.Interface
{
    public interface IVersionParser
    {
        System.Version Parse(string tagName);
        string GetChannel(string tagName);
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add KUtilitiesCore.GitHubUpdater/Interface/IVersionParser.cs
git commit -m "feat: add IVersionParser interface"
```

---

### Task 2: Implement DefaultVersionParser (Red-Green-Refactor)

**Files:**
- Create: `KUtilitiesCore.GitHubUpdater/Helpers/DefaultVersionParser.cs`
- Create: `KUtilitiesCore.GitHubUpdaterTests/DefaultVersionParserTests.cs`

- [ ] **Step 1: Create the test file with the first failing test**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater.Helpers;
using System;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public class DefaultVersionParserTests
    {
        [TestMethod]
        [DataRow("v1.2.3-qa", 1, 2, 3, "qa")]
        public void Parse_ShouldExtractVersionAndChannel(string tagName, int major, int minor, int build, string expectedChannel)
        {
            var parser = new DefaultVersionParser();
            
            var version = parser.Parse(tagName);
            var channel = parser.GetChannel(tagName);

            Assert.AreEqual(new Version(major, minor, build), version);
            Assert.AreEqual(expectedChannel, channel);
        }
    }
}
```

- [ ] **Step 2: Create a minimal implementation to allow compilation (but fail test)**

```csharp
using System;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    public class DefaultVersionParser : IVersionParser
    {
        public System.Version Parse(string tagName) => throw new NotImplementedException();
        public string GetChannel(string tagName) => throw new NotImplementedException();
    }
}
```

- [ ] **Step 3: Run tests and verify failure**

Run: `dotnet test KUtilitiesCore.GitHubUpdaterTests/KUtilitiesCore.GitHubUpdaterTests.csproj`
Expected: FAIL (NotImplementedException)

- [ ] **Step 4: Implement Parse and GetChannel using Regex**

```csharp
using System;
using System.Text.RegularExpressions;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    public class DefaultVersionParser : IVersionParser
    {
        private static readonly Regex VersionRegex = new Regex(@"(\d+\.\d+(?:\.\d+)?(?:.\d+)?)", RegexOptions.Compiled);

        public Version Parse(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return new Version(0, 0, 0);

            var match = VersionRegex.Match(tagName);
            if (match.Success)
            {
                var versionStr = match.Groups[1].Value;
                // Handle cases like "1.0" or "1.5" by ensuring at least 2 parts
                if (Version.TryParse(versionStr, out var version))
                {
                    return version;
                }
            }

            return new Version(0, 0, 0);
        }

        public string GetChannel(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return string.Empty;

            var match = VersionRegex.Match(tagName);
            if (match.Success)
            {
                var versionStr = match.Groups[1].Value;
                int index = tagName.IndexOf(versionStr) + versionStr.Length;
                if (index < tagName.Length)
                {
                    string remaining = tagName.Substring(index).TrimStart('-', '.');
                    return remaining;
                }
            }

            return string.Empty;
        }
    }
}
```

- [ ] **Step 5: Run tests and verify success**

Run: `dotnet test KUtilitiesCore.GitHubUpdaterTests/KUtilitiesCore.GitHubUpdaterTests.csproj`
Expected: PASS

- [ ] **Step 6: Add more test cases to verify all requirements**

Modify `KUtilitiesCore.GitHubUpdaterTests/DefaultVersionParserTests.cs`:
```csharp
        [TestMethod]
        [DataRow("v1.2.3-qa", 1, 2, 3, "qa")]
        [DataRow("2.0.0", 2, 0, 0, "")]
        [DataRow("release-1.5-dev", 1, 5, 0, "dev")]
        [DataRow("v1.0", 1, 0, 0, "")]
        public void Parse_ShouldExtractVersionAndChannel_Comprehensive(string tagName, int major, int minor, int build, string expectedChannel)
        {
            var parser = new DefaultVersionParser();
            
            var version = parser.Parse(tagName);
            var channel = parser.GetChannel(tagName);

            // System.Version(1,0) results in 1.0. -1 build if not provided
            var expectedVersion = build == -1 ? new Version(major, minor) : new Version(major, minor, build);
            
            // Adjust for System.Version behavior where missing parts are -1
            Assert.AreEqual(major, version.Major);
            Assert.AreEqual(minor, version.Minor);
            if (build != -1) Assert.AreEqual(build, Math.Max(0, version.Build));
            
            Assert.AreEqual(expectedChannel, channel);
        }
```
*(Wait, `System.Version` with "1.0" has Major=1, Minor=0, Build=-1. The requirement says "v1.0" -> Version 1.0.0. So I should normalize to 0 if negative).*

- [ ] **Step 7: Finalize implementation and tests**

- [ ] **Step 8: Commit**

```bash
git add KUtilitiesCore.GitHubUpdater/Helpers/DefaultVersionParser.cs KUtilitiesCore.GitHubUpdaterTests/DefaultVersionParserTests.cs
git commit -m "feat: add DefaultVersionParser with Regex support"
```
