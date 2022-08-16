using BookMyBook_DataAccess;
using BookMyBook_DataAccess.Repository;
using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using Microsoft.AspNetCore.Mvc;

namespace BookMyBookWeb.Controllers
{[Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        public CategoryController(IUnitofwork unitofwork)
        {
            _unitofwork = unitofwork;            
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objcategoryList = _unitofwork.Category.GetAll();
            return View(objcategoryList);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name==obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display order can not be same as Name");
            }
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                TempData["Success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj); 
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = _unitofwork.Category.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDb = _db.Categories.Find(id);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display order can not be same as Name");
            }
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["Success"] = "Category Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj); ;
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }
            //var CategoriesFromDB = _db.Categories.Find(id);
            var CategoriesFromDB = _unitofwork.Category.GetFirstOrDefault(c => c.Id == id);
            return View(CategoriesFromDB);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            //var obj = _db.Categories.Find(id);
            var obj = _unitofwork.Category.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitofwork.Category.Remove(obj);
            _unitofwork.Save();
            TempData["Success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
        }

    }
}
