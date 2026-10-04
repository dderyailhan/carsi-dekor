using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Slides;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public IndexModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    public List<Slide> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _db.Slides.AsNoTracking()
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var slide = await _db.Slides.FirstOrDefaultAsync(s => s.Id == id);
        if (slide is null) return RedirectToPage();

        var path = slide.ImagePath;
        _db.Slides.Remove(slide);
        await _db.SaveChangesAsync();
        _images.Delete(path);

        TempData["Flash"] = "Slayt silindi.";
        return RedirectToPage();
    }
}
