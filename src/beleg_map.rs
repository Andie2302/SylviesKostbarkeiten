use std::collections::BTreeMap;
use chrono::Datelike;
use crate::beleg::KassenBeleg;
use crate::beleg_liste::KassenBelegListe;

type MonatsGruppenMap = BTreeMap<(i32, u32), KassenBelegListe>;

#[derive(Debug, Default)]
pub struct BelegArchiv {
    monats_gruppen: MonatsGruppenMap,
}

impl BelegArchiv {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn hinzufügen(&mut self, beleg: KassenBeleg) {
        let datum = beleg.datum();
        let monatschlüssel = (datum.year(), datum.month());

        self.monats_gruppen
            .entry(monatschlüssel)
            .or_default()
            .push(beleg);
    }

    pub fn monat(&self, jahr: i32, monat: u32) -> Option<&KassenBelegListe> {
        self.monats_gruppen.get(&(jahr, monat))
    }

    pub fn alle_monate(&self) -> &MonatsGruppenMap {
        &self.monats_gruppen
    }

    /// Gibt alle Belege des Archivs gefiltert nach MwSt-Satz zurück.
    /// Beispiel: `archiv.filtere_nach_mwst(20.0)` für alle 20%-Belege.
    pub fn filtere_nach_mwst(&self, prozent: f64) -> KassenBelegListe {
        let mut ergebnis = KassenBelegListe::default();
        for liste in self.monats_gruppen.values() {
            for beleg in liste {
                if (beleg.mwst() - prozent).abs() < 0.01 {
                    ergebnis.push(beleg.clone());
                }
            }
        }
        ergebnis
    }

    /// Gesamtumsatz (Brutto) über alle Monate.
    pub fn gesamtbrutto(&self) -> f64 {
        self.monats_gruppen
            .values()
            .map(|l| l.gesamtbrutto())
            .sum()
    }

    /// Gesamtanzahl aller Belege.
    pub fn gesamtanzahl(&self) -> usize {
        self.monats_gruppen.values().map(|l| l.len()).sum()
    }
}