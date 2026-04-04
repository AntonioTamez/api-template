using Company.Template.Application.Abstractions.Data;
using Company.Template.Application.Customers.RegisterCustomer;
using Company.Template.Domain.Customers;
using Moq;
using Xunit;

namespace Company.Template.Application.UnitTests.Customers;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCustomerCommand("Ada", "Lovelace", "ada@example.com");
        _customerRepository.Setup(repo => repo.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterCustomerCommandHandler(_customerRepository.Object, _unitOfWork.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _customerRepository.Verify(repo => repo.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
