
### `docs/migration-from-mediatr.md`
```md
# Migration from MediatR

- Replace MediatR interfaces:
  - IRequest<T> -> DomainRelay.Abstractions.IRequest<T>
  - IRequestHandler<TReq,TRes> -> same name in Abstractions
  - INotification / INotificationHandler -> same

- Replace registration:
  - services.AddMediatR(...) -> services.AddDomainRelay(...)

- Behaviors:
  - IPipelineBehavior<TReq,TRes> exists with the same shape.

Notes:
- DomainRelay core has no built-in multi-DbContext transaction selector.
- Publish strategy can be sequential or parallel via DomainRelayOptions.
