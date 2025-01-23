using AutoMapper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Helper
{
    public class AutoMapperHandler : Profile
    {
        public AutoMapperHandler()
        {
            CreateMap<TblCustomer, Customermodal>().ForMember(item => item.Statusname, static opt => opt.MapFrom(
                item => (item.IsActive != null && item.IsActive.Value) ? "Active":"InActive")).ReverseMap();
            
            //CreateMap<TblCustomer, Customermodal>().ForMember(item => item.Statusname, opt => opt.MapFrom(
            //    item => item.IsActive.HasValue && item.IsActive.Value ? "Active" : "InActive"));
        }
    }
}
