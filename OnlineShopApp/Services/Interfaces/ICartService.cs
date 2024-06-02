using OnlineShopApp.Dtos.Cart;
using OnlineShopApp.Models;

namespace OnlineShopApp.Services.Interfaces
{
	public interface ICartService
	{
		Task<ServiceResponse<string>> AddOrUpdateProductsToCart(List<CartDataDTO> cartData);
		Task<ServiceResponse<string>> RemoveProductFromCart(CartDataDTO cartData);
		Task<ServiceResponse<List<ProductDTO>>> GetUserCartData();
		Task<ServiceResponse<GetOrderDTO>> CheckOut(CheckoutDTO data);
		Task<ServiceResponse<GetShippingInfoDTO>> GetShippingInfo();
		Task<ServiceResponse<List<GetOrderHistoryDTO>>> GetUserOrderHistory();
		Task<ServiceResponse<GetOrderDTO>> GetOrderedProducts(int orderId);
	}
}
