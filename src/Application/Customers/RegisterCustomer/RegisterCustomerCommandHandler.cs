using Company.Template.Application.Abstractions.Data;
using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Domain.Customers;
using Company.Template.Domain.Errors;
using Company.Template.Domain.Shared;

namespace Company.Template.Application.Customers.RegisterCustomer;

internal sealed class RegisterCustomerCommandHandler : ICommandHandler<RegisterCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _customerRepository.ExistsByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<Guid>(DomainErrors.Customer.EmailAlreadyExists(request.Email));
        }

        var customerResult = Customer.Create(request.FirstName, request.LastName, request.Email);

        if (customerResult.IsFailure)
        {
            return Result.Failure<Guid>(customerResult.Error);
        }

        await _customerRepository.AddAsync(customerResult.Value, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(customerResult.Value.Id.Value);
    }
}
