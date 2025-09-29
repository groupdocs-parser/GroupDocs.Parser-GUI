# GroupDocs.Parser GUI  - Document Parser and Template Editor

[![Product Page](https://img.shields.io/badge/Product%20Page-2865E0?style=for-the-badge&logo=appveyor&logoColor=white)](https://products.groupdocs.com/parser/net/) 
[![Docs](https://img.shields.io/badge/Docs-2865E0?style=for-the-badge&logo=Hugo&logoColor=white)](https://docs.groupdocs.com/parser/net/) 
[![Demos](https://img.shields.io/badge/Demos-2865E0?style=for-the-badge&logo=appveyor&logoColor=white)](https://products.groupdocs.app/parser/total) 
[![API Reference](https://img.shields.io/badge/API%20Reference-2865E0?style=for-the-badge&logo=html5&logoColor=white)](https://reference.groupdocs.com/parser/net/) 
[![Blog](https://img.shields.io/badge/Blog-2865E0?style=for-the-badge&logo=WordPress&logoColor=white)](https://blog.groupdocs.com/category/parser/) 
[![Free Support](https://img.shields.io/badge/Free%20Support-2865E0?style=for-the-badge&logo=Discourse&logoColor=white)](https://forum.groupdocs.com/c/parser) 
[![Temporary License](https://img.shields.io/badge/Temporary%20License-2865E0?style=for-the-badge&logo=rocket&logoColor=white)](https://purchase.groupdocs.com/temporary-license)

This repository provides a **WPF-based document parser and template editor** built on [GroupDocs.Parser for .NET](https://products.groupdocs.com/parser/net/).  
It allows you to visually define parsing templates for documents and then apply them to extract structured data.

---

## 📖 Overview

The solution includes project(s):

- **GroupDocs.Parser.GUI** – Windows desktop GUI for creating and editing parsing templates.  

![Parser GUI Demo](./images/app-demo-01.png)

---

## ⚙️ Features

- **Open & View Files** – Load PDF or image documents into the viewer.
- **Set License** – Apply a valid GroupDocs license.
- **Zoom Controls** – Adjust document zoom level (`+`, `-`, `110%`).
- **Parse Methods**
  - **Text** – Create a visual template field for text extraction.
  - **Barcode** – Create a visual template field for barcode extraction.
  - **OCR** – Enable text recognition for scanned images.
  - **Parse Fields** – Extract values using defined template fields.
  - **Parse Document** – Run full parsing logic over the entire document.
- **Template Management**
  - **Generate Template** – Automatically generate template JSON based on selected fields.
  - **Visibility** – Toggle template field overlays on the preview.
  - **Template Fields Panel** – Manage and inspect detected fields (Invoice Number, Date, Total, Tax, Details, etc.).

- Visual template editor for PDFs and scanned images.
- Supports three field types:  
  - **Field**  
  - **Table** (🚧 not supported in Beta)  
  - **Barcode**  
- Save & load templates as XML definition files.  
- Apply templates to documents via console tool.  
- OCR support for scanned PDFs and TIFFs.

---

## 📂 Repository Structure

```
├── Distribution/        # Precompiled binaries
│   └── GroupDocs.Parser.GUI/
├── src/                 # Source code
│   └── GUI/
├── Examples/            # Sample documents, templates & output files
```

---

## 🚀 Getting Started

### Set License

Update `config.json` with your GroupDocs.Parser license:

```json
{
  "LicensePath": "D:\\Licenses\\GroupDocs.Parser.NET.lic"
}
```

👉 If you don’t have a license, request a free temporary one here:  
[Get Temporary License](https://purchase.groupdocs.com/temporary-license/)

---

### Create a Parsing Template

Run the GUI:

```bash
.\Distribution\GroupDocs.Parser.GUI\GroupDocs.Parser.GUI.bat
```

Steps:
1. Open a document.  
2. (Optional) Click **Generate Template** to enable auto field adjustment.  
3. Add fields visually (Field / Barcode).  
4. Save the template as XML.  
5. Try parsing inside the GUI.  

---

## User Interface

### Top Toolbar

- **Set License**: Load GroupDocs license.
- **Open File**: Browse and open a document file.
- **Zoom Controls**: Adjust view scale.
- **Parsing Controls**:
  - `Text`: Add a text extraction field visually.
  - `Barcode`: Add a barcode extraction field visually.
  - `OCR`: Enable OCR for scanned documents.
  - `500`: Set parsing resolution (DPI).
  - `Parse Fields`: Extract based on template fields only.
  - `Parse Document`: Parse entire document.
  - `Visibility`: Show/hide template field rectangles.
  - `Generate Template`: Export defined fields into a reusable template file.

### Document Viewer

- Displays the loaded file.
- Allows placing template fields by clicking **Text** or **Barcode** and drawing over regions.
- Extracted fields are highlighted with bounding boxes.

---

## 🛠 Installation for Developers

Clone the repository and build with .NET:

```bash
git clone https://github.com/groupdocs-parser/groupdocs-parser-gui.git
cd groupdocs-parser-gui/src
dotnet build
```

Run GUI:

```bash
dotnet run --project GUI
```

Run Console Parser:

```bash
dotnet run --project DocumentParser
```

---

## 📌 Beta Limitations

- Supported documents:  
  - PDFs with text  
  - Scanned PDFs & TIFF images (with OCR enabled)  
- Supported field types
  - Text field
  - Barcode field

- Templates work **per page** (can be reused across pages with same structure).  

---

## 🔮 Roadmap

- Automatic detection of scanned vs text-based docs (auto OCR toggle).  
- Implement Table field support.  
- Improve GUI usability & stability.  
- Automated matching between documents and templates.  

---

## 🤝 Contributing

This project is open-source.  
You are welcome to:
- Suggest new features.  
- Submit pull requests.  
- Extend the tool with your own dev capabilities.  

---

## 📜 License

This tool is provided for **customer convenience** under open-source terms.  
For core parsing functionality, a [GroupDocs.Parser for .NET](https://products.groupdocs.com/parser/net/) license is required.  
