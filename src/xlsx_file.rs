use rust_xlsxwriter::*;

// ─────────────────────────────────────────────
//  Constants
// ─────────────────────────────────────────────
const EXCEL_BLUE: u32 = 0x4472C4;

// ─────────────────────────────────────────────
//  Reusable cell value types (for write_row)
// ─────────────────────────────────────────────
pub enum CellValue<'a> {
    Text(&'a str),
    Number(f64),
    Currency(f64),
    Integer(i64),
    Percent(f64), // 0.25 → "25.00 %"
    Empty,
}

// ─────────────────────────────────────────────
//  Main struct
// ─────────────────────────────────────────────
pub struct XlsxFile {
    workbook: Option<Workbook>,
    pub fmt_header: Format,
    pub fmt_currency: Format,
    pub fmt_integer: Format,
    pub fmt_percent: Format,
    pub fmt_bold: Format,
    pub fmt_center: Format,
    pub fmt_date: Format,
}

// ─────────────────────────────────────────────
//  Format construction
// ─────────────────────────────────────────────
fn build_formats() -> (Format, Format, Format, Format, Format, Format, Format) {
    let fmt_header = Format::new()
        .set_bold()
        .set_font_color(Color::White)
        .set_background_color(Color::RGB(EXCEL_BLUE))
        .set_align(FormatAlign::Center)
        .set_border_bottom(FormatBorder::Medium);

    let fmt_currency = Format::new().set_num_format("#,##0.00 €");
    let fmt_integer = Format::new().set_num_format("#,##0");
    let fmt_percent = Format::new().set_num_format("0.00%");
    let fmt_bold = Format::new().set_bold();
    let fmt_center = Format::new().set_align(FormatAlign::Center);
    let fmt_date = Format::new().set_num_format("DD.MM.YYYY");

    (
        fmt_header,
        fmt_currency,
        fmt_integer,
        fmt_percent,
        fmt_bold,
        fmt_center,
        fmt_date,
    )
}

impl XlsxFile {
    pub fn new() -> Self {
        let (fmt_header, fmt_currency, fmt_integer, fmt_percent, fmt_bold, fmt_center, fmt_date) =
            build_formats();

        Self {
            workbook: Some(Workbook::new()),
            fmt_header,
            fmt_currency,
            fmt_integer,
            fmt_percent,
            fmt_bold,
            fmt_center,
            fmt_date,
        }
    }

    // ─── Sheets ──────────────────────────────────────────────────────────────

    /// Add a new worksheet and return a mutable reference to it.
    pub fn add_sheet(&mut self, name: &str) -> Result<&mut Worksheet, XlsxError> {
        match self.workbook {
            Some(ref mut wb) => {
                let ws = wb.add_worksheet().set_name(name)?;
                Ok(ws)
            }
            None => Err(XlsxError::ParameterError(
                "Workbook has already been saved and is no longer available.".to_string(),
            )),
        }
    }

    // ─── Convenience write methods ───────────────────────────────────────────

    /// Write a header row using the header format.
    ///
    /// ```rust
    /// xlsx.write_headers(ws, 0, &["Article", "Qty", "Price"])?;
    /// ```
    pub fn write_headers(
        &self,
        ws: &mut Worksheet,
        row: u32,
        headers: &[&str],
    ) -> Result<(), XlsxError> {
        for (col, &text) in headers.iter().enumerate() {
            ws.write_with_format(row, col as u16, text, &self.fmt_header)?;
        }
        Ok(())
    }

    /// Write a single cell value, applying the appropriate format.
    fn write_cell(
        &self,
        ws: &mut Worksheet,
        row: u32,
        col: u16,
        val: &CellValue,
    ) -> Result<(), XlsxError> {
        match val {
            CellValue::Text(s) => ws.write(row, col, *s)?,
            CellValue::Number(n) => ws.write(row, col, *n)?,
            CellValue::Currency(n) => ws.write_with_format(row, col, *n, &self.fmt_currency)?,
            CellValue::Integer(n) => {
                ws.write_with_format(row, col, *n as f64, &self.fmt_integer)?
            }
            CellValue::Percent(n) => ws.write_with_format(row, col, *n, &self.fmt_percent)?,
            CellValue::Empty => ws.write_blank(row, col, &Format::new())?,
        };
        Ok(())
    }

    /// Write a data row with mixed types via `CellValue`.
    ///
    /// ```rust
    /// xlsx.write_row(ws, 1, &[
    ///     CellValue::Text("Apple"),
    ///     CellValue::Integer(42),
    ///     CellValue::Currency(3.99),
    /// ])?;
    /// ```
    pub fn write_row(
        &self,
        ws: &mut Worksheet,
        row: u32,
        values: &[CellValue],
    ) -> Result<(), XlsxError> {
        for (col, val) in values.iter().enumerate() {
            self.write_cell(ws, row, col as u16, val)?;
        }
        Ok(())
    }

    /// Write multiple rows at once.
    ///
    /// ```rust
    /// let rows = vec![
    ///     vec![CellValue::Text("A"), CellValue::Currency(10.0)],
    ///     vec![CellValue::Text("B"), CellValue::Currency(20.0)],
    /// ];
    /// xlsx.write_rows(ws, 1, &rows)?;
    /// ```
    pub fn write_rows(
        &self,
        ws: &mut Worksheet,
        start_row: u32,
        rows: &[Vec<CellValue>],
    ) -> Result<(), XlsxError> {
        for (i, row) in rows.iter().enumerate() {
            self.write_row(ws, start_row + i as u32, row)?;
        }
        Ok(())
    }

    /// Set the widths of multiple columns at once.
    ///
    /// ```rust
    /// xlsx.set_column_widths(ws, &[20.0, 10.0, 15.0])?;
    /// ```
    pub fn set_column_widths(
        &self,
        ws: &mut Worksheet,
        widths: &[f64],
    ) -> Result<(), XlsxError> {
        for (col, &width) in widths.iter().enumerate() {
            ws.set_column_width(col as u16, width)?;
        }
        Ok(())
    }

    /// Convenience method: write headers, data rows, and optional column widths in one step.
    ///
    /// ```rust
    /// xlsx.write_table(
    ///     ws,
    ///     &["Product", "Qty", "Price"],
    ///     &rows,
    ///     Some(&[25.0, 10.0, 15.0]),
    /// )?;
    /// ```
    pub fn write_table(
        &self,
        ws: &mut Worksheet,
        headers: &[&str],
        rows: &[Vec<CellValue>],
        col_widths: Option<&[f64]>,
    ) -> Result<(), XlsxError> {
        self.write_headers(ws, 0, headers)?;
        self.write_rows(ws, 1, rows)?;
        if let Some(widths) = col_widths {
            self.set_column_widths(ws, widths)?;
        }
        Ok(())
    }

    // ─── Save ────────────────────────────────────────────────────────────────

    /// Save the workbook to the given file path.
    pub fn save(mut self, path: &str) -> Result<(), XlsxError> {
        if let Some(mut wb) = self.workbook.take() {
            wb.save(path)?;
        }
        Ok(())
    }
}

impl Default for XlsxFile {
    fn default() -> Self {
        Self::new()
    }
}