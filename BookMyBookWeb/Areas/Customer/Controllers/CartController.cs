using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using BookMyBook_Models.ViewModels;
using BookMyBook_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookMyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitofwork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader= new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser=_unitofwork.ApplicationUser.GetFirstOrDefault(u=>u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name=ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            ApplicationUser applicationUser = _unitofwork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PayementStatus = SD.PayementPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else 
            {
                ShoppingCartVM.OrderHeader.PayementStatus = SD.PayementDelayedPayement;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }


            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitofwork.OrderDetail.Add(orderDetail);
                _unitofwork.Save();
            }



            //Stripe Settings
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:44354/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/index",
                };

                foreach (var item in ShoppingCartVM.ListCart)
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
                ShoppingCartVM.OrderHeader.SessionId = session.Id;
                ShoppingCartVM.OrderHeader.PayementIntentId = session.PaymentIntentId;
                _unitofwork.OrderHeader.Update(ShoppingCartVM.OrderHeader);
                _unitofwork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id= ShoppingCartVM.OrderHeader.Id});

            }
        }
        public IActionResult OrderConfirmation(int id) 
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(u=>u.Id==id);
            if (orderHeader.PayementStatus !=SD.PayementDelayedPayement)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderHeader.PayementStatus = SD.PayementStatusApproved;
                    orderHeader.OrderStatus = SD.StatusApproved;
                    orderHeader.PayementDate = DateTime.Now;
                    _unitofwork.OrderHeader.Update(orderHeader);
                    _unitofwork.Save();
                }

            }
            
            List<ShoppingCart> shoppingCarts = _unitofwork.ShoppingCart.GetAll(u=>u.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
            _unitofwork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitofwork.Save();
            return View(id);
        }
        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price50;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }
        public IActionResult plus(int cartId) 
        { 
            var cart=_unitofwork.ShoppingCart.GetFirstOrDefault(u=>u.Id == cartId);
           _unitofwork.ShoppingCart.IncrementCount(cart, 1);
            _unitofwork.ShoppingCart.Update(cart);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));        
        }
        public IActionResult minus(int cartId)
        {
            var cart = _unitofwork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitofwork.ShoppingCart.Remove(cart);
            }
            else 
            {
                _unitofwork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitofwork.ShoppingCart.Update(cart);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitofwork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitofwork.ShoppingCart.Remove(cart);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
