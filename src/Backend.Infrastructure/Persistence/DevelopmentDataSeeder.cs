using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsRelational())
            await dbContext.Database.MigrateAsync(cancellationToken);
        else
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var accounts = new[]
        {
            new { FullName = "Admin User", Email = "admin@minijira.com", Password = "Admin@123", Role = UserRole.Admin },
            new { FullName = "Project Manager", Email = "manager@minijira.com", Password = "Manager@123", Role = UserRole.ProjectManager },
            new { FullName = "Employee User", Email = "employee@minijira.com", Password = "Employee@123", Role = UserRole.Employee }
        };

        foreach (var account in accounts)
        {
            var exists = await dbContext.Users.AnyAsync(user => user.Email == account.Email, cancellationToken);
            if (exists) continue;

            dbContext.Users.Add(new User(
                account.FullName,
                account.Email,
                passwordHasher.HashPassword(account.Password),
                account.Role));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
