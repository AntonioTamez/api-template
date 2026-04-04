using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Application.Customers.Models;
using Company.Template.Domain.Customers;
using Company.Template.Domain.Shared;

namespace Company.Template.Application.Customers.GetCustomerById;

internal sealed class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken).ConfigureAwait(false);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(new Error("Customer.NotFound", "Customer not found."));
        }

        var response = new CustomerResponse(customer.Id.Value, customer.FirstName, customer.LastName, customer.Email.Value);

        return Result.Success(response);
    }
}
