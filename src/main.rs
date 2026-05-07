use std::fs::File;
use std::io::Read;
use std::path::Path;
use walkdir::WalkDir;
use zip::ZipArchive;

use SylviesKostbarkeiten::beleg_map::BelegArchiv;
use SylviesKostbarkeiten::csv_file;
use SylviesKostbarkeiten::xlsx_file::{CellValue, XlsxFile};

const CSV_EXTENSION: &str = "csv";
const ZIP_EXTENSION: &str = "zip";
const CSV_SUFFIX: &str = ".csv";

fn main() {
    let args: Vec<String> = std::env::args().collect();

    // Optionaler MwSt-Filter als zweites Argument, z. B.: ./programm /pfad 20
    let root_path = args.get(1).map(String::as_str).unwrap_or("/home/andreas/Schreibtisch/Sylvie/");
    let mwst_filter: Option<f64> = args.get(2).and_then(|s| s.parse().ok());

    let mut archiv = BelegArchiv::new();

    println!("━━━ Verarbeitung gestartet: {} ━━━\n", root_path);
    process_folder(root_path, &mut archiv);
    println!("━━━ Verarbeitung abgeschlossen ━━━\n");

    // ── Monatsübersicht ─────────────────────────────────────────────────────────
    println!("📅 Monatsübersicht ({} Belege gesamt)\n", archiv.gesamtanzahl());

    for (schluessel, liste) in archiv.alle_monate() {
        let (jahr, monat) = schluessel;
        println!(
            "  {:02}/{}: {:>4} Belege | Brutto: {:>10.2} € | Netto: {:>10.2} € | MwSt.: {:>8.2} €",
            monat,
            jahr,
            liste.len(),
            liste.gesamtbrutto(),
            liste.gesamtnetto(),
            liste.gesamtmwst_betrag(),
        );
    }

    println!("\n  {}", String::from('─').repeat(70));
    println!("  Gesamt: {:>10.2} € Brutto\n", archiv.gesamtbrutto());

    // ── MwSt-Filter ─────────────────────────────────────────────────────────────
    if let Some(prozent) = mwst_filter {
        println!("🔍 Filter: Belege mit {:.0}% MwSt.\n", prozent);
        let gefiltert = archiv.filtere_nach_mwst(prozent);

        if gefiltert.is_empty() {
            println!("  Keine Belege mit {:.0}% MwSt. gefunden.", prozent);
        } else {
            for beleg in &gefiltert {
                println!("  {}", beleg.zusammenfassung());
            }
            println!(
                "\n  Summe ({} Belege): Brutto {:.2} € | Netto {:.2} € | MwSt. {:.2} €",
                gefiltert.len(),
                gefiltert.gesamtbrutto(),
                gefiltert.gesamtnetto(),
                gefiltert.gesamtmwst_betrag(),
            );
        }
    } else {
        // Zeige verfügbare MwSt-Sätze als Hinweis
        println!("💡 Tipp: MwSt-Satz als Argument angeben, um zu filtern:");
        println!("   Beispiel: {} \"{}\" 20", args[0], root_path);

        // Sammle alle Sätze aus dem ersten Monat (als Vorschau)
        if let Some(liste) = archiv.alle_monate().values().next() {
            let saetze = liste.vorhandene_mwst_saetze();
            if !saetze.is_empty() {
                let saetze_str: Vec<String> = saetze.iter().map(|s| format!("{:.0}%", s)).collect();
                println!("   Gefundene Sätze (Stichprobe 1. Monat): {}", saetze_str.join(", "));
            }
        }
    }

    let mut xlsx = XlsxFile::new();

    // Diese Variable hat gefehlt!
    let rows = vec![
        vec![CellValue::Text("Apfel"), CellValue::Integer(10), CellValue::Currency(1.99)],
        vec![CellValue::Text("Birne"), CellValue::Integer(5), CellValue::Currency(2.49)],
    ];

    // Hier packen wir add_sheet direkt in den Aufruf:
    xlsx.write_table(
        xlsx.add_sheet("Bestellung").expect("Sheet Fehler"),
        &["Produkt", "Menge", "Preis"],
        &rows,
        Some(&[20.0, 10.0, 15.0])
    ).expect("Schreib Fehler");

    xlsx.save("bestellung.xlsx").expect("Speicher Fehler");

    println!("✅ Excel-Datei 'bestellung.xlsx' wurde erfolgreich erstellt!");
}

pub fn process_folder(root_path: &str, archiv: &mut BelegArchiv) {
    for entry in WalkDir::new(root_path).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if !path.is_file() {
            continue;
        }

        match path.extension().and_then(|s| s.to_str()) {
            Some(CSV_EXTENSION) => {
                println!("  📄 CSV: {:?}", path.file_name().unwrap_or_default());
                csv_file::process_csv_file(path, archiv);
            }
            Some(ZIP_EXTENSION) => {
                println!("  🗜  ZIP: {:?}", path.file_name().unwrap_or_default());
                process_zip_file(path, archiv);
            }
            _ => {}
        }
    }
}

fn process_zip_file(path: &Path, archiv: &mut BelegArchiv) {
    let file = match File::open(path) {
        Ok(f) => f,
        Err(e) => {
            eprintln!("  ⚠ ZIP konnte nicht geöffnet werden ({:?}): {}", path, e);
            return;
        }
    };
    let mut archive = match ZipArchive::new(file) {
        Ok(a) => a,
        Err(e) => {
            eprintln!("  ⚠ Ungültiges ZIP ({:?}): {}", path, e);
            return;
        }
    };

    for i in 0..archive.len() {
        let mut entry = match archive.by_index(i) {
            Ok(e) => e,
            Err(e) => {
                eprintln!("  ⚠ Fehler beim ZIP-Eintrag {}: {}", i, e);
                continue;
            }
        };

        // zip >= 0.6: entry.name() gibt &str zurück (kein Result mehr)
        // Wir nutzen .expect(), um den Namen aus dem Result zu holen.
        let entry_name = entry.name().expect("Ungültiger Dateiname im ZIP").to_string();
        if entry_name.ends_with(CSV_SUFFIX) {
            println!("    📄 In ZIP: {}", entry_name);
            let mut content = String::new();
            if let Err(e) = entry.read_to_string(&mut content) {
                eprintln!("    ⚠ Lesefehler: {}", e);
                continue;
            }
            csv_file::process_csv_content(&content, archiv);
        }
    }
}