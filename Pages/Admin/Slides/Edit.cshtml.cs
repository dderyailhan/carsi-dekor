using System.ComponentModel.DataAnnotations;
using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarsiDekor.Web.Pages.Admin.Slides;

public class EditModel : PageModel
{
    private const string ImageFolder = "slides";

    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public EditModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    public class InputModel
    {
        [StringLength(120)] public string? Title { get; set; }
        [StringLength(250)] public string? Subtitle { get; set; }
        [StringLength(60)] public string? LinkText { get; set; }
        [StringLength(300)] public string? LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? ImageFile { get; set; }

    public string? CurrentImage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            // Yeni slayt: sırayı otomatik sona koy
            Input.DisplayOrder = (await _db.Slides.Select(s => (int?)s.DisplayOrder).MaxAsync() ?? 0) + 1;
            return Page();
        }

        var slide = await _db.Slides.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (slide is null) return NotFound();

        Input = new InputModel
        {
            Title = slide.Title,
            Subtitle = slide.Subtitle,
            LinkText = slide.LinkText,
            LinkUrl = slide.LinkUrl,
            DisplayOrder = slide.DisplayOrder,
            IsActive = slide.IsActive
        };
        CurrentImage = slide.ImagePath;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Slide? slide = null;
        if (id is not null)
        {
            slide = await _db.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();
        }

        // Yeni slaytta fotoğraf zorunlu
        if (slide is null && ImageFile is not { Length: > 0 })
        {
            ModelState.AddModelError(nameof(ImageFile), "Bir fotoğraf seçin.");
        }

        if (ImageFile is { Length: > 0 })
        {
            var error = await _images.ValidateAsync(ImageFile);
            if (error is not null) ModelState.AddModelError(nameof(ImageFile), error);
        }

        // Buton adresi sadece site içi (/...) veya http(s) olabilir
        var url = Input.LinkUrl?.Trim();
        if (!string.IsNullOrEmpty(url)
            && !(url.StartsWith('/') && !url.StartsWith("//"))
            && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Input.LinkUrl", "Adres / ile (site içi) ya da https:// ile başlamalı.");
        }

        if (!ModelState.IsValid)
        {
            CurrentImage = slide?.ImagePath;
            return Page();
        }

        if (slide is null)
        {
            slide = new Slide();
            _db.Slides.Add(slide);
        }

        string? oldImage = null;
        if (ImageFile is { Length: > 0 })
        {
            oldImage = slide.ImagePath;
            slide.ImagePath = await _images.SaveAsync(ImageFile, ImageFolder);
        }

        slide.Title = string.IsNullOrWhiteSpace(Input.Title) ? null : Input.Title.Trim();
        slide.Subtitle = string.IsNullOrWhiteSpace(Input.Subtitle) ? null : Input.Subtitle.Trim();
        slide.LinkText = string.IsNullOrWhiteSpace(Input.LinkText) ? null : Input.LinkText.Trim();
        slide.LinkUrl = string.IsNullOrEmpty(url) ? null : url;
        slide.DisplayOrder = Input.DisplayOrder;
        slide.IsActive = Input.IsActive;

        await _db.SaveChangesAsync();
        _images.Delete(oldImage);

        TempData["Flash"] = "Slayt kaydedildi.";
        return RedirectToPage("/Admin/Slides/Index");
    }
}
