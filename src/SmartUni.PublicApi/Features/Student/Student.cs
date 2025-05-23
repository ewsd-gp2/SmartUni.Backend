﻿using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Features.Meeting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartUni.PublicApi.Features.Student
{
    public class Student : BaseEntity
    {
        public Guid Id { get; set; }

        [MaxLength(50)] public required string Name { get; set; }

        public bool IsDeleted { get; set; }

        public required Enums.GenderType Gender { get; set; }

        public required Enums.MajorType Major { get; set; }

        //public Guid? AllocationID { get; set; }
        public Allocation.Allocation? Allocation { get; set; }
        [JsonIgnore]
        public virtual BaseUser Identity { get; set; }
        public string UserCode => UserCodeHelpers.GenerateUserCode(Enums.UserCodePrefix.Stu, Name, Identity.Email!);
        public Guid IdentityId { get; set; }
        public List<MeetingParticipant> Meetings { get; set; } = [];
        public byte[]? Image { get; set; }
        public void UpdateStudentName(string name)
        {
            Name = name;
        }

        public void UpdateStudentEmail(string email)
        {
            Identity.Email = email;
        }

        public void UpdateModifiedBy(Guid updatedBy)
        {
            UpdatedBy = updatedBy;
        }

        public void UpdateStudentGender(Enums.GenderType gender)
        {
            Gender = gender;
        }

        public void UpdateStudentPhoneNumber(string phoneNumber)
        {
            Identity.PhoneNumber = phoneNumber;
        }

        public void UpdateStudentMajor(Enums.MajorType major)
        {
            Major = major;
        }

        public void DeleteStudent()
        {
            IsDeleted = true;
        }
    }
}