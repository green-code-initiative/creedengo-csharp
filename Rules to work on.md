Ongoing existing rules list that could be analyzed/implemented in our C# project.
They seem to be good candidates but need reviewing first.

Note: rules already covered by the .NET SDK's built-in analyzers (CAxxxx) or compiler diagnostics
(SYSLIB1xxx) are excluded — for example MA0110 (regex source generator) is unnecessary because
SYSLIB1045 fires automatically on net7+, and a number of MA / RCS / S rules are superseded by CA1820,
CA1825-CA1877, etc.

From [Roslynator](https://github.com/dotnet/roslynator):
+ Optimize LINQ method call: [RCS1077](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1077/) — covers ~15 patterns; the green-relevant sub-scope worth implementing:
  + `items.Where(f => f is Foo).Cast<Foo>()` → `items.OfType<Foo>()`
  + `enumerable.OrderBy(f => f).Reverse()` → `enumerable.OrderByDescending(f => f)` (overlaps with RCS1200/S3169)
  + `listOfT.Select(f => M(f)).ToList()` → `listOfT.ConvertAll(f => M(f))`
  + `enumerable.SelectMany(f => f.Items).Count()` → `enumerable.Sum(f => f.Items.Count)`
+ Remove redundant 'ToCharArray' call: [RCS1107](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1107/)
+ Use Regex instance instead of static method: [RCS1186](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1186/)
+ Avoid unnecessary boxing of value type: [RCS1198](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1198/)
+ Call 'Enumerable.ThenBy' instead of 'Enumerable.OrderBy': [RCS1200](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1200/)
+ Optimize method call: [RCS1235](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1235/)

From [Meziantou Analyzer](https://github.com/meziantou/Meziantou.Analyzer):
+ Combine LINQ methods: [MA0029](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0029.md)
+ Remove useless ToString call: [MA0044](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0044.md)
+ Do not use finalizer: [MA0055](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0055.md)
+ Use Attribute.IsDefined instead of GetCustomAttribute(s): [MA0179](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0179.md)

From [SonarQube](https://www.sonarsource.com/products/sonarqube/):
+ Private fields only used as local variables in methods should become local variables: [S1450](https://rules.sonarsource.com/csharp/RSPEC-1450/)
+ Logging templates should be constant: [S2629](https://rules.sonarsource.com/csharp/RSPEC-2629/)
+ Duplicate casts should not be made: [S3247](https://rules.sonarsource.com/csharp/RSPEC-3247/)
+ "Assembly.GetExecutingAssembly" should not be called: [S3902](https://rules.sonarsource.com/csharp/RSPEC-3902/)
+ Start index should be used instead of calling Substring: [S4635](https://rules.sonarsource.com/csharp/RSPEC-4635/)
+ "Find" method should be used instead of the "FirstOrDefault" extension: [S6602](https://rules.sonarsource.com/csharp/RSPEC-6602/)
+ The collection-specific "TrueForAll" method should be used instead of the "All" extension: [S6603](https://rules.sonarsource.com/csharp/RSPEC-6603/)
+ Collection-specific "Exists" method should be used instead of the "Any" extension: [S6605](https://rules.sonarsource.com/csharp/RSPEC-6605/)
+ "Min/Max" properties of "Set" types should be used instead of the "Enumerable" extension methods: [S6609](https://rules.sonarsource.com/csharp/RSPEC-6609/)
+ "First" and "Last" properties of "LinkedList" should be used instead of the "First()" and "Last()" extension methods: [S6613](https://rules.sonarsource.com/csharp/RSPEC-6613/)
