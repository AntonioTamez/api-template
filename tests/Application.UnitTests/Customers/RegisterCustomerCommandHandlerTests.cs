using Company.Template.Application.Abstractions.Data;
using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Application.Customers.RegisterCustomer;
using Company.Template.Domain.Customers;
using Company.Template.Domain.Errors;
using Moq;
using Xunit;

namespace Company.Template.Application.UnitTests.Customers;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDomainEventDispatcher> _domainEventDispatcher = new();

    private RegisterCustomerCommandHandler CreateHandler() =>
        new(_customerRepository.Object, _unitOfWork.Object, _domainEventDispatcher.Object);

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCustomerCommand("Ada", "Lovelace", "ada@example.com");
        _customerRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Customer.EmailAlreadyExists(command.Email), result.Error);
        _customerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _domainEventDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IReadOnlyCollection<Domain.Abstractions.IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCustomerIsRegistered()
    {
        // Arrange
        var command = new RegisterCustomerCommand("Ada", "Lovelace", "ada@example.com");
        _customerRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(command.FirstName, result.Value.FirstName);
        Assert.Equal(command.LastName, result.Value.LastName);
        Assert.Equal(command.Email, result.Value.Email);
        _customerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _domainEventDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IReadOnlyCollection<Domain.Abstractions.IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
