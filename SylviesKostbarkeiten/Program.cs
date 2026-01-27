using SylviesKostbarkeiten.io;

Console.WriteLine("Lese Kassen-Menü ein...");

string desktopPfad = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

string dateiPfad = Path.Combine(desktopPfad, "quelle.csv");

var parser = new MenueParser();

if (File.Exists(dateiPfad))
{
    var gruppen = parser.Parse(dateiPfad);
    foreach (var gruppe in gruppen)
    {
        Console.WriteLine($"Gruppe: {gruppe.Name} mit {gruppe.Artikel.Count} Artikeln.");
    }
}
else
{
    Console.WriteLine($"Datei nicht gefunden unter: {dateiPfad}");
}