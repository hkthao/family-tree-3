The build is failing due to an incompatible NuGet package:

**Error summary:**
- `FluentAssertions 1.3.0.1` is not compatible with `net8.0 (.NETCoreApp,Version=v8.0)`. It only supports older .NET Framework versions.

**Solution:**
1. **Update FluentAssertions:**  
   Replace `FluentAssertions 1.3.0.1` with a recent version that supports `.NET 8.0`.

**How to fix:**
Edit `/source/tests/Application.UnitTests/Application.UnitTests.csproj` and update the package reference:

```xml
<ItemGroup>
  <PackageReference Include="FluentAssertions" Version="6.13.0" />
</ItemGroup>
```
*(Choose the latest stable version available. Version 6.x supports .NET 8.)*

2. **Restore packages again:**
- Run `dotnet restore backend.sln` after updating the package reference.

**Links for reference:**
- [Application.UnitTests.csproj (ref: a6b901319f63e9f3468e0898fab42bdb6b3088bd)](https://github.com/hkthao/family-tree-3/blob/a6b901319f63e9f3468e0898fab42bdb6b3088bd/tests/Application.UnitTests/Application.UnitTests.csproj)
- [FluentAssertions NuGet](https://www.nuget.org/packages/FluentAssertions)

**Summary:**  
Update the `FluentAssertions` package in your unit test project to a version compatible with .NET 8, then rerun the build. This will resolve the compatibility error and allow the job to succeed.