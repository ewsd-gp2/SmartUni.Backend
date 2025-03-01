using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Allocation
{
    public class Allocation:BaseEntity
    {
        public Guid Id { get; set; }
        public Guid tutor_id { get; set; }
        public Guid student_id { get; set; }
        public required bool is_deleted { get; set; }
        public required bool is_allocated { get; set; }
        public void UpdateAllocation(Guid Student_ID,Guid Tutor_ID,bool Is_Deleted,Guid Updated_By,DateTime Updated_On)
        {
            tutor_id = Tutor_ID;
            student_id = Student_ID;
            is_deleted = Is_Deleted;
            UpdatedBy = Updated_By;
            UpdatedOn = Updated_On;
        }
    }
}
