using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanCaCanh.Models;
using WebBanCaCanh.Service;

namespace WebBanCaCanh.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly IBannerService _bannerService;

        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }
        public async Task<ActionResult> Index()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return View(banners);
        }
        public ActionResult Create()
        {
            return View();
        }
        // POST: Admin/Banner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Banner banner, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                    imageFile.SaveAs(imagePath);
                    banner.ImageUrl = imageName;
                }

                banner.CreatedAt = DateTime.Now;
                var result = await _bannerService.AddBannerAsync(banner);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(banner);
        }
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var banner = await _bannerService.GetBannerByIdAsync(id.Value);
            if (banner == null)
            {
                return HttpNotFound();
            }

            return View(banner);
        }
        // POST: Admin/Banner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Banner banner, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingBanner = await _bannerService.GetBannerByIdAsync(banner.BannerId);
                if (existingBanner == null)
                {
                    return HttpNotFound();
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(existingBanner.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/Content/Images/"), existingBanner.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new image
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(Server.MapPath("~/Content/Images/"), imageName);
                    imageFile.SaveAs(imagePath);
                    banner.ImageUrl = imageName;
                }
                else
                {
                    // Retain the old image URL if no new image is uploaded
                    banner.ImageUrl = existingBanner.ImageUrl;
                }

                var result = await _bannerService.UpdateBannerAsync(banner);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(banner);
        }
        [HttpPost]
      
        public async Task<JsonResult> Delete(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null)
            {
                return Json(new { success = false });
            }

            var result = await _bannerService.DeleteBannerAsync(id);
            if (result)
            {
                DeleteImageFile(banner.ImageUrl);
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
