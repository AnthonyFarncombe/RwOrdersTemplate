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
using System.IO;
using System.Text.RegularExpressions;
using EntityFramework.Extensions;

namespace RwOrders.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Products.Where(p => !p.Inactive).OrderBy(p => p.StockCode).ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            var orders = await db.OrderItems
                .Where(i => i.ProductID == product.ID)
                .Select(i => new ProductOrderViewModel
                {
                    ID = i.OrderID,
                    EventDate = i.Order.EventDay.EventDate,
                    EventName = i.Order.EventDay.Name,
                    CustomerName = i.Order.CustomerName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToListAsync();
            ViewBag.Orders = orders;
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "StockCode,Description,UnitPrice")] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Product existing = await db.Products.Where(p => p.StockCode == product.StockCode).FirstOrDefaultAsync();
                    if (existing != null)
                    {
                        if (!existing.Inactive)
                            return RedirectToAction("Details", new { id = existing.ID });
                        existing.Description = product.Description;
                        existing.UnitPrice = product.UnitPrice;
                        existing.Inactive = false;
                        db.Entry(existing).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        return RedirectToAction("Details", new { id = existing.ID });
                    }
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = product.ID });
                }
            }
            catch (DataException /* dex */)
            {
                // Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            if (TryUpdateModel(product, "", new string[] { "StockCode", "Description", "UnitPrice" }))
            {
                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = product.ID });
                }
                catch (DataException /* dex */)
                {
                    // Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            Product product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                Product product = await db.Products.FindAsync(id);
                if (product.OrderItems.Count > 0 || product.ConsignmentItems.Count > 0)
                {
                    product.Inactive = true;
                    db.Entry(product).Property(p => p.Inactive).IsModified = true;
                }
                else
                    db.Products.Remove(product);
                await db.SaveChangesAsync();
            }
            catch (DataException /* dex */)
            {
                // Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GetProductDetails(string stockCode)
        {
            if (string.IsNullOrEmpty(stockCode))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = await db.Products.Where(p => p.StockCode == stockCode && !p.Inactive).FirstOrDefaultAsync();
            if (product == null)
                return HttpNotFound();
            return Json(new { product.ID, product.StockCode, product.Description, product.UnitPrice }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Import(string error)
        {
            if (!string.IsNullOrEmpty(error))
                ModelState.AddModelError("", error);
            List<Product> import = Session["ProductCsvImport"] as List<Product>;
            if (import != null)
            {
                List<string> existing = await db.Products.Select(p => p.StockCode).ToListAsync();
                ViewBag.ImportCount = import.Count;
                ViewBag.NewCount = import.Select(p => p.StockCode).Except(existing).Count();
                ViewBag.MatchedCount = import.Count - ViewBag.NewCount;
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadCsvProducts(HttpPostedFileBase file)
        {
            Session["ProductCsvImport"] = null;
            if (file != null && file.ContentLength > 0)
            {
                List<Product> products = new List<Product>();
                using (Microsoft.VisualBasic.FileIO.TextFieldParser parser
                    = new Microsoft.VisualBasic.FileIO.TextFieldParser(file.InputStream))
                {
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");
                    string[] fields;
                    bool headersOkay = false;
                    int i = 0;

                    while (!parser.EndOfData)
                    {
                        i++;
                        fields = parser.ReadFields();

                        if (fields.Length != 3)
                            return RedirectToAction("Import", new { error = $"{fields.Length} fields found in row {i} instead of 3" });

                        if (!headersOkay)
                        {
                            if (fields[0] != "StockCode" || fields[1] != "Description" || fields[2] != "UnitPrice")
                                return RedirectToAction("Import", new { error = "Incorrect column headers" });
                            headersOkay = true;
                            continue;
                        }

                        Product product = new Product { StockCode = fields[0].ToUpper(), Description = fields[1] };
                        if (string.IsNullOrWhiteSpace(product.StockCode) || !Regex.IsMatch(product.StockCode, @"^[A-Z\d]+$") || product.StockCode.Length > 20)
                            return RedirectToAction("Import", new { error = $"Invalid product code in line {i}" });
                        if (string.IsNullOrWhiteSpace(product.Description) || product.Description.Length > 100)
                            return RedirectToAction("Import", new { error = $"Invalid description in line {i}" });
                        decimal unitPrice = 0;
                        if (!decimal.TryParse(fields[2], out unitPrice))
                            return RedirectToAction("Import", new { error = $"Invalid unit price in line {i}" });
                        product.UnitPrice = unitPrice;
                        products.Add(product);
                    }
                }
                Session["ProductCsvImport"] = products;
                return RedirectToAction("Import");
            }
            else
                return RedirectToAction("Import", new { error = "Error uploading csv file" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> ImportCsvProducts(bool? overwrite = false)
        {
            List<Product> import = Session["ProductCsvImport"] as List<Product>;
            if (import == null)
                return RedirectToAction("Import");
            if (overwrite.GetValueOrDefault())
                    await db.Products.UpdateAsync(p => new Product { Inactive = true });
            List<string> existing = await db.Products.Select(p => p.StockCode).ToListAsync();
            for (int i = import.Count - 1; i >= 0; i--)
            {
                string stockCode = import[i].StockCode;
                if (existing.Contains(stockCode))
                {
                    Product product = await db.Products.Where(p => p.StockCode == stockCode).FirstOrDefaultAsync();
                    product.Description = import[i].Description;
                    product.UnitPrice = import[i].UnitPrice;
                    product.Inactive = false;
                    db.Entry(product).State = EntityState.Modified;
                    import.Remove(import[i]);
                }
            }
            import.ForEach(p => db.Products.Add(p));
            await db.SaveChangesAsync();
            Session["ProductCsvImport"] = null;
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
