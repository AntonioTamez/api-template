using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Application.Customers;

namespace Company.Template.Application.Customers.RegisterCustomer;

public sealed record RegisterCustomerCommand(string FirstName, string LastName, string Email) : ICommand<CustomerResponse>;
