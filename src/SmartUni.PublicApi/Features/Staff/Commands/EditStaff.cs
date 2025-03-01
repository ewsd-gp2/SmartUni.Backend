using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Staff.Commands
{
    public class EditStaff
    {
        private sealed record Request(string Name, string Email, string PhoneNumber,bool IsDeleted,string Gender,Guid UpdatedBy);

        public sealed class Endpoint : IEndpoint
        {
            
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/editStaff/{id:guid}",
                        ([FromRoute] Guid id, [FromBody] Request request, [FromServices] ILogger<Endpoint> logger,
                                [FromServices] SmartUniDbContext dbContext, CancellationToken cancellationToken) =>
                            HandleAsync(id, request, logger, dbContext, cancellationToken))
                    .Produces<Ok>()
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
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to edit staff with ID: {Id} and request: {Request}", id, request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult);
                }

                Staff? staff = await dbContext.Staff.FindAsync([id], cancellationToken);

                if (staff is null)
                {
                    logger.LogWarning("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                staff.UpdateStaffName(request.Name);
                staff.UpdateStaffEmail(request.Email);
                staff.UpdateStaffPhoneNumber(request.PhoneNumber);
                staff.DeleteStaffAcc(request.IsDeleted);
                staff.UpdateStaffUpdatedBy(request.UpdatedBy);
                staff.UpdateStaffUpdatedOn(DateTime.UtcNow);
                staff.UpdateStaffGender(request.Gender);
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

                RuleFor(x => x.IsDeleted).NotEmpty();
                RuleFor(x => x.Gender).NotEmpty();
                RuleFor(x => x.UpdatedBy).NotEqual(Guid.Empty);

            }
        }
    }
}
