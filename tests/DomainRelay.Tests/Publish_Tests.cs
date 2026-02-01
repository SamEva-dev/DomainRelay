using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class Publish_Tests
{
    private sealed record SomethingHappened(int X) : INotification;

    private sealed class H1 : INotificationHandler<SomethingHappened>
    {
        public static int Seen;
        public Task Handle(SomethingHappened notification, CancellationToken ct) { Seen += notification.X; return Task.CompletedTask; }
    }

    private sealed class H2 : INotificationHandler<SomethingHappened>
    {
        public static int Seen;
        public Task Handle(SomethingHappened notification, CancellationToken ct) { Seen += notification.X * 10; return Task.CompletedTask; }
    }

    [Fact]
    public async Task Publish_calls_all_handlers()
    {
        H1.Seen = 0; H2.Seen = 0;

        var sc = new ServiceCollection();
        sc.AddDomainRelay(reg => { }, r => r.Assemblies.Add(typeof(Publish_Tests).Assembly));
        var sp = sc.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Publish(new SomethingHappened(2));

        Assert.Equal(2, H1.Seen);
        Assert.Equal(20, H2.Seen);
    }
}
