using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System;

namespace RwOrders.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First Name")]
        [StringLength(10)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(10)]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        [Display(Name = "Last Visit")]
        public DateTime? LastVisit { get; set; }

        [Display(Name = "Last Page")]
        public string LastPage { get; set; }

        [Display(Name = "Last Browser")]
        public string LastBrowser { get; set; }

        [StringLength(15)]
        [Display(Name = "Last IP")]
        public string LastIP { get; set; }

        [Display(Name = "Number of Visits")]
        public int NumberOfVisits { get; set; }

        [InverseProperty("CreatedBy")]
        public virtual ICollection<EventDay> EventDaysCreated { get; set; }

        [InverseProperty("TakenBy")]
        public virtual ICollection<Order> OrdersCreated { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim("Id", Id));
            userIdentity.AddClaim(new Claim("FirstName", FirstName));
            userIdentity.AddClaim(new Claim("LastName", LastName));

            return userIdentity;
        }
    }

    public static class IdentityExtensions
    {
        public static string GetId(this IIdentity identity)
        {
            if (identity == null)
                return null;
            return (identity as ClaimsIdentity).FirstOrNull("Id");
        }

        public static string GetFirstName(this IIdentity identity)
        {
            if (identity == null)
                return null;
            return (identity as ClaimsIdentity).FirstOrNull("FirstName");
        }

        public static string GetLastName(this IIdentity identity)
        {
            if (identity == null)
                return null;
            return (identity as ClaimsIdentity).FirstOrNull("LastName");
        }

        public static string GetFullName(this IIdentity identity)
        {
            if (identity == null)
                return null;
            string firstName = (identity as ClaimsIdentity).FirstOrNull("FirstName");
            string lastName = (identity as ClaimsIdentity).FirstOrNull("LastName");
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                return null;
            return $"{firstName} {lastName}".Trim();
        }

        internal static string FirstOrNull(this ClaimsIdentity identity, string claimType)
        {
            var val = identity.FindFirst(claimType);
            return val == null ? null : val.Value;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Consignment> Consignments { get; set; }
        public DbSet<ConsignmentItem> ConsignmentItems { get; set; }
        public DbSet<EventDay> EventDays { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}