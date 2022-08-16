using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using BookMyBook_Models.ViewModels;
using BookMyBook_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookMyBookWeb.Areas.Admin.Controllers
{[Area("Admin")]
[Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitofwork unitofwork)
        {
            _unitofwork=unitofwork;
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitofwork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_Pay_Now()
        {
             OrderVM.OrderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitofwork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");
            
            var domain = "https://localhost:44354/";
            //stripe settings
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PayementConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            OrderVM.OrderHeader.SessionId = session.Id;
            OrderVM.OrderHeader.PayementIntentId = session.PaymentIntentId;
            _unitofwork.OrderHeader.Update(OrderVM.OrderHeader);
            _unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            return View(OrderVM);
        }
        public IActionResult PayementConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderId);
            if (orderHeader.PayementStatus == SD.PayementDelayedPayement)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderHeader.PayementStatus = SD.PayementStatusApproved;
                    orderHeader.OrderStatus = orderHeader.OrderStatus;
                    orderHeader.PayementDate = DateTime.Now;
                    _unitofwork.OrderHeader.Update(orderHeader);
                    _unitofwork.Save();
                }

            }

            return View(orderHeaderId);
        }
        [HttpPost]
        public IActionResult UpdateOrderDetail()
        {
            var orderFromDb = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
            orderFromDb.Name=OrderVM.OrderHeader.Name;
            orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderFromDb.City = OrderVM.OrderHeader.City;
            orderFromDb.State = OrderVM.OrderHeader.State;
            orderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (OrderVM.OrderHeader.Carrier!=null)
            {
                orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitofwork.OrderHeader.Update(orderFromDb);
            _unitofwork.Save();
            TempData["Success"] = "Order Details updated successfully";
            return RedirectToAction("Details", "Order", new { orderId=orderFromDb.Id});

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult StartProcessing()
        {
            var orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitofwork.OrderHeader.Update(orderHeader);
            _unitofwork.Save();
            TempData["Success"] = "Order status updated successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeader.TrackingNumber=OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PayementStatus==SD.PayementDelayedPayement)
            {
                orderHeader.PayementDueDate = DateTime.Now.AddDays(30);
            }
            _unitofwork.OrderHeader.Update(orderHeader);
            _unitofwork.Save();
            TempData["Success"] = "Order shipped successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles =SD.Role_User_Admin + ","+ SD.Role_User_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            if (orderHeader.PayementStatus == SD.PayementStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = "ch_3LVxxOSGC5XYAVU11TlLPuTJ"
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PayementStatus = SD.StatusRefunded;
            }
            else 
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PayementStatus = SD.StatusCancelled;
            }
            _unitofwork.OrderHeader.Update(orderHeader);
            _unitofwork.Save();           
            TempData["Success"] = "Order cancelled successfully";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });

        }
        public IActionResult Index()
        {
            return View();
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_User_Admin) || User.IsInRole(SD.Role_User_Employee))
            {
                orderHeaders = _unitofwork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else 
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitofwork.OrderHeader.GetAll(u=>u.ApplicationUserId==claim.Value, includeProperties: "ApplicationUser");
            }
            switch (status) 
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u=>u.PayementStatus==SD.PayementDelayedPayement);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:                    
                    break;

            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
