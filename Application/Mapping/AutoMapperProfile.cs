using Application.Dtos;
using Application.ResponseDto;
using AutoMapper;
using Core.Entities;


namespace Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDetailsDto, UserDetails>().ReverseMap();
            CreateMap<Inventory, InventoryDto>().ReverseMap();
            CreateMap<PurchaseRequest, PurchaseRequestDto>().ReverseMap();
            CreateMap<PurchaseRequest, PurchaseRequestResponseDto>().ReverseMap();
            CreateMap<FileEntityDto, FileEntity>().ReverseMap();
            CreateMap<Item, ItemDto>().ReverseMap();
            CreateMap<RequestItemDto, RequestItem>().ReverseMap();
            CreateMap<InventoryDtos, Inventory>().ReverseMap();
            CreateMap<ItemRequestForDto, ItemRequestFor>().ReverseMap();

            CreateMap<ItemRequestFor, ItemRequestForDto>().ReverseMap();
            CreateMap<RequestItem, RequestItemDto>().ReverseMap();


        }
    }
}
