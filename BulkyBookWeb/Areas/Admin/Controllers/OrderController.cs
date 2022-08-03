using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnityOfWork _unityOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        
        public OrderController(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _unityOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId,
                includeProperties: "ApplicationUser"),
                OrderDetail = _unityOfWork.OrderDetail.GetAll(x => x.OrderId == orderId,
                includeProperties: "Product")
        };

            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unityOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id,
                tracked: false);

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unityOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unityOfWork.Save();

            TempData["success"] = "Order details updated successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unityOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unityOfWork.Save();

            TempData["success"] = "Order status updated successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unityOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id,
                tracked: false);
            
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unityOfWork.OrderHeader.Update(orderHeader);
            _unityOfWork.Save();

            TempData["success"] = "Order shipped successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unityOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                orderHeaders = _unityOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == claim.Value,
                    includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
