using Chien.Roles.Dto;
using System.Collections.Generic;

namespace Chien.Web.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }
}