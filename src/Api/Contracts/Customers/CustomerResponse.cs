namespace Company.Template.Api.Contracts.Customers;

public sealed record CustomerResponse(Guid Id, string FirstName, string LastName, string Email);
