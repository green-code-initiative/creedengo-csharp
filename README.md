[![Creedengo on NuGet](https://img.shields.io/nuget/v/Creedengo.svg)](https://www.nuget.org/packages/Creedengo/) [![Creedengo on NuGet](https://img.shields.io/nuget/dt/Creedengo)](https://www.nuget.org/packages/Creedengo/)

Creedengo-C#
===========

_creedengo_ is a collective project aiming to reduce environmental footprint of software at the code level. The goal of the project is to provide a list of static code analyzers to highlight code structures that may have a negative ecological impact: energy and resources over-consumption, "fatware", shortening terminals' lifespan, etc.

_creedengo_ is based on evolving catalogs of [good practices](https://github.com/green-code-initiative/creedengo-rules-specifications/tree/main/docs/rules), for various technologies. This set of Roslyn analyzers implements these catalogs as rules for scanning your C# projects.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](https://github.com/green-code-initiative/creedengo-common/blob/main/doc/CODE_OF_CONDUCT.md)

🚀 Getting Started
------------------

There are several ways you can use the creedengo C# analyzers in your .Net projects:
1. As a [NuGet package](#-nuget-package)
2. As a [.NET tool](#-net-tool)
3. As an [analyzer for SonarQube](#-analyzer-for-sonarqube).

🧩 NuGet package
-----------------
The package is available on nuget.org at this address : https://www.nuget.org/packages/Creedengo, and can be added to your projects/solutions like any NuGet package. Once referenced and restored, the creedengo analyzers are automatically integrated in your IDE/compilation process, and will list any applicable info/alert.

Pre-requisite : .Net Standard 2.0, which can be used in a wide range of projects. See [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version) for details about the supported Frameworks.

🧩 .Net tool
-----------------
The .Net tool is available on nuget.org at this address : https://www.nuget.org/packages/Creedengo.Tool, and can be fetched on your machine using the following command :

`dotnet tool install --global Creedengo.Tool`

See [.Net tools documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for additional information.

Once installed, you can launch an analyzis on an existing codebase like this :

`creedengo-cli analyze path/to/mySolution.sln path/to/myReport.html`.

The file to analyze can be a .sln, a .slnx or a .csproj. The report format depends on it's required extension, the following are currently supported : .html, .json and .csv.

Pre-requisite : .Net 8 SDK.

🧩 Analyzer for SonarQube
-----------------
Creedengo C# can use [SonarScanner for .Net](https://docs.sonarsource.com/sonarqube/latest/analyzing-source-code/scanners/sonarscanner-for-dotnet/) to integrate with [SonarQube](https://www.sonarsource.com/products/sonarqube/), and uses a custom import addition to enrich what is reported to Sonar (severity, description, url page, category, and so on). See our [dedicated repository](https://github.com/green-code-initiative/creedengo-csharp-sonarqube) for more information.

🌿 Creedengo Rules
-------------------

|Id|Description|Severity|Code fix|
|--|-----------|:------:|:------:|
|[GCI69](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI69/csharp/GCI69.asciidoc)|Don’t call loop invariant functions in loop conditions|⚠️|❌|
|[GCI72](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI72/csharp/GCI72.asciidoc)|Don’t execute SQL queries in loops|⚠️|❌|
|[GCI75](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI75/csharp/GCI75.asciidoc)|Don’t concatenate `strings` in loops|⚠️|❌|
|[GCI81](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI81/csharp/GCI81.asciidoc)|Specify `struct` layouts|⚠️|✔️|
|[GCI82](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI82/csharp/GCI82.asciidoc)|Variable can be made constant|ℹ️|✔️|
|[GCI83](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI83/csharp/GCI83.asciidoc)|Replace Enum `ToString()` with `nameof`|⚠️|✔️|
|[GCI84](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI84/csharp/GCI84.asciidoc)|Avoid `async void` methods|⚠️|✔️|
|[GCI85](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI85/csharp/GCI85.asciidoc)|Make type `sealed`|ℹ️|✔️|
|[GCI86](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI86/csharp/GCI86.asciidoc)|`GC.Collect` should not be called|⚠️|❌|
|[GCI87](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI87/csharp/GCI87.asciidoc)|Use collection indexer|⚠️|✔️|
|[GCI88](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI88/csharp/GCI88.asciidoc)|Dispose resource asynchronously|⚠️|✔️|
|[GCI91](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI91/csharp/GCI91.asciidoc)|Use `Where` before `OrderBy`|⚠️|✔️|
|[GCI92](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI92/csharp/GCI92.asciidoc)|Use `Length` to test empty `strings`|⚠️|✔️|
|[GCI93](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI93/csharp/GCI93.asciidoc)|Return `Task` directly|ℹ️|✔️|
|[GCI95](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/GCI93/csharp/GCI95.asciidoc)|Use `is` operator instead of `as` operator|ℹ️|✔️|

🌿 Customized Roslyn Rules
-------------------

Creedengo C# customizes the severity of the following native Roslyn rules.

|Id|Description|Old Severity|New Severity|
|--|-----------|:----------:|:----------:|
|[CA1001](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1001)|Types that own disposable fields should be disposable|💤|⚠️|
|[CA1802](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1802)|Use Literals Where Appropriate|💤|⚠️|
|[CA1805](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1805)|Do not initialize unnecessarily|💤|⚠️|
|[CA1813](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1813)|Avoid unsealed attributes|💤|⚠️|
|[CA1816](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816)|Call GC.SuppressFinalize correctly|ℹ️|⚠️|
|[CA1821](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1821)|Remove empty finalizers|ℹ️|⚠️|
|[CA1822](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1822)|Mark members as static|ℹ️|⚠️|
|[CA1824](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1824)|Mark assemblies with NeutralResourcesLanguageAttribute|ℹ️|⚠️|
|[CA1825](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1825)|Avoid zero-length array allocations|ℹ️|⚠️|
|[CA1826](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1826)|Use property instead of Linq Enumerable method|ℹ️|⚠️|
|[CA1827](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1827)|Do not use Count()/LongCount() when Any() can be used|ℹ️|⚠️|
|[CA1828](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1828)|Do not use CountAsync/LongCountAsync when AnyAsync can be used|ℹ️|⚠️|
|[CA1829](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1829)|Use Length/Count property instead of Enumerable.Count method|ℹ️|⚠️|
|[CA1830](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1830)|Prefer strongly-typed Append and Insert method overloads on StringBuilder|ℹ️|⚠️|
|[CA1832](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1832)|Use AsSpan or AsMemory instead of Range-based indexers for getting ReadOnlySpan or ReadOnlyMemory portion of an array|ℹ️|⚠️|
|[CA1833](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1833)|Use AsSpan or AsMemory instead of Range-based indexers for getting Span or Memory portion of an array|ℹ️|⚠️|
|[CA1834](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1834)|Use StringBuilder.Append(char) for single character strings|ℹ️|⚠️|
|[CA1835](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1835)|Prefer the memory-based overloads of ReadAsync/WriteAsync methods in stream-based classes|ℹ️|⚠️|
|[CA1836](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1836)|Prefer IsEmpty over Count when available|ℹ️|⚠️|
|[CA1837](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1837)|Use Environment.ProcessId instead of Process.GetCurrentProcess().Id|ℹ️|⚠️|
|[CA1838](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1838)|Avoid StringBuilder parameters for P/Invokes|💤|⚠️|
|[CA1839](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1839)|Use Environment.ProcessPath instead of Process.GetCurrentProcess().MainModule.FileName|ℹ️|⚠️|
|[CA1840](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1840)|Use Environment.CurrentManagedThreadId instead of Thread.CurrentThread.ManagedThreadId|ℹ️|⚠️|
|[CA1841](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1841)|Prefer Dictionary Contains methods|ℹ️|⚠️|
|[CA1842](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1842)|Do not use 'WhenAll' with a single task|ℹ️|⚠️|
|[CA1843](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1843)|Do not use 'WaitAll' with a single task|ℹ️|⚠️|
|[CA1844](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1844)|Provide memory-based overrides of async methods when subclassing 'Stream'|ℹ️|⚠️|
|[CA1845](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1845)|Use span-based 'string.Concat'|ℹ️|⚠️|
|[CA1846](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1846)|Prefer AsSpan over Substring|ℹ️|⚠️|
|[CA1847](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1847)|Use String.Contains(char) instead of String.Contains(string) with single characters|ℹ️|⚠️|
|[CA1850](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1850)|Prefer static HashData method over ComputeHash|ℹ️|⚠️|
|[CA1853](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1853)|Unnecessary call to 'Dictionary.ContainsKey(key)'|ℹ️|⚠️|
|[CA1854](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1854)|Prefer the IDictionary.TryGetValue(TKey, out TValue) method|ℹ️|⚠️|
|[CA1855](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1855)|Use Span<T>.Clear() instead of Span<T>.Fill()|ℹ️|⚠️|
|[CA1858](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1858)|Use StartsWith instead of IndexOf|ℹ️|⚠️|
|[CA1859](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1859)|Prefer concrete types when possible for improved performance|ℹ️|⚠️|
|[CA1860](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1860)|Avoid using 'Enumerable.Any()' extension method|ℹ️|⚠️|
|[CA1863](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1863)|Use 'CompositeFormat'|💤|⚠️|
|[CA1864](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1864)|Prefer the 'IDictionary.TryAdd(TKey, TValue)' method|ℹ️|⚠️|
|[CA1865-7](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1865-ca1867)|Use 'string.Method(char)' instead of 'string.Method(string)' for string with single char|ℹ️|⚠️|
|[CA1868](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1868)|Unnecessary call to 'Contains' for sets|ℹ️|⚠️|
|[CA1869](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1869)|Cache and reuse 'JsonSerializerOptions' instances|ℹ️|⚠️|
|[CA1870](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1870)|Use a cached 'SearchValues' instance|ℹ️|⚠️|
|[CA1871](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1871)|Do not pass a nullable struct to 'ArgumentNullException.ThrowIfNull'|ℹ️|⚠️|
|[CA1872](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1872)|Prefer 'Convert.ToHexString' and 'Convert.ToHexStringLower' over call chains based on 'BitConverter.ToString'|ℹ️|⚠️|
|[CA2009](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2009)|Do not call ToImmutableCollection on an ImmutableCollection value|ℹ️|⚠️|
|[CA2215](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2215)|Dispose methods should call base class dispose|💤|⚠️|
|[CA2218](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2218)|Override GetHashCode on overriding Equals|ℹ️|⚠️|
|[CA2251](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2251)|Use String.Equals over String.Compare|💤|⚠️|
|[CA2264](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2264)|Do not pass a non-nullable value to 'ArgumentNullException.ThrowIfNull'|ℹ️|⚠️|

🤝 Contribution
---------------

See [contribution](https://github.com/green-code-initiative/creedengo-common/blob/main/doc/CONTRIBUTING.md) on the central repository.

🤓 Main contributors
--------------------

See [main contributors](https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/README.md#-main-contributors) on the central repository.
