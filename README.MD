# Choir Music System

A cross-platform web application for managing choir music sheets, planning music for Mass, generating combined choir music-sheet PDFs, and eventually generating PowerPoint presentations.

The application is designed to be developed on both **macOS and Windows** using the same codebase.

## Project Goals

The system will provide a central library for choir music and make preparing for Mass easier.

The initial focus is on PDF music sheets. PowerPoint functionality will be added as a later phase, but the application architecture should support it from the beginning.

The overall goal is:

```text
Music Library
      ↓
Create Mass
      ↓
Select Music
      ↓
 ┌────┴─────────┐
 ↓              ↓
Music Pack    PowerPoint
   PDF          PPTX
Phase 1       Future
```

---

# Phase 1 — Music Sheets

**Status: Core Music Library complete; Mass Planning and Music Pack generation in progress.**

Phase 1 provides:

* Upload PDF music sheets
* Maintain the music library
* Edit music information
* Replace existing PDF files
* Archive music
* Search the library
* Preview music sheets
* Create a Mass
* Select music for each Mass part
* Control the order of music
* Generate one combined choir music pack PDF
* Avoid duplicate PDFs in the generated music pack

For example, if one PDF contains the complete Mass setting and is selected for Kyrie, Gloria, Holy, Amen and Lamb of God, the PDF should only appear once in the generated music pack.

---

# Future Phase — PowerPoint

PowerPoint generation will be added after the music-sheet workflow is stable.

The future PowerPoint functionality should support:

* Maintaining song lyrics
* Importing lyrics from existing PowerPoint files
* Using an existing choir PowerPoint template
* Maintaining PowerPoint templates
* Uploading and maintaining background images
* Selecting a background when creating a presentation
* Applying the selected background to all slides
* Applying an overlay to slides containing text
* Configurable text-overlay opacity
* Maintaining consistent fonts and positioning
* Generating lyric slides automatically
* Generating songs in Mass order
* Exporting the completed presentation as `.pptx`

The database and file-storage architecture should take these future requirements into account.

---

# Technology Stack

## Backend

* ASP.NET Core 10
* C#
* Razor Pages

## Database

* Entity Framework Core
* SQLite for initial development

Future database options may include:

* PostgreSQL
* Microsoft SQL Server

## Frontend

* Razor Pages
* HTML
* CSS
* Bootstrap
* JavaScript where required

A separate JavaScript framework such as React or Angular is not required for the initial application.

## Documents

Phase 1:

* Cross-platform PDF processing library

Future:

* Open XML SDK or another suitable cross-platform PowerPoint library

## Development

* Visual Studio Code
* Git
* GitHub
* .NET CLI

---

# Supported Development Platforms

Development should work on:

* macOS
* Windows

The project should not contain code that depends unnecessarily on either operating system.

Use cross-platform .NET functionality such as:

```csharp
Path.Combine(...)
```

instead of manually constructing Windows or macOS paths.

Do not hard-code paths such as:

```text
C:\ChoirMusic\
```

or:

```text
/Users/username/ChoirMusic/
```

---

# Development Architecture

The initial architecture should remain simple.

```text
Browser
   ↓
ASP.NET Core Razor Pages
   ↓
Application Services
   ↓
Entity Framework Core
   ↓
ChoirDbContext
   ↓
SQLite
```

File operations should be handled separately:

```text
Application Services
        ↓
   File Storage
        ↓
Storage/
   ├── MusicSheets/
   ├── Templates/       Future
   ├── Backgrounds/     Future
   └── Generated/       Future
```

This separation will make it easier to move files to cloud storage in the future.

---

# Database Portability

SQLite will be used during initial development because it is lightweight, cross-platform, and does not require a separate database server.

However, the application must not depend directly on SQLite-specific functionality.

All normal database access should go through:

```text
Application
     ↓
Services
     ↓
ChoirDbContext
     ↓
Entity Framework Core
     ↓
Database Provider
```

This allows the provider to change later:

```text
Entity Framework Core
        │
        ├── SQLite
        │      Initial
        │
        ├── PostgreSQL
        │      Future option
        │
        └── SQL Server
               Future option
```

Avoid database-specific SQL wherever practical.

Connection strings must be stored in configuration and must not be hard-coded into application services.

Initial example:

```json
{
  "ConnectionStrings": {
    "ChoirDatabase": "Data Source=Data/choir.db"
  }
}
```

