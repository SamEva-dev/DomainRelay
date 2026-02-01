using System.Diagnostics;

namespace DomainRelay.Diagnostics;

public static class DomainRelayActivity
{
    public const string SourceName = "DomainRelay";
    public static readonly ActivitySource Source = new(SourceName);
}
