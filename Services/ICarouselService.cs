namespace TransAbon.Services;

public interface ICarouselService
{
    /// <summary>
    /// Returnează sursa imaginii pentru indexul dat (ciclic, ca în PHP),
    /// sau null dacă folderul carousel-images nu conține imagini.
    /// </summary>
    string? GetImageSrc(int currentIndex);
}
