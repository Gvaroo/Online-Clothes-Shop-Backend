using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineShopApp.Data;
using OnlineShopApp.Dtos.Cart;
using OnlineShopApp.Models;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Models.Cart;
using OnlineShopApp.Models.Product;
using OnlineShopApp.Services.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace OnlineShopApp.Services.Implementations
{
	public class CartService : ICartService
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IEmailSender _emailSender;
		private readonly IDistributedCache _distributedCache;
		private readonly ITokenValidationService _tokenValidation;
		public CartService(ApplicationDbContext db, IMapper mapper, IEmailSender emailSender, IDistributedCache distributedCache, ITokenValidationService tokenValidation)
		{
			_db = db;
			_mapper = mapper;
			_emailSender = emailSender;
			_distributedCache = distributedCache;
			_tokenValidation = tokenValidation;
		}

		//  Adds or updates products in the user's cart
		public async Task<ServiceResponse<string>> AddOrUpdateProductsToCart(List<CartDataDTO> cartData)
		{
			var response = new ServiceResponse<string>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await GetUserWithCartAsync(userId);

				if (user == null)
				{
					return CreateErrorResponse("User not found");
				}

				var existingCartItems = GetExistingCartItems(user);
				var cartItemsToAdd = await ValidateAndPrepareCartItems(cartData, existingCartItems, response);

				if (!response.Success)
				{
					return response;
				}

				UpdateUserCart(user, cartItemsToAdd);

				await _db.SaveChangesAsync();

				response.Data = "Product(s) were added successfully to the cart";
			}
			catch (Exception ex)
			{
				return HandleException(response, ex);
			}
			return response;
		}

		//  Processes the checkout of items in the user's cart
		public async Task<ServiceResponse<GetOrderDTO>> CheckOut(CheckoutDTO data)
		{
			var response = new ServiceResponse<GetOrderDTO>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await GetUserWithCartAndShippingAsync(userId);

				if (user == null || !IsCartValid(user.Cart, response))
				{
					return response;
				}

				var order = CreateOrder(user);
				UpdateShippingInfo(user, data.ShippingInfo);

				var orderItemsToAdd = new List<OrderItems>();
				var totalAmount = await ValidateAndPrepareOrderItems(user, orderItemsToAdd, response);

				if (!response.Success)
				{
					return response;
				}

				order.OrderItems = orderItemsToAdd;
				order.TotalAmount = totalAmount;

				await ProcessOrderTransaction(user, order, orderItemsToAdd, data, response);
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		public async Task<ServiceResponse<GetOrderDTO>> GetOrderedProducts(int orderId)
		{

			var response = new ServiceResponse<GetOrderDTO>();
			try
			{
				//get userId from token
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
							.Include(c => c.Order)
							.ThenInclude(c => c.OrderItems)			
						.FirstOrDefaultAsync(c => c.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
					return response;
				}
				var order = user.Order.FirstOrDefault(o => o.OrderId == orderId);

				if (order == null)
				{
					response.Success = false;
					response.Message = $"Order with id {orderId} not found for the user";
					return response;
				}
				var getOrder = new GetOrderDTO
				{
					OrderId = orderId,
					OrderDate = order.OrderDate.ToString(),
					TotalPrice = order.TotalAmount,
					Products = order.OrderItems.Select(o => new ProductDTO
					{
						ProductId = o.ProductId,
						Name = o.Name,
						Image = o.Image,
						Price = o.OriginalPrice,
						quantity = o.Quantity,
						MaxPrice = o.TotalPrice


					}).ToList(),
				};
				response.Data = getOrder;
			}
			catch (Exception ex)
			{
				response.Success= false;
				response.Message= ex.Message;
			}
			return response;
			
		}
				

		public async Task<ServiceResponse<GetShippingInfoDTO>> GetShippingInfo()
		{
			var response = new ServiceResponse<GetShippingInfoDTO>();
			try
			{
				//get userId from token
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
							   .Include(u => u.ShippingInfo)
							   .FirstOrDefaultAsync(u => u.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
				}
				response.Data = _mapper.Map<GetShippingInfoDTO>(user.ShippingInfo);
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		//Retrieves the current user's cart data and returns it as a list of ProductDTO objects.
		public async Task<ServiceResponse<List<ProductDTO>>> GetUserCartData()
		{
			var response = new ServiceResponse<List<ProductDTO>>();

			try
			{
				//get userId from token
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
					.Include(c => c.Cart)
					.Include(c => c.Cart.CartItem)
					.FirstOrDefaultAsync(c => c.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
					return response;
				}
				//Get user cartData				
				var cartItems = await _db.CartItems
					.Include(c => c.Product)
					.ThenInclude(c => c.Images)
					.Include(c => c.Product.Sizes)									
					.Where(c => c.Cart.CartId == user.Cart.CartId)
					.ToListAsync();

				//cartData to return in response
				var cartData = new List<ProductDTO>();
				//if cart is null
				if (cartItems == null)
				{
					response.Data = cartData;
					return response;
				}

				foreach (var item in cartItems)
				{
					var product = await _db.Product
							   .FirstOrDefaultAsync(c => c.Id == item.Product.Id);					
					var mappedProduct = _mapper.Map<ProductDTO>(product);

					//add main product image
					mappedProduct.Image = item.Product.Images[0].ImageUrl;

					//add correct quantity
					mappedProduct.quantity = item.Quantity;

					//add product size
					mappedProduct.SizeId = item.SizeId;
					mappedProduct.ProductSize = item.Product.Sizes.Find(s=> s.SizeId == item.SizeId).SizeName;


					cartData.Add(mappedProduct);
				}

				response.Data = cartData;

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		public async Task<ServiceResponse<List<GetOrderHistoryDTO>>> GetUserOrderHistory()
		{
			var response = new ServiceResponse<List<GetOrderHistoryDTO>>();
			try
			{
				//get userid from token
				var userid = _tokenValidation.GetUserIdFromToken();
				var orders = await _db.Order
								  .Include(or => or.User)
								  .Include(or => or.OrderItems)
								  .Where(or => or.User.Id == userid)
								  .OrderByDescending(or => or.OrderId)
								  .ToListAsync();
				response.Data = orders.SelectMany(o => o.OrderItems.Select(item => new GetOrderHistoryDTO
				{
					OrderId = o.OrderId,
					Name = item.Name,
					Quantity = item.Quantity,
					OriginalPrice= item.OriginalPrice,
					TotalPrice= item.TotalPrice,
					ProductSize = item.ProductSize,
					OrderDate = o.OrderDate.ToShortDateString()
				})).ToList();

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Removes or minimizes added products from cart
		public async Task<ServiceResponse<string>> RemoveProductFromCart(CartDataDTO cartData)
		{
			var response = new ServiceResponse<string>();

			try
			{
				//get userId from token
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
					.Include(c => c.Cart)
					 .ThenInclude(cart => cart.CartItem) // Include CartItem for Cart
					 .ThenInclude(cartItem => cartItem.Product) // Include Product for CartItem
					.FirstOrDefaultAsync(c => c.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
					return response;
				}
				//check product in db
				var product = await _db.Product.FindAsync(cartData.ProductId);

				if (product == null)
				{
					response.Success = false;
					response.Message = $"Product with ID {product.Id} was not found on web.It's either was removed or was sold out!";
					return response;
				}
				else if (product.Quantity < cartData.Quantity)
				{
					response.Success = false;
					response.Message = $"Product with ID {product.Id} quantity isnt enough for your quantity.Please minimazite it and try again";
					return response;
				}

				//check cartItem
				var cartItem = user.Cart.CartItem.Find(c => c.Product.Id == cartData.ProductId && c.SizeId == cartData.SizeId);
				if (cartItem == null)
				{
					response.Success = false;
					response.Message = $"Product with ID {product.Id} was not found. Please reload page and try adding same product again";
					return response;
				}

				//minimazie quantity
				cartItem.Quantity -= cartData.Quantity;

				//Delete it if its lower or same as 0
				if (cartItem.Quantity <= 0)
					user.Cart.CartItem.Remove(cartItem);

				await _db.SaveChangesAsync();

				response.Data = "Product was removed successfuly";
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}


		// Private helper methods
		private async Task<User> GetUserWithCartAsync(int userId)
		{
			return await _db.Users
							.Include(c => c.Cart)
							.ThenInclude(cart => cart.CartItem)
							.ThenInclude(cartItem => cartItem.Product)
							.FirstOrDefaultAsync(c => c.Id == userId);
		}

		private Dictionary<CartItemKey, CartItem> GetExistingCartItems(User user)
		{
			return user.Cart?.CartItem?.ToDictionary(
				c => new CartItemKey { ProductId = c.Product.Id, SizeId = c.SizeId },
				c => c) ?? new Dictionary<CartItemKey, CartItem>();
		}

		private async Task<List<CartItem>> ValidateAndPrepareCartItems(List<CartDataDTO> cartData, Dictionary<CartItemKey, CartItem> existingCartItems, ServiceResponse<string> response)
		{
			var cartItemsToAdd = new List<CartItem>();
			var guest = cartData.Any(item => item.Guest == true);

			foreach (var item in cartData)
			{
				var product = await _db.Product.Include(p => p.Sizes).FirstOrDefaultAsync(p => p.Id == item.ProductId);
				if (!IsValidProduct(product, item, response))
				{
					return null;
				}

				var key = new CartItemKey { ProductId = item.ProductId, SizeId = item.SizeId };
				var existingItem = existingCartItems.ContainsKey(key) ? existingCartItems[key] : null;

				if (existingItem != null && existingItem.SizeId == item.SizeId)
				{
					existingItem.Quantity = guest ? existingItem.Quantity + item.Quantity : item.Quantity;
				}
				else
				{
					cartItemsToAdd.Add(new CartItem { Product = product, Quantity = item.Quantity, SizeId = item.SizeId });
				}
			}
			return cartItemsToAdd;
		}

		private bool IsValidProduct(Product product, CartDataDTO item, ServiceResponse<string> response)
		{
			if (product == null)
			{
				response.Success = false;
				response.Message = $"Product with ID {item.ProductId} was not found.";
				return false;
			}
			else if (product.Quantity < item.Quantity)
			{
				response.Success = false;
				response.Message = $"Insufficient quantity for product ID {item.ProductId}.";
				return false;
			}
			else if (!product.Sizes.Any(s => s.SizeId == item.SizeId))
			{
				response.Success = false;
				response.Message = $"Size not found for product ID {item.ProductId}.";
				return false;
			}
			return true;
		}

		private void UpdateUserCart(User user, List<CartItem> cartItemsToAdd)
		{
			if (user.Cart == null)
			{
				user.Cart = new Cart { User = user, CartItem = cartItemsToAdd };
				_db.Cart.Add(user.Cart);
			}
			else
			{
				user.Cart.CartItem.AddRange(cartItemsToAdd);
			}
		}

	

		private async Task<User> GetUserWithCartAndShippingAsync(int userId)
		{
			return await _db.Users
							.Include(c => c.ShippingInfo)
							.Include(c => c.Cart)
							.ThenInclude(c => c.CartItem)
							.ThenInclude(c => c.Product)
							.FirstOrDefaultAsync(c => c.Id == userId);
		}

		private bool IsCartValid(Cart cart, ServiceResponse<GetOrderDTO> response)
		{
			if (cart?.CartItem == null || !cart.CartItem.Any())
			{
				response.Success = false;
				response.Message = "The cart is empty. Add items to the cart before proceeding.";
				return false;
			}
			return true;
		}

		private Order CreateOrder(User user)
		{
			return new Order
			{
				OrderDate = DateTime.UtcNow,
				User = user
			};
		}

		private void UpdateShippingInfo(User user, GetShippingInfoDTO shippingInfoDto)
		{
			if (user.ShippingInfo != null)
			{
				_mapper.Map(shippingInfoDto, user.ShippingInfo);
			}
			else
			{
				user.ShippingInfo = _mapper.Map<ShippingInfo>(shippingInfoDto);
			}
		}

		private async Task<decimal> ValidateAndPrepareOrderItems(User user, List<OrderItems> orderItemsToAdd, ServiceResponse<GetOrderDTO> response)
		{
			decimal totalAmount = 0;
			var productIds = user.Cart.CartItem.Select(c => c.Product.Id).ToList();
			var productsInDb = await _db.Product
										.Include(c => c.Images)
										.Include(c => c.Sizes)
										.Where(c => productIds.Contains(c.Id))
										.ToListAsync();

			var tasks = user.Cart.CartItem.Select(async item =>
			{
				var productInDb = productsInDb.FirstOrDefault(p => p.Id == item.Product.Id);

				if (productInDb != null && ValidateOrderItemQuantity(item, productInDb, response))
				{
					var orderProduct = PrepareOrderItem(item, productInDb, orderItemsToAdd);
					totalAmount += orderProduct.TotalPrice;
					productInDb.Quantity -= item.Quantity;
					if(_distributedCache !=null)
					await _distributedCache.RemoveAsync($"Product_{item.Product.Id}");
				}
				else
				{
					response.Success = false;
					response.Message = $"Product with ID {item.Product.Id} was not found or invalid.";
				}
			});

			await Task.WhenAll(tasks);

			return totalAmount;
		}

		private bool ValidateOrderItemQuantity(CartItem item, Product productInDb, ServiceResponse<GetOrderDTO> response)
		{
			if (item.Quantity <= 0 || item.Quantity > productInDb.Quantity)
			{
				response.Success = false;
				response.Message = $"Invalid quantity for product with ID {item.Product.Id}.";
				return false;
			}
			return true;
		}

		private OrderItems PrepareOrderItem(CartItem item, Product productInDb, List<OrderItems> orderItemsToAdd)
		{
			var orderProduct = _mapper.Map<OrderItems>(item.Product);
			orderProduct.Image = item.Product.Images[0].ImageUrl;
			orderProduct.Id = 0;
			orderProduct.TotalPrice = item.Quantity * productInDb.Price;
			orderProduct.Quantity = item.Quantity;
			orderProduct.Order = orderItemsToAdd.FirstOrDefault()?.Order;
			orderProduct.ProductSize = productInDb.Sizes.FirstOrDefault(s => s.SizeId == item.SizeId)?.SizeName;
			orderItemsToAdd.Add(orderProduct);

			return orderProduct;
		}

		private async Task ProcessOrderTransaction(User user, Order order, List<OrderItems> orderItemsToAdd, CheckoutDTO data, ServiceResponse<GetOrderDTO> response)
		{
			using var transaction = await _db.Database.BeginTransactionAsync();
			try
			{
				_db.Order.Add(order);
				user.Cart.CartItem.Clear();
				await _db.SaveChangesAsync();
				await transaction.CommitAsync();
				await SendOrderConfirmationEmail(user, order, orderItemsToAdd, data);

				var orderedProducts = PrepareOrderedProducts(orderItemsToAdd);
				response.Data = new GetOrderDTO { OrderId = order.OrderId, Products = orderedProducts };
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				response.Success = false;
				response.Message = "An error occurred while processing the order. Please try again.";
			}
		}

		private List<ProductDTO> PrepareOrderedProducts(List<OrderItems> orderItemsToAdd)
		{
			return orderItemsToAdd.Select(item =>
			{
				//var product = _mapper.Map<ProductDTO>(item);
				var product = new ProductDTO
				{
					ProductId = item.ProductId,
					Name = item.Name,
					Image = item.Image,
					Price = item.OriginalPrice,
					MaxPrice = item.TotalPrice,
					quantity = item.Quantity,
				    ProductSize = item.ProductSize
			};
				
				return product;
			}).ToList();
		}
		private ServiceResponse<string> CreateErrorResponse(string message)
		{
			return new ServiceResponse<string> { Success = false, Message = message };
		}

		private ServiceResponse<string> HandleException(ServiceResponse<string> response, Exception ex)
		{
			response.Success = false;
			response.Message = ex.Message;
			return response;
		}

		private async Task SendOrderConfirmationEmail(User user, Order order, List<OrderItems> orderedProducts, CheckoutDTO info)
		{
			// Read the email template file
			string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "OrderConfirmationTemplate.html");
			string emailTemplate = await File.ReadAllTextAsync(templatePath);

			// Replace the placeholders with actual data using string interpolation
			emailTemplate = emailTemplate.Replace("{{FirstName}}", user.FullName);
			emailTemplate = emailTemplate.Replace("{{OrderDate}}", order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"));
			emailTemplate = emailTemplate.Replace("{{ShippingAddress}}", info.ShippingInfo.ShippingAddress);


			// Replace the ordered products information using string interpolation
			StringBuilder orderedProductsBuilder = new StringBuilder();
			foreach (var item in orderedProducts)
			{
				orderedProductsBuilder.AppendLine("<div class=\"order-details\">");
				orderedProductsBuilder.AppendLine($"<p class=\"product-name\">- Product Name: {item.Name}</p>");
				orderedProductsBuilder.AppendLine($"<p>  Quantity: {item.Quantity}</p>");
				orderedProductsBuilder.AppendLine($"<p>  Size: {item.ProductSize}</p>");
				orderedProductsBuilder.AppendLine($"<p>  Price: ${item.OriginalPrice}</p>");
				orderedProductsBuilder.AppendLine($"<p>  Total: ${item.OriginalPrice * item.Quantity}</p>");
				orderedProductsBuilder.AppendLine("</div>");
			}

			emailTemplate = emailTemplate.Replace("{{OrderedProducts}}", orderedProductsBuilder.ToString());
			emailTemplate = emailTemplate.Replace("{{TotalAmount}}", order.TotalAmount.ToString());

			// Send email for order confirmation
			await _emailSender.SendEmailAsyncBackground(user.Email, "Order Confirmation - Order #" + order.OrderId.ToString(), emailTemplate);
		}
	}
}
