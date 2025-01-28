using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Service
{
    public interface IUserService 
    {
        Task<List<GetUserModal>> GetAllUser();
        Task<APIResponse> UserRegistration(UserRegister userRegister);
        Task<APIResponse> ConfirmRegister(ConfirmRegister confirmRegister);
        Task<APIResponse> ResetPasswrod(ResetPassword resetPassword);
        Task<APIResponse> ForgotPasswrod(string username);
        Task<APIResponse> UpdatePasswrod(UpdatePassword updatePassword);
        Task<APIResponse> UpdateStatus(UpdateStatus updateStatus);
        Task<APIResponse> UpdateRole(UpdateRoleModal updateRoleModal);
        Task<GetUserModal> GetByUsername(string username);

    }
}
