using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShopApp.Dtos.Product;
using OnlineShopApp.Services.Interfaces;
using System.Drawing.Printing;

namespace OnlineShopApp.Controllers
{
	/// <summary>
	/// Controller for managing products.
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ProductController : ControllerBase
	{
		private readonly IProductService _productService;

		public ProductController(IProductService productService)
		{
			_productService = productService;
		}

		/// <summary>
		/// Retrieves a paginated list of all products.
		/// </summary>
		/// <param name="pageNumber">Page number for pagination.</param>
		/// <param name="pageSize">Number of products per page.</param>
		/// <returns>A list of products.</returns>
		[HttpGet]
		public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 9)
		{			
			var result = await _productService.GetAllProducts(pageNumber, pageSize);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Adds a new product to the catalog. Only accessible by Admin.
		/// </summary>
		/// <param name="newProduct">Product details to add.</param>
		/// <returns>Result of the add operation.</returns>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddProduct(AddProductDTO newProduct)
		{
			var result = await _productService.AddProduct(newProduct);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves the details of a specific product by ID.
		/// </summary>
		/// <param name="productId">ID of the product to retrieve.</param>
		/// <returns>Product details.</returns>
		[HttpGet("{productId}")]
		public async Task<IActionResult> GetProduct(int productId)
		{
			var result = await _productService.GetProduct(productId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Deletes a specific product by ID. Only accessible by Admin.
		/// </summary>
		/// <param name="productId">ID of the product to delete.</param>
		/// <returns>Result of the delete operation.</returns>
		[HttpDelete("{productId}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteProduct(int productId)
		{
			var result = await _productService.DeleteProduct(productId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Adds a review and rating for a product. Accessible by authenticated users.
		/// </summary>
		/// <param name="review">Review and rating details.</param>
		/// <returns>Result of the add operation.</returns>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddProductReviewAndRating(AddProductReviewAndRatingDTO review)
		{
			var result = await _productService.AddProductReviewAndRating(review);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves ratings and reviews for a specific product by ID.
		/// </summary>
		/// <param name="productId">ID of the product to get reviews for.</param>
		/// <returns>List of ratings and reviews.</returns>
		[HttpGet("{productId}")]
		public async Task<IActionResult> GetProductRatingsAndReviews(int productId)
		{
			var result = await _productService.GetProductRatingsAndReviews(productId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves products added by the authenticated admin user.
		/// </summary>
		/// <returns>List of products added by the user.</returns>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetUserAddedProducts()
		{
			var result = await _productService.GetUserAddedProducts();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Restocks specified products. Only accessible by Admin.
		/// </summary>
		/// <param name="restockProducts">List of products to restock.</param>
		/// <returns>Result of the restock operation.</returns>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RestockProducts(List<RestockProductDTO> restockProducts)
		{
			var result = await _productService.RestockProducts(restockProducts);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves all available filter options for products.
		/// </summary>
		/// <returns>List of filter options.</returns>
		[HttpGet]
		public async Task<IActionResult> GetAllFilterOptions()
		{
			var result = await _productService.GetAllFilterOptions();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Retrieves filtered product data based on specified filter criteria.
		/// </summary>
		/// <param name="filter">Filter criteria.</param>
		/// <param name="pageNumber">Page number for pagination.</param>
		/// <param name="pageSize">Number of products per page.</param>
		/// <returns>Filtered list of products.</returns>
		[HttpPost]
		public async Task<IActionResult> GetFilterData(GetFilterDataDTO filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 9)
		{
			var result = await _productService.GetFilterData(filter, pageNumber, pageSize);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}
}
