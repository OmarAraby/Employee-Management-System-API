using EmployeeManagementSys.API.HandleFiles;
using EmployeeManagementSys.BL;
using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Register the DbContext 
builder.Services.AddDataAccessServices(builder.Configuration);
// Register the business services
builder.Services.AddBusinessServices();

builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


#region HandleFiles

// handle image upload
//if folder doesn't exist create it
var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
Directory.CreateDirectory(imageFolderPath);


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageFolderPath),
    RequestPath = "/api/static-files"
});
#endregion

app.MapControllers();


#region Seeding
// Seed data at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<Employee>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    // Seed roles
    string[] roles = { "Admin", "Employee" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
        }
    }

    // Seed initial admin
    var adminEmail = "admin@company.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new Employee
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            NationalId = "ADMIN123456789",
            Age = 30,
            CreatedDate = DateTime.UtcNow,
            Status = EmployeeStatus.Active,
            RequiresPasswordReset = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
#endregion

app.Run();