Entity Framework Core migrations should be used to manage schema changes.

Changing database provider in the future may require new provider-specific migrations, but the application's domain models and business services should remain largely unchanged.

---

# File Storage

PDF files should **not** be stored directly in the database.

The database stores metadata about each music sheet and the location of its associated file.

During development, files will be stored locally.

Example:

```text
Storage/
└── MusicSheets/
    ├── here-i-am-lord.pdf
    ├── mass-of-creation.pdf
    └── one-bread-one-body.pdf
```

Future files may include:

```text
Storage/
├── MusicSheets/
├── PowerPointTemplates/
├── Backgrounds/
└── Generated/
    ├── MusicPacks/
    └── PowerPoints/
```

File-storage operations should eventually be accessed through a storage service so local storage can be replaced by cloud storage without redesigning the application.

---

# Initial Data Model

## MusicSheet

Represents a music sheet maintained in the library.

Initial fields may include:

```text
Id
Title
SuggestedMassPart
Composer
Arrangement
Key
PdfFileName
PdfPath
Notes
IsActive
CreatedDate
UpdatedDate
```

## Mass

Represents a Mass being planned.

Initial fields may include:

```text
Id
Name
MassDate
Notes
CreatedDate
UpdatedDate
```

## MassMusicSheet

Connects music from the library to a Mass.

Initial fields may include:

```text
Id
MassId
MusicSheetId
MassPart
DisplayOrder
```

This allows the same music sheet to be reused across many Masses.

---

# Mass Parts

The initial system should support common Mass parts such as:

```text
Entrance
Kyrie
Gloria
Psalm
Alleluia
Offertory
Holy
Memorial Acclamation
Amen
Our Father
Lamb of God
Communion
Recessional
```

The design should allow additional parts to be added later without significant code changes.

---

# Duplicate Music-Sheet Handling

One PDF may contain several parts of the Mass.

For example:

```text
Mass of Creation.pdf
│
├── Kyrie
├── Gloria
├── Holy
├── Memorial Acclamation
├── Amen
└── Lamb of God
```

A Mass may therefore contain:

```text
Kyrie                 → Mass of Creation
Gloria                → Mass of Creation
Holy                  → Mass of Creation
Memorial Acclamation  → Mass of Creation
Amen                   → Mass of Creation
Lamb of God            → Mass of Creation
```

When generating the choir music pack:

```text
Mass of Creation.pdf
```

should only be included **once**.

The generated PDF should preserve the appropriate Mass/music order while removing duplicate source documents.

---

# Project Structure

The initial project structure should be similar to:

```text
ChoirMusicSystem/
│
├── Data/
│   └── ChoirDbContext.cs
│
├── Models/
│   ├── MusicSheet.cs
│   ├── Mass.cs
│   └── MassMusicSheet.cs
│
├── Services/
│   ├── MusicSheetService.cs
│   ├── FileStorageService.cs
│   └── PdfMergeService.cs
│
├── Pages/
│   ├── MusicLibrary/
│   ├── Masses/
│   └── Index.cshtml
│
├── Storage/
│   ├── MusicSheets/
│   └── Generated/
│
├── wwwroot/
│
├── appsettings.json
├── Program.cs
├── ChoirMusicSystem.csproj
├── .gitignore
└── README.md
```

Future folders may include:

```text
Storage/
├── PowerPointTemplates/
└── Backgrounds/

Services/
└── PowerPointService.cs
```

---

# Installation

## macOS

### 1. Install Homebrew

Check whether Homebrew is already installed:

```bash
brew --version
```

If it is not installed, install Homebrew from its official website.

### 2. Install Git

```bash
brew install git
```

Verify:

```bash
git --version
```

### 3. Install Visual Studio Code

Install Visual Studio Code for macOS.

Verify:

```bash
code --version
```

If `code` is not available from Terminal, open Visual Studio Code and use the Command Palette:

```text
Shell Command: Install 'code' command in PATH
```

### 4. Install .NET 10 SDK

Install the .NET 10 SDK for macOS.

Verify:

```bash
dotnet --version
```

Also check:

```bash
dotnet --list-sdks
```

A .NET 10 SDK should be listed.

### 5. Install SQLite

```bash
brew install sqlite
```

Verify:

```bash
sqlite3 --version
```

### 6. Install Entity Framework CLI

```bash
dotnet tool install --global dotnet-ef
```

Verify:

