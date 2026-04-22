namespace DomainRelay.Mapping.Planning;

internal static class FastPathAnalyzer
{
    public static bool IsSimpleAssignableMap(TypeMap typeMap)
    {
        if (typeMap.BeforeMapActions.Count > 0 || typeMap.AfterMapActions.Count > 0)
        {
            return false;
        }

        return typeMap.MemberMaps.All(m =>
            !m.Ignored
            && m.Condition is null
            && m.NullSubstitute is null
            && m.ValueResolver is not null);
    }
}