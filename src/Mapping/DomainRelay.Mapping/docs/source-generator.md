# Source generator

The source generator supports simple mapping generation through `[GenerateMapping]`.

## Example

```csharp
[GenerateMapping(typeof(User), typeof(UserDto))]
public sealed partial class MappingHints
{
}