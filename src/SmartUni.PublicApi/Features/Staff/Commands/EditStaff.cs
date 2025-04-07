using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Staff.Commands
{
    public class EditStaff
    {
        private sealed record Request(string Name, string Email, string PhoneNumber, Enums.GenderType Gender);

        public sealed class Endpoint : IEndpoint
        {
            
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/staff/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, ClaimsPrincipal claims,
                                CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, claims, cancellationToken))
                    .Produces(200)
                    .WithDescription("Update an existing staff")
                    .Accepts<Request>("application/json")
                    .Produces<BadRequest<ValidationResult>>(StatusCodes.Status400BadRequest)
                    .Produces<NotFound>(StatusCodes.Status404NotFound)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Staff));
            }

            private static async Task<Results<Ok, IResult>> HandleAsync(
                Guid id,
                Request request,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit staff with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Staff? staff = await dbContext.Staff.Where(x => !x.IsDeleted).Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (staff is null)
                {
                    logger.LogWarning("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                staff.UpdateStaffName(request.Name);
                staff.Identity.Email = request.Email;
                staff.Identity.PhoneNumber = request.PhoneNumber;
                staff.UpdateStaffUpdatedOn(DateTime.UtcNow);
                staff.UpdateStaffGender(request.Gender);
                staff.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully edited staff with ID: {Id}", id);

                return TypedResults.Ok();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x => x.PhoneNumber).NotEmpty();
                RuleFor(x => x.Gender).IsInEnum();

            }
        }
    }
}
