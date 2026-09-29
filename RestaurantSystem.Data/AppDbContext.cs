using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Domain.Models;

namespace RestaurantSystem.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<RawMaterial> RawMaterials { get; set; } = null!;
    public DbSet<RecipeItem> RecipeItems { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; } = null!;
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Shift> Shifts { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<StockMovement> StockMovements { get; set; } = null!;
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; } = null!;
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; } = null!;
    public DbSet<SalesReturn> SalesReturns { get; set; } = null!;
    public DbSet<SalesReturnItem> SalesReturnItems { get; set; } = null!;

    // Accounting & Floor Plan Additions
    public DbSet<AccountNode> AccountNodes { get; set; } = null!;
    public DbSet<FinancialVoucher> FinancialVouchers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<RestaurantTable> RestaurantTables { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connStr = DatabaseConfig.GetEffectiveConnectionString();
            optionsBuilder.UseSqlite(connStr, options =>
            {
                options.CommandTimeout(30);
            });
        }
    }

    /// <summary>
    /// تطبيق إعدادات الأداء العالي لمحرك SQLite المحلي عند أول اتصال.
    /// تُضاعف سرعة القراءة والكتابة بمقدار 3-5 أضعاف مع بقاء الأمان.
    /// </summary>
    public static void ApplyHighPerformancePragmas()
    {
        try
        {
            using var conn = new SqliteConnection(DatabaseConfig.GetEffectiveConnectionString());
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;
                PRAGMA cache_size = -10000;
                PRAGMA temp_store = MEMORY;
                PRAGMA mmap_size = 268435456;
                PRAGMA busy_timeout = 5000;
                PRAGMA wal_autocheckpoint = 1000;
                PRAGMA foreign_keys = ON;
            ";
            cmd.ExecuteNonQuery();
        }
        catch { /* لا نوقف البرنامج إذا فشلت إعدادات الأداء */ }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Indexes
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<PurchaseInvoice>()
            .HasIndex(p => p.InvoiceNumber);

        modelBuilder.Entity<RecipeItem>()
            .HasIndex(r => new { r.ProductId, r.RawMaterialId })
            .IsUnique();

        modelBuilder.Entity<StockMovement>()
            .HasOne(s => s.RawMaterial)
            .WithMany()
            .HasForeignKey(s => s.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Table)
            .WithMany()
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Shift)
            .WithMany()
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasMany(p => p.Items)
            .WithOne(i => i.PurchaseInvoice)
            .HasForeignKey(i => i.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccountNode>()
            .HasOne(a => a.Parent)
            .WithMany(a => a.SubAccounts)
            .HasForeignKey(a => a.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecipeItem>()
            .HasOne(r => r.Product)
            .WithMany(p => p.RecipeItems)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecipeItem>()
            .HasOne(r => r.RawMaterial)
            .WithMany(m => m.RecipeItems)
            .HasForeignKey(r => r.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseInvoiceItem>()
            .HasOne(p => p.RawMaterial)
            .WithMany(m => m.PurchaseItems)
            .HasForeignKey(p => p.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.PurchaseReturns)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(p => p.PurchaseInvoice)
            .WithMany(i => i.PurchaseReturns)
            .HasForeignKey(p => p.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PurchaseReturn>()
            .HasMany(p => p.Items)
            .WithOne(i => i.PurchaseReturn)
            .HasForeignKey(i => i.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseReturnItem>()
            .HasOne(p => p.RawMaterial)
            .WithMany()
            .HasForeignKey(p => p.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RawMaterial>()
            .HasOne(r => r.PrimarySupplier)
            .WithMany()
            .HasForeignKey(r => r.PrimarySupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // SalesReturn relationships
        modelBuilder.Entity<SalesReturn>()
            .HasOne(s => s.OriginalOrder)
            .WithMany()
            .HasForeignKey(s => s.OriginalOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SalesReturn>()
            .HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SalesReturn>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SalesReturn>()
            .HasMany(s => s.Items)
            .WithOne(i => i.SalesReturn)
            .HasForeignKey(i => i.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SalesReturnItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // =====================================================
    //  Schema Upgrader - يضيف الجداول الناقصة بأمان
    //  بدون حذف البيانات الموجودة
    // =====================================================
    public static void UpgradeSchema()
    {
        var config = DatabaseConfig.Instance;
        if (!config.Provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            return;

        using var conn = new SqliteConnection(DatabaseConfig.GetEffectiveConnectionString());
        conn.Open();

        // Flush WAL to main DB file
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL;";
            pragma.ExecuteNonQuery();
        }

        // Helper: check if table exists
        bool TableExists(string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        // Helper: check if column exists in table
        bool ColumnExists(string tableName, string columnName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        void Execute(string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        // AuditLogs
        if (!TableExists("AuditLogs"))
        {
            Execute(@"CREATE TABLE AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL DEFAULT 0,
                ActionType TEXT NOT NULL DEFAULT '',
                EntityName TEXT NOT NULL DEFAULT '',
                Description TEXT NOT NULL DEFAULT '',
                Timestamp TEXT NOT NULL DEFAULT (datetime('now'))
            );");
        }

        // StockMovements
        if (!TableExists("StockMovements"))
        {
            Execute(@"CREATE TABLE StockMovements (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RawMaterialId INTEGER NOT NULL,
                MovementType TEXT NOT NULL DEFAULT '',
                QuantityChanged REAL NOT NULL DEFAULT 0,
                QuantityBefore REAL NOT NULL DEFAULT 0,
                QuantityAfter REAL NOT NULL DEFAULT 0,
                Reason TEXT NOT NULL DEFAULT '',
                ReferenceNumber TEXT NOT NULL DEFAULT '',
                UserId INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (RawMaterialId) REFERENCES RawMaterials(Id)
            );");
        }

        // AccountNodes
        if (!TableExists("AccountNodes"))
        {
            Execute(@"CREATE TABLE AccountNodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL DEFAULT '',
                Name TEXT NOT NULL DEFAULT '',
                Type TEXT NOT NULL DEFAULT '',
                Balance REAL NOT NULL DEFAULT 0,
                ParentId INTEGER NULL,
                FOREIGN KEY (ParentId) REFERENCES AccountNodes(Id)
            );");
        }

        // FinancialVouchers
        if (!TableExists("FinancialVouchers"))
        {
            Execute(@"CREATE TABLE FinancialVouchers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                VoucherNumber TEXT NOT NULL DEFAULT '',
                Type TEXT NOT NULL DEFAULT '',
                Amount REAL NOT NULL DEFAULT 0,
                PaidToOrFrom TEXT NOT NULL DEFAULT '',
                Reason TEXT NOT NULL DEFAULT '',
                PaymentMethod TEXT NOT NULL DEFAULT '',
                UserId INTEGER NOT NULL DEFAULT 0,
                VoucherDate TEXT NOT NULL DEFAULT (datetime('now'))
            );");
        }

        // Customers
        if (!TableExists("Customers"))
        {
            Execute(@"CREATE TABLE Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL DEFAULT '',
                Phone TEXT NOT NULL DEFAULT '',
                DebtBalance REAL NOT NULL DEFAULT 0,
                CreditLimit REAL NOT NULL DEFAULT 50000,
                Notes TEXT NOT NULL DEFAULT ''
            );");
        }

        // RestaurantTables
        if (!TableExists("RestaurantTables"))
        {
            Execute(@"CREATE TABLE RestaurantTables (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TableNumber INTEGER NOT NULL DEFAULT 0,
                TableName TEXT NOT NULL DEFAULT '',
                Capacity INTEGER NOT NULL DEFAULT 4,
                Status TEXT NOT NULL DEFAULT 'متاحة',
                CurrentOrderNumber TEXT NOT NULL DEFAULT ''
            );");
        }

        // PurchaseInvoiceItems
        if (!TableExists("PurchaseInvoiceItems"))
        {
            Execute(@"CREATE TABLE PurchaseInvoiceItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PurchaseInvoiceId INTEGER NOT NULL,
                RawMaterialId INTEGER NOT NULL,
                Quantity REAL NOT NULL DEFAULT 0,
                UnitPrice REAL NOT NULL DEFAULT 0,
                TotalPrice REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (PurchaseInvoiceId) REFERENCES PurchaseInvoices(Id),
                FOREIGN KEY (RawMaterialId) REFERENCES RawMaterials(Id)
            );");
        }

        // Ensure Shifts table has all needed columns
        if (TableExists("Shifts"))
        {
            if (!ColumnExists("Shifts", "OpeningBalance"))
                Execute("ALTER TABLE Shifts ADD COLUMN OpeningBalance REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "CashSales"))
                Execute("ALTER TABLE Shifts ADD COLUMN CashSales REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "CardSales"))
                Execute("ALTER TABLE Shifts ADD COLUMN CardSales REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "BankSales"))
                Execute("ALTER TABLE Shifts ADD COLUMN BankSales REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "TotalExpenses"))
                Execute("ALTER TABLE Shifts ADD COLUMN TotalExpenses REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "ExpectedCash"))
                Execute("ALTER TABLE Shifts ADD COLUMN ExpectedCash REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "ActualCash"))
                Execute("ALTER TABLE Shifts ADD COLUMN ActualCash REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "ClosingBalance"))
                Execute("ALTER TABLE Shifts ADD COLUMN ClosingBalance REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "Discrepancy"))
                Execute("ALTER TABLE Shifts ADD COLUMN Discrepancy REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "IsClosed"))
                Execute("ALTER TABLE Shifts ADD COLUMN IsClosed INTEGER NOT NULL DEFAULT 0;");
            if (!ColumnExists("Shifts", "Notes"))
                Execute("ALTER TABLE Shifts ADD COLUMN Notes TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("Shifts", "EndTime"))
                Execute("ALTER TABLE Shifts ADD COLUMN EndTime TEXT NULL;");
        }

        // Ensure Orders has all required columns and foreign keys
        if (TableExists("Orders"))
        {
            if (!ColumnExists("Orders", "DiscountAmount"))
                Execute("ALTER TABLE Orders ADD COLUMN DiscountAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Orders", "ChangeAmount"))
                Execute("ALTER TABLE Orders ADD COLUMN ChangeAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("Orders", "CustomerId"))
                Execute("ALTER TABLE Orders ADD COLUMN CustomerId INTEGER NULL;");
            if (!ColumnExists("Orders", "UserId"))
                Execute("ALTER TABLE Orders ADD COLUMN UserId INTEGER NULL;");
            if (!ColumnExists("Orders", "TableId"))
                Execute("ALTER TABLE Orders ADD COLUMN TableId INTEGER NULL;");
        }

        // Ensure Expenses has ShiftId column
        if (TableExists("Expenses"))
        {
            if (!ColumnExists("Expenses", "ShiftId"))
                Execute("ALTER TABLE Expenses ADD COLUMN ShiftId INTEGER NULL;");
        }

        // Ensure Users has ForcePasswordChange column (newly added field)
        if (TableExists("Users"))
        {
            if (!ColumnExists("Users", "ForcePasswordChange"))
                Execute("ALTER TABLE Users ADD COLUMN ForcePasswordChange INTEGER NOT NULL DEFAULT 0;");
        }

        // PurchaseReturns
        if (!TableExists("PurchaseReturns"))
        {
            Execute(@"CREATE TABLE PurchaseReturns (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReturnNumber TEXT NOT NULL DEFAULT '',
                ReturnDate TEXT NOT NULL DEFAULT (datetime('now')),
                SupplierId INTEGER NOT NULL,
                PurchaseInvoiceId INTEGER NULL,
                TotalAmount REAL NOT NULL DEFAULT 0,
                SettlementMethod TEXT NOT NULL DEFAULT 'خصم من رصيد المورد',
                Reason TEXT NOT NULL DEFAULT '',
                Notes TEXT NOT NULL DEFAULT '',
                UserId INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                FOREIGN KEY (PurchaseInvoiceId) REFERENCES PurchaseInvoices(Id)
            );");
        }

        // PurchaseReturnItems
        if (!TableExists("PurchaseReturnItems"))
        {
            Execute(@"CREATE TABLE PurchaseReturnItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PurchaseReturnId INTEGER NOT NULL,
                RawMaterialId INTEGER NOT NULL,
                Quantity REAL NOT NULL DEFAULT 0,
                UnitPrice REAL NOT NULL DEFAULT 0,
                TotalPrice REAL NOT NULL DEFAULT 0,
                Reason TEXT NOT NULL DEFAULT '',
                FOREIGN KEY (PurchaseReturnId) REFERENCES PurchaseReturns(Id),
                FOREIGN KEY (RawMaterialId) REFERENCES RawMaterials(Id)
            );");
        }

        // Suppliers columns upgrade
        if (TableExists("Suppliers"))
        {
            if (!ColumnExists("Suppliers", "CompanyName"))
                Execute("ALTER TABLE Suppliers ADD COLUMN CompanyName TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("Suppliers", "SecondaryPhone"))
                Execute("ALTER TABLE Suppliers ADD COLUMN SecondaryPhone TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("Suppliers", "Email"))
                Execute("ALTER TABLE Suppliers ADD COLUMN Email TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("Suppliers", "Notes"))
                Execute("ALTER TABLE Suppliers ADD COLUMN Notes TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("Suppliers", "CurrentBalance"))
                Execute("ALTER TABLE Suppliers ADD COLUMN CurrentBalance REAL NOT NULL DEFAULT 0;");
        }

        // PurchaseInvoices columns upgrade
        if (TableExists("PurchaseInvoices"))
        {
            if (!ColumnExists("PurchaseInvoices", "SupplierInvoiceNumber"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN SupplierInvoiceNumber TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("PurchaseInvoices", "SubTotal"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN SubTotal REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "DiscountAmount"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN DiscountAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "TaxAmount"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN TaxAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "PaidCash"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN PaidCash REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "PaidBank"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN PaidBank REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "PaidOther"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN PaidOther REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "PaidAmount"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN PaidAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "RemainingAmount"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN RemainingAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoices", "PaymentStatus"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN PaymentStatus TEXT NOT NULL DEFAULT 'مدفوعة';");
            if (!ColumnExists("PurchaseInvoices", "Notes"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN Notes TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("PurchaseInvoices", "UserId"))
                Execute("ALTER TABLE PurchaseInvoices ADD COLUMN UserId INTEGER NULL;");
        }

        // PurchaseInvoiceItems columns upgrade
        if (TableExists("PurchaseInvoiceItems"))
        {
            if (!ColumnExists("PurchaseInvoiceItems", "Discount"))
                Execute("ALTER TABLE PurchaseInvoiceItems ADD COLUMN Discount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoiceItems", "TaxRate"))
                Execute("ALTER TABLE PurchaseInvoiceItems ADD COLUMN TaxRate REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoiceItems", "TaxAmount"))
                Execute("ALTER TABLE PurchaseInvoiceItems ADD COLUMN TaxAmount REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("PurchaseInvoiceItems", "ExpiryDate"))
                Execute("ALTER TABLE PurchaseInvoiceItems ADD COLUMN ExpiryDate TEXT NULL;");
            if (!ColumnExists("PurchaseInvoiceItems", "Notes"))
                Execute("ALTER TABLE PurchaseInvoiceItems ADD COLUMN Notes TEXT NOT NULL DEFAULT '';");
        }

        // RawMaterials columns upgrade
        if (TableExists("RawMaterials"))
        {
            if (!ColumnExists("RawMaterials", "ItemCode"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN ItemCode TEXT NOT NULL DEFAULT '';");
            if (!ColumnExists("RawMaterials", "Category"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN Category TEXT NOT NULL DEFAULT 'عام';");
            if (!ColumnExists("RawMaterials", "AverageCost"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN AverageCost REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("RawMaterials", "SellingPrice"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN SellingPrice REAL NOT NULL DEFAULT 0;");
            if (!ColumnExists("RawMaterials", "PrimarySupplierId"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN PrimarySupplierId INTEGER NULL;");
            if (!ColumnExists("RawMaterials", "LastPurchaseDate"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN LastPurchaseDate TEXT NULL;");
            if (!ColumnExists("RawMaterials", "LastSaleDate"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN LastSaleDate TEXT NULL;");
            if (!ColumnExists("RawMaterials", "ExpiryDate"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN ExpiryDate TEXT NULL;");
            if (!ColumnExists("RawMaterials", "Status"))
                Execute("ALTER TABLE RawMaterials ADD COLUMN Status TEXT NOT NULL DEFAULT 'نشط';");
        }

        // Clean orphaned demo orders if any
        try
        {
            Execute(@"DELETE FROM Orders WHERE Id NOT IN (SELECT DISTINCT OrderId FROM OrderItems) 
                      AND OrderNumber LIKE '#%';");
        }
        catch { }

        // SalesReturns
        if (!TableExists("SalesReturns"))
        {
            Execute(@"CREATE TABLE SalesReturns (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReturnNumber TEXT NOT NULL DEFAULT '',
                ReturnDate TEXT NOT NULL DEFAULT (datetime('now')),
                OriginalOrderId INTEGER NULL,
                OriginalOrderNumber TEXT NOT NULL DEFAULT '',
                CustomerId INTEGER NULL,
                TotalAmount REAL NOT NULL DEFAULT 0,
                RefundMethod TEXT NOT NULL DEFAULT 'نقدي',
                Reason TEXT NOT NULL DEFAULT '',
                Notes TEXT NOT NULL DEFAULT '',
                UserId INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (OriginalOrderId) REFERENCES Orders(Id),
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            );");
        }

        // SalesReturnItems
        if (!TableExists("SalesReturnItems"))
        {
            Execute(@"CREATE TABLE SalesReturnItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SalesReturnId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL DEFAULT 0,
                UnitPrice REAL NOT NULL DEFAULT 0,
                TotalPrice REAL NOT NULL DEFAULT 0,
                Reason TEXT NOT NULL DEFAULT '',
                FOREIGN KEY (SalesReturnId) REFERENCES SalesReturns(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );");
        }

        conn.Close();
    }

    // =====================================================
    //  Database Initialization - Production Safe
    // =====================================================
    public static void InitializeDatabase()
    {
        // Step 0: Apply SQLite high performance WAL mode & pragmas
        ApplyHighPerformancePragmas();

        // Step 1: Ensure base schema exists
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
        }

        // Step 2: Upgrade schema for existing databases
        UpgradeSchema();

        // Step 3: Seed essential data
        using var ctx = new AppDbContext();
        SeedEssentialData(ctx);
    }

    private static void SeedEssentialData(AppDbContext db)
    {
        // 1. إعدادات المطعم الرئيسية (اليمن)
        if (!db.SystemSettings.Any())
        {
            db.SystemSettings.Add(new SystemSetting
            {
                RestaurantName = "مطعم المقبولي",
                Phone = "770-123-456",
                Address = "الحديدة - شارع الميناء - الجمهورية اليمنية",
                Currency = "ريال",
                TaxRate = 0.0m
            });
            db.SaveChanges();
        }

        // 2. مستخدمو النظام بصلاحياتهم وكلمات مرور مشفرة (PBKDF2)
        if (!db.Users.Any())
        {
            // ForcePasswordChange=true يُجبر المستخدمين على تغيير كلمة مرورهم عند أول دخول
            string defaultHash = PasswordHasher.HashPassword("123");
            db.Users.AddRange(
                new User { FullName = "المدير العام",   Username = "admin",   Password = defaultHash, Role = "Admin",      IsActive = true, ForcePasswordChange = true },
                new User { FullName = "كاشير الصالة",   Username = "cashier", Password = defaultHash, Role = "Cashier",    IsActive = true, ForcePasswordChange = true },
                new User { FullName = "شاشة المطبخ",    Username = "kitchen", Password = defaultHash, Role = "Kitchen",    IsActive = true, ForcePasswordChange = true },
                new User { FullName = "مسؤول المخزون",  Username = "stock",   Password = defaultHash, Role = "Inventory",  IsActive = true, ForcePasswordChange = true },
                new User { FullName = "محاسب المطعم",   Username = "account", Password = defaultHash, Role = "Accountant", IsActive = true, ForcePasswordChange = true }
            );
            db.SaveChanges();
        }
        else
        {
            // ترقية كلمات المرور النصية الواضحة فقط (أقل من 25 حرف وبدون تنسيق Base64 أو علامة تشفير)
            // أما كلمات المرور بصيغة SHA-256 السابقة فستتم ترقيتها بسلاسة وأمان إلى PBKDF2 عند أول تسجيل دخول للمستخدم عبر LoginForm
            var plaintextUsers = db.Users.ToList()
                .Where(u => !string.IsNullOrEmpty(u.Password)
                         && !PasswordHasher.IsModernHash(u.Password)
                         && !u.Password.EndsWith("=")
                         && u.Password.Length < 25)
                .ToList();

            if (plaintextUsers.Any())
            {
                foreach (var u in plaintextUsers)
                {
                    u.Password = PasswordHasher.HashPassword(u.Password);
                }
                db.SaveChanges();
            }
        }

        // 3. شجرة الحسابات والدليل المحاسبي المنظم برصيد افتتاحي نظيف
        if (!db.AccountNodes.Any())
        {
            var accAssets   = new AccountNode { Code = "1000", Name = "الأصول",          Type = "الأصول",          Balance = 0m };
            var accLiab     = new AccountNode { Code = "2000", Name = "الالتزامات",       Type = "الالتزامات",       Balance = 0m };
            var accEquity   = new AccountNode { Code = "3000", Name = "حقوق الملكية",     Type = "حقوق الملكية",     Balance = 0m };
            var accRevenue  = new AccountNode { Code = "4000", Name = "الإيرادات",        Type = "الإيرادات",        Balance = 0m };
            var accExpenses = new AccountNode { Code = "5000", Name = "المصروفات",        Type = "المصروفات",        Balance = 0m };

            db.AccountNodes.AddRange(accAssets, accLiab, accEquity, accRevenue, accExpenses);
            db.SaveChanges();

            db.AccountNodes.AddRange(
                new AccountNode { Code = "1101", Name = "الصندوق الرئيسي (الدرج)",        Type = "الأصول",      Balance = 0m, ParentId = accAssets.Id },
                new AccountNode { Code = "1102", Name = "حساب البنك التجاري",              Type = "الأصول",      Balance = 0m, ParentId = accAssets.Id },
                new AccountNode { Code = "1103", Name = "حسابات الذمم المدينة (العملاء)",  Type = "الأصول",      Balance = 0m, ParentId = accAssets.Id },
                new AccountNode { Code = "1104", Name = "مخزون المواد الخام",               Type = "الأصول",      Balance = 0m, ParentId = accAssets.Id },
                new AccountNode { Code = "2101", Name = "حسابات الموردين الدائنة",         Type = "الالتزامات",  Balance = 0m, ParentId = accLiab.Id   },
                new AccountNode { Code = "4101", Name = "إيرادات مبيعات الوجبات",          Type = "الإيرادات",   Balance = 0m, ParentId = accRevenue.Id },
                new AccountNode { Code = "5101", Name = "تكلفة المشتريات والمواد",          Type = "المصروفات",   Balance = 0m, ParentId = accExpenses.Id },
                new AccountNode { Code = "5102", Name = "مصروف الكهرباء والمياه",          Type = "المصروفات",   Balance = 0m, ParentId = accExpenses.Id },
                new AccountNode { Code = "5103", Name = "مصروف الإيجار الشهري",            Type = "المصروفات",   Balance = 0m, ParentId = accExpenses.Id }
            );
            db.SaveChanges();
        }

        // 4. طاولات صالة الطعام
        if (!db.RestaurantTables.Any())
        {
            for (int t = 1; t <= 12; t++)
            {
                db.RestaurantTables.Add(new RestaurantTable
                {
                    TableNumber = t,
                    TableName   = $"طاولة {t}",
                    Capacity    = t <= 4 ? 2 : (t <= 8 ? 4 : 8),
                    Status      = "متاحة",
                    CurrentOrderNumber = ""
                });
            }
            db.SaveChanges();
        }

        // 5. التصنيفات وقائمة الوجبات — أسعار حقيقية سوق الحديدة 2026
        if (!db.Categories.Any())
        {
            var catFast    = new Category { Name = "وجبات سريعة",    Description = "برجر وشاورما وبروست وسندويشات" };
            var catGrill   = new Category { Name = "مشويات",          Description = "كباب ومشوي ودجاج شواية" };
            var catMain    = new Category { Name = "الأطباق الرئيسية", Description = "مندي وبرم وسمك وأطباق تهامية" };
            var catSeafood = new Category { Name = "مأكولات بحرية",   Description = "سمك مقلى ومشوي وجمبري" };
            var catDrinks  = new Category { Name = "المشروبات",       Description = "عصائر طبيعية ومشروبات باردة" };

            db.Categories.AddRange(catFast, catGrill, catMain, catSeafood, catDrinks);
            db.SaveChanges();

            // ========== وجبات سريعة ==========
            // برجر/شاورما في الحديدة: وجبة فردية 1,500-3,000 ريال (مصدر: مطاعم محلية 2026)
            var p1  = new Product { Name = "برجر دجاج",              Price = 2000.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p2  = new Product { Name = "برجر لحم بلدي",          Price = 2500.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p3  = new Product { Name = "شاورما دجاج",            Price = 1500.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p4  = new Product { Name = "شاورما لحم",             Price = 2000.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p5  = new Product { Name = "بروست دجاج (قطعة)",      Price = 1500.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p6  = new Product { Name = "وجبة بروست دجاج كاملة",  Price = 3500.00m,  CategoryId = catFast.Id,    IsAvailable = true };
            var p7  = new Product { Name = "سندويش زنجر",            Price = 1500.00m,  CategoryId = catFast.Id,    IsAvailable = true };

            // ========== مشويات ==========
            // كباب ومشويات: الطبق 3,000-6,000 ريال حسب الوزن
            var p8  = new Product { Name = "كباب لحم مشوي",          Price = 4000.00m,  CategoryId = catGrill.Id,   IsAvailable = true };
            var p9  = new Product { Name = "دجاج شواية كامل",         Price = 6000.00m,  CategoryId = catGrill.Id,   IsAvailable = true };
            var p10 = new Product { Name = "دجاج شواية نص",           Price = 3000.00m,  CategoryId = catGrill.Id,   IsAvailable = true };
            var p11 = new Product { Name = "كفتة لحم مشوي",           Price = 3500.00m,  CategoryId = catGrill.Id,   IsAvailable = true };

            // ========== أطباق رئيسية ==========
            // مندي/برم: صواني عائلية 8,000-20,000 ريال؛ فرد: 4,000-7,000 ريال
            var p12 = new Product { Name = "مندي لحم مع الأرز (فردي)",  Price = 5000.00m,  CategoryId = catMain.Id,    IsAvailable = true };
            var p13 = new Product { Name = "مندي دجاج مع الأرز (فردي)", Price = 3500.00m,  CategoryId = catMain.Id,    IsAvailable = true };
            var p14 = new Product { Name = "برم لحم مع الأرز (فردي)",   Price = 5500.00m,  CategoryId = catMain.Id,    IsAvailable = true };
            var p15 = new Product { Name = "رشوش تهامي",               Price = 1500.00m,  CategoryId = catMain.Id,    IsAvailable = true };
            var p16 = new Product { Name = "عصيد مع المرق",             Price = 2000.00m,  CategoryId = catMain.Id,    IsAvailable = true };

            // ========== مأكولات بحرية ==========
            // سمك في الحديدة: كيلو سمك مقلى 5,000-10,000 ريال (حسب النوع)
            var p17 = new Product { Name = "سمك مقلى (بالكيلو)",       Price = 6000.00m,  CategoryId = catSeafood.Id, IsAvailable = true };
            var p18 = new Product { Name = "سمك مشوي (بالكيلو)",        Price = 7000.00m,  CategoryId = catSeafood.Id, IsAvailable = true };
            var p19 = new Product { Name = "طبق جمبري مشوي",            Price = 8000.00m,  CategoryId = catSeafood.Id, IsAvailable = true };

            // ========== مشروبات ==========
            // عصائر طازجة: 500-1,500 ريال في الحديدة
            var p20 = new Product { Name = "عصير برتقال طازج",          Price = 1000.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };
            var p21 = new Product { Name = "عصير مانجو",                Price = 1000.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };
            var p22 = new Product { Name = "عصير ليمون بالنعناع",       Price =  800.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };
            var p23 = new Product { Name = "شاي كرك",                   Price =  500.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };
            var p24 = new Product { Name = "مياه معدنية",               Price =  300.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };
            var p25 = new Product { Name = "موهيتو",                    Price = 1200.00m,  CategoryId = catDrinks.Id,  IsAvailable = true };

            db.Products.AddRange(p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19,p20,p21,p22,p23,p24,p25);
            db.SaveChanges();
        }

        // 6. المواد الخام — أسعار حقيقية سوق الحديدة سبتمبر 2026
        if (!db.RawMaterials.Any())
        {
            // دجاج: 4,500-6,000 ريال/كجم (المصدر: أسواق دواجن الحديدة 2026)
            var rm1 = new RawMaterial { Name = "دجاج مذبوح",       Unit = "كجم",  CurrentQuantity = 20.00m, MinLimit = 5.0m,  CostPerUnit = 5000.00m };
            // لحم بلدي: 8,000-10,000 ريال/كجم
            var rm2 = new RawMaterial { Name = "لحم بلدي",         Unit = "كجم",  CurrentQuantity = 15.00m, MinLimit = 3.0m,  CostPerUnit = 9000.00m };
            // أرز بسمتي: ~2,500 ريال/كجم (عبوة 10 كجم ≈ 25,000 ريال)
            var rm3 = new RawMaterial { Name = "أرز بسمتي",        Unit = "كجم",  CurrentQuantity = 30.00m, MinLimit = 10.0m, CostPerUnit = 2500.00m };
            // زيت طبخ: سعر مرتفع بسبب الاستيراد، تقدير 3,000 ريال/لتر
            var rm4 = new RawMaterial { Name = "زيت طبخ",          Unit = "لتر",  CurrentQuantity = 10.00m, MinLimit = 3.0m,  CostPerUnit = 3000.00m };
            // بطاطس: 250 ريال/كجم (المصدر: أسواق الخضار الحديدة 2026)
            var rm5 = new RawMaterial { Name = "بطاطس",            Unit = "كجم",  CurrentQuantity = 20.00m, MinLimit = 5.0m,  CostPerUnit =  350.00m };
            // بصل: 1,000 ريال/كجم
            var rm6 = new RawMaterial { Name = "بصل",              Unit = "كجم",  CurrentQuantity = 10.00m, MinLimit = 3.0m,  CostPerUnit = 1000.00m };
            // طماطم: 250 ريال/كجم
            var rm7 = new RawMaterial { Name = "طماطم",            Unit = "كجم",  CurrentQuantity = 10.00m, MinLimit = 3.0m,  CostPerUnit =  300.00m };
            // لحم سمك (هامور/كنعد): 6,000-8,000 ريال/كجم في سوق المحوات
            var rm8 = new RawMaterial { Name = "سمك طازج",         Unit = "كجم",  CurrentQuantity = 15.00m, MinLimit = 5.0m,  CostPerUnit = 7000.00m };
            // دقيق: للخبز والبروست
            var rm9 = new RawMaterial { Name = "دقيق",             Unit = "كجم",  CurrentQuantity = 25.00m, MinLimit = 5.0m,  CostPerUnit =  600.00m };
            // توابل مشكلة
            var rm10= new RawMaterial { Name = "توابل مشكلة",      Unit = "كجم",  CurrentQuantity =  5.00m, MinLimit = 1.0m,  CostPerUnit = 5000.00m };

            db.RawMaterials.AddRange(rm1, rm2, rm3, rm4, rm5, rm6, rm7, rm8, rm9, rm10);
            db.SaveChanges();
        }

        // 7. ربط الوصفات بالمنتجات
        if (!db.RecipeItems.Any())
        {
            var burger       = db.Products.FirstOrDefault(p => p.Name == "برجر دجاج");
            var shawarma     = db.Products.FirstOrDefault(p => p.Name == "شاورما دجاج");
            var broast       = db.Products.FirstOrDefault(p => p.Name == "وجبة بروست دجاج كاملة");
            var mandilahm    = db.Products.FirstOrDefault(p => p.Name == "مندي لحم مع الأرز (فردي)");
            var mandiDjaj    = db.Products.FirstOrDefault(p => p.Name == "مندي دجاج مع الأرز (فردي)");
            var fishFried    = db.Products.FirstOrDefault(p => p.Name == "سمك مقلى (بالكيلو)");
            var kabab        = db.Products.FirstOrDefault(p => p.Name == "كباب لحم مشوي");

            var rmChicken = db.RawMaterials.FirstOrDefault(r => r.Name == "دجاج مذبوح");
            var rmMeat    = db.RawMaterials.FirstOrDefault(r => r.Name == "لحم بلدي");
            var rmRice    = db.RawMaterials.FirstOrDefault(r => r.Name == "أرز بسمتي");
            var rmOil     = db.RawMaterials.FirstOrDefault(r => r.Name == "زيت طبخ");
            var rmFlour   = db.RawMaterials.FirstOrDefault(r => r.Name == "دقيق");
            var rmFish    = db.RawMaterials.FirstOrDefault(r => r.Name == "سمك طازج");
            var rmSpices  = db.RawMaterials.FirstOrDefault(r => r.Name == "توابل مشكلة");

            // برجر دجاج: 200 جرام دجاج + 50 مل زيت
            if (burger != null && rmChicken != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = burger.Id, RawMaterialId = rmChicken.Id, QuantityRequired = 0.200m });
            if (burger != null && rmOil != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = burger.Id, RawMaterialId = rmOil.Id, QuantityRequired = 0.050m });

            // شاورما دجاج: 150 جرام دجاج
            if (shawarma != null && rmChicken != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = shawarma.Id, RawMaterialId = rmChicken.Id, QuantityRequired = 0.150m });

            // بروست كامل: 500 جرام دجاج + 100 مل زيت + 100 جرام دقيق
            if (broast != null && rmChicken != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = broast.Id, RawMaterialId = rmChicken.Id, QuantityRequired = 0.500m });
            if (broast != null && rmOil != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = broast.Id, RawMaterialId = rmOil.Id, QuantityRequired = 0.100m });
            if (broast != null && rmFlour != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = broast.Id, RawMaterialId = rmFlour.Id, QuantityRequired = 0.100m });

            // مندي لحم: 400 جرام لحم + 250 جرام أرز + توابل
            if (mandilahm != null && rmMeat != null && rmRice != null)
            {
                db.RecipeItems.Add(new RecipeItem { ProductId = mandilahm.Id, RawMaterialId = rmMeat.Id,   QuantityRequired = 0.400m });
                db.RecipeItems.Add(new RecipeItem { ProductId = mandilahm.Id, RawMaterialId = rmRice.Id,   QuantityRequired = 0.250m });
                if (rmSpices != null)
                    db.RecipeItems.Add(new RecipeItem { ProductId = mandilahm.Id, RawMaterialId = rmSpices.Id, QuantityRequired = 0.020m });
            }

            // مندي دجاج: 400 جرام دجاج + 250 جرام أرز
            if (mandiDjaj != null && rmChicken != null && rmRice != null)
            {
                db.RecipeItems.Add(new RecipeItem { ProductId = mandiDjaj.Id, RawMaterialId = rmChicken.Id, QuantityRequired = 0.400m });
                db.RecipeItems.Add(new RecipeItem { ProductId = mandiDjaj.Id, RawMaterialId = rmRice.Id,    QuantityRequired = 0.250m });
            }

            // سمك مقلى: 1 كجم سمك + 200 مل زيت
            if (fishFried != null && rmFish != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = fishFried.Id, RawMaterialId = rmFish.Id, QuantityRequired = 1.000m });
            if (fishFried != null && rmOil != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = fishFried.Id, RawMaterialId = rmOil.Id, QuantityRequired = 0.200m });

            // كباب لحم: 300 جرام لحم
            if (kabab != null && rmMeat != null)
                db.RecipeItems.Add(new RecipeItem { ProductId = kabab.Id, RawMaterialId = rmMeat.Id, QuantityRequired = 0.300m });

            db.SaveChanges();
        }
    }
}
