# Projection

Projection is intended for `IQueryable` scenarios.

## API

- `IProjectionBuilder`
- `ProjectTo<TSource, TDestination>()`

## Supported subset
- convention-based members
- `MapFrom(Expression)`
- flattening
- simple record constructor projection

## Example

```csharp
var projected = query.ProjectTo<User, UserDto>(projectionBuilder);

Important

Do not use runtime-only delegates and assume they will translate to providers.