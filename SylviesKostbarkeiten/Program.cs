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
        Console.WriteLine($"\nGruppe: {gruppe.Name} (ID: {gruppe.Id})");
        foreach (var artikel in gruppe.Artikel.Take(2))
        {
            Console.WriteLine($"  - {artikel.NameLong}: {artikel.Price:C2}");
        }
    }
}
else
{
    Console.WriteLine($"Datei nicht gefunden unter: {dateiPfad}");
}