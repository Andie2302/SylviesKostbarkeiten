use std::fs::File;
use std::io::{BufReader, Read};
use std::path::Path;
use walkdir::WalkDir;
use zip::ZipArchive;

const CSV_EXTENSION: &str = "csv";
const ZIP_EXTENSION: &str = "zip";
const CSV_SUFFIX: &str = ".csv";

pub fn process_folder(root_path: &str) {
    for entry in WalkDir::new(root_path).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if !path.is_file() {
            continue;
        }

        let extension = path.extension().and_then(|s| s.to_str());
        match extension {
            Some(CSV_EXTENSION) => process_csv_file(path),
            Some(ZIP_EXTENSION) => process_zip_file(path),
            _ => {}
        }
    }
}

fn process_zip_file(path: &Path) {
    println!("Searching ZIP: {:?}", path);
    let file = File::open(path).expect("Could not open ZIP file");
    let mut archive = ZipArchive::new(file).expect("Invalid ZIP archive");

    for i in 0..archive.len() {
        let mut entry = archive.by_index(i).unwrap();

        // entry.name() liefert ein Result<Cow<str>, ZipError>
        // Mit .expect() oder .unwrap() kommst du an den eigentlichen Namen
        let entry_name = entry.name().expect("Invalid filename in ZIP");

        if entry_name.ends_with(CSV_SUFFIX) {
            println!("  Found in ZIP: {}", entry_name);

            let mut content = String::new();
            // entry implementiert das Read-Trait, wir lesen also direkt im Stream
            entry.read_to_string(&mut content).ok();

            process_csv_content(content);
        }
    }
}

fn process_csv_file(path: &Path) {
    println!("Processing CSV: {:?}", path);
    let file = File::open(path).expect("Could not open file");
    let _reader = BufReader::new(file);
    // dein_parser.parse_reader(_reader);
}

fn process_csv_content(content: String) {
    println!("--- CSV content ---");
    println!("{}", content);
    println!("--- End of CSV content ---");
}