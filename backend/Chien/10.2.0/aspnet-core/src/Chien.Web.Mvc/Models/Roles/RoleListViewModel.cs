using Chien.Roles.Dto;
using System.Collections.Generic;

namespace Chien.Web.Models.Roles;

public class RoleListViewModel
{
    public IReadOnlyList<PermissionDto> Permissions { get; set; }
}
