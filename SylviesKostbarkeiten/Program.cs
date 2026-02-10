using System.Globalization;
using QuestPDF.Infrastructure;
using SylviesKostbarkeiten;
using SylviesKostbarkeiten.io;

QuestPDF.Settings.License = LicenseType.Community;

Console.WriteLine("Hello World!");

//MenüTools.Preisanpassung("quelle.csv", "quelle_neu.csv", 3m);


// ... Dein bisheriger Code für das Kassen-Menü ...

Console.WriteLine("\n--- Teste Verkaufsparser ---");

// 1. Pfad zur Verkaufsdatei (passe den Dateinamen an deine Datei an)
var verkaufsDateiPfad = Tools.GetDesktopDatei("verkaeufe.csv");

if (File.Exists(verkaufsDateiPfad))
{
    // 2. Parser instanziieren und Datei einlesen
    var verkaufsParser = new VerkaufsParser();
    var verkaufsDaten = verkaufsParser.Parse(verkaufsDateiPfad);

    Console.WriteLine($"Erfolgreich {verkaufsDaten.Count} Verkaufspositionen eingelesen.");


    //##################


// ... nach dem Einlesen der verkaufsDaten ...

    Console.WriteLine("\n--- Monatsstatistik ---");

// 1. Daten nach Jahr und Monat gruppieren
    var monatsAuswertung = verkaufsDaten
        .Where(v => v.Brutto > 0) // Nur echte Umsätze berücksichtigen
        .GroupBy(v => new { v.Zeitstempel.Year, v.Zeitstempel.Month })
        .OrderBy(g => g.Key.Year)
        .ThenBy(g => g.Key.Month);

    foreach (var monat in monatsAuswertung)
    {
        // Monatstitel erstellen (z.B. "August 2025")
        var monatAnzeige = new DateTime(monat.Key.Year, monat.Key.Month, 1)
            .ToString("MMMM yyyy", new CultureInfo("de-AT"));

        var monatsUmsatz = monat.Sum(v => v.Brutto);
        var anzahlBelege = monat.Select(v => v.BelegNr).Distinct().Count();
        var anzahlPositionen = monat.Count();

        Console.WriteLine(
            $"{monatAnzeige,-15}: {monatsUmsatz,10:C2} ({anzahlBelege} Belege, {anzahlPositionen} Artikel)");
    }

    foreach (var monat in monatsAuswertung)
    {
        SteueberaterTools.GeneriereSteuerberaterExport(monat.Key.Year, monat.Key.Month, monat.ToList());

        // Monatstitel erstellen (z.B. "August 2025")
        var monatAnzeige = new DateTime(monat.Key.Year, monat.Key.Month, 1)
            .ToString("MMMM yyyy", new CultureInfo("de-AT"));

        // Berechnungen für den Monat
        var gesamtBrutto = monat.Sum(v => v.Brutto);
        var gesamtNetto = monat.Sum(v => v.Netto);

        // MwSt nach Sätzen aufteilen
        var mwst10 = monat.Where(v => v.MwStSatz == 10).Sum(v => v.MwStEuro);
        var mwst20 = monat.Where(v => v.MwStSatz == 20).Sum(v => v.MwStEuro);

        // Andere Sätze (z.B. 0% bei Gutscheinen oder 13%) falls vorhanden
        var mwstAndere = monat.Where(v => v.MwStSatz != 10 && v.MwStSatz != 20).Sum(v => v.MwStEuro);

        var anzahlBelege = monat.Select(v => v.BelegNr).Distinct().Count();

        // Schöne Ausgabe
        Console.WriteLine($"\n>> {monatAnzeige.ToUpper()} <<");
        Console.WriteLine($"   Belege: {anzahlBelege,5} | Brutto: {gesamtBrutto,10:C2}");
        Console.WriteLine($"   -----------------------------------------");
        Console.WriteLine($"   MwSt 10%: {mwst10,10:C2}");
        Console.WriteLine($"   MwSt 20%: {mwst20,10:C2}");
        if (mwstAndere > 0) Console.WriteLine($"   MwSt Sonst: {mwstAndere,8:C2}");
        Console.WriteLine($"   Netto Ges: {gesamtNetto,10:C2}");
    }

    
    // Finde alle Artikel, die weniger als 5-mal im Jahr verkauft wurden
    var streichListe = verkaufsDaten
        .Where(v => v.ArtikelName != "-" && v.Brutto > 0)
        .GroupBy(v => v.ArtikelName)
        .Select(g => new { Name = g.Key, Menge = g.Sum(x => x.Menge) })
        .Where(x => x.Menge < 5)
        .OrderBy(x => x.Menge);

    Console.WriteLine("\n--- Vorschlag für Sortiments-Optimierung (weniger als 5 Verkäufe) ---");
    foreach (var item in streichListe)
    {
        Console.WriteLine($"- {item.Name} (nur {item.Menge}x verkauft)");
    }
    

// Ganz am Ende der Program.cs
    AnalyseTools.GeneriereManagementBericht(verkaufsDaten);
    
    
    


// 1. Menü einlesen
var menueParser = new MenueParser();
var alleArtikel = menueParser.Parse("Fertig.csv")
    .SelectMany(g => g.Artikel)
    .Where(a => a.Price > 0m && !string.IsNullOrWhiteSpace(a.GroupName))
    .ToList();

// 2. Filter einbauen.
// Wir wollen auf der Speisekarte keine "Trenner" (Preis 0,00) 
// und keine Artikel ohne Gruppennamen sehen.


Console.WriteLine("Generiere Speisekarte als PDF...");

// Aufruf der Methode
var kartenTool = new KartenTool();
kartenTool.ErstelleSpeisekarte(alleArtikel);

Console.WriteLine("Speisekarte wurde unter 'Speisekarte_Neu.pdf' gespeichert.");

}
