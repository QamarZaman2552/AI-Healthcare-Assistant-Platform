using AIHealthcareAssistant.Application.Common.Models;

namespace AIHealthcareAssistant.Application.Features.Admin;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<List<UserResponse>> GetUsersAsync();
    Task<SystemStatusResponse> GetSystemStatusAsync();
}
