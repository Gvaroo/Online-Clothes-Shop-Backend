using AutoMapper;
using OnlineShopApp.Dtos.Auth;
using OnlineShopApp.Dtos.Cart;
using OnlineShopApp.Dtos.Product;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Models.Cart;
using OnlineShopApp.Models.Product;

namespace OnlineShopApp.Profiles
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<RegisterUserDTO, User>().ReverseMap();
			CreateMap<AddProductDTO, Product>().ReverseMap();
			CreateMap<GetCategoryAndSubCategoriesDTO, Category>().ReverseMap()
						   .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));
			CreateMap<GetProductsDTO, Product>().ReverseMap();							
			CreateMap<GetSubCategoriesDTO, ProductSubCategories>().ReverseMap();
			CreateMap<GetCategoryAndSubCategoriesDTO, GetSubCategoriesDTO>().ReverseMap();
			CreateMap<ProductRating, GetProductRatingDTO>().ReverseMap();
			CreateMap<ProductReviews, GetProductReviewsDTO>().ReverseMap();
			CreateMap<Product, GetProductRatingAndReviewDTO>();
			CreateMap<GetCategoryDTO, Category>().ReverseMap();			
			CreateMap<GetSubCategoriesDTO, ProductSubCategories>().ReverseMap();
			CreateMap<GetBrandDTO, ProductBrand>().ReverseMap();
			CreateMap<GetProductSizeDTO, Size>().ReverseMap();
			CreateMap<GetColorDTO, ProductColor>().ReverseMap();
			CreateMap<GetGenderDTO, Gender>().ReverseMap();
			CreateMap<GetProductImageDTO, ProductImages>()
		 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
		 .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
		 .ReverseMap();	
	
			CreateMap<Product, GetProductDTO>()
			.ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
			.ReverseMap();
			CreateMap<GetUserDTO, User>().ReverseMap();
			CreateMap<GetRatingDTO, ProductRating>().ReverseMap();
			CreateMap<UserInfoDTO, User>().ReverseMap();			
			CreateMap<ProductDTO, Product>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
				 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                 .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.MaxQuantity))
                 .ReverseMap();
			CreateMap<Product, OrderItems>()
			  .ForPath(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
			  .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))			 
			  .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Price))
			  .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))			  
			  .ReverseMap();
			CreateMap<GetShippingInfoDTO, ShippingInfo>().ReverseMap();
			CreateMap<UpdateProfileDTO, User>()
				.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
				.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))				
				.ReverseMap();

		}
	}
}
