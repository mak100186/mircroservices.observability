using FluentResults;

namespace Extensions;

public static class ResultExtensions
{
    public static string GetErrors(this IResultBase result) => string.Join(", ", result.Errors.Select(error => error.Message));
}
