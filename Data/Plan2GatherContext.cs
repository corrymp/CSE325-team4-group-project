using Microsoft.EntityFrameworkCore;
using Plan2Gather.Models;
namespace Plan2Gather.Data;

public class Plan2GatherContext(DbContextOptions<Plan2GatherContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Console.WriteLine("TODO(Corry) => Remember to remove this (Plan2GatherContext.cs:9) before publishing - only for debug use!");
        optionsBuilder.EnableSensitiveDataLogging();
    }
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Event> Events { get; set; } = default!;
    public DbSet<Guest> Guests { get; set; } = default!;
    public DbSet<Availability> Availabilities { get; set; } = default!;
}