using Company.Template.Application.Abstractions.Data;
using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Application.Customers.Models;
using Company.Template.Domain.Customers;
using Company.Template.Domain.Errors;
using Company.Template.Domain.Shared;

namespace Company.Template.Application.Customers.RegisterCustomer;

internal sealed class RegisterCustomerCommandHandler : ICommandHandler<RegisterCustomerCommand, CustomerResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public RegisterCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<Result<CustomerResponse>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _customerRepository.ExistsByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<CustomerResponse>(DomainErrors.Customer.EmailAlreadyExists(request.Email));
        }

        var customerResult = Customer.Create(request.FirstName, request.LastName, request.Email);

        if (customerResult.IsFailure)
        {
            return Result.Failure<CustomerResponse>(customerResult.Error);
        }

        var customer = customerResult.Value;

        await _customerRepository.AddAsync(customer, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _domainEventDispatcher.DispatchAsync(customer.DomainEvents, cancellationToken).ConfigureAwait(false);
        customer.ClearDomainEvents();

        var response = new CustomerResponse(customer.Id.Value, customer.FirstName, customer.LastName, customer.Email.Value);

        return Result.Success(response);
    }
}
