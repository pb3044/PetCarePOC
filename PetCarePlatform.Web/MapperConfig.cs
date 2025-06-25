using AutoMapper;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<PetOwner, PetOwnerViewModel>().ReverseMap();
        }
    }
}