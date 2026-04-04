using Company.Template.Application.Abstractions.Messaging;

namespace Company.Template.Application.Customers.RegisterCustomer;

public sealed record RegisterCustomerCommand(string FirstName, string LastName, string Email) : ICommand<Guid>;
