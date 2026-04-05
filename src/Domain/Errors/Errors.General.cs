using Company.Template.Domain.Shared;

namespace Company.Template.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static Error ValueIsRequired(string name) => new($"General.{name}.Required", $"{name} is required.", ErrorType.Validation);

        public static Error InvalidFormat(string name) => new($"General.{name}.InvalidFormat", $"{name} is not in a valid format.", ErrorType.Validation);
    }

    public static class Customer
    {
        public static Error NotFound(Guid id) => new("Customer.NotFound", $"Customer with id '{id}' was not found.", ErrorType.NotFound);

        public static Error EmailAlreadyExists(string email) => new("Customer.EmailAlreadyExists", $"The email '{email}' is already registered.", ErrorType.Conflict);
    }
}
