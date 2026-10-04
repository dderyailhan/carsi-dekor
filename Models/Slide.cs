using System.ComponentModel.DataAnnotations;

namespace CarsiDekor.Web.Models;

// Ana sayfadaki büyük slider'ın bir slaydı (yönetim panelinden yönetilir)
public class Slide
{
    public int Id { get; set; }

    // Yüklenen fotoğrafın sitedeki adresi: /uploads/slides/xxxx.jpg
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(120)] public string? Title { get; set; }
    [MaxLength(250)] public string? Subtitle { get; set; }

    // Buton: ikisi de doluysa gösterilir
    [MaxLength(300)] public string? LinkUrl { get; set; }
    [MaxLength(60)] public string? LinkText { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
