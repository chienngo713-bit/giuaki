using Abp.Application.Services;
using Chien.MultiTenancy.Dto;

namespace Chien.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

