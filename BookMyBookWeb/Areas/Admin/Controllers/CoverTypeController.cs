using BookMyBook_DataAccess;
using BookMyBook_DataAccess.Repository;
using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using Microsoft.AspNetCore.Mvc;

namespace BookMyBookWeb.Controllers
{[Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        public CoverTypeController(IUnitofwork unitofwork)
        {
            _unitofwork = unitofwork;            
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> objcoverTypeList = _unitofwork.CoverType.GetAll();
            return View(objcoverTypeList);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {            
            if (ModelState.IsValid)
            {
                _unitofwork.CoverType.Add(obj);
                _unitofwork.Save();
                TempData["Success"] = "Cover Type Created Successfully";
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
            var covertype = _unitofwork.CoverType.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDb = _db.Categories.Find(id);
            return View(covertype);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
           
            if (ModelState.IsValid)
            {
                _unitofwork.CoverType.Update(obj);
                _unitofwork.Save();
                TempData["Success"] = "Cover Type Updated Successfully";
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
            var CoverTypesFromDB = _unitofwork.CoverType.GetFirstOrDefault(c => c.Id == id);
            return View(CoverTypesFromDB);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            //var obj = _db.Categories.Find(id);
            var obj = _unitofwork.CoverType.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitofwork.CoverType.Remove(obj);
            _unitofwork.Save();
            TempData["Success"] = "CoverTypes Deleted Successfully";
            return RedirectToAction("Index");
        }

    }
}
