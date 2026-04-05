using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Application.Customers;

namespace Company.Template.Application.Customers.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerResponse>;
