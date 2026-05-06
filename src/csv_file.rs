use std::fs::File;
use std::io::Cursor;
use std::path::Path;
use std::error::Error;

use crate::beleg::KassenBeleg;
use crate::beleg_map::BelegArchiv;

/// Semikolon als Trennzeichen für deutsche CSV-Exporte.
const CSV_DELIMITER: u8 = b';';

/// Erstellt einen konfigurierten CSV-Reader für den gegebenen Reader.
fn build_csv_reader<R: std::io::Read>(reader: R) -> csv::Reader<R> {
    csv::ReaderBuilder::new()
        .delimiter(CSV_DELIMITER)
        .has_headers(true)
        .from_reader(reader)
}

/// Liest CSV-Daten aus einem beliebigen Reader (File, Cursor, etc.)
/// und füllt das Archiv.
pub fn parse_csv_to_archiv<R: std::io::Read>(
    reader: R,
    archiv: &mut BelegArchiv,
) -> Result<(), Box<dyn Error>> {
    let mut csv_reader = build_csv_reader(reader);

    for result in csv_reader.deserialize() {
        let beleg: KassenBeleg = result?;
        archiv.hinzufügen(beleg);
    }

    Ok(())
}

pub fn process_csv_file(path: &Path, archiv: &mut BelegArchiv) {
    let file = File::open(path).expect("Konnte Datei nicht öffnen");
    if let Err(e) = parse_csv_to_archiv(file, archiv) {
        eprintln!("Fehler beim Parsen von {:?}: {}", path, e);
    }
}

pub fn process_csv_content(content: &str, archiv: &mut BelegArchiv) {
    let cursor = Cursor::new(content);
    if let Err(e) = parse_csv_to_archiv(cursor, archiv) {
        eprintln!("Fehler beim Parsen eines ZIP-Inhalts: {}", e);
    }
}