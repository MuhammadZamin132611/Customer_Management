using LearnAPI.Modal;
using LearnAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }


        [HttpPost("Userregisteration")]
        public async Task<IActionResult> Userregisteration(UserRegister userRegister)
        {
            var data = await userService.UserRegistration(userRegister);
            return Ok(data);
        }

        [HttpPost("ConfirmRegister")]
        public async Task<IActionResult> ConfirmRegister(ConfirmRegister confirmRegister)
        {
            var data = await userService.ConfirmRegister(confirmRegister);
            return Ok(data);
        }


        [HttpPost("Resetpassword")]
        public async Task<IActionResult> Resetpassword(ResetPassword resetPassword)
        {
            var data = await userService.ResetPasswrod(resetPassword);
            return Ok(data);
        }


        [HttpGet("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            var data = await userService.ForgotPasswrod(username);
            return Ok(data);
        }


        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword)
        {
            var data = await userService.UpdatePasswrod(updatePassword);
            return Ok(data);
        }


        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(UpdateStatus updateStatus)
        {
            var data = await userService.UpdateStatus(updateStatus);
            return Ok(data);
        }


        [HttpPost("UpdateRole")]
        public async Task<IActionResult> UpdateRole(UpdateRoleModal updateRoleModal)
        {
            var data = await userService.UpdateRole(updateRoleModal);
            return Ok(data);
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await userService.GetAllUser();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("GetByUsername")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var data = await userService.GetByUsername(username);
            if (data == null)
            {
                return NotFound();
            } 
            return Ok(data);
        }

    }
}
