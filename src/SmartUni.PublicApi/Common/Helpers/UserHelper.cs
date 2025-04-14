using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Common.Helpers
{
    public static class UserHelper
    {
        public static async Task<string> GetUserNameByUserId(Guid userId, SmartUniDbContext dbContext)
        {
            var user = await dbContext.Users.Include(x => x.Tutor)
                .Include(baseUser => baseUser.Student)
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id == userId);

            var name = user?.Role switch
            {
                Enums.RoleType.Student => user.Student?.Name,
                Enums.RoleType.Tutor => user.Tutor?.Name,
                Enums.RoleType.Staff => user.Staff?.Name,
                _ => throw new ArgumentOutOfRangeException()
            };

            return name!;
        }
        
        public static async Task<byte[]?> GetUserAvatarByUserId(Guid userId, SmartUniDbContext dbContext)
        {
            var user = await dbContext.Users.Include(x => x.Tutor)
                .Include(baseUser => baseUser.Student)
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id == userId);

            var image = user?.Role switch
            {
                Enums.RoleType.Student => user.Student?.Image,
                Enums.RoleType.Tutor => user.Tutor?.Image,
                Enums.RoleType.Staff => user.Staff?.Image,
                _ => throw new ArgumentOutOfRangeException()
            };

            return image!;
        }
    }
}