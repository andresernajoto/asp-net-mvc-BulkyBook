using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnityOfWork _unityOfWork;

        // this do the implementation of database to controller
        public CompanyController(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            
            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unityOfWork.Company  .GetFirstOrDefault(x => x.Id == id);
                return View(company);
            }            
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unityOfWork.Company.Add(obj);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    _unityOfWork.Company.Update(obj);
                    TempData["success"] = "Company updated successfully";
                }

                _unityOfWork.Save();
                
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unityOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        // POST
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unityOfWork.Company.GetFirstOrDefault(x => x.Id == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unityOfWork.Company.Remove(obj);
            _unityOfWork.Save();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
