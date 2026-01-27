using DocumentFormat.OpenXml.Spreadsheet;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SylviesKostbarkeiten.models;
using Colors = QuestPDF.Helpers.Colors;

namespace SylviesKostbarkeiten;

public class KartenTool
{
    // Beispiel-Logik für QuestPDF
    public void ErstelleSpeisekarte(List<KassenArtikelInfo> artikel)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(2, Unit.Centimetre);
                page.Header().Text("Sylvies Kostbarkeiten").FontSize(28).SemiBold().FontColor("#1b4f72");

                page.Content().Column(col =>
                {
                    foreach (var gruppe in artikel.GroupBy(a => a.GroupName))
                    {
                        col.Item().PaddingTop(10).Text(gruppe.Key).FontSize(18).Bold();
                    
                        foreach (var art in gruppe)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text(art.NameLong);
                                row.ConstantItem(50).AlignRight().Text($"{art.Price:N2} €");
                            });
                            // Hier könnte man Platzhalter für Allergene lassen
                            col.Item().Text("Allergene: A, C, G").FontSize(8).FontColor(Colors.Grey.Medium);
                        }
                    }
                });
            });
        }).GeneratePdf("Speisekarte_Neu.pdf");
    }
}