using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanCaCanh.Models;
using WebBanCaCanh.Service;

namespace WebBanCaCanh.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<ActionResult> Index(int? page)
        {
            const int pageSize = 5;
            int pageNumber = (page ?? 1);

            var products = await _productService.GetAllProductsAsync();
            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            return View(pagedProducts);
        }

        // GET: Product/Create
        public async Task<ActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Product product, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;
            if (ModelState.IsValid)
            {
                // Thêm sản phẩm vào cơ sở dữ liệu
                var result = await _productService.AddProductAsync(product);
                if (result)
                {
                    // Xử lý các tệp ảnh
                    if (imageFiles != null)
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                                file.SaveAs(imagePath);

                                var productImage = new ProductImage
                                {
                                    ProductId = product.ProductId,
                                    ImageUrl = imageName
                                };
                                await _productService.AddProductImageAsync(productImage);
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
            }
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(Product product, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            product.UpdatedAt = DateTime.Now;
            if (ModelState.IsValid)
            {
                // Cập nhật thông tin sản phẩm
                var result = await _productService.UpdateProductAsync(product);
                if (result)
                {
                    // Xử lý các tệp ảnh
                    if (imageFiles != null)
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                                file.SaveAs(imagePath);

                                var productImage = new ProductImage
                                {
                                    ProductId = product.ProductId,
                                    ImageUrl = imageName
                                };
                                await _productService.AddProductImageAsync(productImage);
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
            }
            product = await _productService.GetProductByIdAsync(product.ProductId);
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteImage(int imageId)
        {
            var result = await _productService.DeleteProductImageAsync(imageId);
            return Json(new { success = result });
        }

    }
}
