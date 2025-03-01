using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Student
{
    public class Student:BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        [MaxLength(50)] public required string Email { get; set; }

        [MaxLength(20)] public required string PhoneNumber { get; set; }
        public required bool is_deleted { get; set; }
        public required string gender { get; set; }

        public void UpdateStudentName(string name)
        {
            Name = name;
        }

        public void UpdateStudentEmail(string email)
        {
            Email = email;
        }

        public void UpdateStudentPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
        public void DeleteStudentfAcc(bool isdeleted)
        {
            is_deleted = isdeleted;
        }
    }
}
