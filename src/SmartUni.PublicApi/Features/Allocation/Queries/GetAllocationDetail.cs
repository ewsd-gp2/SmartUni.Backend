using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Allocation.Queries
{
    public class GetAllocationDetail
    {
        private sealed record Response(
            Guid Id,
            string TutorName,
            Guid TutorID,
            string StudentName,
            Guid StudentID,
            Guid CreatedBy,
            string StaffName,
            bool Is_Deleted);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/allocationDetail/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Ok<Response>, NotFound>> HandleAsync(
                Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for allocation with ID: {Id}", id);

                var detailAllocation = await dbContext.Allocations
                    .Where(x => x.Id == id)
                    .Join(dbContext.Student,
                        allocation => allocation.StudentId,
                        student => student.Id,
                        (allocation, student) => new { allocation, student })
                    .Join(dbContext.Tutor,
                        allocationAndStudent => allocationAndStudent.allocation.TutorId,
                        tutor => tutor.Id,
                        (allocationAndStudent, tutor) => new
                        {
                            Allocation = allocationAndStudent.allocation,
                            StudentName = allocationAndStudent.student.Name,
                            StudentID = allocationAndStudent.student.Id,
                            TutorName = tutor.Name,
                            TutorID = tutor.Id,
                            AllocationID = allocationAndStudent.allocation.Id,
                            allocationAndStudent.allocation.CreatedBy,
                            allocationAndStudent.allocation.IsDeleted
                        })
                    .Join(dbContext.Staff,
                        allocationAndTutor => allocationAndTutor.CreatedBy,
                        staff => staff.Id,
                        (allocationAndTutor, staff) => new
                        {
                            allocationAndTutor.AllocationID,
                            allocationAndTutor.StudentName,
                            allocationAndTutor.StudentID,
                            allocationAndTutor.TutorName,
                            allocationAndTutor.TutorID,
                            allocationAndTutor.CreatedBy,
                            StaffName = staff.Name,
                            allocationAndTutor.IsDeleted
                        })
                    .FirstOrDefaultAsync(cancellationToken);

                if (detailAllocation != null)
                {
                    Response response = new(
                        detailAllocation.AllocationID,
                        detailAllocation.TutorName,
                        detailAllocation.TutorID,
                        detailAllocation.StudentName,
                        detailAllocation.StudentID,
                        detailAllocation.CreatedBy,
                        detailAllocation.StaffName,
                        detailAllocation.IsDeleted
                    );

                    logger.LogInformation("Successfully fetched details for allocation with ID: {Id}", id);
                    return TypedResults.Ok(response);
                }

                // Handle case where allocation is not found
                logger.LogWarning("Allocation with ID: {Id} not found", id);
                return TypedResults.NotFound();
            }
        }
    }
}