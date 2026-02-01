using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class DiScanning_Tests
{
    private sealed record Q() : IRequest<string>;
    private sealed class QHandler : IRequestHandler<Q, string>
    {
        public Task<string> Handle(Q request, CancellationToken ct) => Task.FromResult("ok");
    }

    [Fact]
    public void Scanning_registers_handlers()
    {
        var sc = new ServiceCollection();
        sc.AddDomainRelay(reg => { }, r => r.Assemblies.Add(typeof(DiScanning_Tests).Assembly));
        var sp = sc.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        Assert.NotNull(mediator);
    }
}
