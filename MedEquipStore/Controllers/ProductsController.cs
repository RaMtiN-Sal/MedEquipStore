using MedEquipStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MedEquipStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public ProductsController(StoreDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index(string? searchTerm,int? categoryId, string? sortOrder, decimal? minPrice,decimal? maxPrice)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            
            if (minPrice.HasValue && minPrice.Value >= 0)
            {
                query = query.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue && maxPrice.Value >= 0)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "stock_asc" => query.OrderBy(p => p.StockQuantity),
                "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
                _ => query.OrderBy(p => p.Name)
            };

            ViewBag.AllCategories = await _context.Categories.ToListAsync();
            var products = await query.ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);

        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create() // It Just sends us to Create page really
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name"); // we need to fill the dropdown list with categories
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile) // We handle inputs in View not here, ModelState will check them
                                                                                       // view part is only for filling datas, No more Url Train of parameters!
        {
            if (ModelState.IsValid)
            {
                if(imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }





                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product); // here if user hit the save button and one thing is missing, already data will not wipe out. UX friendly

        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id , Product product, IFormFile? imageFile)
        {
            if(id != product.ProductId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if(imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