```bash
dotnet ef --version
```

If already installed:

```bash
dotnet tool update --global dotnet-ef
```

---

# Windows

## 1. Install Git

Install Git for Windows.

Verify from PowerShell or Windows Terminal:

```powershell
git --version
```

## 2. Install Visual Studio Code

Install Visual Studio Code for Windows.

Verify:

```powershell
code --version
```

## 3. Install .NET 10 SDK

Install the .NET 10 SDK for Windows.

Verify:

```powershell
dotnet --version
```

Also check:

```powershell
dotnet --list-sdks
```

## 4. Install SQLite

SQLite can be installed using Windows Package Manager:

```powershell
winget install SQLite.SQLite
```

Verify:

```powershell
sqlite3 --version
```

## 5. Install Entity Framework CLI

```powershell
dotnet tool install --global dotnet-ef
```

Verify:

```powershell
dotnet ef --version
```

If already installed:

```powershell
dotnet tool update --global dotnet-ef
```

---

# Visual Studio Code Extensions

Install the following extensions on both macOS and Windows:

* C# Dev Kit
* C#
* GitHub Pull Requests and Issues

A SQLite viewer extension can also be installed if desired.

---

# Verify the Development Environment

Before starting development, verify the tools.

## macOS

```bash
git --version
code --version
dotnet --version
sqlite3 --version
dotnet ef --version
```

## Windows

```powershell
git --version
code --version
dotnet --version
sqlite3 --version
dotnet ef --version
```

If all commands return valid versions, the development environment is ready.

---

# Create the Project

The project only needs to be created once.

From Terminal on macOS or PowerShell on Windows:

```bash
dotnet new webapp -n ChoirMusicSystem
cd ChoirMusicSystem
code .
```

---

# Install Project Packages

Add Entity Framework Core SQLite:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Add Entity Framework Core design support:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
```

Add PDF metadata/title extraction support:

```bash
dotnet add package PdfPig
```

Add PDF music-pack generation support:

```bash
dotnet add package PdfSharpCore
```

After cloning the repository on another computer, restore project dependencies with:

```bash
dotnet restore
```

---

# HTTPS Development Certificate

Trust the ASP.NET Core development certificate:

```bash
dotnet dev-certs https --trust
```

macOS may request permission to add the certificate to the system keychain.

Windows may display a certificate confirmation prompt.

---

# Run the Application

From the project directory:

```bash
dotnet watch
```

The application will display its development URL.

For example:

```text
https://localhost:7000
```

Open the displayed address in a browser.

Stop the application with:

```text
Ctrl + C
```

---

# Database Setup

Once `ChoirDbContext` and the initial models have been created, create the first migration:

```bash
dotnet ef migrations add InitialCreate
```

Create the local SQLite database:

```bash
dotnet ef database update
```

The same commands work on macOS and Windows.

---

# Git

## Initialize Repository

When creating the project for the first time:

```bash
git init
git add .
git commit -m "Initial choir music system"
```

A GitHub remote can then be added:

```bash
git remote add origin <repository-url>
git branch -M main
git push -u origin main
```

---

# Working Between Mac and Windows

Before leaving one development computer:

```bash
git status
git add .
git commit -m "Describe changes"
git push
```

On the other computer:

```bash
git pull
dotnet restore
dotnet ef database update
dotnet watch
```

This allows development to continue between macOS and Windows using the same codebase.

---

# Local Development Data

Source code and database migrations should be stored in Git.

The local SQLite database and uploaded PDF files should normally not be committed.

Each development computer can maintain its own test database and test PDFs.

Production file storage and database backup will be addressed before deployment.

---

# Recommended `.gitignore`

```gitignore
bin/
obj/

.vscode/

