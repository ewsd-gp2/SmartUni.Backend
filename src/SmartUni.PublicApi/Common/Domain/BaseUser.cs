using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Features.Tutor;

namespace SmartUni.PublicApi.Common.Domain
{
    public class BaseUser : IdentityUser<Guid>
    {
        public virtual Tutor Tutor { get; set; }

        public DateTime? LastActiveDate { get; set; }
    }
}