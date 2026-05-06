use std::cmp::Ordering;
use serde::Deserialize;
use chrono::NaiveDateTime;

const DATE_FORMAT: &str = "%d.%m.%y %H:%M";

fn unix_epoch() -> NaiveDateTime {
    chrono::DateTime::from_timestamp(0, 0)
        .expect("timestamp 0 is always valid")
        .naive_utc()
}

fn parse_german_number(number: &str) -> f64 {
    number.replace(',', ".").replace('%', "").parse().unwrap_or(0.0)
}

#[derive(Debug, Deserialize)]
pub struct KassenBeleg {
    #[serde(rename = "Beleg Nr")]
    beleg_nr: String,
    #[serde(rename = "Erstellt")]
    datum_raw: String,
    #[serde(rename = "Artikel lang")]
    artikel: String,
    #[serde(rename = "MwSt.")]
    mwst_satz: String,
    #[serde(rename = "Netto")]
    netto_raw: String,
    #[serde(rename = "Brutto")]
    brutto_raw: String,
}

impl KassenBeleg {
    pub fn brutto(&self) -> f64 {
        parse_german_number(&self.brutto_raw)
    }

    pub fn netto(&self) -> f64 {
        parse_german_number(&self.netto_raw)
    }

    pub fn mwst(&self) -> f64 {
        parse_german_number(&self.mwst_satz)
    }

    pub fn artikel(&self) -> &str {
        &self.artikel
    }

    pub fn beleg_nr(&self) -> &str {
        &self.beleg_nr
    }

    pub fn datum(&self) -> NaiveDateTime {
        NaiveDateTime::parse_from_str(&self.datum_raw, DATE_FORMAT)
            .unwrap_or_else(|_| unix_epoch())
    }
}
impl PartialEq for KassenBeleg {
    fn eq(&self, other: &Self) -> bool {
        self.beleg_nr == other.beleg_nr
    }
}


impl Eq for KassenBeleg {}


impl Ord for KassenBeleg {
    fn cmp(&self, other: &Self) -> Ordering {
        // Wir sortieren weiterhin nach Datum, damit die Liste chronologisch bleibt
        let res = self.datum().cmp(&other.datum());

        if res == Ordering::Equal {
            // Wenn das Datum gleich ist, entscheidet die Belegnummer
            return self.beleg_nr.cmp(&other.beleg_nr);
        }
        res
    }
}

impl PartialOrd for KassenBeleg {
    fn partial_cmp(&self, other: &Self) -> Option<Ordering> {
        Some(self.cmp(other))
    }
}