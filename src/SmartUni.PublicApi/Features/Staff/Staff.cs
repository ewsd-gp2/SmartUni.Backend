using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Staff
{
    public class Staff : BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        public bool IsDeleted { get; set; }
        public required Enums.GenderType Gender { get; set; }
        public virtual BaseUser Identity { get; set; }
        public string UserCode => UserCodeHelpers.GenerateUserCode(Enums.UserCodePrefix.Sta, Name, Identity.Email!);
        public Guid IdentityId { get; set; }
        public byte[]? Image { get; set; }

        public void UpdateStaffName(string name)
        {
            Name = name;
        }
        public void UpdateStaffGender(Enums.GenderType Gender)
        {
            this.Gender = Gender;
        }

        public void UpdateStaffPhoneNumber(string phoneNumber)
        {
            Identity.PhoneNumber = phoneNumber;
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