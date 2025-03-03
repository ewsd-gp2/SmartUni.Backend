using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Student
{
    public class Student : BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        [MaxLength(50)] public required string Email { get; set; }

        [MaxLength(20)] public required string PhoneNumber { get; set; }
        public required bool IsDeleted { get; set; }
        public required Enums.GenderType Gender { get; set; }
        public required Enums.MajorType Major { get; set; }
        public Guid? AllocationID { get; set; }
        public Allocation.Allocation? Allocation { get; set; }
        public void UpdateStudentName(string name)
        {
            Name = name;
        }

        public void UpdateStudentEmail(string email)
        {
            Email = email;
        }
        public void UpdateStudentGender(Enums.GenderType gender)
        {
            Gender=gender;
        }

        public void UpdateStudentPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void DeleteStudentfAcc(bool isdeleted)
        {
            IsDeleted = isdeleted;
        }
        public void DeleteStudent()
        {
            IsDeleted = true;
        }
    }
}