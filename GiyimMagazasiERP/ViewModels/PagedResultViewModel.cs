namespace GiyimMagazasiERP.ViewModels;

public class PagedResultViewModel<T>
{
    public List<T> Items { get; set; } = new();

    public string? Arama { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool OncekiSayfaVarMi => Page > 1;

    public bool SonrakiSayfaVarMi => Page < TotalPages;
}