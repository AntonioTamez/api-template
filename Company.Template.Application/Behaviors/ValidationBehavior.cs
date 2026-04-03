using Company.Template.Domain.Shared;
using FluentValidation;
using MediatR;

namespace Company.Template.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var error = new Error("Validation", string.Join("; ", failures.Select(f => f.ErrorMessage)));

        object? response = typeof(TResponse) == typeof(Result)
            ? Result.Failure(error)
            : typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>)
                ? typeof(Result)
                    .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                    .MakeGenericMethod(typeof(TResponse).GenericTypeArguments[0])
                    .Invoke(null, new object[] { error })
                : null;

        if (response is null)
        {
            throw new InvalidOperationException("ValidationBehavior only supports Result/Result<T> responses.");
        }

        return (TResponse)response;
    }
}
