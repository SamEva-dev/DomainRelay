
## `docs/expression-translation.md`

```md id="t4u5v6"
# Expression translation

Expression translation rewrites an expression written for the destination type into an equivalent expression over the source type.

## API

- `IExpressionTranslator`
- `Translate<TSource, TDestination, TResult>()`
- `WhereTranslated(...)`
- `OrderByTranslated(...)`

## Supported subset
- simple members
- `MapFrom(Expression)`
- flattening
- simple string methods:
  - `Contains`
  - `StartsWith`
  - `EndsWith`