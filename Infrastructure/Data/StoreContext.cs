using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StoreContext:IdentityDbContext<AppUser,AppRole,string>
{
    public StoreContext(DbContextOptions<StoreContext> options):base(options)
    {
        
    }
}
