using System.Net;
using Microsoft.AspNetCore.Mvc;
using Models.Exceptions;

namespace Microservices.Observability.ServiceDefaults;

public static partial class ProblemDetailsExceptionEnricher
{
    public static void EnrichWithExceptionDetails(this ProblemDetails problem, Exception exception) => problem.Extensions = new Dictionary<string, object?>
    {
        ["exception"] = new
        {
            exception.Message,
            exception.StackTrace,
            exception.Data
        }
    };

    public static ProblemDetails ToProblemDetails(this Exception exception) => exception switch
    {
        WrongInputException => new ProblemDetails()
        {
            Type = "WrongInput",
            Status = (int)HttpStatusCode.BadRequest
        },
        _ => new ProblemDetails()
        {
            Type = "Unexpected",
            Status = (int)HttpStatusCode.InternalServerError
        }
    };
}
