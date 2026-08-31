using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Data;

public class MessDbContext : DbContext
{
    public MessDbContext(DbContextOptions<MessDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }

    public DbSet<Mess> Messes { get; set; }

    public DbSet<MessJoinRequest> MessJoinRequests { get; set; }

    public DbSet<Meal> Meals { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<MemberPayment> MemberPayments { get; set; }
    public DbSet<MemberCashTransfer> MemberCashTransfers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Member belongs to one Mess
        // One Mess has many Members
        modelBuilder.Entity<Member>()
            .HasOne(m => m.Mess)
            .WithMany(m => m.Members)
            .HasForeignKey(m => m.MessId)
            .OnDelete(DeleteBehavior.SetNull);

        // Mess has one AdminMember
        // Member has one AdminMess
        modelBuilder.Entity<Mess>()
            .HasOne(m => m.AdminMember)
            .WithOne(m => m.AdminMess)
            .HasForeignKey<Mess>(m => m.AdminMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // MessCode must be unique
        modelBuilder.Entity<Mess>()
            .HasIndex(m => m.MessCode)
            .IsUnique();

        // A member can administer only one mess
        modelBuilder.Entity<Mess>()
            .HasIndex(m => m.AdminMemberId)
            .IsUnique();

        // Mess → Join Requests
        modelBuilder.Entity<MessJoinRequest>()
            .HasOne(r => r.Mess)
            .WithMany()
            .HasForeignKey(r => r.MessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member → Join Requests
        modelBuilder.Entity<MessJoinRequest>()
            .HasOne(r => r.Member)
            .WithMany()
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member → Meals
        modelBuilder.Entity<Meal>()
            .HasOne(m => m.Member)
            .WithMany()
            .HasForeignKey(m => m.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // One meal record per member per date
        modelBuilder.Entity<Meal>()
            .HasIndex(m => new
            {
                m.MemberId,
                m.MealDate
            })
            .IsUnique();


        // Mess → Expenses
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Mess)
            .WithMany(m => m.Expenses)
            .HasForeignKey(e => e.MessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member → Recorded Expenses
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.RecordedByMember)
            .WithMany(m => m.RecordedExpenses)
            .HasForeignKey(e => e.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);


        // Mess → Member Payments
        modelBuilder.Entity<MemberPayment>()
            .HasOne(p => p.Mess)
            .WithMany(m => m.MemberPayments)
            .HasForeignKey(p => p.MessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member → Payments made by the member
        modelBuilder.Entity<MemberPayment>()
            .HasOne(p => p.Member)
            .WithMany(m => m.Payments)
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Member → Payments recorded by admin
        modelBuilder.Entity<MemberPayment>()
            .HasOne(p => p.RecordedByMember)
            .WithMany(m => m.RecordedPayments)
            .HasForeignKey(p => p.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);



        // Mess → Member Cash Transfers
        modelBuilder.Entity<MemberCashTransfer>()
            .HasOne(t => t.Mess)
            .WithMany(m => m.MemberCashTransfers)
            .HasForeignKey(t => t.MessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member → Cash Transfers received
        modelBuilder.Entity<MemberCashTransfer>()
            .HasOne(t => t.Member)
            .WithMany(m => m.CashTransfers)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Member → Cash Transfers recorded by admin
        modelBuilder.Entity<MemberCashTransfer>()
            .HasOne(t => t.RecordedByMember)
            .WithMany(m => m.RecordedCashTransfers)
            .HasForeignKey(t => t.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);


        // Expense → Member
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);



    }
}