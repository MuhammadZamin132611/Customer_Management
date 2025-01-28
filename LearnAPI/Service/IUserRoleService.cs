using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Service
{
    public interface IUserRoleService
    {
        Task<APIResponse> AssignRolePermission(List<TblRolepermission> _data);
        Task<List<TblRole>> GetAllRoles();
        Task<List<TblMenu>> GetAllMenu();
        Task<List<Appmenucs>> GetAllMenubyRole(string userrole);
        Task<MenuPermission> GetAllMenuPermissionbyRole(string userrole, string menucode);
    }
}
