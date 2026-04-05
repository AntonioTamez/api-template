using Asp.Versioning;
using Company.Template.Api.Contracts.Customers;
using Company.Template.Application.Customers.GetCustomerById;
using Company.Template.Application.Customers;
using Company.Template.Application.Customers.RegisterCustomer;
using Company.Template.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Company.Template.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(request.FirstName, request.LastName, request.Email);
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        return CreatedAtAction(nameof(GetCustomerById), new { customerId = result.Value.Id }, result.Value);
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(customerId);
        var result = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        return Ok(result.Value);
    }

    private IActionResult ToActionResult(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
