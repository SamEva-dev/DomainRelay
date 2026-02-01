# DomainRelay — Manuel complet d’utilisation (net8/net9)

DomainRelay est une implémentation “Mediator” pensée pour la **Clean Architecture** :
- **Send** : Commands/Queries (Use Cases)
- **Publish** : Notifications / Domain Events
- **Pipeline behaviors** : validation, transactions, diagnostics, etc.
- **EF Core Outbox premium** : persistance fiable des events + dispatcher résilient
- **Transports plug-in** : RabbitMQ en référence, mais architecture compatible Kafka/HTTP/ASB, etc.

---

## 1) Packages et responsabilités

### Noyau
- **DomainRelay.Abstractions**
  - Interfaces : `IMediator`, `IRequest<T>`, `IRequestHandler<,>`, `INotification`, `INotificationHandler<>`, `IPipelineBehavior<,>`
  - Type `Unit`

- **DomainRelay**
  - Implémentation de `IMediator` (`Mediator`)
  - Options runtime : `DomainRelayOptions` (publish strategy, wrapping exceptions)

- **DomainRelay.DependencyInjection**
  - `AddDomainRelay(...)` : enregistrement DI + scanning des handlers/behaviors

### Optionnels (recommandés en prod)
- **DomainRelay.Diagnostics**
  - Pipeline behavior `DiagnosticsBehavior<,>` basé sur `ActivitySource` (tracing)

- **DomainRelay.Validation**
  - Pipeline behavior `FluentValidationBehavior<,>` (FluentValidation)

- **DomainRelay.EFCore**
  - `TransactionBehavior<,>` (transactions EF Core)
  - **Outbox premium** : interceptor SaveChanges + dispatcher + admin API

### Transport (plugin)
- **DomainRelay.Transport.RabbitMQ**
  - Implémentation `IOutboxPublisher` via RabbitMQ
  - Routing topic par `TypeKey` (ou router custom)

---

## 2) Concepts clés

### 2.1 Requests (Commands/Queries)
- Un **Command** ou une **Query** est un type qui implémente `IRequest<TResponse>`
- Un **Handler** implémente `IRequestHandler<TRequest, TResponse>`
- `Send` exécute **un seul** handler par request (obligatoire)

### 2.2 Notifications (Domain Events)
- Un event/notif implémente `INotification`
- Un handler implémente `INotificationHandler<TNotification>`
- `Publish` appelle **tous** les handlers enregistrés (0..N)

### 2.3 Pipeline behaviors
Un behavior est un “middleware” autour du handler :
- validation
- transaction
- logs/metrics
- retry (si tu le souhaites)
- auditing
- multi-tenancy / org context

