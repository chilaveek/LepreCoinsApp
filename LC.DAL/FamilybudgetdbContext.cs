using Microsoft.EntityFrameworkCore;

namespace LepreCoins.Models;

public partial class FamilybudgetdbContext : DbContext
{
    public FamilybudgetdbContext(DbContextOptions<FamilybudgetdbContext> options)
        : base(options)
    {
    }

    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<IncomeCategory> IncomeCategories => Set<IncomeCategory>();
    public DbSet<Saving> Savings => Set<Saving>();
    public DbSet<TransferToSaving> TransferToSavings => Set<TransferToSaving>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== BUDGET =====
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("budget");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EstablishedAmount)
                .HasPrecision(19);

            entity.Property(e => e.CurrentExpenses)
                .HasPrecision(19);
        });

        // ===== EXPENSE =====
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("expense");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Sum)
                .HasPrecision(19);

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.Categoryid);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.Userid);

            entity.HasOne(e => e.Wallet)
                .WithMany(w => w.Expenses)
                .HasForeignKey(e => e.Walletid);
        });

        // ===== EXPENSE CATEGORY =====
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.ToTable("expense_category");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasMaxLength(255);

            entity.Property(e => e.CategoryType)
                .HasMaxLength(255);

            entity.HasOne(e => e.Budget)
                .WithMany(b => b.ExpenseCategories)
                .HasForeignKey(e => e.Budgetid);
        });

        // ===== FAMILY =====
        modelBuilder.Entity<Family>(entity =>
        {
            entity.ToTable("family");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasMaxLength(255);
        });

        // ===== INCOME =====
        modelBuilder.Entity<Income>(entity =>
        {
            entity.ToTable("income");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Sum)
                .HasPrecision(19);

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.HasOne(e => e.Incomecategory)
                .WithMany(c => c.Incomes)
                .HasForeignKey(e => e.Incomecategoryid);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Incomes)
                .HasForeignKey(e => e.Userid);

            entity.HasOne(e => e.Wallet)
                .WithMany(w => w.Incomes)
                .HasForeignKey(e => e.Walletid);
        });

        // ===== INCOME CATEGORY =====
        modelBuilder.Entity<IncomeCategory>(entity =>
        {
            entity.ToTable("income_category");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Category)
                .HasMaxLength(255);
        });

        // ===== SAVING =====
        modelBuilder.Entity<Saving>(entity =>
        {
            entity.ToTable("saving");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.GoalName)
                .HasMaxLength(255);

            entity.Property(e => e.TargetAmount)
                .HasPrecision(19);

            entity.Property(e => e.CurrentAmount)
                .HasPrecision(19);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Savings)
                .HasForeignKey(e => e.Userid);
        });

        // ===== TRANSFER TO SAVING =====
        modelBuilder.Entity<TransferToSaving>(entity =>
        {
            entity.ToTable("transfer_to_saving");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Sum)
                .HasPrecision(19);

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.HasOne(e => e.Savings)
                .WithMany(s => s.TransferToSavings)
                .HasForeignKey(e => e.Savingsid);

            entity.HasOne(e => e.User)
                .WithMany(u => u.TransferToSavings)
                .HasForeignKey(e => e.Userid);
        });

        // ===== USER =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255);

            entity.HasOne(e => e.Family)
                .WithMany(f => f.Users)
                .HasForeignKey(e => e.Familyid);

            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Users)
                .HasForeignKey(e => e.Budgetid);
        });

        // ===== WALLET =====
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallet");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasMaxLength(255);

            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("RUB");

            entity.Property(e => e.Balance)
                .HasPrecision(19)
                .HasDefaultValue(0m);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(e => e.Userid);
        });
    }
}
