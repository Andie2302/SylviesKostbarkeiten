using System.Globalization;
using ClosedXML.Excel;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten;

public static class SteueberaterTools
{


    public static void GeneriereSteuerberaterExport(int jahr, int monat, List<VerkaufsPositionInfo> daten)
    {
        var kultur = new CultureInfo("de-AT");
        var monatsName = new DateTime(jahr, monat, 1).ToString("MMMM", kultur);
        var fileName = $"{jahr}-{monat:D2}_Kassa_{monatsName}.xlsx";

        using var workbook = new XLWorkbook();
        // --- BLATT 1: KASSA LOSUNG ---
        var ws1 = workbook.Worksheets.Add("Kassa Losung");
        
        // Header-Info (wie in Sylvies Datei)
        ws1.Cell(1, 1).Value = "Kassa Losung";
        ws1.Cell(1, 4).Value = $"{monatsName} {jahr}";
        ws1.Range("A1:A1").Style.Font.Bold = true;

        // Spaltenüberschriften
        string[] headers = ["Datum", "Anfangsbestand", "Endbestand", "Tageslosung", "20%", "10%"];
        for (var i = 0; i < headers.Length; i++) {
            ws1.Cell(3, i + 1).Value = headers[i];
            ws1.Cell(3, i + 1).Style.Font.Bold = true;
            ws1.Cell(3, i + 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        var tagesGruppen = daten.GroupBy(v => v.Zeitstempel.Date).OrderBy(g => g.Key);
        var row = 4;
        var anfangsbestand = 160.0m; // Fixwert aus Sylvies Vorlage

        var summeTageslosung = 0m;
        var summeWert20 = 0m;
        var summeWert10 = 0m;
        foreach (var tag in tagesGruppen)
        {
            decimal losung = tag.Sum(v => v.Brutto);
    
            ws1.Cell(row, 1).Value = tag.Key.ToString("dddd, d. MMMM yyyy", kultur);
            ws1.Cell(row, 2).Value = anfangsbestand;
            ws1.Cell(row, 3).Value = anfangsbestand + losung;
            ws1.Cell(row, 4).Value = losung;
            summeTageslosung += losung;

            // KORREKTUR: Hier den Brutto-Umsatz pro Steuersatz summieren, nicht nur die MwSt
            var wert20 = tag.Where(v => v.MwStSatz == 20).Sum(v => v.Brutto);
            var wert10 = tag.Where(v => v.MwStSatz == 10).Sum(v => v.Brutto);
            summeWert20 += wert20;
            summeWert10 += wert10;
            ws1.Cell(row, 5).Value = wert20;
            ws1.Cell(row, 6).Value = wert10;
    
            ws1.Range(row, 2, row, 6).Style.NumberFormat.Format = "#,##0.00";
            
            //Format START
            for(var i=2;i<7;i++)
                ws1.Cell(row, i).Style.NumberFormat.Format = "#,##0.00 €";
            //Format ENDE
            
            row++;
        }
        ws1.Cell(row, 3).Value = "Σ";
        ws1.Cell(row, 4).Value = summeTageslosung;
        ws1.Cell(row, 5).Value = summeWert20;
        ws1.Cell(row, 6).Value = summeWert10;
        for(var i=4;i<7;i++)
            ws1.Cell(row, i).Style.NumberFormat.Format = "#,##0.00 €";
        
        // --- BLATT 2: KÜCHENUMSÄTZE ---
        var ws2 = workbook.Worksheets.Add("Küchenumsätze 10%");
        ws2.Cell(1, 1).Value = "Küchenumsätze 10 %";
        ws2.Cell(1, 3).Value = monatsName;
        ws2.Cell(1, 4).Value = jahr;

        string[] headers2 = ["Datum", "Artikel", "Stück", "Preis", "Gesamt"];
        for (var i = 0; i < headers2.Length; i++) {
            ws2.Cell(3, i + 1).Value = headers2[i];
            ws2.Cell(3, i + 1).Style.Font.Bold = true;
        }

        var rowK = 4;
        // Nur 10% MwSt (typisch für Küche/Speisen)
        var bruttoSumme = 0m;
        foreach (var pos in daten.Where(v => v.MwStSatz == 10).OrderBy(v => v.Zeitstempel))
        {
            ws2.Cell(rowK, 1).Value = pos.Zeitstempel.ToString("dddd, d. MMMM yyyy", kultur);
            ws2.Cell(rowK, 2).Value = pos.ArtikelName;
            ws2.Cell(rowK, 3).Value = pos.Menge;
            ws2.Cell(rowK, 4).Value = (pos.Brutto / (pos.Menge == 0 ? 1 : pos.Menge)); // Einzelpreis
            ws2.Cell(rowK, 5).Value = pos.Brutto;
            bruttoSumme += pos.Brutto;
            
            ws2.Cell(rowK, 4).Style.NumberFormat.Format = "#,##0.00 €";
            ws2.Cell(rowK, 5).Style.NumberFormat.Format = "#,##0.00 €";
            rowK++;
        }
        ws2.Cell(rowK, 5).Value = bruttoSumme;
        ws2.Cell(rowK, 5).Style.NumberFormat.Format = "#,##0.00 €";
        ws1.Columns().AdjustToContents();
        ws2.Columns().AdjustToContents();

        
        workbook.SaveAs(fileName);
        Console.WriteLine($"Datei erstellt: {fileName}");
    }
}