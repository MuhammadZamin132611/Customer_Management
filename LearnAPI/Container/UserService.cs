using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;

namespace LearnAPI.Container
{
    public class UserService : IUserService
    {
        private readonly LearndataContext context;

        public UserService(LearndataContext learnData)
        {
            this.context = learnData;
        }
        public async Task<APIResponse> ConfirmRegister(ConfirmRegister confirmRegister)
        {
            APIResponse response = new APIResponse();
            bool otpresponse = await ValidateOtp(confirmRegister.UserName, confirmRegister.OtpText);
            if(!otpresponse)
            {
                response.Result = "Fail";
                response.Message = "Invalid OTP or Expired";
            }
            else
            {
                var _tempdata = await this.context.TblTempusers.FirstOrDefaultAsync(item => item.Id == confirmRegister.UserId);
                var _user = new TblUser()
                {
                    Username = confirmRegister.UserName,
                    Name = _tempdata.Name,
                    Password = _tempdata.Password,
                    Email = _tempdata.Email,
                    Phone = _tempdata.Phone,
                    Failattempt = 0,
                    Isactive = true,
                    Islocked = false,
                    Role = "User"
                };
                await this.context.TblUsers.AddAsync(_user);
                await this.context.SaveChangesAsync();
                await UpdatePWDManager(confirmRegister.UserName, _tempdata.Password);
                response.Result = "Pass";
                response.Message = "Register Successfully.";
            }

            return response;
        }

        
        public async Task<APIResponse> UserRegistration(UserRegister userRegister)
        {
            APIResponse response = new APIResponse();
            int userid = 0;
            bool isvalid = true;
            try
            {
                // dupticate user
                var _user = await this.context.TblUsers.Where(item => item.Username == userRegister.UserName).ToListAsync();
                if (_user.Count > 0)
                {
                    response.Result = "Fail";
                    response.Message = "Duplicate Username.";
                    isvalid = false;
                }
                // dupticate user
                var _userEmail = await this.context.TblUsers.Where(item => item.Email == userRegister.Email).ToListAsync();
                if (_userEmail.Count > 0)
                {
                    response.Result = "Fail";
                    response.Message = "Duplicate Email.";
                    isvalid = false;
                }


                if (userRegister != null && isvalid)
                {
                    var _tempuser = new TblTempuser()
                    {
                        Code = userRegister.UserName,
                        Name = userRegister.Name,
                        Email = userRegister.Email,
                        Phone = userRegister.Phone,
                        Password = userRegister.Password
                    };
                    await this.context.TblTempusers.AddAsync(_tempuser);
                    await this.context.SaveChangesAsync();
                    userid = _tempuser.Id;
                    string OTPText = Generaterandomnumber();
                    await UpdateOtp(userRegister.UserName, OTPText, "register");
                    await SendOtpMail(userRegister.Email, OTPText, userRegister.Name);
                    response.Result = "Pass";
                    response.ResponseCode = 200;
                    response.Message = userid.ToString();
                }
            }
            catch (Exception ex)
            {
                response.Result = "Fail";
            }
            return response;
        }

        public async Task<APIResponse> ResetPasswrod(ResetPassword resetPassword)
        {
            APIResponse response = new APIResponse();
            var _user = await this.context.TblUsers.FirstOrDefaultAsync(item => 
            item.Username == resetPassword.Username && item.Password == resetPassword.OldPassword && item.Isactive == true);
            if (_user != null)
            {
                var _pwdhistory = await ValidatePwdHistory(resetPassword.Username, resetPassword.NewPassword);
                if(_pwdhistory)
                {
                    response.Result = "Fail";
                    response.Message = "Don't use the same password the used in last 3 transaction";
                    //return response;
                }
                else
                {
                    _user.Password = resetPassword.NewPassword;
                    await this.context.SaveChangesAsync();
                    await UpdatePWDManager(resetPassword.Username, resetPassword.NewPassword);
                    response.Result = "Pass";
                    response.Message = "Password updated successfully.";

                }


            }
            else
            {
                response.Result = "Fail";
                response.Message = "Failed to validate old password.";
            }
            return response;
        }


        public async Task<APIResponse> ForgotPasswrod(string username)
        {
            APIResponse response = new APIResponse();
            var _user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Username == username && item.Isactive == true);
            if (_user != null)
            {
                string otptext = Generaterandomnumber();
                await UpdateOtp(username, otptext, "forgotpassword");
                await SendOtpMail(_user.Email, otptext, _user.Name);
                response.Result = "Pass";
                response.Message = "OTP sent to your email.";
            }
            else
            {
                response.Result = "Fail";
                response.Message = "Invalid Username.";
            }
            return response;
        }

