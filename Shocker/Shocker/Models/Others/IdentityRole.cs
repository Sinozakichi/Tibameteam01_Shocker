using System.Security.Claims;
using System.Security.Principal;

namespace Shocker.Models.Others
{
    public static class IdentityRole
    {
        /// <summary>
        /// 角色
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserRole(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));
            return identity is ClaimsIdentity identity1 ? identity1.FindFirst(ClaimTypes.Role)?.Value : null;
        }

    }
}
