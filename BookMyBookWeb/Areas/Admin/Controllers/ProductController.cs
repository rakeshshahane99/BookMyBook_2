using BookMyBook_DataAccess;
using BookMyBook_DataAccess.Repository;
using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using BookMyBook_Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookMyBookWeb.Controllers
{[Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitofwork unitofwork, IWebHostEnvironment hostEnvironment)
        {
            _unitofwork = unitofwork;            
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();         
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitofwork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList=_unitofwork.CoverType.GetAll().Select(i => new SelectListItem
                { 
                Text=i.Name,
                Value=i.Id.ToString()
                })
            };

            if (id == null || id == 0)
            {
                //Create Product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitofwork.Product.GetFirstOrDefault(u=>u.Id==id);
                return View(productVM);
                //Update product

            }
            
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\products\");
                    var extension = Path.GetExtension(file.FileName);
                    if (obj.Product.ImageUrl != null)
                    { 
                        var oldImagePath=Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }                    
                    }

                    using (var filestream = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    obj.Product.ImageUrl = Path.Combine(uploads + filename + extension);
                }
                if (obj.Product.Id == 0)
                {
                    _unitofwork.Product.Add(obj.Product);
                }
                else 
                {
                    _unitofwork.Product.Update(obj.Product);
                }
               
                _unitofwork.Save();
                TempData["Success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj); ;
        }     
       

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        { 
            var productList=_unitofwork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }
        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            //var obj = _db.Categories.Find(id);
            var obj = _unitofwork.Product.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
               return Json(new { success=false, message="Error while deleting" });
            }
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitofwork.Product.Remove(obj);
            _unitofwork.Save();
            return Json(new { success = true, message = "Product deleted successfully" });
            
        }
        #endregion

    }
}
