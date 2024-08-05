using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanCaCanh.Service;
using WebBanCaCanh.Models;

namespace WebBanCaCanh.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: Order
        public async Task<ActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<ActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return PartialView("_OrderDetailsPartial", order); // Return partial view for modal
        }
        // POST: Order/EditStatus/5
        [HttpPost]
        public async Task<ActionResult> EditStatus(int id, string status)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.OrderStatus = status;

            if (await _orderService.UpdateOrderAsync(order))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (await _orderService.DeleteOrderAsync(id))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

    }
}
