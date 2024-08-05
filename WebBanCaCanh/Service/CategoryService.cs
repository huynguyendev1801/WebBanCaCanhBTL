using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebBanCaCanh.Models;

namespace WebBanCaCanh.Service
{
    public class CategoryService: ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        // Phương thức để lấy tất cả các danh mục bao gồm các sản phẩm của mỗi danh mục
        public async Task<List<Category>> GetAllCategoriesWithProductsAsync()
        {
            return await _context.Categories
                .Include(c => c.Products.Select(p => p.ProductImages))
                .ToListAsync();
        }
       

        public async Task<Category> GetCategoryWithProductsByCategoryIdAsync(int? categoryId)
        {
            if (categoryId.HasValue)
            {
                return await _context.Categories
                    .Where(c => c.CategoryId == categoryId)
                    .Include(c => c.Products.Select(p => p.ProductImages))
                    .FirstOrDefaultAsync();
            }
            else
            {
                return await _context.Categories
                    .Include(c => c.Products.Select(p => p.ProductImages))
                    .FirstOrDefaultAsync();
            }
        }


        public async Task<int> GetCategoryCountAsync()
        {
            return await _context.Categories.CountAsync();
        }

        // Phương thức để lấy tất cả các danh mục
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        // Phương thức để lấy một danh mục bằng ID
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        // Phương thức để thêm một danh mục mới
        public async Task<bool> AddCategoryAsync(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu có
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
                if (existingCategory == null)
                {
                    return false; // Category not found
                }

                // Update the properties
                existingCategory.CategoryName = category.CategoryName;
                existingCategory.ImageUrl = category.ImageUrl;

                // Save changes
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Handle the error
                return false;
            }
        }


        // Phương thức để xóa một danh mục
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu có
                return false;
            }
        }
    }
}