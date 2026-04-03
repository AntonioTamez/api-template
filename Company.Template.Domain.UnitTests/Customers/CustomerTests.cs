using Company.Template.Domain.Customers;
using Xunit;

namespace Company.Template.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        const string firstName = "Ada";
        const string lastName = "Lovelace";
        const string email = "ada@example.com";

        // Act
        var result = Customer.Create(firstName, lastName, email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(firstName, result.Value.FirstName);
        Assert.Equal(lastName, result.Value.LastName);
        Assert.Equal(email, result.Value.Email.Value);
    }
}
