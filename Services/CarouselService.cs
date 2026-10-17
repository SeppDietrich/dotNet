namespace TransAbon.Services;

/// <summary>Scanează wwwroot/carousel-images la fiecare cerere, deci orice imagine adăugată apare automat.</summary>
public class CarouselService : ICarouselService
{
    private const string FolderName = "carousel-images";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private readonly IWebHostEnvironment _env;

    public CarouselService(IWebHostEnvironment env) => _env = env;

    public string? GetImageSrc(int currentIndex)
    {
        var dir = Path.Combine(_env.WebRootPath ?? "", FolderName);
        if (!Directory.Exists(dir)) return null;

        // Ordine alfabetică, ca scandir() din PHP
        var images = Directory.EnumerateFiles(dir)
            .Select(f => Path.GetFileName(f))
            .Where(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        if (images.Count == 0) return null;

        var index = ((currentIndex % images.Count) + images.Count) % images.Count;
        return $"/{FolderName}/{Uri.EscapeDataString(images[index])}";
    }
}