Data/*.db
Data/*.db-shm
Data/*.db-wal

Storage/MusicSheets/*
!Storage/MusicSheets/.gitkeep

Storage/Generated/*
!Storage/Generated/.gitkeep

.DS_Store
Thumbs.db
```

Do not store passwords, connection-string credentials, API keys, or other secrets in Git.

---

# Development Roadmap

## Stage 1 — Foundation — Complete

Implemented:

* ASP.NET Core 10 Razor Pages project
* SQLite configuration
* Entity Framework Core and migrations
* Core `MusicSheet`, `Mass`, and `MassMusicSheet` models
* Local cross-platform storage structure
* Git/GitHub workflow

## Stage 2 — Music Library — Complete

Implemented:

* Upload individual PDF music sheets
* Bulk upload existing PDF music sheets
* Automatic title detection from PDFs using PdfPig
* Maintain title, composer, arrangement, key, suggested Mass part, and notes
* Edit music metadata
* Replace an existing PDF
* View/preview PDFs
* Delete music sheets and their physical PDF files
* Instant client-side Music Library search
* Reuse the same music sheet across multiple Masses

The completed Music Library workflow is:

```text
Upload PDF / Bulk Upload
        ↓
Detect Title
        ↓
Maintain Metadata
        ↓
Music Library
   ├── Search
   ├── View PDF
   ├── Edit / Replace
   └── Delete
```

PDF remains the source document used when generating choir music packs. Optional MuseScore support may be considered later and is not part of the current phase.

## Stage 3 — Mass Planning — In Progress

Implemented:

* Create a Mass
* Edit and delete a Mass
* Duplicate a Mass together with its music plan
* Mass name, date, and notes
* Standard Mass parts, including Our Father
* Select music from the Music Library
* Multiple songs per Mass part
* Preserve music display order
* Search while planning by title, composer, arrangement, suggested Mass part, and key
* Display richer song information to distinguish music with the same title
* Open a selected music-sheet PDF in a new browser tab
* Prioritise useful song metadata while keeping `MusicSheetId` as the saved selection identifier

Current Mass parts:

```text
Entrance
Kyrie
Gloria
Psalm
Alleluia
Offertory
Holy
Memorial Acclamation
Amen
Our Father
Lamb of God
Communion
Recessional
```

## Stage 4 — Music Pack Generation — In Progress

Implemented foundation:

* PdfSharpCore PDF merge service
* Read selected music for a Mass
* Sort selections by Mass/display order
* Deduplicate selections by `MusicSheetId`
* Include a source PDF only once when it is used for several Mass parts
* Merge source PDFs into one choir music pack
* Save generated packs under `Storage/Generated/MusicPacks/`

The workflow is:

```text
Music Library
      ↓
Create / Duplicate Mass
      ↓
Plan Music
      ↓
Generate Music Pack
      ↓
Combined PDF
```

Further testing and polish of generated packs remains before this stage is considered complete.

## Stage 5 — PowerPoint — Future

PowerPoint work starts only after the PDF music-sheet workflow is stable. Planned capabilities include importing lyrics/order from existing presentations, using a choir template, configurable backgrounds and overlays, preserving template fonts/layout/logo, and generating songs in Mass order.

```text
Music Library
      ↓
Song / Lyrics Library
      ↓
Create Mass
      ↓
Select Music
      ↓
PowerPoint Settings
      │
      ├── Template
      ├── Background
      └── Overlay Opacity
      ↓
Generate PPTX
```

---

# Design Principles

The project should follow these principles throughout development:

**Cross-platform.** Development and execution should work on macOS and Windows.

**Simple first.** Complete the core music-sheet workflow before adding unnecessary functionality.

**Database independent.** Application logic should use Entity Framework Core and avoid depending directly on SQLite.

**Storage independent.** Application logic should not assume files will always be stored on the local filesystem.

**Reusable music.** Music is maintained once in the library and reused across multiple Masses.

**Future PowerPoint support.** Decisions made during Phase 1 should not prevent PowerPoint templates, lyrics, backgrounds, or presentation generation from being added later.

**No hard-coded paths.** Use configuration and cross-platform path handling.

**No secrets in source control.** Credentials and secrets must remain outside Git.

---

# Current Milestone

The Music Library core workflow is complete. Development is currently focused on completing and polishing Mass Planning and Music Pack generation.

```text
Music Library                 COMPLETE
      ↓
Create / Edit / Duplicate Mass    WORKING
      ↓
Plan Music                        WORKING
      ↓
Generate Music Pack               IN PROGRESS
      ↓
PowerPoint                        FUTURE
```

The next priority is to verify and polish the generated choir music pack before beginning PowerPoint functionality.

---

# Backup Requirements

The local application data is split between the SQLite database and physical files. A useful backup must therefore include both:

```text
Data/choir.db
Storage/MusicSheets/
```

The database contains the music metadata and file paths, while `Storage/MusicSheets/` contains the actual PDF music sheets. Backing up only one of these locations is not sufficient to restore the complete Music Library.

Generated music packs under `Storage/Generated/` can normally be regenerated from the database and source PDFs.
