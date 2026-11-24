using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.DBInitializer;

public class DBInitializer : IDBInitializer
{
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public DBInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            if ((await _db.Database.GetPendingMigrationsAsync()).Any())
            {
                await _db.Database.MigrateAsync();
            }
        }
        catch (Exception e)
        {
            throw;
        }

        if (!await _roleManager.RoleExistsAsync(SD.RoleCustomer))
        {
            await _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomer));
            await _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin));
            await _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee));
        }

        ApplicationUser user = await _userManager.FindByEmailAsync("admin@bb.com");
        if (user == null)
        {
            var result = await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@bb.com",
                Email = "admin@bb.com",
                EmailConfirmed = true,
                Name = "admin",
                PhoneNumber = "55588888888",
                StreetAddress = "123 Main Street",
                State = "WA",
                PostalCode = "0101",
                City = "London",
            }, "Admin123*");

            if (result.Succeeded)
            {
                user = await _userManager.FindByEmailAsync("admin@bb.com");
                await _userManager.AddToRoleAsync(user, SD.RoleAdmin);
            }
        }
    }
}