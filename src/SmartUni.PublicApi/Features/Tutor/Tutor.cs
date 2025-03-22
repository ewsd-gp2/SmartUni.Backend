using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;

namespace SmartUni.PublicApi.Features.Tutor
{
    public class Tutor : BaseEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public required Enums.GenderType Gender { get; set; }

        public required Enums.MajorType Major { get; set; }

        public string UserCode => UserCodeHelpers.GenerateUserCode(Enums.UserCodePrefix.Tut, Name, Identity.Email!);

        public bool IsDeleted { get; private set; }

        public virtual BaseUser Identity { get; set; }

        public Guid IdentityId { get; set; }

        public void UpdateTutorName(string name)
        {
            Name = name;
        }

        public void UpdateTutorEmail(string email)
        {
            Identity.Email = email;
        }

        public void UpdateTutorPhoneNumber(string phoneNumber)
        {
            Identity.PhoneNumber = phoneNumber;
        }

        public void DeleteTutor()
        {
            IsDeleted = true;
        }
    }
}