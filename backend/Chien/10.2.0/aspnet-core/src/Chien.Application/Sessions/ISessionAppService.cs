using Abp.Application.Services;
using Chien.Sessions.Dto;
using System.Threading.Tasks;

namespace Chien.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
