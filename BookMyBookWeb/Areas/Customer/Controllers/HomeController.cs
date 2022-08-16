using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using BookMyBook_Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookMyBookWeb.Controllers
{[Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofwork _unitofwork;
        public HomeController(ILogger<HomeController> logger, IUnitofwork unitofwork)
        {
            _logger = logger;
            _unitofwork=unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> prodcutlist = _unitofwork.Product.GetAll(includeProperties:"Category,CoverType");
            return View(prodcutlist);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cartobj = new() 
            {
                Product = _unitofwork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType"),
                ProductId = productId,
                Count = 1
            };
            
            return View(cartobj);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)   ;
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDB = _unitofwork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (cartFromDB == null)
            {
                _unitofwork.ShoppingCart.Add(shoppingCart);
            }
            else 
            {
            
                _unitofwork.ShoppingCart.IncrementCount(cartFromDB, shoppingCart.Count);
                _unitofwork.ShoppingCart.Update(cartFromDB);
            }
         
            _unitofwork.Save();
           

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}