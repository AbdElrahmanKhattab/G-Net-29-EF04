using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace EF_04
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== National Bank - Management ===");
                Console.WriteLine("1) Add Customer");
                Console.WriteLine("2) Open Account");
                Console.WriteLine("3) Update Account Status");
                Console.WriteLine("4) Remove Account");
                Console.WriteLine("5) List Customers");
                Console.WriteLine("0) Exit");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": AddCustomer(db); break;
                        case "2": OpenAccount(db); break;
                        case "3": UpdateStatus(db); break;
                        case "4": RemoveAccount(db); break;
                        case "5": ListCustomers(db); break;
                        case "0": return;
                        default: Console.WriteLine("Invalid choice"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key...");
                Console.ReadKey();
            }
        }



        static void AddCustomer(AppDbContext db)
        {
            Console.WriteLine("\n--- Add New Customer ---");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("National ID: ");
            var nid = Console.ReadLine();

            Console.Write("Date Of Birth (yyyy-MM-dd): ");
            var dob = DateTime.Parse(Console.ReadLine());

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Phone: ");
            var phone = Console.ReadLine();

            Console.Write("Address: ");
            var address = Console.ReadLine();

            Console.WriteLine("Customer Type:");
            Console.WriteLine("1) Individual");
            Console.WriteLine("2) Business");
            Console.Write("Choice: ");

            string type = Console.ReadLine() == "1" ? "Individual" : "Business";

            var c = new Customer
            {
                FullName = name,
                NationalId = nid,
                DateOfBirth = dob,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = type
            };

            db.Customers.Add(c);
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nCustomer created successfully. CustomerId = {c.Id}");
            Console.ResetColor();
        }

        static void OpenAccount(AppDbContext db)
        {
            Console.WriteLine("\n--- Open New Account ---");

            Console.Write("Account Number: ");
            var accNum = Console.ReadLine();

            Console.WriteLine("Account Type:");
            Console.WriteLine("1) Savings");
            Console.WriteLine("2) Current");
            Console.WriteLine("3) Business");
            Console.Write("Choice: ");

            string accType = Console.ReadLine() switch
            {
                "1" => "Savings",
                "2" => "Current",
                _ => "Business"
            };

            Console.Write("Branch Code: ");
            var branchCode = Console.ReadLine();

            Console.Write("Customer Id: ");
            int cid = int.Parse(Console.ReadLine());

            Console.WriteLine("Ownership Role:");
            Console.WriteLine("1) Primary");
            Console.WriteLine("2) CoHolder");
            Console.Write("Choice: ");

            string role = Console.ReadLine() == "1" ? "Primary" : "CoHolder";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nValidating branch '{branchCode}' and customer #{cid}...");
            Console.ResetColor();

            var branch = db.Branches.FirstOrDefault(b => b.Code == branchCode);
            var customer = db.Customers.Find(cid);

            if (branch == null || customer == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Validation failed: invalid branch or customer.");
                Console.ResetColor();
                return;
            }

            var acc = new Account
            {
                AccountNumber = accNum,
                AccountType = accType,
                OpeningDate = DateTime.Now,
                CurrentBalance = 0,
                BranchId = branch.Id
            };

            db.Accounts.Add(acc);
            db.SaveChanges();

            db.CustomerAccounts.Add(new CustomerAccount
            {
                AccountId = acc.Id,
                CustomerId = cid,
                OwnershipStartDate = DateTime.Now,
                OwnershipType = role,
                AccountStatus = "Active"
            });

            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Account '{accNum}' created and linked to customer {cid} as {role} owner.");
            Console.ResetColor();
        }
        static void UpdateStatus(AppDbContext db)
        {
            Console.WriteLine("\n--- Update Account Status ---");

            Console.Write("Account Number: ");
            var accNum = Console.ReadLine();

            Console.Write("Customer Id: ");
            int cid = int.Parse(Console.ReadLine());

            Console.WriteLine("New Status:");
            Console.WriteLine("1) Active");
            Console.WriteLine("2) Closed");
            Console.Write("Choice: ");

            string status = Console.ReadLine() == "1" ? "Active" : "Closed";

            var link = db.CustomerAccounts
                .Include(x => x.Account)
                .FirstOrDefault(x => x.Account.AccountNumber == accNum && x.CustomerId == cid);

            if (link == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid account number or customer ID.");
                Console.ResetColor();
                return;
            }

            link.AccountStatus = status;
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Status updated to {status}.");
            Console.ResetColor();
        }
        static void RemoveAccount(AppDbContext db)
        {
            Console.WriteLine("\n--- Remove Account ---");

            Console.Write("Account Number: ");
            var accNum = Console.ReadLine();

            Console.Write("Customer Id: ");
            int cid = int.Parse(Console.ReadLine());

            var link = db.CustomerAccounts
                .Include(x => x.Account)
                .FirstOrDefault(x => x.Account.AccountNumber == accNum && x.CustomerId == cid);

            if (link == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Operation failed: Account or Customer not found.");
                Console.ResetColor();
                return;
            }

            var acc = link.Account;

            db.CustomerAccounts.Remove(link);
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ownership link deleted.");

            if (!db.CustomerAccounts.Any(x => x.AccountId == acc.Id))
            {
                db.Accounts.Remove(acc);
                db.SaveChanges();
                Console.WriteLine($"That was the last owner — account '{accNum}' was also removed.");
            }

            Console.ResetColor();
        }

        static void ListCustomers(AppDbContext db)
        {
            Console.WriteLine("\n--- All Customers ---");

            var customers = db.Customers
                .Include(c => c.CustomerAccounts)
                .ThenInclude(ca => ca.Account)
                .ThenInclude(a => a.Branch)
                .ToList();

            foreach (var c in customers)
            {
                Console.WriteLine($"#{c.Id} {c.FullName} ({c.CustomerType})");

                if (c.CustomerAccounts.Count == 0)
                {
                    Console.WriteLine("   (No accounts)");
                    continue;
                }

                foreach (var ca in c.CustomerAccounts)
                {
                    Console.WriteLine($"   {ca.Account.AccountNumber} {ca.Account.AccountType} Balance: {ca.Account.CurrentBalance} {ca.OwnershipType} {ca.AccountStatus} @ {ca.Account.Branch.Name}");
                }
            }
        }

    }

    #region Models

    class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public int ManagerId { get; set; }
        public Manager Manager { get; set; }

        public List<Account> Accounts { get; set; }
    }

    class Manager
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
    }

    class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string CustomerType { get; set; }

        public List<CustomerAccount> CustomerAccounts { get; set; }
    }

    class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public DateTime OpeningDate { get; set; }
        public decimal CurrentBalance { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; }

        public List<CustomerAccount> CustomerAccounts { get; set; }
        public List<Transaction> Transactions { get; set; }
    }

    class Transaction
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Note { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
    }

    class CustomerAccount
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public DateTime OwnershipStartDate { get; set; }
        public string OwnershipType { get; set; }
        public string AccountStatus { get; set; }
    }

    #endregion

    #region DbContext

    class AppDbContext : DbContext
    {
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=.;Database=BankDB;Trusted_Connection=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerAccount>()
                .HasKey(x => new { x.CustomerId, x.AccountId });

            modelBuilder.Entity<CustomerAccount>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.CustomerAccounts)
                .HasForeignKey(x => x.CustomerId);

            modelBuilder.Entity<CustomerAccount>()
                .HasOne(x => x.Account)
                .WithMany(x => x.CustomerAccounts)
                .HasForeignKey(x => x.AccountId);

            // Seed
            modelBuilder.Entity<Manager>().HasData(
                new Manager { Id = 1, FullName = "Ahmed Ali", Email = "a@mail.com", PhoneNumber = "010", HireDate = DateTime.Now }
            );

            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "Cairo", Code = "CAI-01", Address = "Cairo", PhoneNumber = "123", ManagerId = 1 }
            );
        }
    }

    #endregion
}