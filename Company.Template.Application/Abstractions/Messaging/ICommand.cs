using Company.Template.Domain.Shared;
using MediatR;

namespace Company.Template.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommand : IRequest<Result>;
