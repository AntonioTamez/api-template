using Company.Template.Api.Contracts.Customers;
using Company.Template.Application.Customers.GetCustomerById;
using Company.Template.Application.Customers.RegisterCustomer;
using Company.Template.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApplicationCustomerResponse = Company.Template.Application.Customers.Models.CustomerResponse;

namespace Company.Template.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
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
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(request.FirstName, request.LastName, request.Email);
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        var query = new GetCustomerByIdQuery(result.Value);
        var queryResult = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (queryResult.IsFailure)
        {
            return ToActionResult(queryResult.Error);
        }

        var response = Map(queryResult.Value);

        return CreatedAtAction(nameof(GetCustomerById), new { customerId = response.Id }, response);
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

        return Ok(Map(result.Value));
    }

    private IActionResult ToActionResult(Error error)
    {
        var statusCode = error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }

    private static CustomerResponse Map(ApplicationCustomerResponse response)
        => new(response.Id, response.FirstName, response.LastName, response.Email);
}
