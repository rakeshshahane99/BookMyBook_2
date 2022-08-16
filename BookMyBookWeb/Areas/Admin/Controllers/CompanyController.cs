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
    public class CompanyController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        public CompanyController(IUnitofwork unitofwork)
        {
            _unitofwork = unitofwork;    
        }
        public IActionResult Index()
        {
            return View();         
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            

            if (id == null || id == 0)
            {
                //Create Product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(company);
            }
            else
            {
                company = _unitofwork.Company.GetFirstOrDefault(u=>u.Id==id);
                return View(company);
                //Update product

            }
            
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
           
            if (ModelState.IsValid)
            {
                
                if (obj.Id == 0)
                {
                    _unitofwork.Company.Add(obj);
                }
                else 
                {
                    _unitofwork.Company.Update(obj);
                }
               
                _unitofwork.Save();
                TempData["Success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj); ;
        }     
       

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        { 
            var companyList = _unitofwork.Company.GetAll();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            //var obj = _db.Categories.Find(id);
            var obj = _unitofwork.Company.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
               return Json(new { success=false, message="Error while deleting" });
            }
            
            _unitofwork.Company.Remove(obj);
            _unitofwork.Save();
            return Json(new { success = true, message = "Company deleted successfully" });
            
        }
        #endregion

    }
}
