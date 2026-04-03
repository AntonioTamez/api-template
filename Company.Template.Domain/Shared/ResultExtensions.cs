namespace Company.Template.Domain.Shared;

public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Error);
        }

        return Result.Success(mapper(result.Value));
    }

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Error);
        }

        return binder(result.Value);
    }
}
