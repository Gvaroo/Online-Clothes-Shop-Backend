using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineShopApp.Data;
using OnlineShopApp.Dtos.Product;
using OnlineShopApp.Models;
using OnlineShopApp.Models.Product;
using OnlineShopApp.Services.Interfaces;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;



namespace OnlineShopApp.Services.Implementations
{
	public class ProductService : IProductService
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;
		private readonly IDistributedCache _distributedCache;
		private readonly IAuditService _auditService;
		private readonly ITokenValidationService _tokenValidation;

		public ProductService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, IDistributedCache distributedCache, IAuditService auditService, ITokenValidationService tokenValidation)
		{
			_db = db;
			_mapper = mapper;
			_configuration = configuration;
			_distributedCache = distributedCache;
			_auditService = auditService;
			_tokenValidation = tokenValidation;
		}


		public async Task<ServiceResponse<string>> AddProduct(AddProductDTO newProduct)
		{
			var response = new ServiceResponse<string>();
			try
			{
				if (newProduct.Quantity <= 0)
				{
					response.Success = false;
					response.Message = "Product quantity must be at least one.";
					return response;
				}

				// Get user ID from token
				var userId = _tokenValidation.GetUserIdFromToken();
				if (userId == null)
				{
					response.Success = false;
					response.Message = "Invalid user token.";
					return response;
				}

				// create Product entity
				var product = new Product { Name= newProduct.Name,Description = newProduct.Description,Price = newProduct.Price,Quantity= newProduct.Quantity };

				// Fetch related entities from the database
				product.User = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
				product.Category = await _db.Category.FirstOrDefaultAsync(c => c.Id == newProduct.CategoryId);
				product.SubCategories = await _db.ProductSubCategories.FirstOrDefaultAsync(s => s.Id == newProduct.SubCategoryId);
				product.ProductColor = await _db.ProductColors.Where(s => newProduct.ColorIds.Contains(s.Id)).ToListAsync();
				product.Sizes = await _db.Sizes.Where(s => newProduct.SizeIds.Contains(s.SizeId)).ToListAsync();
				product.Gender = await _db.Gender.FirstOrDefaultAsync(s => s.Id == newProduct.GenderId);
				

				// Handle product brand
				if (!string.IsNullOrEmpty(newProduct.newBrand))
				{
					var newBrand = new ProductBrand { Name = newProduct.newBrand };
					await _db.ProductBrand.AddAsync(newBrand);
					product.ProductBrand = newBrand;
				}
				else
				{
					product.ProductBrand = await _db.ProductBrand.FirstOrDefaultAsync(c => c.Id == newProduct.BrandId);					
				}

				// Upload images and set product images
				product.Images = await UploadImagesFromBase64Async(newProduct.Images);

				// Add and save the new product
				await _db.Product.AddAsync(product);
				await _db.SaveChangesAsync();

				response.Data = "Your product has been added!";
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"An error occurred: {ex.Message}";
			}
			return response;
		}
		

		public async Task<ServiceResponse<PagedResult<GetProductsDTO>>> GetAllProducts(int pageNumber = 1, int pageSize = 9)
		{
			var response = new ServiceResponse<PagedResult<GetProductsDTO>>();
			try
			{
				var totalProducts = await _db.Product.CountAsync();
				var productsQuery = _db.Product
			     .Include(p => p.Images)
			     .Include(p => p.Category)
			     .Include(p => p.SubCategories)
			     .Include(p => p.Sizes)
			     .OrderByDescending(c => c.Id);

				var pagedProducts = await productsQuery
					.Skip((pageNumber - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

				response.Data = new PagedResult<GetProductsDTO>
				{
					TotalCount = totalProducts,
					Items = pagedProducts.Select(p => new GetProductsDTO
					{
						Id = p.Id,
						Name = p.Name,
						Price = p.Price,
						Quantity = p.Quantity,
						Category = _mapper.Map<GetCategoryDTO>(p.Category),
						SubCategories = _mapper.Map<GetSubCategoriesDTO>(p.SubCategories),
						DefaultSize = p.Sizes.Any() ? _mapper.Map<GetProductSizeDTO>(p.Sizes.First()) : null,
						Image = p.Images.FirstOrDefault() != null ? _mapper.Map<GetProductImageDTO>(p.Images.First()) : null
					}).ToList()
				};
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;

			}
			return response;
		}

		//Uploads list of images to Cloudinary and gets back list of uploaded images urls
		private async Task<List<ProductImages>> UploadImagesFromBase64Async(List<UploadImageDTO> uploadImageDTOs)
		{
			var uploadTasks = uploadImageDTOs.Select(dto => UploadImageFromBase64Async(dto.ImageData));
			var results = await Task.WhenAll(uploadTasks);
			return results.ToList();
		}
		//Uploads single image to Cloudinary and gets back  uploaded image url
		private async Task<ProductImages> UploadImageFromBase64Async(string base64String)

		{
			Account account = CloudinaryConfig.GetAccount();
			Cloudinary cloudinary = new Cloudinary(account);

			byte[] imageBytes = Convert.FromBase64String(base64String);

			using (MemoryStream ms = new MemoryStream(imageBytes))
			{
				ImageUploadParams uploadParams = new ImageUploadParams()
				{
					File = new FileDescription("image", ms),
					UploadPreset = _configuration.GetSection("Cloudinary:UploadPreset").Value
				};

				ImageUploadResult uploadResult = await cloudinary.UploadAsync(uploadParams);

				return new ProductImages
				{
					ImageUrl = uploadResult.Url.ToString()
				};
			}
		}

		// Get Single Product		
		public async Task<ServiceResponse<GetProductDTO>> GetProduct(int productId)
		{
			var response = new ServiceResponse<GetProductDTO>();

			// Check if the product data is in the cache
			string cacheKey = $"Product_{productId}";

			if (_distributedCache != null)
			{
				var cachedProductData = await _distributedCache.GetStringAsync(cacheKey);

				if (cachedProductData != null)
				{
					// If data is found in the cache, deserialize and return it
					var cachedProduct = JsonSerializer.Deserialize<GetProductDTO>(cachedProductData);
					response.Data = cachedProduct;
					return response;
				}
			}

			// If data is not in the cache, fetch it from the database
			try
			{
				var product = await _db.Product
									  .Include(p => p.Category)
									  .Include(p => p.SubCategories)
									  .Include(p => p.ProductRating)
									  .Include(p => p.ProductReviews)
									  .Include(p => p.Images)
									  .Include(p => p.ProductBrand)
									  .Include(p => p.Sizes)
									  .FirstOrDefaultAsync(p => p.Id == productId);
				if (product != null)
				{
					// Get average rating of product
					double averageRating = product.ProductRating.Any() ?
										 product.ProductRating.Average(pr => pr.RatingValue) : 0;

					// Map Product
					var mappedProduct = _mapper.Map<GetProductDTO>(product);

					// Add Product Brand and Sizes manually
					mappedProduct.Brand = _mapper.Map<GetBrandDTO>(product.ProductBrand);
					mappedProduct.Sizes = product.Sizes.Select(s => _mapper.Map<GetProductSizeDTO>(s)).ToList();

					// Map all images
					mappedProduct.Images = product.Images.Select(img => _mapper.Map<GetProductImageDTO>(img)).ToList();

					response.Data = mappedProduct;

					// Round the average rating to the nearest whole number
					response.Data.AverageRating = (int)Math.Round(averageRating);
					// Get product reviews count
					response.Data.ReviewsCount = product.ProductReviews.Count();

					// Store the product data in the cache for future use
					if (_distributedCache != null)
					{
						var cacheOptions = new DistributedCacheEntryOptions
						{
							AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
						};

						string serializedProductData = JsonSerializer.Serialize(response.Data);
						await _distributedCache.SetStringAsync(cacheKey, serializedProductData, cacheOptions);
					}
				}
				else
				{
					response.Success = false;
					response.Message = "Product was not found";
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}

			return response;
		}


		// Deletes product from database
		public async Task<ServiceResponse<string>> DeleteProduct(int productId)
		{
			var response = new ServiceResponse<string>();
			try
			{
				var product = await _db.Product.FindAsync(productId);
				if (product != null)
				{
					_db.Product.Remove(product);
					await _db.SaveChangesAsync();
					response.Data = "Product has been deleted!";
				}
				else
				{
					response.Success = false;
					response.Message = "Product was not found";
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Adds user review and rating for this specific product
		public async Task<ServiceResponse<string>> AddProductReviewAndRating(AddProductReviewAndRatingDTO review)
		{
			var response = new ServiceResponse<string>();
			try
			{
				var product = await _db.Product.FindAsync(review.ProductId);
				if (product == null)
				{
					response.Success = false;
					response.Message = "Product not found!";
					return response;
				}
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users.Include(u => u.Products)
										  .Include(u => u.ProductReviews)
										  .Include(u => u.ProductRating)
										  .FirstOrDefaultAsync(u => u.Id == userId);

				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found!";
					return response;
				}
				// Check if the user has reached the rate limit for submitting rating and reviews
				var timeWindow = TimeSpan.FromHours(24);
				var userRatingCount = await _db.ProductRating
					.Include(r => r.User)
					.Include(r => r.Product)
					.CountAsync(r => r.User.Id == userId && r.Product.Id == review.ProductId &&
					EF.Functions.DateDiffHour(r.TimeStamp, DateTime.UtcNow) <= timeWindow.TotalHours);


				if (userRatingCount >= 2)
				{
					response.Success = false;
					response.Message = $"You have reached the maximum allowed submissions within {timeWindow.TotalHours} hours.";
					return response;
				}

				if (review.RatingValue < 1 || review.RatingValue > 5)
				{
					response.Success = false;
					response.Message = "Invalid rating value. Please provide a rating between 1 and 5.";
					return response;
				}

				// Record the new review and rating
				var productRating = new ProductRating { Product = product, RatingValue = review.RatingValue, TimeStamp = DateTime.UtcNow };
				user.ProductRating.Add(productRating);
				var productReview = new ProductReviews { Product = product, ReviewText = review.Review, DateStamp = DateTime.UtcNow, ProductRating = productRating };
				user.ProductReviews.Add(productReview);

				await _db.SaveChangesAsync();

				// Invalidate the cache for this product using its product ID
				if (_distributedCache != null)
				{
					string cacheKey = $"Product_{product.Id}";
					await _distributedCache.RemoveAsync(cacheKey);
				}
				response.Data = "Review and rating added successfully.";


			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;

			}
			return response;
		}


		// Gets All ratings and reviews for specific product
		public async Task<ServiceResponse<GetProductRatingAndReviewDTO>> GetProductRatingsAndReviews(int productId)
		{
			var response = new ServiceResponse<GetProductRatingAndReviewDTO>();
			try
			{
				var productReviews = await _db.ProductReviews
									 .Include(p => p.Product)
									 .Include(p => p.ProductRating)
									 .Include(p => p.User)
									 .Where(p => p.Product.Id == productId)
									 .OrderByDescending(p => p.Id)
									 .ToListAsync();


				var ratingAndReviewDto = new GetProductRatingAndReviewDTO();

				ratingAndReviewDto.ProductReviews = _mapper.Map<List<GetProductReviewsDTO>>(productReviews);

				response.Data = ratingAndReviewDto;

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}

			return response;
		}

		// Gets user added products from db 
		public async Task<ServiceResponse<List<GetProductsDTO>>> GetUserAddedProducts()
		{
			var response = new ServiceResponse<List<GetProductsDTO>>();

			try
			{
				//get user Id from token
				var userId = _tokenValidation.GetUserIdFromToken();
				var products = await _db.Product
							   .Include(pr => pr.Category)
							   .Include(pr => pr.SubCategories)
							   .Include(pr => pr.User)
							   .Where(pr => pr.User.Id == userId)
							   .OrderBy(c => c.Id)
							   .ToListAsync();
				response.Data = products.Select(p => _mapper.Map<GetProductsDTO>(p)).ToList();
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Restocks list of product

		public async Task<ServiceResponse<string>> RestockProducts(List<RestockProductDTO> restockProducts)
		{
			var response = new ServiceResponse<string>();
			try
			{
				var productIds = restockProducts.Select(item => item.ProductID).ToList();
				var productsToUpdate = _db.Product.Where(product => productIds.Contains(product.Id)).ToList();

				// Create a dictionary for quick product lookups using IDs
				var productDictionary = productsToUpdate.ToDictionary(p => p.Id);
				foreach (var restockProduct in restockProducts)
				{
					if (productDictionary.TryGetValue(restockProduct.ProductID, out var product))
					{
						// Update the product quantity
						product.Quantity = restockProduct.RestockQuantity;

						// Invalidate the cache for this product
						if (_distributedCache != null)
						{
							string cacheKey = $"Product_{restockProduct.ProductID}";
							await _distributedCache.RemoveAsync(cacheKey);
						}
					}
					else
					{
						response.Success = false;
						response.Message = $"The product with ID {restockProduct.ProductID} was not found.";
						return response;
					}
				}

				// Bulk update: Update all modified products in the database
				_db.Product.UpdateRange(productsToUpdate);
				await _db.SaveChangesAsync();

				//get userId from token;
				var userId = _tokenValidation.GetUserIdFromToken();

				// Log the restocking events to the audit trail
				foreach (var restockProduct in restockProducts)
				{
					await _auditService.LogRestock(restockProduct.ProductID, restockProduct.RestockQuantity, userId);
				}

				response.Data = "Products restocked successfully.";
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}

			return response;
		}

		// Gets all filter options(category,size etc) from database for filter feature
		public async Task<ServiceResponse<GetAllFilterOptionsDTO>> GetAllFilterOptions()
		{
			var response = new ServiceResponse<GetAllFilterOptionsDTO>();
			try
			{
				var categories = await _db.Category.ToListAsync();
				var subCategories = await _db.ProductSubCategories.ToListAsync();
				var genders = await _db.Gender.ToListAsync();
				var sizes = await _db.Sizes.ToListAsync();
				var brands = await _db.ProductBrand.ToListAsync();
				var colors = await _db.ProductColors.ToListAsync();

				var allFilterOptions = new GetAllFilterOptionsDTO
				{
					Categories = categories.Select(c => _mapper.Map<GetCategoryDTO>(c)).ToList(),
					SubCategories = subCategories.Select(s => _mapper.Map<GetSubCategoriesDTO>(s)).ToList(),
					Genders = genders.Select(g => _mapper.Map<GetGenderDTO>(g)).ToList(),
					Sizes = sizes.Select(s => _mapper.Map<GetProductSizeDTO>(s)).ToList(),
					Brands = brands.Select(b => _mapper.Map<GetBrandDTO>(b)).ToList(),
					Colors = colors.Select(c => _mapper.Map<GetColorDTO>(c)).ToList(),
					SortOptions = new List<GetProductSortDTO>(){
					new GetProductSortDTO {Id = 1, Name = "Whats New", },
					 new GetProductSortDTO {Id = 2, Name = "Price high to low"},
					 new GetProductSortDTO {Id = 3, Name = "Price low to high"}
				}
	
				};				

				response.Data = allFilterOptions;
				

			}catch(Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Get list of products based on filter data
		public async Task<ServiceResponse<PagedResult<GetProductsDTO>>> GetFilterData(GetFilterDataDTO filterInfo, int pageNumber = 1, int pageSize = 9)
		{
			var response = new ServiceResponse<PagedResult<GetProductsDTO>>();
			try
			{


				IQueryable<Product> query = _db.Product;
				query = ApplyFilter(query,filterInfo);

				// Include related entities
				query = query.Include(p => p.Images)
						 .Include(p => p.Category)
						 .Include(p => p.SubCategories)
						 .Include(p => p.ProductColor)
						 .Include(p => p.ProductBrand)
						 .Include(p => p.Sizes)
						 .Include(p => p.Gender);

				//For Pagination
				var totalProducts = await query.CountAsync();
				var pagedProducts = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
					.ToListAsync();

				// Order products
				query = ApplySorting(query, filterInfo);
				var productsDto = await query
	                 .Select(p => new GetProductsDTO
	                    {
		                 Id = p.Id,
						 Name = p.Name,
						 Price = p.Price,
						 Quantity = p.Quantity,
						 Category = _mapper.Map<GetCategoryDTO>(p.Category),
						 SubCategories = _mapper.Map<GetSubCategoriesDTO>(p.SubCategories),
						 Image = p.Images.FirstOrDefault() != null ? _mapper.Map<GetProductImageDTO>(p.Images.First()) : null
					 })
	                 .ToListAsync();


				// Create the PagedResult
				var pagedResult = new PagedResult<GetProductsDTO>
				{
					TotalCount = totalProducts,
					Items = productsDto
				};

				response.Data = pagedResult;				

			}
			catch(Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Helper method for GetFilterData function
		private IQueryable<Product> ApplyFilter(IQueryable<Product> query, GetFilterDataDTO filterInfo)
		{
			query = filterInfo.IsAllFiltersCleared ? query : query;

			query = filterInfo.CategoryId.HasValue ? query.Where(p => p.Category.Id == filterInfo.CategoryId.Value) : query;
			query = filterInfo.SubCategoryId.HasValue ? query.Where(p => p.SubCategories.Id == filterInfo.SubCategoryId.Value) : query;
			query = filterInfo.GenderId.HasValue ? query.Where(p => p.Gender.Id == filterInfo.GenderId.Value) : query;
			query = filterInfo.BrandId.HasValue ? query.Where(p => p.ProductBrand.Id == filterInfo.BrandId.Value) : query;
			query = filterInfo.ColorId.HasValue ? query.Where(p => p.ProductColor.Any(color => color.Id == filterInfo.ColorId.Value)) : query;
			query = filterInfo.SizeId.HasValue ? query.Where(p => p.Sizes.Any(size => size.SizeId == filterInfo.SizeId.Value)) : query;
			query = filterInfo.MinimumPrice.HasValue && filterInfo.MinimumPrice >= 0 ? query.Where(p => p.Price >= filterInfo.MinimumPrice.Value) : query;
			query = filterInfo.MaximumPrice.HasValue && filterInfo.MaximumPrice >= 0 ? query.Where(p => p.Price <= filterInfo.MaximumPrice.Value) : query;
			query = !string.IsNullOrEmpty(filterInfo.ProductName) ? query.Where(p => p.Name.Contains(filterInfo.ProductName)) : query;

			return query;
		}	

		// Helper method. Sorts list of product by filter specific

		private IQueryable<Product> ApplySorting(IQueryable<Product> query, GetFilterDataDTO filterInfo)
		{
			switch (filterInfo.SortId)
			{
				case 1:
					return query.OrderByDescending(c => c.Id);
				case 2:
					return query.OrderByDescending(c => c.Price);
				case 3:
					return query.OrderBy(c => c.Price);
				default:
					return query.OrderByDescending(c => c.Id);
			}
		}

	}
}