        public async Task<APIResponse> UpdatePasswrod(UpdatePassword updatePassword)
        {
            APIResponse response = new APIResponse();

            bool otpvalidation = await ValidateOtp(updatePassword.username, updatePassword.otptext);
            if (otpvalidation)
            {
                bool pwdhistory = await ValidatePwdHistory(updatePassword.username, updatePassword.peassword);
                if (pwdhistory)
                {
                    response.Result = "Fail";
                    response.Message = "Don't use the same password the used in last 3 transaction";
                    return response;
                }
                else
                {
                    var _user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Username == updatePassword.username && item.Isactive == true);
                    if(_user != null)
                    {
                        _user.Password = updatePassword.peassword;
                        await this.context.SaveChangesAsync();
                        await UpdatePWDManager(updatePassword.username, updatePassword.peassword);
                        response.Result = "Pass";
                        response.Message = "Password updated successfully.";
                    } 
                }
            }
            else
            {
                response.Result = "Fail";
                response.Message = "Invalid OTP or Expired.";
            }

            return response;
        }

        public async Task<APIResponse> UpdateStatus(UpdateStatus updateStatus)
        {
            APIResponse response = new APIResponse();
            var _user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Username == updateStatus.Username);
            if (_user != null)
            {
                _user.Isactive = updateStatus.Status;
                await this.context.SaveChangesAsync();
                response.Result = "Pass";
                response.Message = "User status updated successfully.";
            }
            else
            {
                response.Result = "Fail";
                response.Message = "Invalid Username.";
            }
            return response;
        }

        public async Task<APIResponse> UpdateRole(UpdateRoleModal updateRoleModal)
        {
            APIResponse response = new APIResponse();
            var _user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Username == updateRoleModal.username && item.Isactive == true);
            if (_user != null)
            {
                _user.Role = updateRoleModal.role;
                await this.context.SaveChangesAsync();
                response.Result = "Pass";
                response.Message = "User role updated successfully.";
            }
            else
            {
                response.Result = "Fail";
                response.Message = "Invalid User.";
            }
            return response;
        }

        private async Task UpdateOtp(string username, string otptext, string otptype)
        {
            var _opt = new TblOtpManager()
            {
                Username = username,
                Otptext = otptext,
                Expiration = DateTime.Now.AddMinutes(30),
                Createddate = DateTime.Now,
                Otptype = otptype
            };
            await this.context.TblOtpManagers.AddAsync(_opt);
            await this.context.SaveChangesAsync();
        }

        private async Task<bool> ValidateOtp(string username, string OTPText)
        {
            bool respose = false;
            var _data = await this.context.TblOtpManagers.FirstOrDefaultAsync(item =>
            item.Username == username && item.Otptext == OTPText && item.Expiration > DateTime.Now);
            if (_data != null)
            {
                respose = true;
            }
            return respose;
        }

        private async Task UpdatePWDManager(string username, string password)
        {
            var _pwdManager = new TblPwdManger()
            {
                Username = username,
                Password = password,
                ModifyDate = DateTime.Now
            };
            await this.context.TblPwdMangers.AddAsync(_pwdManager);
            await this.context.SaveChangesAsync();
        }

        private string Generaterandomnumber()
        {
            Random random = new Random();
            string randomno = random.Next(0, 1000).ToString("D6");
            return randomno;
        }

        private async Task SendOtpMail(string useremail, string Otptext, string Name)
        {

        }

        private async Task<bool> ValidatePwdHistory(string Username, string password)
        {
            bool response = false;
            var _pwd = await this.context.TblPwdMangers.Where(item=> item.Username == Username)
                .OrderByDescending(p=> p.ModifyDate).Take(3).ToListAsync();
            if(_pwd.Count>0)
            {
                var validate = _pwd.Where(item => item.Password == password);
                if (validate.Any())
                {
                    response = true;
                }
            }
            return response;
        }

        public async Task<List<GetUserModal>> GetAllUser()
        {
            var users = await this.context.TblUsers.ToListAsync();
            var userModals = users.Select(user => new GetUserModal
            {
                UserName = user.Username,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.Isactive,
                Role = user.Role
            }).ToList();

            return userModals;
        }

        public async Task<GetUserModal> GetByUsername(string username)
        {
            GetUserModal response = new GetUserModal();
            var _user = await this.context.TblUsers.FindAsync(username);
            if (_user != null) 
            {
                response.UserName = _user.Username;
                response.Name = _user.Name;
                response.Email = _user.Email;
                response.Phone = _user.Phone;
                response.IsActive = _user.Isactive;
                response.Role = _user.Role;
            }
            return response;
        }

        
    }
}
