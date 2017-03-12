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
using System.Text;
using OfficeOpenXml;

namespace RwOrders.Controllers
{
    public class EventDaysController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            var model = await db.EventDays
                .OrderByDescending(d => d.EventDate)
                .Select(d => new EventDayViewModel
                {
                    ID = d.ID,
                    Name = d.Name,
                    EventDate = d.EventDate,
                    Total = d.Orders.Sum(o => o.OrderItems.Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) - o.Vouchers) ?? 0
                }).ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            EventDay eventDay = await db.EventDays.FindAsync(id);
            if (eventDay == null)
                return HttpNotFound();
            ViewBag.EventDayID = eventDay.ID;
            var model = new EventDayViewModel
            {
                ID = eventDay.ID,
                CreatedBy = eventDay.CreatedBy?.FullName,
                Name = eventDay.Name,
                EventDate = eventDay.EventDate,
                Total = eventDay.Orders.Sum(o => o.OrderItems.Sum(i => i.Quantity * i.UnitPrice) - o.Vouchers)
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,EventDate")] EventDay eventDay)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    eventDay.CreatedByID = User.Identity.GetId();
                    db.EventDays.Add(eventDay);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = eventDay.ID });
                }
            }
            catch (DataException /* dex */)
            {
                // Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(eventDay);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            EventDay eventDay = await db.EventDays.FindAsync(id);
            if (eventDay == null)
                return HttpNotFound();
            ViewBag.EventDayID = eventDay.ID;
            return View(eventDay);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            EventDay eventDay = await db.EventDays.FindAsync(id);
            if (eventDay == null)
                return HttpNotFound();
            if (TryUpdateModel(eventDay, "", new string[] { "Name", "EventDate" }))
            {
                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = eventDay.ID });
                }
                catch (DataException /* dex */)
                {
                    // Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(eventDay);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            EventDay eventDay = await db.EventDays.FindAsync(id);
            if (eventDay == null)
                return HttpNotFound();
            ViewBag.EventDayID = eventDay.ID;
            var model = new EventDayViewModel
            {
                ID = eventDay.ID,
                CreatedBy = eventDay.CreatedBy?.FullName,
                Name = eventDay.Name,
                EventDate = eventDay.EventDate,
                Total = eventDay.Orders.Sum(o => o.OrderItems.Sum(i => i.Quantity * i.UnitPrice))
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            EventDay eventDay = await db.EventDays.FindAsync(id);
            db.EventDays.Remove(eventDay);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DownloadOrders(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var orders = await db.Orders.Where(o => o.EventDayID == id)
                .OrderBy(o => o.ID)
                .Select(o => new
                {
                    o.ID,
                    EventName = o.EventDay.Name,
                    TakenBy = o.TakenBy.FirstName + " " + o.TakenBy.LastName,
                    o.CustomerName,
                    o.Locality,
                    o.Email,
                    o.AccountNo,
                    o.Notes,
                    o.PaymentMethod,
                    o.Vouchers
                }).ToListAsync();

            var items = await db.OrderItems.Where(i => i.Order.EventDayID == id)
                .OrderBy(i => i.OrderID).ThenBy(i => i.ID)
                .Select(i => new
                {
                    i.ID,
                    i.OrderID,
                    i.StockCode,
                    i.Description,
                    i.Quantity,
                    i.UnitPrice
                }).ToListAsync();

            using (ExcelPackage pck = new ExcelPackage())
            {
                string[] orderFields = { "ID", "EventName", "TakenBy", "CustomerName", "Locality", "Email", "AccountNo", "Notes", "PaymentMethod", "Vouchers", "ItemsTotal" };
                ExcelWorksheet wsOrders = pck.Workbook.Worksheets.Add("Orders");
                wsOrders.Cells[1, 1, 1, orderFields.Length].Style.Font.Bold = true;

                for (int i = 0; i < orderFields.Length; i++)
                    wsOrders.Cells[1, i + 1].Value = orderFields[i];

                for (int i = 0; i < orders.Count; i++)
                {
                    wsOrders.SetValue(i + 2, 1, orders[i].ID);
                    wsOrders.SetValue(i + 2, 2, orders[i].EventName);
                    wsOrders.SetValue(i + 2, 3, orders[i].TakenBy);
                    wsOrders.SetValue(i + 2, 4, orders[i].CustomerName);
                    wsOrders.SetValue(i + 2, 5, orders[i].Locality);
                    wsOrders.SetValue(i + 2, 6, orders[i].Email);
                    wsOrders.SetValue(i + 2, 7, orders[i].AccountNo);
                    wsOrders.SetValue(i + 2, 8, orders[i].Notes);
                    wsOrders.SetValue(i + 2, 9, orders[i].PaymentMethod);
                    wsOrders.SetValue(i + 2, 10, orders[i].Vouchers);
                }

                string[] itemFields = { "ID", "OrderID", "StockCode", "Description", "Quantity", "UnitPrice", "Total" };
                ExcelWorksheet wsItems = pck.Workbook.Worksheets.Add("Items");
                wsItems.Cells[1, 1, 1, itemFields.Length].Style.Font.Bold = true;

                for (int i = 0; i < itemFields.Length; i++)
                    wsItems.Cells[1, i + 1].Value = itemFields[i];

                for (int i = 0; i < items.Count; i++)
                {
                    wsItems.SetValue(i + 2, 1, items[i].ID);
                    wsItems.SetValue(i + 2, 2, items[i].OrderID);
                    wsItems.SetValue(i + 2, 3, items[i].StockCode);
                    wsItems.SetValue(i + 2, 4, items[i].Description);
                    wsItems.SetValue(i + 2, 5, items[i].Quantity);
                    wsItems.SetValue(i + 2, 6, items[i].UnitPrice);
                    wsItems.Cells[i + 2, 7].FormulaR1C1 = "=RC[-2]*RC[-1]";
                }

                for (int i = 2; i <= orders.Count + 1; i++)
                    wsOrders.Cells[i, 11].FormulaR1C1
                        = $"=SUMIF(Items!R2C2:R{items.Count + 1}C2,Orders!RC1,Items!R2C7:R{items.Count + 1}C7)";

                wsOrders.Cells[wsOrders.Dimension.Address].AutoFitColumns();
                wsItems.Cells[wsItems.Dimension.Address].AutoFitColumns();

                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DownloadProductSales(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var products = await db.OrderItems
                .Where(o => o.Order.EventDayID == id)
                .GroupBy(o => o.ProductID)
                .OrderByDescending(o => o.Sum(i => i.Quantity * i.UnitPrice))
                .Select(o => new
                {
                    StockCode = o.FirstOrDefault().StockCode,
                    Description = o.FirstOrDefault().Description,
                    Quantity = o.Sum(s => s.Quantity),
                    UnitPrice = o.FirstOrDefault().UnitPrice
                }).ToListAsync();

            using (ExcelPackage pck = new ExcelPackage())
            {
                string[] fields = { "StockCode", "Description", "Quantity", "UnitPrice", "Total" };
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Products");
                ws.Cells[1, 1, 1, fields.Length].Style.Font.Bold = true;

                for (int i = 0; i < fields.Length; i++)
                    ws.Cells[1, i + 1].Value = fields[i];

                for (int i = 0; i < products.Count; i++)
                {
                    ws.SetValue(i + 2, 1, products[i].StockCode);
                    ws.SetValue(i + 2, 2, products[i].Description);
                    ws.SetValue(i + 2, 3, products[i].Quantity);
                    ws.SetValue(i + 2, 4, products[i].UnitPrice);
                    ws.Cells[i + 2, 5].FormulaR1C1 = "=RC[-2]*RC[-1]";
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                return File(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
