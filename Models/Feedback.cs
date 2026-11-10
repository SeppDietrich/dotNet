namespace TransAbon.Models;

/// <summary>Mesaj trimis prin formularul de feedback. Nu se confundă cu FeedbackViewModel.</summary>
public class Feedback
{
    public int Id { get; set; }
    public string Nume { get; set; } = "";
    public string Email { get; set; } = "";

    /// <summary>Evaluare de la 1 la 5.</summary>
    public int Evaluare { get; set; }

    public string Mesaj { get; set; } = "";
    public DateTime DataTrimiterii { get; set; } = DateTime.UtcNow;
}
