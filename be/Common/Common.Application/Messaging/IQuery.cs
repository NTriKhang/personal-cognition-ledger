using Common.Domain;
using MediatR;

namespace Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
