using System.Globalization;
using System.Text;

namespace SylviesKostbarkeiten.io;

public class MenueParser
{
    private static readonly CultureInfo GermanCulture = new CultureInfo("de-AT");

    public List<Kassengruppe> Parse(string filePath)
    {
        var gruppen = new List<Kassengruppe>();
        // Nutze Encoding.Latin1 oder UTF8, je nachdem was die Kasse ausspuckt
        var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1); 
        
        Kassengruppe? aktuelleGruppe = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Zerlegen, aber Anführungszeichen ignorieren
            var p = line.Split(';').Select(s => s.Trim('"')).ToArray();

            // Fall 1: Neue Gruppe (GROUP_NAME an Index 0 vorhanden)
            if (!string.IsNullOrEmpty(p[0]))
            {
                aktuelleGruppe = new Kassengruppe(
                    Name: p[0],
                    Id: p[1],
                    Color: p[2],
                    Active: p[3] == "1",
                    Printer: p[4],
                    Artikel: new List<KassenArtikel>()
                );
                gruppen.Add(aktuelleGruppe);
            }
            // Fall 2: Artikelzeile (Erste 5 Spalten leer, ARTICLE_NAME_LONG vorhanden)
            else if (aktuelleGruppe != null && !string.IsNullOrEmpty(p[5]))
            {
                aktuelleGruppe.Artikel.Add(MapToArtikel(p));
            }
        }
        return gruppen;
    }

    private KassenArtikel MapToArtikel(string[] p)
    {
        return new KassenArtikel(
            NameLong: p[5],
            NameShort: p[6],
            Id: p[7],
            Color: p[8],
            EmptyItem: p[9] == "1",
            Price: ParseMoney(p[10]),
            Vat: ParseMoney(p[11]),
            ImmediateEdit: p[12] == "1",
            ImmediateField: p[13],
            AccountNumber: p[14],
            ArticleNumber: p[15],
            EanCode: p[16],
            PrinterId: p[17],
            ExtraIds: p[18],
            Favorite: p[19] == "1",
            FavoriteIndex: int.TryParse(p[20], out var idx) ? idx : 0,
            FavoriteColor: p[21]
        );
    }

    private decimal ParseMoney(string value) => 
        decimal.TryParse(value, NumberStyles.Any, GermanCulture, out var res) ? res : 0m;
}