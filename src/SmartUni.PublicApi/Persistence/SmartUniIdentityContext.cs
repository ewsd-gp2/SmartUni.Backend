using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SmartUni.PublicApi.Persistence
{
    public class SmartUniIdentityContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey,
        IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>,
        IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public SmartUniIdentityContext(DbContextOptions options) : base(options) { }
        protected SmartUniIdentityContext() { }
    }
}