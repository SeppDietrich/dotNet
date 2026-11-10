namespace TransAbon.Models;

public class AdminJurnalViewModel
{
    public string? Utilizator { get; set; }
    public string? Actiune { get; set; }
    public DateOnly? DeLa { get; set; }
    public DateOnly? PanaLa { get; set; }

    public int PaginaCurenta { get; set; } = 1;
    public int TotalPagini { get; set; } = 1;
    public int TotalEvenimente { get; set; }

    public IReadOnlyList<string> ActiuniDisponibile { get; set; } = [];
    public IReadOnlyList<EvenimentJurnalItemViewModel> Evenimente { get; set; } = [];
}

public class EvenimentJurnalItemViewModel
{
    public int Id { get; set; }
    public DateTime DataOra { get; set; }
    public string UtilizatorAfisat { get; set; } = "";
    public string Actiune { get; set; } = "";
    public string Obiect { get; set; } = "";
}
