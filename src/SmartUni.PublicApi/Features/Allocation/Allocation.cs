using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Features.Allocation
{
    public class Allocation : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public required bool IsDeleted { get; set; }
        public required bool IsAllocated { get; set; }

        public void UpdateAllocation(Guid studentId, Guid tutorId, bool isDeleted, Guid updatedBy, DateTime updatedOn)
        {
            TutorId = tutorId;
            StudentId = studentId;
            IsDeleted = isDeleted;
            UpdatedBy = updatedBy;
            UpdatedOn = updatedOn;
        }
    }
}