using Company.Template.Domain.Shared;
using MediatR;

namespace Company.Template.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
