using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Features.Allocation
{
    public class Allocation : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }

        public void UpdateAllocation(Guid studentId, Guid tutorId, Guid updatedBy, DateTime updatedOn)
        {
            TutorId = tutorId;
            StudentId = studentId;
            UpdatedBy = updatedBy;
            UpdatedOn = updatedOn;
        }
    }
}