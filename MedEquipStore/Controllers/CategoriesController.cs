using MedEquipStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedEquipStore.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public CategoriesController(StoreDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if(imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categories");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    category.ImageUrl = "/images/categories/" + uniqueFileName;

                }




                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? imageFile)
        {
            if(id != category.CategoryId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if(imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_environment.WebRootPath, category.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categories");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName); // this is about filename
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName); // and this is about location 

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    category.ImageUrl = "/images/categories/" + uniqueFileName;

                }
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("DeleteWithProducts")]
        public async Task<IActionResult> DeleteWithProductsConfirmed(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
            if(category == null)
            {
                return NotFound();
            }
            _context.Products.RemoveRange(category.Products);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
