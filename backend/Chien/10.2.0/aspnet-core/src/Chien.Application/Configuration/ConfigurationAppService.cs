using Abp.Authorization;
using Abp.Runtime.Session;
using Chien.Configuration.Dto;
using System.Threading.Tasks;

namespace Chien.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : ChienAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
