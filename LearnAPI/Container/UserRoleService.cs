using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;

namespace LearnAPI.Container
{
    public class UserRoleService : IUserRoleService
    {
        private readonly LearndataContext context;

        public UserRoleService(LearndataContext context)
        {
            this.context = context;
        }
        public async Task<APIResponse> AssignRolePermission(List<TblRolepermission> _data)
        {
            APIResponse response = new APIResponse();
            int processcount = 0;
            try
            {
                using (var dbtransaction = await this.context.Database.BeginTransactionAsync())
                {
                    if (_data.Count > 0)
                    {
                        _data.ForEach(item =>
                        {
                            var userdata = this.context.TblRolepermissions.FirstOrDefault(item1 => item1.Userrole == item.Userrole &&
                            item1.Menucode == item.Menucode);
                            if (userdata != null)
                            {
                                userdata.Haveview = item.Haveview;
                                userdata.Haveadd = item.Haveadd;
                                userdata.Havedelete = item.Havedelete;
                                userdata.Haveedit = item.Haveedit;
                                processcount++;
                                //this.context.TblRolepermissions.Update(userdata);
                            }
                            else
                            {
                                this.context.TblRolepermissions.Add(item);
                                processcount++;
                            }
                        });
                        if(_data.Count == processcount)
                        {
                            await this.context.SaveChangesAsync();
                            await dbtransaction.CommitAsync();
                            response.Result = "Pass";
                            response.Message = "Saved Successfully.";
                        }
                        else
                        {
                            await dbtransaction.RollbackAsync();
                        }
                    }
                    else
                    {
                        response.Result = "Faill";
                        response.Message = "Failed.";
                    }
                }
            }
            catch (Exception ex)
            {
                response = new APIResponse();
                //response.Result = "Faill";
                //response.Message = ex.Message;
            }
            return response;
        }

        public async Task<List<TblMenu>> GetAllMenu()
        {
            return await this.context.TblMenus.ToListAsync();
        }

        public async Task<List<Appmenucs>> GetAllMenubyRole(string userrole)
        {
            List<Appmenucs> appmenucs = new List<Appmenucs>();

            var accessdata = (from menu in this.context.TblRolepermissions.Where(o => o.Userrole == userrole && o.Haveview) 
                              join m in this.context.TblMenus on menu.Menucode equals m.Code into _jointable
                              from p in _jointable.DefaultIfEmpty()
                              select new {code=menu.Menucode, name = p.Name}).ToList();
            if (accessdata.Any())
            {
                accessdata.ForEach(item =>
                {
                    appmenucs.Add(new Appmenucs()
                    {
                        code = item.code,
                        Name = item.name
                    });
                });
            }
            return appmenucs;
        }

        public async Task<List<TblRole>> GetAllRoles()
        {
            return await this.context.TblRoles.ToListAsync();
        }

        public async Task<MenuPermission> GetAllMenuPermissionbyRole(string userrole, string menucode)
        {
            MenuPermission menuPermission = new MenuPermission();

            var _data = this.context.TblRolepermissions.FirstOrDefault(o => o.Userrole == userrole && o.Haveview 
            && o.Menucode == menucode);
            if (_data != null)
            {
                menuPermission.code = _data.Menucode;
                menuPermission.Haveview = _data.Haveview;
                menuPermission.Haveadd = _data.Haveadd;
                menuPermission.Haveedit = _data.Haveedit;
                menuPermission.Havedelete = _data.Havedelete;
            }

            return menuPermission;
        }

        
    }
}
