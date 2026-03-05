using System.Collections.Concurrent;

namespace DomainRelay.Internal;

public static class TypeNameCache
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    public static string GetFriendlyName(Type t)
        => Cache.GetOrAdd(t, static x => x.FullName ?? x.Name);
}
