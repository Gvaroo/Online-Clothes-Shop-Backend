using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShopApp.Dtos.Cart;
using OnlineShopApp.Models.Product;
using OnlineShopApp.Services.Interfaces;

namespace OnlineShopApp.Controllers
{
	/// <summary>
	/// Controller for managing the user's shopping cart and order process.
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class CartController : ControllerBase
	{
		private readonly ICartService _cartService;

		public CartController(ICartService cartService)
		{
			_cartService = cartService;
		}

		/// <summary>
		/// Adds or updates products in the user's cart.
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddOrUpdateProductsToCart(List<CartDataDTO> cartData)
		{
			var result = await _cartService.AddOrUpdateProductsToCart(cartData);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves the current user's cart data.
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetUserCartData()
		{
			var result = await _cartService.GetUserCartData();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Deletes a product from the user's cart.
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> DeleteProductFromCart(CartDataDTO cartData)
		{
			var result = await _cartService.RemoveProductFromCart(cartData);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Processes the user's checkout.
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Checkout(CheckoutDTO checkoutData)
		{
			var result = await _cartService.CheckOut(checkoutData);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves the user's order history.
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetUserOrderHistory()
		{
			var result = await _cartService.GetUserOrderHistory();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves the products from a specific order by order ID.
		/// </summary>
		[HttpGet("{orderId}")]
		[Authorize]
		public async Task<IActionResult> GetOrderedProducts(int orderId)
		{
			var result = await _cartService.GetOrderedProducts(orderId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves the user's shipping information.
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetShippingInfo()
		{
			var result = await _cartService.GetShippingInfo();
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}

}
