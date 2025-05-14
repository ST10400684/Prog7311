using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Prog.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<AgriEnergyConnectContext>(options =>
   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Make sure the database is created and initialize sample data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AgriEnergyConnectContext>();
    dbContext.Database.EnsureCreated();

    // Seed initial data if database is empty
    if (!dbContext.Users.Any())
    {
        // Add some initial categories
        var categories = new List<ProductCategory>
        {
            new ProductCategory { CategoryName = "Fruits", Description = "Fresh fruits from the farm" },
            new ProductCategory { CategoryName = "Vegetables", Description = "Fresh vegetables from the farm" },
            new ProductCategory { CategoryName = "Dairy", Description = "Fresh dairy products" },
            new ProductCategory { CategoryName = "Grains", Description = "Various grains and cereals" }
        };
        dbContext.ProductCategories.AddRange(categories);
        dbContext.SaveChanges();

        // Add a sample employee user
        var employee = new User
        {
            Username = "employee",
            PasswordHash = "1000:D2mAXMvOo5RLzhZ3xL5AmjUfbvt/uQm3:4zPVQiZ+XuR5QwnKNfnP9v0pfaI=", // Password: employee123
            Salt = "D2mAXMvOo5RLzhZ3xL5AmjUfbvt/uQm3",
            Email = "employee@agrienergyconnect.com",
            UserRole = "Employee",
            CreatedDate = DateTime.Now
        };
        dbContext.Users.Add(employee);

        // Add a sample farmer user and profile
        var farmerUser = new User
        {
            Username = "farmer",
            PasswordHash = "1000:SuZ6qQHYTLzMZekYpLg0HswR6kH+9oZj:QDcM4GRxvSQV/NAR6TA/mIlKNuM=", // Password: farmer123
            Salt = "SuZ6qQHYTLzMZekYpLg0HswR6kH+9oZj",
            Email = "farmer@agrienergyconnect.com",
            UserRole = "Farmer",
            CreatedDate = DateTime.Now
        };
        dbContext.Users.Add(farmerUser);
        dbContext.SaveChanges();

        // Create farmer profile
        var farmer = new Farmer
        {
            UserId = farmerUser.UserId,
            FarmerName = "John Smith",
            FarmName = "Green Valley Farm",
            Location = "Western Cape",
            ContactNumber = "0812345678",
            RegistrationDate = DateTime.Now
        };
        dbContext.Farmers.Add(farmer);
        dbContext.SaveChanges();

        // Add sample products
        var products = new List<Product>
        {
            new Product
            {
                FarmerId = farmer.FarmerId,
                CategoryId = 1, // Fruits
                ProductName = "Organic Apples",
                Description = "Fresh organic apples from Green Valley",
                ProductionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                CreatedDate = DateTime.Now
            },
            new Product
            {
                FarmerId = farmer.FarmerId,
                CategoryId = 2, // Vegetables
                ProductName = "Organic Carrots",
                Description = "Fresh organic carrots from Green Valley",
                ProductionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                CreatedDate = DateTime.Now
            }
        };
        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();
    }
}

app.Run();