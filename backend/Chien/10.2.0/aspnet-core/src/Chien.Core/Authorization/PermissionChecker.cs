using Abp.Authorization;
using Chien.Authorization.Roles;
using Chien.Authorization.Users;

namespace Chien.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
