using System.Text;

namespace SylviesKostbarkeiten;

public static class MenüTools
{
    public static void Preisanpassung(string quelldateiPfad, string zieldateiPfad, decimal prozent)
    {
        if (!File.Exists(quelldateiPfad))
            throw new FileNotFoundException("Quelldatei nicht gefunden.", quelldateiPfad);
        Console.WriteLine("Lese Kassen-Menü ein...");
        var menueDateiPfad = quelldateiPfad;
        var gruppen = Tools.LeseMenueDatei(menueDateiPfad);
        var erhoehungProzent = prozent;
        var angepassteGruppen = gruppen.Select(g => g with
        {
            Artikel = Tools.ErhöhePreisUm(g, erhoehungProzent)
        }).ToList();

        var exportPfad = zieldateiPfad;
        var exportInhalt = new List<string>();
        exportInhalt.Add(
            "GROUP_NAME;GROUP_ID;GROUP_COLOR;GROUP_ACTIVE;GROUP_PRINTER_ID;ARTICLE_NAME_LONG;ARTICLE_NAME_SHORT;ARTICLE_ID;ARTICLE_COLOR;ARTICLE_EMPTY_ITEM;ARTICLE_PRICE;ARTICLE_VAT;ARTICLE_IMMEDIATE_EDIT;ARTICLE_IMMEDIATE_FIELD;ARTICLE_ACCOUNT_NUMBER;ARTICLE_ARTICLE_NUMBER;ARTICLE_EAN_CODE;ARTICLE_PRINTER_ID;ARTICLE_EXTRA_IDS;ARTICLE_FAVORITE;ARTICLE_FAVORITE_INDEX;ARTICLE_FAVORITE_COLOR");

        foreach (var gruppe in angepassteGruppen)
        {
            exportInhalt.Add(gruppe.ToCsvLine());
            exportInhalt.AddRange(gruppe.Artikel.Select(artikel => artikel.ToCsvLine()));
        }

        File.WriteAllLines(exportPfad, exportInhalt, Encoding.UTF8);
        Console.WriteLine($"\nPreise wurden um {erhoehungProzent}% erhöht. Datei gespeichert unter: {exportPfad}");
    }
}