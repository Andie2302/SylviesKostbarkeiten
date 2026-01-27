namespace SylviesKostbarkeiten.models;

public record KassenGruppenInfo(
    string Name,
    string Id,
    string Color,
    bool Active,
    string PrinterId,
    List<KassenArtikelInfo> Artikel
);