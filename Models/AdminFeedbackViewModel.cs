namespace TransAbon.Models;

/// <summary>Date pentru pagina de consultare a mesajelor de feedback (/admin/feedback).</summary>
public class AdminFeedbackViewModel
{
    public int TotalMesaje { get; set; }

    /// <summary>Media evaluărilor; null dacă nu există mesaje.</summary>
    public double? MediaEvaluarilor { get; set; }

    /// <summary>Mesajele, de la cel mai nou la cel mai vechi.</summary>
    public IReadOnlyList<Feedback> Mesaje { get; set; } = [];
}