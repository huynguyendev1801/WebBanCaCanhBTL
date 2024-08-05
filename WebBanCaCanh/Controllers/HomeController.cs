using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanCaCanh.Models;
using WebBanCaCanh.Service;
using WebBanCaCanh.ViewModels;

namespace WebBanCaCanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IBannerService _bannerService;
        private readonly IProductService _productService;
        private readonly INewsService _newsService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        public HomeController(ICategoryService categoryService, IBannerService bannerService, IProductService productService, INewsService newsService, IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _categoryService = categoryService;
            _bannerService = bannerService;
            _productService = productService;
          _productService = productService;
            _newsService = newsService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;

        }
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = await userManager.FindByIdAsync(User.Identity.GetUserId());

                if (user != null && await userManager.IsInRoleAsync(user.Id, "Admin"))
                {
                    return RedirectToAction("Index", "AdminHome", new { area = "Admin" });
                }
            }
            var categories = await _categoryService.GetAllCategoriesWithProductsAsync();
            var banners = await _bannerService.GetAllBannersAsync();
            var newsList = await _newsService.GetAllNewsAsync(); // Fetch news items from the database
            ViewBag.News = newsList;
            ViewBag.Banners = banners;
            return View(categories);
        }

        public async Task<ActionResult> Product(int? categoryId, string sortBy = "name_asc")
        {
            var category = await _categoryService.GetCategoryWithProductsByCategoryIdAsync(categoryId);

            // Sorting logic (if applicable)
            switch (sortBy)
            {
                case "name_desc":
                    category.Products = category.Products.OrderByDescending(p => p.ProductName).ToList();
                    break;
                case "price_asc":
                    category.Products = category.Products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    category.Products = category.Products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "newest":
                    category.Products = category.Products.OrderByDescending(p => p.UpdatedAt).ToList();
                    break;
                default: 
                    category.Products = category.Products.OrderBy(p => p.ProductName).ToList();
                    break;
            }

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.CurrentSort = sortBy;

            return View(category);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }

            var relativeProducts = await _productService.GetRelativeProductsAsync(id.Value);
            ViewBag.RelativeProducts = relativeProducts;

            return View(product);
        }

        public async Task<ActionResult> News(int page = 1, int pageSize = 5)
        {
            var allNews = await _newsService.GetAllNewsAsync();
            var pagedNews = allNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new PagedNewsViewModel
            {
                NewsList = pagedNews,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)allNews.Count / pageSize)
            };

            return View(viewModel);
        }

        public async Task<ActionResult> NewsDetail(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return HttpNotFound();
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            var allNews = await _newsService.GetAllNewsAsync();
            var relatedNews = allNews.Where(n => n.NewsId != id)
                                     .OrderByDescending(n => n.CreatedAt)
                                     .Take(5)
                                     .ToList();

            var model = new NewsDetailViewModel
            {
                News = news,
                Categories = categories,
                RelatedNews = relatedNews
            };

            return View(model);
        }


        public async Task<ActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {

                return RedirectToAction("Index");
            }


            var searchResults = await _productService.SearchProductsAsync(query);


            return View(searchResults);
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<OrderDetail>();
            }

            var cart = (List<OrderDetail>)Session["Cart"];

            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new OrderDetail
                {
                    ProductId = productId,
                    Product = product, 
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                cart.Add(newItem);
            }

            Session["Cart"] = cart;
            return RedirectToAction("Cart");
        }

        public async Task<ActionResult> Cart()
        {
            if (Session["Cart"] == null)
            {
                return View();
            }

            var cart = (List<OrderDetail>)Session["Cart"];
            if (cart.Count == 0)
            {
                return View();
            }

            return View(cart);
        }
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            if (Session["Cart"] != null)
            {
                var cart = (List<OrderDetail>)Session["Cart"];
                var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);

                    Session["Cart"] = cart;
                }
            }

            return RedirectToAction("Cart");
        }
        [HttpPost]
        public ActionResult UpdateCart(int productId, string action)
        {
            if (Session["Cart"] != null)
            {
                var cart = (List<OrderDetail>)Session["Cart"];
                var itemToUpdate = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToUpdate != null)
                {
                    if (action == "increase")
                    {
                        itemToUpdate.Quantity++;
                    }
                    else if (action == "decrease")
                    {
                        if (itemToUpdate.Quantity > 1)
                        {
                            itemToUpdate.Quantity--;
                        }
                    }

                    Session["Cart"] = cart;
                }
            }

            return RedirectToAction("Cart");
        }


        public async Task<ActionResult> Checkout()
        {
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Cart");
            }

            var userId = User.Identity.GetUserId();
            var recentOrder = await _orderService.GetMostRecentOrderByUserAsync(userId);

            var model = new CheckoutViewModel();

            if (recentOrder != null)
            {
                model.CustomerName = recentOrder.CustomerName;
                model.PhoneNumber = recentOrder.PhoneNumber;
                model.City = recentOrder.City;
                model.District = recentOrder.District;
                model.Address = recentOrder.Address;
                model.Note = recentOrder.Note;
                model.PaymentMethod = recentOrder.PaymentMethod;
            }

            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Checkout(CheckoutViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

                if (Session["Cart"] != null)
                {
                    var cart = (List<OrderDetail>)Session["Cart"];

                    var order = new Order
                    {
                        OrderDate = DateTime.Now,
                        OrderStatus = "Chưa duyệt",
                        UserId = user != null ? user.Id : null,
                        CustomerName = model.CustomerName,
                        PhoneNumber = model.PhoneNumber,
                        City = model.City,
                        District = model.District,
                        Address = model.Address,
                        Note = model.Note,
                        PaymentMethod = model.PaymentMethod
                    };

                    var isOrderAdded = await _orderService.AddOrderAsync(order);

                    if (isOrderAdded != 0)
                    {
                        Session.Remove("Cart");

                        foreach (var orderDetail in cart)
                        {
                            orderDetail.OrderId = isOrderAdded;
                            orderDetail.Product = null;

                            await _orderDetailService.AddOrderDetailAsync(orderDetail);
                        }

                        return RedirectToAction("CheckoutSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "There was an error processing your order. Please try again.");
                    }
                }
            }
            return View("Checkout", model);
        }
        public ActionResult CheckoutSuccess()
        {
            return View();
        }

        protected int GetCartTotalQuantity()
        {
            if (Session["Cart"] != null)
            {
                var cart = (List<OrderDetail>)Session["Cart"];
                return cart.Count();
            }
            return 0;
        }

        [HttpGet]
        public JsonResult GetTotalQuantity()
        {
            try
            {
                int totalQuantity = GetCartTotalQuantity();
                return Json(new { success = true, totalQuantity }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching the total quantity." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var categoryViewModels = categories.Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList();

            return Json(categoryViewModels, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBreadcrumb(string pageType, int? id = null)
        {
            var breadcrumbs = new BreadcrumbViewModel();
            breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = false });

            switch (pageType.ToLower())
            {
                case "index":
                    // Home page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Trang chủ", Url = Url.Action("Index", "Home"), IsActive = true });
                    break;

                case "news":
                    // News listing page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Kiến Thức Nuôi Cá Cảnh", Url = Url.Action("News", "Home"), IsActive = true });
                    break;

                case "newsdetail":
                    // News detail page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Kiến Thức Nuôi Cá Cảnh", Url = Url.Action("News", "Home"), IsActive = false });
                    if (id.HasValue)
                    {
                        var news = await _newsService.GetNewsByIdAsync(id.Value);
                        if (news != null)
                        {
                            breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = news.Title, Url = Url.Action("NewsDetail", "Home", new { id = news.NewsId }), IsActive = true });
                        }
                    }
                    break;

                case "product":
                    // Product listing page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Danh Sách Sản Phẩm", Url = Url.Action("Product", "Home"), IsActive = true });
                    if (id.HasValue)
                    {
                        var category = await _categoryService.GetCategoryWithProductsByCategoryIdAsync(id.Value);
                        if (category != null)
                        {
                            breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = category.CategoryName, Url = Url.Action("Product", "Home", new { categoryId = category.CategoryId }), IsActive = true });
                        }
                    }
                    break;

                case "details":
                    // Product detail page
                    if (id.HasValue)
                    {
                        var product = await _productService.GetProductByIdAsync(id.Value);
                        if (product != null)
                        {
                            var productCategory = await _categoryService.GetCategoryWithProductsByCategoryIdAsync(product.CategoryId);
                            if (productCategory != null)
                            {
                                breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Danh Sách Sản Phẩm", Url = Url.Action("Product", "Home"), IsActive = false });
                                breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = productCategory.CategoryName, Url = Url.Action("Product", "Home", new { categoryId = productCategory.CategoryId }), IsActive = false });
                            }
                            breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = product.ProductName, Url = Url.Action("Details", "Home", new { id = product.ProductId }), IsActive = true });
                        }
                    }
                    break;

                case "search":
                    // Search results page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Tìm Kiếm", Url = Url.Action("Search", "Home"), IsActive = true });
                    break;

                case "cart":
                    // Cart page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Giỏ Hàng", Url = Url.Action("Cart", "Home"), IsActive = true });
                    break;

                case "checkout":
                    // Checkout page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Giỏ Hàng", Url = Url.Action("Cart", "Home"), IsActive = false });
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Thanh Toán", Url = Url.Action("Checkout", "Home"), IsActive = true });
                    break;

                case "checkoutsuccess":
                    // Checkout success page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Giỏ Hàng", Url = Url.Action("Cart", "Home"), IsActive = false });
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Thanh Toán", Url = Url.Action("Checkout", "Home"), IsActive = false });
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Thanh Toán Thành Công", Url = Url.Action("CheckoutSuccess", "Home"), IsActive = true });
                    break;
                case "login":
                    // Login page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Đăng Nhập", Url = Url.Action("Login", "Account"), IsActive = true });
                    break;

                case "register":
                    // Register page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Đăng Ký", Url = Url.Action("Register", "Account"), IsActive = true });
                    break;

                case "forgotpassword":
                    // Forgot password page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Quên Mật Khẩu", Url = Url.Action("ForgotPassword", "Account"), IsActive = true });
                    break;

                case "resetpassword":
                    // Reset password page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Đặt Lại Mật Khẩu", Url = Url.Action("ResetPassword", "Account"), IsActive = true });
                    break;

                case "changepassword":
                    // Change password page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Đổi Mật Khẩu", Url = Url.Action("ChangePassword", "Manage"), IsActive = true });
                    break;

                case "manageindex":
                    // Manage account index page
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Quản Lý Tài Khoản", Url = Url.Action("Index", "Manage"), IsActive = true });
                    break;
                default:
                    breadcrumbs.Breadcrumbs.Add(new BreadcrumbItem { Title = "Unknown", Url = "#", IsActive = true });
                    break;
            }

            return Json(breadcrumbs, JsonRequestBehavior.AllowGet);
        }

    }
}