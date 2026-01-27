using System.Globalization;

namespace SylviesKostbarkeiten.models;

public record KassenArtikelInfo(
    string NameLong,
    string NameShort,
    string Id,
    string Color,
    bool EmptyItem,
    decimal Price,
    decimal Vat,
    bool ImmediateEdit,
    string ImmediateField,
    string AccountNumber,
    string ArticleNumber,
    string EanCode,
    string PrinterId,
    string ExtraIds,
    bool Favorite,
    int FavoriteIndex,
    string FavoriteColor,
    string GroupName
){
    public string ToCsvLine()
    {
        var culture = new CultureInfo("de-AT");
        // Das Format verlangt 5 leere Spalten am Anfang für Artikelzeilen
        return string.Join(";", 
            "", "", "", "", "", 
            NameLong, NameShort, Id, Color, 
            EmptyItem ? "1" : "0",
            $"\"{Price.ToString("F2", culture)}\"", 
            Vat.ToString("G", culture),
            ImmediateEdit ? "1" : "0",
            ImmediateField, AccountNumber, ArticleNumber, EanCode, 
            PrinterId, ExtraIds, 
            Favorite ? "1" : "0", FavoriteIndex, FavoriteColor);
    }
}