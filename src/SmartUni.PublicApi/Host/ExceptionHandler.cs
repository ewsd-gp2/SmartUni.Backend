using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;

namespace SmartUni.PublicApi.Host;

public static class ExceptionHandler
{
    // ref: https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/blob/main/src/VerticalSliceArchitectureTemplate/Host/FeatureDiscovery.cs
    public static void UseProductionExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context =>
            {
                var defaultExceptionResponse = Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.1");

                await defaultExceptionResponse.ExecuteAsync(context);
            }));
    }

    public sealed class KnownExceptionsHandler : IExceptionHandler
    {
        private static readonly IDictionary<Type, Func<HttpContext, Exception, IResult>> ExceptionHandlers =
            new Dictionary<Type, Func<HttpContext, Exception, IResult>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(InvalidOperationException), HandleInvalidOperationException },
                { typeof(NotFoundException), HandleNotFoundException }
            };

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var type = exception.GetType();

            if (!ExceptionHandlers.TryGetValue(type, out var handler)) return false;

            var result = handler.Invoke(httpContext, exception);
            await result.ExecuteAsync(httpContext);

            return true;
        }

        private static IResult HandleValidationException(HttpContext context, Exception exception)
        {
            var validationException = exception as ValidationException ??
                                      throw new InvalidOperationException(
                                          "Exception is not of type ValidationException");

            return Results.Problem(validationException.ValidationResult.ErrorMessage,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                statusCode: StatusCodes.Status400BadRequest);
        }

        private static IResult HandleInvalidOperationException(HttpContext context, Exception exception)
        {
            var invalidOperationException = exception as InvalidOperationException ??
                                            throw new InvalidOperationException(
                                                "Exception is not of type InvalidOperationException");

            return Results.Problem(invalidOperationException.Message,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                statusCode: StatusCodes.Status400BadRequest);
        }

        private static IResult HandleNotFoundException(HttpContext context, Exception exception)
        {
            var notFoundException = exception as NotFoundException ??
                                    throw new InvalidOperationException(
                                        "Exception is not of type NotFoundException");

            return Results.Problem(notFoundException.Message,
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }
    }
}