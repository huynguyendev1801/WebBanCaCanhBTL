using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanCaCanh.Models;
using WebBanCaCanh.Service;

namespace WebBanCaCanh.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        // Constructor sẽ nhận một thể hiện của ICategoryService thông qua DI
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category
        public async Task<ActionResult> Index()
        {
            // Gọi phương thức GetAllCategoriesAsync từ _categoryService để lấy tất cả các danh mục
            var categories = await _categoryService.GetAllCategoriesAsync();

            // Trả về view với danh sách các danh mục
            return View(categories);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        public async Task<ActionResult> Create(Category category, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                    imageFile.SaveAs(imagePath);
                    category.ImageUrl = imageName;
                }

                var result = await _categoryService.AddCategoryAsync(category);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Tìm danh mục cần chỉnh sửa
            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }
            // Trả về view với danh mục cần chỉnh sửa
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(Category category, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
                if (existingCategory == null)
                {
                    return HttpNotFound();
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/Content/Images/"), existingCategory.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                    imageFile.SaveAs(imagePath);
                    category.ImageUrl = imageName;
                }
                else
                {
                    category.ImageUrl = existingCategory.ImageUrl;
                }

                var result = await _categoryService.UpdateCategoryAsync(category);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(category);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return Json(new { success = false });
            }

            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result)
            {
                DeleteImageFile(category.ImageUrl);
            }

            return Json(new { success = result });
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }
    }
}
