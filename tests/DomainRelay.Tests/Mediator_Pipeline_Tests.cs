using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class Mediator_Pipeline_Tests
{
    private sealed record Ping(int N) : IRequest<int>;

    private sealed class PingHandler : IRequestHandler<Ping, int>
    {
        public Task<int> Handle(Ping request, CancellationToken ct) => Task.FromResult(request.N);
    }

    private sealed class AddOneBehavior : IPipelineBehavior<Ping, int>
    {
        public async Task<int> Handle(Ping request, CancellationToken ct, HandlerDelegate<int> next)
            => (await next()) + 1;
    }

    private sealed class TimesTwoBehavior : IPipelineBehavior<Ping, int>
    {
        public async Task<int> Handle(Ping request, CancellationToken ct, HandlerDelegate<int> next)
            => (await next()) * 2;
    }

    [Fact]
    public async Task Behaviors_wrap_in_order()
    {
        var sc = new ServiceCollection();
        sc.AddTransient<IPipelineBehavior<Ping, int>, AddOneBehavior>();
        sc.AddTransient<IPipelineBehavior<Ping, int>, TimesTwoBehavior>();

        sc.AddDomainRelay(reg => { }, r => r.Assemblies.Add(typeof(Mediator_Pipeline_Tests).Assembly));
        var sp = sc.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();

        // Order: reverse enumeration wrapping -> last registered wraps first.
        // With our composition: behaviors.Reverse() means behavior order is the same as DI enumeration order.
        // Here DI order usually is registration order: AddOne then TimesTwo.
        // Execution: AddOne( TimesTwo( handler ) ) => (N*2)+1
        var res = await mediator.Send(new Ping(3));
        Assert.Equal(7, res);
    }
}
