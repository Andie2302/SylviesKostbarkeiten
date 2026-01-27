namespace SylviesKostbarkeiten.models;

public record VerkaufsPositionInfo(
    string BelegNr,
    DateTime Zeitstempel,
    string KassaBezeichnung,
    string ArtikelName,
    string Gruppe,
    decimal MwStSatz,
    decimal PreisProStk,
    decimal Menge,
    decimal MwStEuro,
    decimal Netto,
    decimal Brutto,
    string BezahlArt
);