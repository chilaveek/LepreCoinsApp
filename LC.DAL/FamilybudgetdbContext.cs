using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LepreCoins.Models;

public partial class FamilybudgetdbContext : DbContext
{
    public FamilybudgetdbContext()
    {
    }

    public FamilybudgetdbContext(DbContextOptions<FamilybudgetdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    public virtual DbSet<Family> Families { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<IncomeCategory> IncomeCategories { get; set; }

    public virtual DbSet<Saving> Savings { get; set; }

    public virtual DbSet<TransferToSaving> TransferToSavings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=familybudgetdb;User Id=postgres;Password=12345;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("budget_pkey");

            entity.ToTable("Budget");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('budget_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CurrentExpenses)
                .HasPrecision(19)
                .HasColumnName("current_expenses");
            entity.Property(e => e.EstablishedAmount)
                .HasPrecision(19)
                .HasColumnName("established_amount");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("expense_pkey");

            entity.ToTable("Expense");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('expense_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.ExpenseType)
                .HasMaxLength(255)
                .HasColumnName("expense_type");
            entity.Property(e => e.Sum)
                .HasPrecision(19)
                .HasColumnName("sum");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Walletid).HasColumnName("walletid");

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.Categoryid)
                .HasConstraintName("expense_categoryid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("expense_userid_fkey");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.Walletid)
                .HasConstraintName("expense_walletid_fkey");
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("expensecategory_pkey");

            entity.ToTable("ExpenseCategory");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('expensecategory_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Attribute)
                .HasMaxLength(255)
                .HasColumnName("attribute");
            entity.Property(e => e.Budgetid).HasColumnName("budgetid");
            entity.Property(e => e.CategoryType)
                .HasMaxLength(255)
                .HasColumnName("category_type");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Budget).WithMany(p => p.ExpenseCategories)
                .HasForeignKey(d => d.Budgetid)
                .HasConstraintName("expensecategory_budgetid_fkey");
        });

        modelBuilder.Entity<Family>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("family_pkey");

            entity.ToTable("Family");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('family_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("income_pkey");

            entity.ToTable("Income");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('income_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Incomecategoryid).HasColumnName("incomecategoryid");
            entity.Property(e => e.Sum)
                .HasPrecision(19)
                .HasColumnName("sum");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Walletid).HasColumnName("walletid");

            entity.HasOne(d => d.Incomecategory).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.Incomecategoryid)
                .HasConstraintName("income_incomecategoryid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("income_userid_fkey");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.Walletid)
                .HasConstraintName("income_walletid_fkey");
        });

        modelBuilder.Entity<IncomeCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("incomecategory_pkey");

            entity.ToTable("IncomeCategory");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('incomecategory_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
        });

        modelBuilder.Entity<Saving>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("savings_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('savings_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.CurrentAmount)
                .HasPrecision(19)
                .HasColumnName("current_amount");
            entity.Property(e => e.GoalName)
                .HasMaxLength(255)
                .HasColumnName("goal_name");
            entity.Property(e => e.ProgressPercent).HasColumnName("progress_percent");
            entity.Property(e => e.TargetAmount)
                .HasPrecision(19)
                .HasColumnName("target_amount");
            entity.Property(e => e.TargetDate).HasColumnName("target_date");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Savings)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("savings_userid_fkey");
        });

        modelBuilder.Entity<TransferToSaving>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transfertosavings_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('transfertosavings_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Isdeposit).HasColumnName("isdeposit");
            entity.Property(e => e.Savingsid).HasColumnName("savingsid");
            entity.Property(e => e.Sum)
                .HasPrecision(19)
                .HasColumnName("sum");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Savings).WithMany(p => p.TransferToSavings)
                .HasForeignKey(d => d.Savingsid)
                .HasConstraintName("transfertosavings_savingsid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.TransferToSavings)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("transfertosavings_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"User_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Budgetid).HasColumnName("budgetid");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Familyid).HasColumnName("familyid");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            entity.HasOne(d => d.Budget).WithMany(p => p.Users)
                .HasForeignKey(d => d.Budgetid)
                .HasConstraintName("User_budgetid_fkey");

            entity.HasOne(d => d.Family).WithMany(p => p.Users)
                .HasForeignKey(d => d.Familyid)
                .HasConstraintName("User_familyid_fkey");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_pkey");

            entity.ToTable("Wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('wallet_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(19)
                .HasDefaultValue(0m)
                .HasColumnName("balance");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValueSql("'RUB'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("wallet_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
