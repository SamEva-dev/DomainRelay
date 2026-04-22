# Migration from AutoMapper

## Similar concepts

- `CreateMap<TSource, TDestination>()`
- `ForMember(..., MapFrom(...))`
- `Ignore(...)`
- `ReverseMap()` simple
- projection support
- expression translation support

## Migration strategy

1. migrate runtime convention maps first
2. migrate explicit member maps
3. migrate nested and collection maps
4. migrate projection separately
5. validate translated expressions separately

## Important difference

Projection and runtime behavior are intentionally separated and documented explicitly.