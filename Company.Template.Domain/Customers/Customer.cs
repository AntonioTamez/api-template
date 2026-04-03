using Company.Template.Domain.Abstractions;
using Company.Template.Domain.Customers.Events;
using Company.Template.Domain.Errors;
using Company.Template.Domain.Primitives;
using Company.Template.Domain.Shared;
using Company.Template.Domain.ValueObjects;

namespace Company.Template.Domain.Customers;

public sealed class Customer : Entity<CustomerId>, IAggregateRoot
{
    private Customer()
        : base(new CustomerId(Guid.Empty))
    {
    }

    private Customer(CustomerId id, string firstName, string lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public Email Email { get; private set; } = null!;

    public static Result<Customer> Create(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<Customer>(DomainErrors.General.ValueIsRequired(nameof(FirstName)));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<Customer>(DomainErrors.General.ValueIsRequired(nameof(LastName)));
        }

        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<Customer>(emailResult.Error);
        }

        var customer = new Customer(CustomerId.New(), firstName.Trim(), lastName.Trim(), emailResult.Value);

        customer.RaiseDomainEvent(new CustomerRegisteredDomainEvent(Guid.NewGuid(), customer.Id, customer.Email.Value));

        return Result.Success(customer);
    }

    public Result UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(DomainErrors.General.ValueIsRequired("Name"));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();

        return Result.Success();
    }

    public Result UpdateEmail(string email)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        Email = emailResult.Value;

        return Result.Success();
    }
}
