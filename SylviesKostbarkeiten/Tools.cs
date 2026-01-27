using SylviesKostbarkeiten.io;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten;

public static class Tools
{
    public static string DesktopPfad => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static string GetDesktopDatei(string dateiName) => Path.Combine(DesktopPfad, dateiName);
    public static List<KassenGruppenInfo> LeseMenueDatei(string menueDateiPfad) => File.Exists(menueDateiPfad) ? new MenueParser().Parse(menueDateiPfad) : [];

    public static List<KassenArtikelInfo> ErhöhePreisUm(KassenGruppenInfo gruppenInfo, decimal erhoehungProzent)
    {
        return gruppenInfo.Artikel.Select(a =>
        {
            if (a.Vat <= 0) return a;
            var neuerPreis = a.Price * (1 + erhoehungProzent / 100);
            neuerPreis = Math.Round(neuerPreis * 10, MidpointRounding.AwayFromZero) / 10;
            return a with { Price = neuerPreis };
        }).ToList();
    }
    
}