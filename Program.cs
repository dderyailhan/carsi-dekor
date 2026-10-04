using CarsiDekor.Web.Data;
using CarsiDekor.Web.Models;
using CarsiDekor.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ImageStorage>();

// Hız: HTML/CSS/JS yanıtlarını sıkıştırarak gönder
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});

// Giriş sistemi: başarılı girişte tarayıcıya şifreli bir çerez verilir
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.Cookie.Name = "CarsiDekor.Admin";
        options.Cookie.HttpOnly = true;                       // JavaScript çereze erişemez
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest                // geliştirirken http'de de çalışsın
            : CookieSecurePolicy.Always;                      // yayında sadece https
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    // /Admin altındaki tüm sayfalar giriş ister, sadece giriş sayfası herkese açık
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
})
// '?' işareti olmayan alanları (ör. List<IFormFile> ExtraFiles) otomatik "zorunlu" sayma.
// Zorunlu olması gereken alanlar zaten [Required] ile açıkça işaretli.
.AddMvcOptions(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

var app = builder.Build();

// Başlangıç verileri (sadece veritabanı tamamen boşsa çalışır — canlıda etkisi yok)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Categories.Any())
    {
        var vitrinler = new Category { Name = "Vitrinler", Slug = "vitrinler", DisplayOrder = 1 };
        var bankolar  = new Category { Name = "Bankolar",  Slug = "bankolar",  DisplayOrder = 2 };
        var nisler    = new Category { Name = "Nişler",    Slug = "nisler",    DisplayOrder = 3 };
        var digerleri = new Category { Name = "Diğerleri", Slug = "digerleri", DisplayOrder = 4 };

        db.Categories.AddRange(vitrinler, bankolar, nisler, digerleri);
        db.SaveChanges();   // Id'ler üretilsin ki alt kategoriler bağlanabilsin

        var mankenler = new Category { Name = "Mankenler", Slug = "mankenler", DisplayOrder = 1, ParentCategoryId = digerleri.Id };
        db.Categories.Add(mankenler);

        db.Categories.AddRange(
            new Category { Name = "Yüzüklük", Slug = "yuzukluk", DisplayOrder = 2, ParentCategoryId = digerleri.Id },
            new Category { Name = "Küpelik", Slug = "kupelik", DisplayOrder = 3, ParentCategoryId = digerleri.Id },
            new Category { Name = "Kolyelik", Slug = "kolyelik", DisplayOrder = 4, ParentCategoryId = digerleri.Id },
            new Category { Name = "CNC Lazer", Slug = "cnc-lazer", DisplayOrder = 5, ParentCategoryId = digerleri.Id },
            new Category { Name = "Polyester", Slug = "polyester", DisplayOrder = 6, ParentCategoryId = digerleri.Id },
            new Category { Name = "Tabla", Slug = "tabla", DisplayOrder = 7, ParentCategoryId = digerleri.Id },
            new Category { Name = "Döner Motor Standı", Slug = "doner-motor-standi", DisplayOrder = 8, ParentCategoryId = digerleri.Id },
            new Category { Name = "Tesbihlik", Slug = "tesbihlik", DisplayOrder = 9, ParentCategoryId = digerleri.Id }
        );
        db.SaveChanges();   // Mankenler Id'si üretilsin ki tipleri bağlanabilsin

        var mankenTipleri = new (string Name, string Slug)[]
        {
            ("Lüks", "luks"), ("Kare", "kare"), ("Narin", "narin"), ("Likya", "likya"),
            ("İbiza", "ibiza"), ("Forza", "forza"), ("Sarı Ayaklı Metal", "sari-ayakli-metal"),
            ("Ayn", "ayn"), ("V", "v-manken"), ("Kare Ahşap", "kare-ahsap"), ("Hasır Set", "hasir-set"),
        };
        for (var i = 0; i < mankenTipleri.Length; i++)
        {
            db.Categories.Add(new Category
            {
                Name = mankenTipleri[i].Name,
                Slug = mankenTipleri[i].Slug,
                DisplayOrder = i + 1,
                ParentCategoryId = mankenler.Id
            });
        }
        db.SaveChanges();
    }

    // İlk yönetici hesabı. Bilgiler kodda değil, user-secrets'ta durur.
    if (!db.AdminUsers.Any())
    {
        var username = app.Configuration["Admin:Username"];
        var password = app.Configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            app.Logger.LogWarning(
                "Yönetici hesabı yok. Oluşturmak için Admin:Username ve Admin:Password ayarlarını girin.");
        }
        else
        {
            db.AdminUsers.Add(new AdminUser
            {
                Username = username.Trim(),
                PasswordHash = PasswordService.Hash(password)
            });
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Var olmayan adreslerde (404) özel 404 sayfasını göster; durum kodu 404 olarak kalır
app.UseStatusCodePagesWithReExecute("/404");

// Yönetim panelinden sonradan yüklenen fotoğrafları sunmak için gerekli.
// Yüklenen dosyaların adı her seferinde benzersiz (GUID) olduğu için uzun süre önbelleğe alınabilir.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path;
        if (path.StartsWithSegments("/uploads") || path.StartsWithSegments("/images"))
        {
            ctx.Context.Response.Headers.CacheControl = "public,max-age=2592000";
        }
    }
});

app.UseRouting();

app.UseAuthentication();   // "bu kişi kim?"
app.UseAuthorization();    // "bu sayfaya girmeye yetkisi var mı?"

// ---- robots.txt: arama motorlarına kurallar + sitemap adresi ----
app.MapGet("/robots.txt", (HttpRequest req, IConfiguration cfg) =>
{
    var baseUrl = Seo.BaseUrl(req, cfg);
    var sb = new StringBuilder();
    sb.AppendLine("User-agent: *");
    sb.AppendLine("Allow: /");
    sb.AppendLine("Disallow: /Admin");
    sb.AppendLine("Disallow: /tesekkurler");
    sb.AppendLine("Disallow: /404");
    sb.AppendLine();
    sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
    return Results.Text(sb.ToString(), "text/plain", Encoding.UTF8);
});

// ---- sitemap.xml: Google'a sitedeki tüm sayfaları bildirir ----
app.MapGet("/sitemap.xml", async (HttpRequest req, IConfiguration cfg, AppDbContext db) =>
{
    var baseUrl = Seo.BaseUrl(req, cfg);
    var urls = new List<(string Loc, DateTime? LastMod)>
    {
        ("/", null),
        ("/Projects", null),
        ("/About", null),
        ("/Contact", null),
        ("/gizlilik-politikasi", null)
    };
    if (CarsiDekor.Web.Data.SiteContent.CaseStudies.Any(c => c.Published))
        urls.Add(("/vaka-calismalari", null));

    var slugs = await db.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).Select(c => c.Slug).ToListAsync();
    urls.AddRange(slugs.Select(sl => ($"/Projects?kategori={sl}", (DateTime?)null)));

    var projects = await db.Projects.AsNoTracking().Where(p => p.IsPublished)
        .Select(p => new { p.Id, p.CreatedAt }).ToListAsync();
    urls.AddRange(projects.Select(p => ($"/Projects/Details/{p.Id}", (DateTime?)p.CreatedAt)));

    var sb = new StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
    foreach (var (loc, lastMod) in urls)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{SecurityElement.Escape(baseUrl + loc)}</loc>");
        if (lastMod.HasValue) sb.AppendLine($"    <lastmod>{lastMod.Value:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("  </url>");
    }
    sb.AppendLine("</urlset>");
    return Results.Text(sb.ToString(), "application/xml", Encoding.UTF8);
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
