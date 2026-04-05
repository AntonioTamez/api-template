namespace Company.Template.Application.Customers;

public sealed record CustomerResponse(Guid Id, string FirstName, string LastName, string Email);
