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
}