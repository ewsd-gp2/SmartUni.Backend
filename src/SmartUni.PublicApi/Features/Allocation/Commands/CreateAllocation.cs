using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Features.Allocation.Models;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Allocation.Commands
{
    public class CreateAllocation
    {
        private sealed record Request(RequestModel Data);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/allocation", HandleAsync)
                    .ProducesValidationProblem()
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Created<RequestModel>, BadRequest<Dictionary<string, string[]>>>>
                HandleAsync(
                    ILogger<Endpoint> logger,
                    SmartUniDbContext dbContext,
                    Request request,
                    CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new allocation with request: {Request}", request);

                Validator validator = new();
                ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);

                    // Create a dictionary to hold validation errors
                    Dictionary<string, string[]> errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    return TypedResults.BadRequest(errors);
                }

                List<Allocation> allocations = MapToDomain(request.Data);

                await dbContext.Allocations.AddRangeAsync(allocations, cancellationToken);
                var allocationMap = allocations
    .ToDictionary(a => a.StudentId, a => a.Id);

                // Update the AllocationID column for each related Student
                await dbContext.Student
                    .Where(s => allocationMap.Keys.Contains(s.Id))
                    .ForEachAsync(s => s.Allocation.Id = allocationMap[s.Id], cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Successfully created allocations with IDs: {Ids}",
                    string.Join(", ", allocations.Select(a => a.Id)));

                return TypedResults.Created("/allocation", request.Data);
            }

            private static List<Allocation> MapToDomain(RequestModel requestModel)
            {
                // Map each RequestAllocationModel to an Allocation
                return requestModel.requestAllocationModels.Select(requestAllocation => new Allocation
                {
                    Id = Guid.NewGuid(),
                    StudentId = requestAllocation.StudentID,
                    TutorId = requestAllocation.TutorID,
                    CreatedBy = requestModel.CreatedBy
                }).ToList();
            }
        }

        private sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Data.requestAllocationModels).NotEmpty()
                    .WithMessage("At least one allocation is required.");

                RuleForEach(x => x.Data.requestAllocationModels).ChildRules(request =>
                {
                    request.RuleFor(x => x.StudentID).NotEmpty().WithMessage("Student ID is required.");
                    request.RuleFor(x => x.TutorID).NotEmpty().WithMessage("Tutor ID is required.");
                });
            }
        }
    }
}