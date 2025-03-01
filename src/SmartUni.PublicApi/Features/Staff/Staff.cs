using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Staff
{
    public class Staff: BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        [MaxLength(50)] public required string Email { get; set; }

        [MaxLength(20)] public required string PhoneNumber { get; set; }
        public required bool is_deleted { get; set; } 
        public required string gender { get; set; }

        public void UpdateStaffName(string name)
        {
            Name = name;
        }

        public void UpdateStaffEmail(string email)
        {
            Email = email;
        }
        public void UpdateStaffGender(string Gender)
        {
            gender = Gender;
        }

        public void UpdateStaffPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
        public void DeleteStaffAcc(bool isdeleted)
        {
            is_deleted = isdeleted;
        }
        public void UpdateStaffUpdatedBy(Guid updated_by)
        {
            UpdatedBy = updated_by;
        }
        public void UpdateStaffUpdatedOn(DateTime updated_on)
        {
            UpdatedOn = updated_on;
        }
    }
}
