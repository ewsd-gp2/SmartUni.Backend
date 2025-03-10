using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Tutor
{
    public sealed class Tutor : BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        [MaxLength(50)] public required string Email { get; set; }

        [MaxLength(20)] public required string PhoneNumber { get; set; }

        public required Enums.GenderType Gender { get; set; }

        public required Enums.MajorType Major { get; set; }

        public string UserCode => UserCodeHelpers.GenerateUserCode(Enums.UserCodePrefix.Stu, Name, Email);

        public bool IsDeleted { get; private set; }

        public void UpdateTutorName(string name)
        {
            Name = name;
        }

        public void UpdateTutorEmail(string email)
        {
            Email = email;
        }

        public void UpdateTutorPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void DeleteTutor()
        {
            IsDeleted = true;
        }
    }
}