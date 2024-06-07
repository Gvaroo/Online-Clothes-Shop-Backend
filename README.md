# About App

This is an online shop backend built with ASP.NET Core. Users can authenticate within the app, view products, search by various filter categories, leave reviews for products, add items to the cart, and make changes such as adjusting sizes, quantities, or deleting items. Users can also complete the checkout process using a credit card. Additionally, users can view detailed transaction history and receive email notifications about various features and order confirmations. The app supports external authentication with Google. Users can update their profile, including changing their email, password, and name.

Admins have additional capabilities, such as adding new products and restocking existing products, to ensure the shop remains up-to-date with inventory.

# Language
- C#

# Architecture
- Repository Pattern 
- SOLID

# Features
- OAuth2 and JWT Authentication
- HTTP Only Cookies for JWT Tokens
- Entity Framework Core
- Cloudinary for Image Uploads
- Hangfire for Background Tasks
- AutoMapper for Object-Object Mapping
- BCrypt for Password Hashing
- FluentValidation for Input Validation
- Google APIs for External Authentication
- Redis for Caching

# Installation
1. **Clone the repository:**
2. Update SQL Configuration:
Change the SQL configuration in appsettings.json (download and set up my [SQL Database](https://drive.google.com/uc?export=download&id=16y2Xs0gOq5cWaVY440sDV5HZHhVP0OFG)).
3. **Caching Configuration:**
The application supports caching with Redis. If Redis is not installed or not running, the application will fall back to using in-memory caching. No additional configuration is required to handle this fallback.
4. Run the API

# Resources
- [SQL Database](https://drive.google.com/uc?export=download&id=16y2Xs0gOq5cWaVY440sDV5HZHhVP0OFG)



Client-side of this project can be found on my [GitHub](https://github.com/Gvaroo/Online-Clothes-Shop-Frontend).
