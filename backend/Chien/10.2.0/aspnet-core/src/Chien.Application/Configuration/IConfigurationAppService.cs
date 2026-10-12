using Chien.Configuration.Dto;
using System.Threading.Tasks;

namespace Chien.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
