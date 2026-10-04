using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarsiDekor.Web.Pages;

// Program.cs'teki UseStatusCodePagesWithReExecute bu sayfayı çalıştırır; durum kodu 404 olarak kalır.
[IgnoreAntiforgeryToken]
public class NotFoundModel : PageModel
{
    public void OnGet()
    {
    }
}
