using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RwOrders.Models;
using RwOrders.Reports;

namespace RwOrders.Controllers
{
    public class ConsignmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Consignments.OrderByDescending(c => c.DispatchDate).ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            return View(consignment);
        }

        public async Task<ActionResult> Print(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            byte[] bytes = ConsignmentNoteReport.GeneratePdf(consignment);
            return File(bytes, "application/pdf");
        }

        public async Task<ActionResult> GetItems(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            var items = consignment.ConsignmentItems
                .OrderBy(c => c.ID)
                .Select(c => new { c.ID, c.ProductID, c.StockCode, c.Description, c.Quantity, c.UnitPrice })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddProduct(int? id, string stockCode)
        {
            if (id == null || string.IsNullOrEmpty(stockCode))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            Product product = await db.Products.Where(p => p.StockCode == stockCode).FirstOrDefaultAsync();
            if (product == null)
                return HttpNotFound();
            ConsignmentItem ci = await db.ConsignmentItems.Where(c => c.ConsignmentID == consignment.ID && c.ProductID == product.ID).FirstOrDefaultAsync();
            if (ci != null)
            {
                ci.Quantity++;
                db.Entry(ci).Property(p => p.Quantity).IsModified = true;
            }
            else
            {
                ci = new ConsignmentItem { ConsignmentID = consignment.ID, ProductID = product.ID, StockCode = product.StockCode, Description = product.Description, UnitPrice = product.UnitPrice };
                db.ConsignmentItems.Add(ci);
            }
            await db.SaveChangesAsync();
            var items = consignment.ConsignmentItems
                .OrderBy(c => c.ID)
                .Select(c => new { c.ID, c.ProductID, c.StockCode, c.Description, c.Quantity, c.UnitPrice })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SaveItem(int? id, string description, int? quantity, decimal? unitPrice)
        {
            if (id == null || string.IsNullOrEmpty(description) || quantity == null || unitPrice == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ConsignmentItem ci = await db.ConsignmentItems.FindAsync(id);
            if (ci == null)
                return HttpNotFound();
            ci.Description = description;
            ci.Quantity = quantity.Value;
            ci.UnitPrice = unitPrice.Value;
            db.Entry(ci).State = EntityState.Modified;
            await db.SaveChangesAsync();
            var items = db.ConsignmentItems
                .Where(c => c.ConsignmentID == ci.ConsignmentID)
                .OrderBy(c => c.ID)
                .Select(c => new { c.ID, c.ProductID, c.StockCode, c.Description, c.Quantity, c.UnitPrice })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DeleteItem(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ConsignmentItem ci = await db.ConsignmentItems.FindAsync(id);
            if (ci == null)
                return HttpNotFound();
            db.ConsignmentItems.Remove(ci);
            await db.SaveChangesAsync();
            var items = db.ConsignmentItems
                .Where(c => c.ConsignmentID == ci.ConsignmentID)
                .OrderBy(c => c.ID)
                .Select(c => new { c.ID, c.ProductID, c.StockCode, c.Description, c.Quantity, c.UnitPrice })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Create()
        {
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            List<ApplicationUser> users = await userManager.Users.OrderBy(o => o.FirstName).ToListAsync();
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.SalesPersonID = new SelectList(users, "Id", "FullName", user.Id);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SalesPersonID,DispatchDate,Campus,Locality,ContactName,Email,ReturnBy,AttentionOf")] Consignment consignment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Consignments.Add(consignment);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { consignment.ID });
                }
            }
            catch (DataException /* dex */)
            {
                // Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            List<ApplicationUser> users = await userManager.Users.OrderBy(o => o.FirstName).ToListAsync();
            ApplicationUser user = await userManager.FindByEmailAsync(User.Identity.Name);
            ViewBag.SalesPersonID = new SelectList(users, "Id", "FullName", consignment.SalesPersonID);
            return View(consignment);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            List<ApplicationUser> users = await userManager.Users.OrderBy(o => o.FirstName).ToListAsync();
            ApplicationUser user = await userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.SalesPersonID = new SelectList(users, "Id", "FullName", user.Id);
            return View(consignment);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            if (TryUpdateModel(consignment, "", new string[] { "SalesPersonID", "DispatchDate", "Campus", "Locality", "ContactName", "Email",
                "ReturnBy", "NumberOfSales", "SaleDates", "AttentionOf", "CompanyName", "Street1", "Street2", "Town", "County", "Postcode", "Country" }))
            {
                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = consignment.ID });
                }
                catch (DataException /* dex */)
                {
                    // Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            List<ApplicationUser> users = await userManager.Users.OrderBy(o => o.FirstName).ToListAsync();
            ApplicationUser user = await userManager.FindByEmailAsync(User.Identity.Name);
            ViewBag.SalesPersonID = new SelectList(users, "Id", "FullName", user.Id);
            return View(consignment);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Consignment consignment = await db.Consignments.FindAsync(id);
            if (consignment == null)
                return HttpNotFound();
            return View(consignment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Consignment consignment = await db.Consignments.FindAsync(id);
            db.Consignments.Remove(consignment);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}