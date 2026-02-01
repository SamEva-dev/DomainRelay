using DomainRelay.Abstractions;
using FluentValidation;

namespace DomainRelay.Validation.Behaviors;

public sealed class FluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            var ctx = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct))).ConfigureAwait(false);

            var failures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToList();
            if (failures.Count > 0)
                throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
