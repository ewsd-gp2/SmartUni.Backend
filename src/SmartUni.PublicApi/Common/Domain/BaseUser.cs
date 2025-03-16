using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Features.Staff;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Features.Tutor;

namespace SmartUni.PublicApi.Common.Domain
{
    public class BaseUser : IdentityUser<Guid>
    {
        public virtual Tutor Tutor { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Student Student { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}