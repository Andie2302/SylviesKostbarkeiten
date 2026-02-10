using System.Globalization;
using ClosedXML.Excel;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten;

public static class AnalyseTools
{
    public static void GeneriereManagementBericht(List<VerkaufsPositionInfo> daten)
    {
        var kultur = new CultureInfo("de-AT");
        var fileName = "Sylvies_Business_Analyse_Gesamt.xlsx";

        using var workbook = new XLWorkbook();

        // --- BLATT 1: TOP-SELLER (Was lieben die Kunden?) ---
        var wsTop = workbook.Worksheets.Add("Top-Seller");
        wsTop.Cell(1, 1).Value = "Ranking";
        wsTop.Cell(1, 2).Value = "Artikel";
        wsTop.Cell(1, 3).Value = "Menge";
        wsTop.Cell(1, 4).Value = "Umsatz Brutto";
        wsTop.Range("A1:D1").Style.Font.Bold = true;

        var topArtikel = daten
            .Where(v => v.Brutto > 0 && v.ArtikelName != "-")
            .GroupBy(v => v.ArtikelName)
            .Select(g => new { Name = g.Key, Menge = g.Sum(x => x.Menge), Umsatz = g.Sum(x => x.Brutto) })
            .OrderByDescending(x => x.Umsatz)
            .ToList();

        for (int i = 0; i < topArtikel.Count; i++)
        {
            wsTop.Cell(i + 2, 1).Value = i + 1;
            wsTop.Cell(i + 2, 2).Value = topArtikel[i].Name;
            wsTop.Cell(i + 2, 3).Value = topArtikel[i].Menge;
            wsTop.Cell(i + 2, 4).Value = topArtikel[i].Umsatz;
            wsTop.Cell(i + 2, 4).Style.NumberFormat.Format = "#,##0.00 €";
        }

        // --- BLATT 2: WOCHENTAGS-CHECK (Wann ist am meisten los?) ---
        var wsTag = workbook.Worksheets.Add("Wochentage");
        wsTag.Cell(1, 1).Value = "Wochentag";
        wsTag.Cell(1, 2).Value = "Durchschnittliche Losung";
        wsTag.Range("A1:B1").Style.Font.Bold = true;

        var wochentage = daten
            .GroupBy(v => v.Zeitstempel.DayOfWeek)
            .Select(g => new { 
                Tag = g.Key, 
                Schnitt = g.GroupBy(x => x.Zeitstempel.Date).Average(d => d.Sum(z => z.Brutto)) 
            })
            .OrderBy(x => ((int)x.Tag + 6) % 7); // Sortiert Montag bis Sonntag

        int rowTag = 2;
        foreach (var w in wochentage)
        {
            wsTag.Cell(rowTag, 1).Value = kultur.DateTimeFormat.GetDayName(w.Tag);
            wsTag.Cell(rowTag, 2).Value = w.Schnitt;
            wsTag.Cell(rowTag, 2).Style.NumberFormat.Format = "#,##0.00 €";
            rowTag++;
        }

        // --- BLATT 3: GRUPPEN-UMSATZ (Kaffee vs. Kuchen vs. Handwerk) ---
        var wsGruppe = workbook.Worksheets.Add("Umsatz nach Gruppen");
        var gruppenUmsatz = daten
            .GroupBy(v => v.Gruppe)
            .Select(g => new { Name = g.Key, Umsatz = g.Sum(x => x.Brutto) })
            .OrderByDescending(x => x.Umsatz);

        int rowG = 2;
        wsGruppe.Cell(1, 1).Value = "Warengruppe";
        wsGruppe.Cell(1, 2).Value = "Umsatz";
        foreach (var g in gruppenUmsatz)
        {
            wsGruppe.Cell(rowG, 1).Value = g.Name;
            wsGruppe.Cell(rowG, 2).Value = g.Umsatz;
            wsGruppe.Cell(rowG, 2).Style.NumberFormat.Format = "#,##0.00 €";
            rowG++;
        }

        wsTop.Columns().AdjustToContents();
        wsTag.Columns().AdjustToContents();
        wsGruppe.Columns().AdjustToContents();

        workbook.SaveAs(fileName);
        Console.WriteLine($"\nManagement-Bericht erstellt: {fileName}");
    }
    
    
    
    
}