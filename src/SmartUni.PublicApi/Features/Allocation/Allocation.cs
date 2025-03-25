using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Features.Allocation
{
    public class Allocation : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Student.Student Student { get; set; }

        public void UpdateAllocation(Guid studentId, Guid tutorId)
        {
            TutorId = tutorId;
            StudentId = studentId;
        }
    }
}