Signature :
```csharp
public interface IPipelineBehavior<TRequest,TResponse>
{
  Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next);
}

2.4 Publish strategy

Le publish des notifications peut être :

Sequential (par défaut) : déterministe, simple, évite la contention

Parallel : accélère si plusieurs handlers lents, nécessite une gestion d’exception claire (Aggregate, fail-fast)

3) Quickstart (sans EF Core)
3.1 Enregistrer DomainRelay
using DomainRelay.DependencyInjection;
using DomainRelay.Publish;

services.AddDomainRelay(
  configureOptions: opt =>
  {
    // opt.PublishStrategy = new ParallelPublishStrategy(); // optionnel
    // opt.WrapExceptions = true; // défaut
  },
  configureRegistration: reg =>
  {
    reg.Assemblies.Add(typeof(Program).Assembly);
    reg.Assemblies.Add(typeof(SomeHandler).Assembly);
  });

3.2 Créer une Command + Handler
using DomainRelay.Abstractions;

public sealed record CreateUser(string Email) : IRequest<Unit>;

public sealed class CreateUserHandler : IRequestHandler<CreateUser, Unit>
{
  public async Task<Unit> Handle(CreateUser request, CancellationToken ct)
  {
    // logique métier
    return Unit.Value;
  }
}

3.3 Exécuter la request
using DomainRelay.Abstractions;

public sealed class UsersController
{
  private readonly IMediator _mediator;
  public UsersController(IMediator mediator) => _mediator = mediator;

  public Task Create(string email, CancellationToken ct)
    => _mediator.Send(new CreateUser(email), ct);
}

4) Notifications / Domain Events (sans Outbox)
4.1 Définir un event
using DomainRelay.Abstractions;

public sealed record UserCreated(Guid UserId) : INotification;

4.2 Définir des handlers (0..N)
public sealed class SendWelcomeEmail : INotificationHandler<UserCreated>
{
  public Task Handle(UserCreated notification, CancellationToken ct)
  {
    // envoyer email
    return Task.CompletedTask;
  }
}

public sealed class IndexUserForSearch : INotificationHandler<UserCreated>
{
  public Task Handle(UserCreated notification, CancellationToken ct)
  {
    // indexation
    return Task.CompletedTask;
  }
}

4.3 Publier
await _mediator.Publish(new UserCreated(userId), ct);

5) Pipeline behaviors : usage et ordre
5.1 Ajouter un behavior custom
public sealed class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  public async Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
  {
    var start = DateTime.UtcNow;
    var res = await next();
    var ms = (DateTime.UtcNow - start).TotalMilliseconds;
    // log ms
    return res;
  }
}

5.2 Enregistrer le behavior
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

5.3 Ordre d’exécution

DomainRelay compose les behaviors autour du handler.

Recommandation pratique

Validation

Authorization (si tu en fais un)

Transaction

Diagnostics/Logging

Handler

Si tu veux un ordre strict “à la MediatR” (priority), on peut ajouter un IOrderedBehavior plus tard.

6) Diagnostics (Activity/Tracing)
6.1 Enregistrer
using DomainRelay.Diagnostics;

services.AddDomainRelayDiagnostics();

6.2 Ce que tu obtiens

Une Activity par Send

Tags : request type, response type, success/failure, exception info (minimale)

Compatible OpenTelemetry si tu ajoutes un exporter côté application

7) Validation (FluentValidation)
7.1 Enregistrer
using DomainRelay.Validation;

services.AddDomainRelayValidation();

7.2 Définir un validator
using FluentValidation;

public sealed class CreateUserValidator : AbstractValidator<CreateUser>
{
  public CreateUserValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
  }
}

7.3 Enregistrer les validators

Tu peux les enregistrer via scanning, ou manuellement :

services.AddTransient<IValidator<CreateUser>, CreateUserValidator>();

8) EF Core Transaction (single ou multi DbContext)
8.1 Single DbContext
using DomainRelay.EFCore;

services.AddDomainRelayEfCoreTransactionResolver<MyDbContext>();

8.2 Multi DbContext (mapping par règles)
services.AddDomainRelayEfCoreTransactionResolver(resolver =>
{
  resolver
    .Map<AuthDbContext>(t => t.FullName!.Contains(".Auth."))
    .Map<AppDbContext>(t => t.FullName!.Contains(".Application."));
});


Bonnes pratiques

Transaction behavior : privilégie les Commands (write-side)

Queries : souvent sans transaction (sauf besoin strict)

9) Outbox EF Core Premium (recommandé en prod)
9.1 Pourquoi l’Outbox

Problème classique : tu fais SaveChanges() + tu publies un event (Rabbit/Kafka/HTTP).
Si l’app crash après SaveChanges mais avant le publish, ton event est perdu.

Solution Outbox

Tu écris tes domain events dans une table Outbox dans la même transaction que SaveChanges.

Un dispatcher background envoie ensuite vers le transport de façon résiliente.

9.2 Contrats Domain Events

Dans DomainRelay.EFCore :

IDomainEvent : EventId, OccurredOnUtc

IHasDomainEvents : DomainEvents, ClearDomainEvents()

Exemple :

using DomainRelay.EFCore.DomainEvents;

public sealed record UserCreated(Guid UserId) : IDomainEvent
{
  public Guid EventId { get; init; } = Guid.NewGuid();
  public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public sealed class User : IHasDomainEvents
{
  private readonly List<IDomainEvent> _events = new();
  public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;
  public void ClearDomainEvents() => _events.Clear();

  public void RaiseCreated(Guid userId) => _events.Add(new UserCreated(userId));
}

9.3 Ajouter la table Outbox au modèle EF

Dans ton DbContext :

using DomainRelay.EFCore.Outbox;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
  base.OnModelCreating(modelBuilder);
  modelBuilder.AddDomainRelayOutbox(tableName: "outbox_messages", schema: "ops");
}

9.4 Enregistrer l’Outbox (version “zero foot-gun”)

Cette méthode force la config DbContext + Factory + interceptor.

using DomainRelay.EFCore;

services.AddDomainRelayEfCoreOutbox<MyDbContext>(b =>
{
  b.WithDbContextOptions((sp, o) =>
  {
    o.UseNpgsql(builder.Configuration.GetConnectionString("db"));
  });

  b.WithOutboxOptions(o =>
  {
    o.Schema = "ops";
    o.TableName = "outbox_messages";
    o.InstanceId = $"api-{Environment.MachineName}";
    o.BatchSize = 200;
    o.PollingInterval = TimeSpan.FromSeconds(1);
    o.LeaseDuration = TimeSpan.FromSeconds(30);
    o.MaxAttempts = 12;
    o.ProcessedRetention = TimeSpan.FromDays(14);
  });

  // Allowlist obligatoire : sécurité désérialisation
  b.WithTypeRegistry(reg =>
  {
    reg.Register<UserCreated>("iam.user.created.v1");
    // register tous tes events
  });
});

9.5 Mécanisme interne (ce que DomainRelay fait)

OutboxSaveChangesInterceptor : au moment de SavingChanges, il :

récupère les DomainEvents des entités trackées

écrit des lignes OutboxMessage

appelle ClearDomainEvents() (évite double capture)

OutboxDispatcherHostedService :

poll DB

claim/lease (TTL) pour éviter double traitement multi-instance

publish via IOutboxPublisher

retries (backoff + jitter), dead-letter après N tentatives

cleanup messages processed

10) Admin Outbox (Stats, DeadLetters, Requeue)

DomainRelay.EFCore enregistre IOutboxAdmin.

10.1 Exemple : endpoint admin
using DomainRelay.EFCore.Outbox.Admin;

public sealed class OutboxAdminController
{
  private readonly IOutboxAdmin _admin;
  public OutboxAdminController(IOutboxAdmin admin) => _admin = admin;

  public Task<OutboxStats> Stats(CancellationToken ct) => _admin.GetStatsAsync(ct);

  public Task<int> Requeue(Guid id, CancellationToken ct)
    => _admin.RequeueAsync(new[] { id }, resetAttempts: false, ct);

  public Task<IReadOnlyList<OutboxMessage>> DeadLetters(CancellationToken ct)
    => _admin.GetDeadLettersAsync(50, ct);
}

10.2 Opérations recommandées

Sur dead-letter :

Inspect LastError, TypeKey, AttemptCount

Corrige la cause (transport down, message invalide, auth, etc.)

Requeue ciblé (pas “requeue all” sans analyse)

11) Transport RabbitMQ (référence) — mais plug-in pour tout
11.1 Principe

DomainRelay.EFCore appelle seulement :

public interface IOutboxPublisher
{
  Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct);
}


RabbitMQ fournit RabbitMqOutboxPublisher : IOutboxPublisher.
Tu peux remplacer par Kafka/HTTP/etc sans toucher à DomainRelay.EFCore.

11.2 Enregistrer RabbitMQ
using DomainRelay.Transport.RabbitMQ;

services.AddDomainRelayRabbitMqPublisher(o =>
{
  o.HostName = "rabbitmq";
  o.UserName = "guest";
  o.Password = "guest";
  o.VirtualHost = "/";
  o.ExchangeName = "locaguest.events";
  o.ExchangeType = "topic";
  o.DeclareExchange = true;
  o.PublisherConfirms = true;
});

11.3 Routing

Le router par défaut : TypeKeyTopicRouter

routingKey = TypeKey normalisé

exchange = options.ExchangeName

Pour customiser :

implémente IOutboxRouter (du package RabbitMQ)

enregistre-le avant le publisher

11.4 Headers & tracing

Si Activity.Current existe :

traceparent / tracestate sont injectés (W3C)

12) Migration depuis MediatR
12.1 Remplacements

IRequest<T> → DomainRelay.Abstractions.IRequest<T>

IRequestHandler<,> → idem

INotification / INotificationHandler<> → idem

IPipelineBehavior<,> → idem

services.AddMediatR(...) → services.AddDomainRelay(...)

12.2 Attention

Publish strategy : sequential vs parallel (à décider)

Outbox : c’est un ajout (fortement recommandé pour prod)

13) CI/CD et publication NuGet
13.1 CI

.github/workflows/ci.yml : build/test sur PR + main

13.2 Publish

.github/workflows/publish.yml : publish sur tag vX.Y.Z

Secret GitHub requis : NUGET_API_KEY

13.3 Versioning

Sur tag : v1.0.0 => 1.0.0

Sur main : alpha.<run_number> (si tu gardes cette stratégie)

14) Architecture recommandée (Clean / CQRS / DDD)
14.1 Pattern conseillé

Commands : write-side, transaction, validation, outbox events

Queries : read-side, pas forcément transaction

Domain events : événements du domaine (créé, modifié, supprimé, statut changé)

Integration events : ce qui sort vers les autres services (via Outbox + transport)

14.2 Règle d’or

Ne publie pas directement au broker depuis un handler métier si tu veux de la fiabilité.

Handler → change state + raise domain events → SaveChanges → Outbox

Dispatcher outbox → broker

15) Checklist troubleshooting (du plus simple au plus critique)
15.1 Send ne trouve pas de handler

Le handler est-il dans une assembly scannée par AddDomainRelay ?

Le handler implémente-t-il exactement IRequestHandler<TRequest,TResponse> ?

Le request implémente-t-il exactement IRequest<TResponse> ?

15.2 Publish n’appelle aucun handler

Les handlers notif sont-ils enregistrés/scannés ?

Ton type notif est-il bien INotification ?

Est-ce que tu publies le bon type (pas une interface/record différent) ?

15.3 Validation ne se déclenche pas

AddDomainRelayValidation() est-il appelé ?

Le validator est-il enregistré pour le bon type IValidator<TRequest> ?

15.4 Transactions EFCore ne s’appliquent pas

AddDomainRelayEfCoreTransactionResolver<DbContext>() est-il appelé ?

Est-ce le bon DbContext injecté ?

Les queries devraient-elles réellement être transactionnelles ?

15.5 Outbox : pas de lignes créées

Tes entités implémentent-elles IHasDomainEvents ?

Le code appelle-t-il RaiseXXX() avant SaveChanges ?

Le OutboxSaveChangesInterceptor est-il bien injecté (via AddDomainRelayEfCoreOutbox) ?

Le mapping Outbox (AddDomainRelayOutbox) est-il dans OnModelCreating ?

15.6 Outbox : lignes créées mais jamais “Processed”

IOutboxPublisher est-il enregistré ?

Le HostedService tourne-t-il (logs) ?

Problème de connexion broker : regarder LastError, AttemptCount

Verrouillage/lease : vérifier LockedBy, LockedUntilUtc, TTL, multi-instance

15.7 Dead-letter augmente

Le transport est down / credentials invalides

Le message est invalide (payload ou routing)

TypeKey non enregistré dans registry (allowlist)

Fix, puis requeue ciblé via IOutboxAdmin

16) FAQ
“Pourquoi une allowlist type registry ?”

Sécurité : éviter de désérialiser des types arbitraires depuis la DB.

“Parallel publish ou sequential ?”

Sequential : recommandé par défaut

Parallel : utile si handlers indépendants et potentiellement lents, et si tu acceptes AggregateException

“Puis-je remplacer RabbitMQ par Kafka plus tard ?”

Oui : tu remplaces simplement IOutboxPublisher (et éventuellement un router).

17) Exemples “prêts prod” (patterns)
17.1 Command handler (write-side) + domain event
public sealed record ActivateUser(Guid UserId) : IRequest<Unit>;

public sealed class ActivateUserHandler : IRequestHandler<ActivateUser, Unit>
{
  private readonly MyDbContext _db;

  public ActivateUserHandler(MyDbContext db) => _db = db;

  public async Task<Unit> Handle(ActivateUser request, CancellationToken ct)
  {
    var user = await _db.Users.FindAsync([request.UserId], ct);
    if (user is null) throw new InvalidOperationException("User not found");

    user.Activate();         // change state
    user.RaiseActivated();   // add domain event

    await _db.SaveChangesAsync(ct); // interceptor writes outbox in same transaction
    return Unit.Value;
  }
}

17.2 Outbox transport event type keys

Bon pattern : domain.entity.action.vN

iam.user.created.v1

lease.contract.signed.v1

billing.invoice.issued.v1

18) Annexes (docs conseillés dans le repo)

docs/getting-started.md

docs/pipelines.md

docs/notifications.md

docs/diagnostics.md

docs/efcore.md

docs/validation.md

docs/efcore-outbox.md

docs/outbox-ops.md

docs/rabbitmq.md

docs/migration-from-mediatr.md