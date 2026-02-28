# KoreCommon Coding Conventions

## Naming
- Prefix all class names with "Kore" (e.g., `KoreCalendarEvent`, `KoreMeshData`)
- Private fields use underscore prefix: `_fieldName`
- Use PascalCase for public properties and methods
- Use camelCase for parameters and local variables

## Comments
- **Use // comments only, never /// XML documentation**
- Begin all files with `// <fileheader>`
- Use inline comments for clarity, not verbose explanations
- No tags like `<summary>`, `<param>`, `<returns>` - use plain text

## Code Style
- Use file-scoped namespaces: `namespace KoreCommon;`
- Use `var` for local variable declarations when type is obvious
- Keep constructors organized by parameter complexity

## Vertical Alignment
- **Align related code vertically whenever possible for improved readability**
- Align assignment operators (=) in related variable declarations
- Align comments at the end of enum values or field declarations
- Align parameter names in similar method calls
- Example:
  ```csharp
  _eventType      = eventType;
  _nextOccurrence = startOccurrence;
  _intervalValue  = intervalValue;
  ```

## Structure
- Organize code by feature areas (Mesh, Network, Util, etc.)
- Place unit tests in parallel UnitTest folders
- Use consistent file naming: `KoreClassName.cs`, `KoreTestClassName.cs`
- Multi-part classes use dot notation: `KoreClassName.Feature.cs`

## Testing
- All tests follow pattern: `KoreTestXXX.RunTests(testLog)`
- Use `testLog.AddResult()` for pass/fail assertions
- Use `testLog.AddComment()` for informational output
- Register tests in `KoreTestCenter.RunTests()`
