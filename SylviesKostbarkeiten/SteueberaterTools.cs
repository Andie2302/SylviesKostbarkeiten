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

        var gesamtBruttoMonat = 0m;
        var gesamtBrutto20 = 0m;
        var gesamtBrutto10 = 0m;
        foreach (var tag in tagesGruppen)
        {
            decimal losung = tag.Sum(v => v.Brutto);
    
            ws1.Cell(row, 1).Value = tag.Key.ToString("dddd, d. MMMM yyyy", kultur);
            ws1.Cell(row, 2).Value = anfangsbestand;
            ws1.Cell(row, 3).Value = anfangsbestand + losung;
            ws1.Cell(row, 4).Value = losung;
            gesamtBruttoMonat += losung;

            // KORREKTUR: Hier den Brutto-Umsatz pro Steuersatz summieren, nicht nur die MwSt
            var wert20 = tag.Where(v => v.MwStSatz == 20).Sum(v => v.Brutto);
            var wert10 = tag.Where(v => v.MwStSatz == 10).Sum(v => v.Brutto);
            gesamtBrutto20 += wert20;
            gesamtBrutto10 += wert10;
            ws1.Cell(row, 5).Value = wert20;
            ws1.Cell(row, 6).Value = wert10;
    
            ws1.Range(row, 2, row, 6).Style.NumberFormat.Format = "#,##0.00";
            
            //Format START
            for(var i=2;i<7;i++)
                ws1.Cell(row, i).Style.NumberFormat.Format = "#,##0.00 €";
            //Format ENDE
            
            row++;
            
            
            
        }
        
        
        
        // Nach der Schleife, die die Tage schreibt (Variable 'row' ist am Ende der Tabelle)
        row++; // Eine Leerzeile lassen

// Summen-Zeile (Σ)
        ws1.Cell(row, 3).Value = "Σ";
        ws1.Cell(row, 4).Value = gesamtBruttoMonat; // Summe Spalte Tageslosung
        ws1.Cell(row, 5).Value = gesamtBrutto20;    // Summe Spalte 20%
        ws1.Cell(row, 6).Value = gesamtBrutto10;    // Summe Spalte 10%
        ws1.Range(row, 4, row, 6).Style.Font.Bold = true;
        
        //Format START
        for(var i=4;i<7;i++)
            ws1.Cell(row, i).Style.NumberFormat.Format = "#,##0.00 €";
        //Format ENDE
        
        row += 2; // Wieder eine Leerzeile

// Netto-Berechnung
        decimal netto20 = Math.Round(gesamtBrutto20 / 1.2m, 2);
        decimal netto10 = Math.Round(gesamtBrutto10 / 1.1m, 2);

        ws1.Cell(row, 3).Value = "Netto";
        ws1.Cell(row, 4).Value = "=";
        ws1.Cell(row, 5).Value = netto20;
        ws1.Cell(row, 6).Value = netto10;
        //Format START
        for(var i=5;i<7;i++)
            ws1.Cell(row, i).Style.NumberFormat.Format = "#,##0.00 €";
        //Format ENDE
        row += 2;

// MwSt Ausweis
        decimal mwst20 = Math.Round(netto20 * 0.2m, 2);
        decimal mwst10 = Math.Round(netto10 * 0.1m, 2);

        ws1.Cell(row, 2).Value = "10% von";
        ws1.Cell(row, 3).Value = netto10;
        ws1.Cell(row, 4).Value = "=";
        ws1.Cell(row, 5).Value = mwst10;
        ws1.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 €";
        ws1.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 €";
        row++;

        ws1.Cell(row, 2).Value = "20% von";
        ws1.Cell(row, 3).Value = netto20;
        ws1.Cell(row, 4).Value = "=";
        ws1.Cell(row, 5).Value = mwst20;
        ws1.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 €";
        ws1.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 €";
        row++;

// Gesamtsumme Steuer
        ws1.Cell(row, 5).Value = mwst10 + mwst20;
        ws1.Cell(row, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
        ws1.Cell(row, 5).Style.Font.Bold = true;
        ws1.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 €";

        
        
        
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