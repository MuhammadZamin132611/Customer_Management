using LearnAPI.Container;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService userRole;

        public UserRoleController(IUserRoleService userRole)
        {
            this.userRole = userRole;
        }

        [HttpPost("AssignRolePermission")]
        public async Task<IActionResult> AssignRolePermission(List<TblRolepermission> rolepermissions)
        {
            var data = await this.userRole.AssignRolePermission(rolepermissions);
            return Ok(data);
        }
        
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var data = await this.userRole.GetAllRoles();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
        
        [HttpGet("GetAllMenu")]
        public async Task<IActionResult> GetAllMenu()
        {
            var data = await this.userRole.GetAllMenu();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("GetAllMenubyRole")]
        public async Task<IActionResult> GetAllMenubyRole(string userrole)
        {
            var data = await this.userRole.GetAllMenubyRole(userrole);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("GetAllMenuPermissionbyRole")]
        public async Task<IActionResult> GetAllMenuPermissionbyRole(string userrole, string menucode)
        {
            var data = await this.userRole.GetAllMenuPermissionbyRole(userrole, menucode);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
    }
}
