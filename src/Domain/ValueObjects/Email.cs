using Company.Template.Domain.Errors;
using Company.Template.Domain.Shared;

namespace Company.Template.Domain.ValueObjects;

public sealed class Email : Primitives.ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(DomainErrors.General.ValueIsRequired("Email"));
        }

        if (!value.Contains('@', StringComparison.Ordinal))
        {
            return Result.Failure<Email>(DomainErrors.General.InvalidFormat("Email"));
        }

        return Result.Success(new Email(value.Trim().ToLowerInvariant()));
    }

    public override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
