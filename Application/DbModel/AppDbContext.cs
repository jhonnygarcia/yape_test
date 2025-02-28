using Application.DbModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.DbModel
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SourceAccount)
                .WithMany() 
                .HasForeignKey(t => t.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.TargetAccount)
                .WithMany()
                .HasForeignKey(t => t.TargetAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Value)
                .HasPrecision(18, 2); // Ensure proper decimal precision

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>(); // Store enum as string

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransferType)
                .HasConversion<string>(); // Store enum as string

            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);            

            modelBuilder.Entity<Account>().HasData(
               new Account
               {
                   Id = new Guid("59d18f47-3036-4bbb-9a60-f023e98ea54d"),
                   Name = "Account 1",
                   Balance = 100000
               },
               new Account
               {
                   Id = new Guid("0f8088c8-e7b7-40ea-af58-266c79a6a181"),
                   Name = "Account 2",
                   Balance = 100000
               }
            );
        }
    }
}
