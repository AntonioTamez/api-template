namespace Company.Template.Domain.Customers;

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());

    public static implicit operator Guid(CustomerId id) => id.Value;
}
