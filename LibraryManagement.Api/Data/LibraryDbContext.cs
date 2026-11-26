using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasIndex(b => b.ISBN).IsUnique();
        modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany()
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Member)
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    
    }
}