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
}
