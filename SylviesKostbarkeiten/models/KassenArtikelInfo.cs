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
    string FavoriteColor
);