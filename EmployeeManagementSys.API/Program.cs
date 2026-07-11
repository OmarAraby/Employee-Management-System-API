using EmployeeManagementSys.API.Email;
using EmployeeManagementSys.API.HandleFiles;
using EmployeeManagementSys.BL;
using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register the DbContext 
builder.Services.AddDataAccessServices(builder.Configuration);
// Register the business services
builder.Services.AddBusinessServices();

builder.Services.AddScoped<IFileService, FileService>();

// Email: bind the "Email" config section (env Email__*) and register the sender.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

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

    // Apply pending EF migrations before seeding — containers start against an
    // empty SQL Server, so the schema must exist before roles/admin are created.
    var dbContext = services.GetRequiredService<EmployeeManagementSysDbContext>();
    await dbContext.Database.MigrateAsync();

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

    // Seed initial admin.
    // The seed password comes from configuration (Seed:AdminPassword — supply via
    // env var SEED__ADMINPASSWORD or user-secrets). No hardcoded credential ships
    // in source. If it's unset, admin seeding is SKIPPED with a warning rather
    // than falling back to a well-known default (the vulnerability this fixes).
    var adminEmail = "admin@company.com";
    var seedAdminPassword = app.Configuration["Seed:AdminPassword"];
    if (string.IsNullOrWhiteSpace(seedAdminPassword))
    {
        app.Logger.LogWarning(
            "Seed:AdminPassword is not configured — skipping initial admin seeding. " +
            "Set SEED__ADMINPASSWORD (env) or the Seed:AdminPassword user-secret to seed an admin.");
    }
    else if (await userManager.FindByEmailAsync(adminEmail) == null)
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
        var result = await userManager.CreateAsync(admin, seedAdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            app.Logger.LogError(
                "Failed to seed initial admin: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
#endregion

app.Run();
