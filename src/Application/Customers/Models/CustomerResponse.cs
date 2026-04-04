namespace Company.Template.Application.Customers.Models;

public sealed record CustomerResponse(Guid Id, string FirstName, string LastName, string Email);
