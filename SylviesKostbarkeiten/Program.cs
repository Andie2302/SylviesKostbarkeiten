using System.Text;
using SylviesKostbarkeiten;

Console.WriteLine("Lese Kassen-Menü ein...");

var menueDateiPfad = Tools.GetDesktopDatei("quelle.csv");

var gruppen = Tools.LeseMenueDatei(menueDateiPfad);
foreach (var gruppe in gruppen)
{
    Console.WriteLine($"\nGruppe: {gruppe.Name} (ID: {gruppe.Id})");
    foreach (var artikel in gruppe.Artikel.Take(2))
    {
        Console.WriteLine($"  - {artikel.NameLong}: {artikel.Price:C2}");
    }
}


var erhoehungProzent = 3m;
var angepassteGruppen = gruppen.Select(g => g with 
{
    Artikel = Tools.ErhöhePreisUm(g, erhoehungProzent)
}).ToList();

var exportPfad = Tools.GetDesktopDatei("quelle_neu.csv");
var exportInhalt = new List<string>();
exportInhalt.Add("GROUP_NAME;GROUP_ID;GROUP_COLOR;GROUP_ACTIVE;GROUP_PRINTER_ID;ARTICLE_NAME_LONG;ARTICLE_NAME_SHORT;ARTICLE_ID;ARTICLE_COLOR;ARTICLE_EMPTY_ITEM;ARTICLE_PRICE;ARTICLE_VAT;ARTICLE_IMMEDIATE_EDIT;ARTICLE_IMMEDIATE_FIELD;ARTICLE_ACCOUNT_NUMBER;ARTICLE_ARTICLE_NUMBER;ARTICLE_EAN_CODE;ARTICLE_PRINTER_ID;ARTICLE_EXTRA_IDS;ARTICLE_FAVORITE;ARTICLE_FAVORITE_INDEX;ARTICLE_FAVORITE_COLOR");

foreach (var gruppe in angepassteGruppen)
{
    exportInhalt.Add(gruppe.ToCsvLine());
    exportInhalt.AddRange(gruppe.Artikel.Select(artikel => artikel.ToCsvLine()));
}

File.WriteAllLines(exportPfad, exportInhalt, Encoding.UTF8);
Console.WriteLine($"\nPreise wurden um {erhoehungProzent}% erhöht. Datei gespeichert unter: {exportPfad}");