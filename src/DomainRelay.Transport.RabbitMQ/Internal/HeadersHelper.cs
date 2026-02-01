using System.Diagnostics;
using System.Text.Json;

namespace DomainRelay.Transport.RabbitMQ.Internal;

internal static class HeadersHelper
{
    public static IDictionary<string, object> BuildHeaders(string? headersJson, bool injectTracing)
    {
        var headers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(headersJson))
        {
            TryMergeJson(headers, headersJson);
        }

        if (injectTracing)
        {
            InjectTracing(headers);
        }

        return headers;
    }

    private static void TryMergeJson(Dictionary<string, object> headers, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return;

            foreach (var p in doc.RootElement.EnumerateObject())
            {
                headers[p.Name] = p.Value.ValueKind switch
                {
                    JsonValueKind.String => p.Value.GetString() ?? "",
                    JsonValueKind.Number => p.Value.TryGetInt64(out var l) ? l : p.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => "",
                    _ => p.Value.GetRawText()
                };
            }
        }
        catch
        {
            // invalid json => ignore headersJson (do not break publishing)
        }
    }

    private static void InjectTracing(Dictionary<string, object> headers)
    {
        var a = Activity.Current;
        if (a is null) return;

        // W3C Trace Context: traceparent + tracestate
        // If already provided, don't overwrite.
        if (!headers.ContainsKey("traceparent") && !string.IsNullOrWhiteSpace(a.Id))
            headers["traceparent"] = a.Id!;

        if (!headers.ContainsKey("tracestate") && !string.IsNullOrWhiteSpace(a.TraceStateString))
            headers["tracestate"] = a.TraceStateString!;
    }
}
