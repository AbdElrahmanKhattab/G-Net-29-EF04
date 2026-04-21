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



    }
}
