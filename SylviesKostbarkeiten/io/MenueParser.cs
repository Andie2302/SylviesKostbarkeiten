using System.Globalization;
using System.Text;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten.io;

public class MenueParser
{
    private static readonly CultureInfo GermanCulture = new CultureInfo("de-AT");

    public List<KassenGruppenInfo> Parse(string filePath)
    {
        var gruppen = new List<KassenGruppenInfo>();
        var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1); 
        
        KassenGruppenInfo? aktuelleGruppe = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split(';').Select(s => s.Trim('"')).ToArray();

            if (!string.IsNullOrEmpty(p[0]))
            {
                aktuelleGruppe = new KassenGruppenInfo(Name: p[0],
                    Id: p[1],
                    Color: p[2],
                    Active: p[3] == "1",
                    PrinterId: p[4], Artikel: []
                );
                gruppen.Add(aktuelleGruppe);
            }
            else if (aktuelleGruppe != null && !string.IsNullOrEmpty(p[5]))
            {
                aktuelleGruppe.Artikel.Add(MapToArtikel(p));
            }
        }
        return gruppen;
    }

    private KassenArtikelInfo MapToArtikel(string[] p)
    {
        return new KassenArtikelInfo(NameLong: p[5],
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