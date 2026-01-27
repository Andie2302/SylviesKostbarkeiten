using System.Globalization;
using System.Text;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten.io;

public class VerkaufsParser
{
    private static readonly CultureInfo GermanCulture = new CultureInfo("de-AT");
    public List<VerkaufsPositionInfo> Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1);
        return (from line in lines where !string.IsNullOrWhiteSpace(line) select line.Split(';') into p where p.Length >= 20 select new VerkaufsPositionInfo(BelegNr: p[0], Zeitstempel: DateTime.ParseExact(p[1], "dd.MM.yy HH:mm", GermanCulture), KassaBezeichnung: p[4], ArtikelName: p[7], Gruppe: p[9], MwStSatz: ParseDecimal(p[11]), PreisProStk: ParseDecimal(p[12]), Menge: ParseDecimal(p[13]), MwStEuro: ParseDecimal(p[17]), Netto: ParseDecimal(p[18]), Brutto: ParseDecimal(p[19]), BezahlArt: p[16])).ToList();
    }
    private decimal ParseDecimal(string value) => decimal.TryParse(value.Trim('"'), NumberStyles.Any, GermanCulture, out var res) ? res : 0m;
}