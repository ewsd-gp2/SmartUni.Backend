using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Staff
{
    public class Staff : BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        [MaxLength(50)] public  string Email { get; set; }

        [MaxLength(20)] public  string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public required Enums.GenderType Gender { get; set; }
        public virtual BaseUser Identity { get; set; }
        public string UserCode => UserCodeHelpers.GenerateUserCode(Enums.UserCodePrefix.Tut, Name, Identity.Email!);
        public Guid IdentityId { get; set; }

        public void UpdateStaffName(string name)
        {
            Name = name;
        }

        public void UpdateStaffEmail(string email)
        {
            Email = email;
        }

        public void UpdateStaffGender(Enums.GenderType Gender)
        {
            this.Gender = Gender;
        }

        public void UpdateStaffPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void DeleteStaffAcc(bool isdeleted)
        {
            IsDeleted = isdeleted;
        }

        public void UpdateStaffUpdatedBy(Guid updated_by)
        {
            UpdatedBy = updated_by;
        }

        public void UpdateStaffUpdatedOn(DateTime updated_on)
        {
            UpdatedOn = updated_on;
        }

        public void DeleteStaff()
        {
            IsDeleted = true;
        }
    }
}