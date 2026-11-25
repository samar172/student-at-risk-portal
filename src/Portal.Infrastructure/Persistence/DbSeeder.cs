using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Portal.Domain;
using Portal.Domain.Entities;
using Portal.Infrastructure.Identity;

namespace Portal.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>
    /// Applies migrations, creates the four roles, a default RiskConfig row and, only when
    /// Seed:AdminEmail and Seed:AdminPassword are configured (user-secrets or environment), an Administrator.
    /// No credentials are stored in source control.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        foreach (var role in Roles.All)
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));

        if (!await db.RiskConfigs.AnyAsync())
        {
            db.RiskConfigs.Add(new RiskConfig());
            await db.SaveChangesAsync();
        }

        var email = cfg["Seed:AdminEmail"];
        var password = cfg["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            log.LogInformation("Seed:AdminEmail/AdminPassword not set; skipping administrator creation.");
            return;
        }

        if (await users.FindByEmailAsync(email) is null)
        {
            var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = "Administrator" };
            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded) await users.AddToRoleAsync(admin, Roles.Administrator);
            else log.LogError("Could not create administrator: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
