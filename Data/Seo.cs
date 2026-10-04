namespace CarsiDekor.Web.Data;

/// <summary>Yol işareti (breadcrumb) öğesi.</summary>
public record Crumb(string Name, string Url);

public static class Seo
{
    /// <summary>
    /// Sitenin tam adresi (https://alanadi.com). Önce appsettings'teki Site:BaseUrl'e bakar,
    /// yoksa gelen isteğin adresini kullanır.
    /// </summary>
    public static string BaseUrl(HttpRequest request, IConfiguration config)
    {
        var configured = config["Site:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured)) return configured.TrimEnd('/');
        return $"{request.Scheme}://{request.Host}";
    }

    /// <summary>"/x" gibi yolu "https://alanadi.com/x" yapar.</summary>
    public static string Absolute(string baseUrl, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return baseUrl;
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
        return baseUrl + (path.StartsWith('/') ? path : "/" + path);
    }
}
