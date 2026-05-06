use std::fs::File;
use std::io::{BufRead, BufReader, Read};
use std::path::Path;
use walkdir::WalkDir;
use zip::ZipArchive;

const CSV_EXTENSION: &str = "csv";
const ZIP_EXTENSION: &str = "zip";
const CSV_SUFFIX: &str = ".csv";
use SylviesKostbarkeiten::csv_search;

fn main() {
    let root_path = "/home/andreas/Schreibtisch/Sylvie/";
    println!("--- Starting search in: {} ---", root_path);
    csv_search::process_folder(root_path);
    println!("--- Search complete ---");
}
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
    println!("\n[ZIP] Searching in: {:?}", path);

    let file = File::open(path).expect("Could not open ZIP file");
    let mut archive = ZipArchive::new(file).expect("Invalid ZIP archive");

    for i in 0..archive.len() {
        let mut entry = archive.by_index(i).expect("Error reading ZIP entry");
        let entry_name = entry.name().expect("Invalid filename in ZIP").to_string();

        if entry_name.ends_with(CSV_SUFFIX) {
            println!("  -> Found CSV in ZIP: {}", entry_name);

            let mut content = String::new();
            if entry.read_to_string(&mut content).is_ok() {
                process_csv_content(&content);
            }
        }
    }
}

fn process_csv_file(path: &Path) {
    println!("\n[CSV] Processing file: {:?}", path);

    let file = File::open(path).expect("Could not open CSV file");
    let reader = BufReader::new(file);

    for line in reader.lines() {
        if let Ok(line) = line {
            println!("{}", line);
        }
    }
}

fn process_csv_content(content: &str) {
    println!("--- CSV content from ZIP ---");
    for line in content.lines() {
        println!("{}", line);
    }
    println!("--- End of ZIP content ---");
}