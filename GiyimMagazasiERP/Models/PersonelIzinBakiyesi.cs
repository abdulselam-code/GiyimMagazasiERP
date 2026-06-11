namespace GiyimMagazasiERP.Models;

public class PersonelIzinBakiyesi
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public int Yil { get; set; }
    public decimal YillikIzinHakki { get; set; } = 14m;
    public decimal DevredenIzinGunu { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Personel Personel { get; set; } = null!;
}
