namespace TransAbon.Services;

public interface ICarouselService
{
    /// <summary>
    /// Returnează sursa imaginii pentru indexul dat (ciclic: după ultima imagine se revine la prima),
    /// sau null dacă folderul carousel-images nu conține imagini.
    /// </summary>
    string? GetImageSrc(int currentIndex);
}
