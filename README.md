# USML Pdf Xml Viewer 2025

Date: 05/08/2025
This WPF application facilitates the tagging, viewing, 

and processing of XML and PDF content—specifically for managing sidenotes, 

placeholders, and OCR-tagged structures like <S#>. 

It integrates AvalonEdit for XML editing, 

CefSharp for visual previewing and 

PdfPig for extracting text from PDF files.

## Authors

- TRB1954 Ma. Pauline Mae J. Mionez


## Documentation

Input:
- XML files
- PDF files

Output:
- Excel for error report

Process: 
- Loading XML and PDF files.
- Editing XML in AvalonEdit.
- Processing sidenote tags with IDs and placeholders.
- Linking sidenote1 to <S#> references.
- Viewing changes live via an embedded CefSharp browser.
- Highlight text from browser viewe as error and label for error reports.
- Exporting Error report.

## Features

- SyncFusion control
- Async await
- Newtonsoft.json 
- DevExpress.MVVM: Binding properties.
- AvalonEdit: Rich text editing for XML.
- CefSharp.WPF: Browser embedding and DOM/JS interaction.
- PdfPig/UglyToad: Used for text extraction via bounding boxes.
- NHunspell: Spell checking engine using Hunspell dictionaries (.aff / .dic).

## Development Tips

- Be cautious with XML formatting—use a non-destructive parser if whitespace/indentation matters.
- When replacing selections in AvalonEdit, always preserve caret offset carefully.
- Use ReloadBrowserViewer(true) only after major DOM changes; it wires up JS listeners again.
- Consider implementing an undo/redo stack for editing XML if needed.


## Used By

This project is used by the following Team Leaders:

- Mam Roselle Avendaño
