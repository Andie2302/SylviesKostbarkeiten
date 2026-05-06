use std::fs::File;
use std::io::Read;
use std::path::Path;
use walkdir::WalkDir;
use zip::ZipArchive;

// Importiere deine Module aus der Library
use SylviesKostbarkeiten::beleg_map::BelegArchiv;
use SylviesKostbarkeiten::csv_file;

const CSV_EXTENSION: &str = "csv";
const ZIP_EXTENSION: &str = "zip";
const CSV_SUFFIX: &str = ".csv";

fn main() {
    let root_path = "/home/andreas/Schreibtisch/Sylvie/";

    // 1. Initialisiere das zentrale Archiv
    let mut archiv = BelegArchiv::new();

    println!("--- Starte Verarbeitung in: {} ---", root_path);

    // 2. Starte die Suche und übergib das Archiv
    process_folder(root_path, &mut archiv);

    println!("--- Verarbeitung abgeschlossen ---\n");

    // 3. Test-Ausgabe: Zeige alle Monate und die Anzahl der Belege
    for (schluessel, liste) in archiv.alle_monate() {
        let (jahr, monat) = schluessel;
        println!("Monat {:02}/{}: {} Belege gefunden.", monat, jahr, liste.len());

        // Optional: Summe berechnen
        let summe: f64 = liste.into_iter().map(|b| b.brutto()).sum();
        println!("  -> Gesamtumsatz (Brutto): {:.2} €", summe);
    }
}

pub fn process_folder(root_path: &str, archiv: &mut BelegArchiv) {
    for entry in WalkDir::new(root_path).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if !path.is_file() {
            continue;
        }

        let extension = path.extension().and_then(|s| s.to_str());
        match extension {
            // Nutze die Funktionen aus csv_file.rs statt nur zu printen
            Some(CSV_EXTENSION) => csv_file::process_csv_file(path, archiv),
            Some(ZIP_EXTENSION) => process_zip_file(path, archiv),
            _ => {}
        }
    }
}

fn process_zip_file(path: &Path, archiv: &mut BelegArchiv) {
    let file = File::open(path).expect("ZIP konnte nicht geöffnet werden");
    let mut archive = ZipArchive::new(file).expect("Ungültiges ZIP");

    for i in 0..archive.len() {
        let mut entry = archive.by_index(i).expect("Fehler beim ZIP-Eintrag");
        let entry_name = entry.name().expect("Ungültiger Name").to_string();

        if entry_name.ends_with(CSV_SUFFIX) {
            let mut content = String::new();
            if entry.read_to_string(&mut content).is_ok() {
                // Nutze die Funktion für ZIP-Inhalte
                csv_file::process_csv_content(&content, archiv);
            }
        }
    }
}