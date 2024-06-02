using OnlineShopApp.Dtos.Product;
using OnlineShopApp.Models;

namespace OnlineShopApp.Services.Interfaces
{
	public interface IProductService
	{
		Task<ServiceResponse<string>> AddProduct(AddProductDTO newProduct);		
		Task<ServiceResponse<PagedResult<GetProductsDTO>>> GetAllProducts(int pageNumber, int pageSize);
		Task<ServiceResponse<GetProductDTO>> GetProduct(int productId);
		Task<ServiceResponse<GetProductRatingAndReviewDTO>> GetProductRatingsAndReviews(int productId);
		Task<ServiceResponse<string>> DeleteProduct(int productId);
		Task<ServiceResponse<string>> AddProductReviewAndRating(AddProductReviewAndRatingDTO review);
		Task<ServiceResponse<List<GetProductsDTO>>> GetUserAddedProducts();
		Task<ServiceResponse<string>> RestockProducts(List<RestockProductDTO> restockProducts);
		Task<ServiceResponse<GetAllFilterOptionsDTO>> GetAllFilterOptions();
		Task<ServiceResponse<PagedResult<GetProductsDTO>>> GetFilterData(GetFilterDataDTO filterInfo, int pageNumber, int pageSize);

	}
}
