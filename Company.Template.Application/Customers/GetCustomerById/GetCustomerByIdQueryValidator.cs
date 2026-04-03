using FluentValidation;

namespace Company.Template.Application.Customers.GetCustomerById;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
