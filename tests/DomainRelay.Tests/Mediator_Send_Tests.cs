using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class Mediator_Send_Tests
{
    private sealed record Ping(string Value) : IRequest<string>;
    private sealed class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken ct) => Task.FromResult("pong:" + request.Value);
    }

    private sealed record VoidPing(string Value) : IRequest;
    private sealed class VoidPingHandler : IRequestHandler<VoidPing>
    {
        public static string? LastValue;

        public Task Handle(VoidPing request, CancellationToken ct)
        {
            LastValue = request.Value;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Send_returns_response()
    {
        var sc = new ServiceCollection();
        sc.AddDomainRelay(reg => { }, r => r.Assemblies.Add(typeof(Mediator_Send_Tests).Assembly));
        var sp = sc.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var res = await mediator.Send(new Ping("a"));
        Assert.Equal("pong:a", res);
    }

    [Fact]
    public async Task Send_void_request_does_not_require_response_option()
    {
        VoidPingHandler.LastValue = null;

        var sc = new ServiceCollection();
        sc.AddDomainRelay(reg => { }, r => r.Assemblies.Add(typeof(Mediator_Send_Tests).Assembly));
        var sp = sc.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Send(new VoidPing("x"));

        Assert.Equal("x", VoidPingHandler.LastValue);
    }
}
