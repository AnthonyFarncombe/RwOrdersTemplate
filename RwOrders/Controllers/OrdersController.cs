using RwOrders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;
using PagedList;
using Microsoft.AspNet.Identity.Owin;
using System.Data;

namespace RwOrders.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index(int? eventDayID, string currentFilter, string searchString, int? page)
        {
            if (eventDayID == null)
                return RedirectToAction("Index", "EventDays");
            ViewBag.EventDayID = eventDayID;
            IQueryable<Order> orders = db.Orders.Where(o => o.EventDayID == eventDayID);
            if (await orders.CountAsync() > 0)
            {
                decimal itemTotal = (await db.OrderItems.Where(o => o.Order.EventDayID == eventDayID).SumAsync(o => (decimal?)(o.Quantity * o.UnitPrice))) ?? 0;
                decimal voucherTotal = await orders.SumAsync(o => o.Vouchers);
                ViewBag.Total = itemTotal - voucherTotal;
            }
            else
                ViewBag.Total = 0;

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            if (!string.IsNullOrEmpty(searchString))
                searchString = searchString.Trim();
            ViewBag.CurrentFilter = searchString;

            if (!string.IsNullOrEmpty(searchString))
                orders = orders.Where(o => o.CustomerName.Contains(searchString)
                    || o.Locality.Contains(searchString) || o.Email.Contains(searchString)
                    || o.OrderItems.Any(i => i.StockCode.Contains(searchString)) || o.OrderItems.Any(i => i.Description.Contains(searchString)));
            orders = orders.OrderBy(o => o.ID);

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create(int? eventDayID)
        {
            if (eventDayID == null)
                return RedirectToAction("Index", "EventDays");
            ViewBag.EventDayID = eventDayID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "EventDayID,CustomerName,Locality,Email,AccountNo,Notes,PaymentMethod,Vouchers")]Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ApplicationUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                    order.TakenByID = user.Id;
                    db.Orders.Add(order);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Edit", new { id = order.ID });
                }
            }
            catch (DataException /* dex */)
            {
                // Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.EventDayID = order.EventDayID;
            return View(order);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();
            ViewBag.EventDayID = order.EventDayID;
            OrderViewModel ovm = new OrderViewModel
            {
                ID = order.ID,
                CustomerName = order.CustomerName,
                Locality = order.Locality,
                Email = order.Email,
                AccountNo = order.AccountNo,
                EmailConfirmation = false,
                Notes = order.Notes,
                PaymentMethod = order.PaymentMethod,
                Vouchers = order.Vouchers
            };
            order.OrderItems.ToList().ForEach(i => ovm.OrderItems.Add(new OrderItemViewModel
                {
                    ID = i.ID,
                    ProductID = i.ProductID,
                    StockCode = i.StockCode,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }));
            return View(ovm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(OrderViewModel ovm)
        {
            if (ModelState.IsValid)
            {
                Order order = await db.Orders.FindAsync(ovm.ID);
                if (order == null)
                    return HttpNotFound();
                order.CustomerName = ovm.CustomerName;
                order.Locality = ovm.Locality;
                order.Email = ovm.Email;
                order.AccountNo = ovm.AccountNo;
                order.Notes = ovm.Notes;
                order.PaymentMethod = ovm.PaymentMethod;
                order.Vouchers = ovm.Vouchers;

                // Remove items
                List<OrderItem> oldItems = (from x in order.OrderItems
                                            join y in ovm.OrderItems on x.ID equals y.ID into gp
                                            from y in gp.DefaultIfEmpty()
                                            where y == null
                                            select x).ToList();
                oldItems.ForEach(i => db.OrderItems.Remove(i));
                await db.SaveChangesAsync();

                // Add items
                List<OrderItemViewModel> newItems = (from x in ovm.OrderItems
                                                     join y in order.OrderItems on x.ID equals y.ID into gp
                                                     from y in gp.DefaultIfEmpty()
                                                     where y == null
                                                     select x).ToList();
                newItems.ForEach(i => order.OrderItems.Add(new OrderItem
                    {
                        ID = i.ID,
                        ProductID = i.ProductID,
                        StockCode = i.StockCode,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }));
                await db.SaveChangesAsync();

                // Update items
                List<OrderItemViewModel> updateItems = (from x in ovm.OrderItems
                                                        join y in order.OrderItems on x.ID equals y.ID
                                                        where x.Quantity != y.Quantity || x.UnitPrice != x.UnitPrice
                                                        select x).ToList();
                foreach (OrderItemViewModel oivm in updateItems)
                {
                    OrderItem oi = order.OrderItems.Where(i => i.ID == oivm.ID).FirstOrDefault();
                    oi.Quantity = oivm.Quantity;
                    oi.UnitPrice = oivm.UnitPrice;
                    db.Entry(oi).State = EntityState.Modified;
                }
                await db.SaveChangesAsync();

                if (ovm.EmailConfirmation)
                    await RubyConfig.EmailConfirmation(ovm);
                return RedirectToAction("Details", new { id = ovm.ID });
            }
            ViewBag.EventDayID = await db.Orders.Where(o => o.ID == ovm.ID).Select(o => o.EventDayID).FirstOrDefaultAsync();
            return View(ovm);
        }

        public async Task<ActionResult> FindProduct(string stockCode)
        {
            if (string.IsNullOrEmpty(stockCode))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = await db.Products.Where(p => p.StockCode == stockCode && !p.Inactive).FirstOrDefaultAsync();
            if (product == null)
                return HttpNotFound();
            return Json(new
                {
                    ID = product.ID,
                    StockCode = product.StockCode,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice
                }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();
            ViewBag.EventDayID = order.EventDayID;
            ViewBag.TotalValue = order.OrderItems.Sum(o => o.Quantity * o.UnitPrice);
            //ViewBag.TotalQty = order.OrderItems.Sum(o => o.Quantity);
            return View(order);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
                return HttpNotFound();
            ViewBag.EventDayID = order.EventDayID;
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Order order = await db.Orders.FindAsync(id);
            int eventDayID = order.EventDayID;
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { eventDayID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}