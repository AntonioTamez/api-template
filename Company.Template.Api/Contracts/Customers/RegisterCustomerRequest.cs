namespace Company.Template.Api.Contracts.Customers;

public sealed record RegisterCustomerRequest(string FirstName, string LastName, string Email);
