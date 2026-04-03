namespace Company.Template.Domain.Shared;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new("None", string.Empty);
}
