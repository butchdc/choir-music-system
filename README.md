# Choir Music System

A cross-platform web application for managing choir music, planning music and presentation content for Mass, generating combined choir music-sheet PDFs, and generating PowerPoint presentations.

The system provides a central place for choir preparation across multiple churches and venues.

The application is designed to be developed and operated using the same codebase across **macOS, Windows, and Linux/Docker environments**.

---

# Current Application Status

The core Choir Music System is now operational.

```text
Dashboard                         COMPLETE
Music Library                     COMPLETE
Mass Management                   COMPLETE
Mass Planning                     COMPLETE
Presentation Library              COMPLETE
Presentation Planning             COMPLETE
Mass Templates                    COMPLETE
Music Pack Generation             COMPLETE
PowerPoint Generation             COMPLETE
Custom Song PowerPoints           COMPLETE
Final Mass PowerPoint Workflow    COMPLETE
Backup & Restore                  COMPLETE
Multi-Church / Venue Support      IMPLEMENTED
Docker Deployment                 IMPLEMENTED

Security & Public Deployment      IMPLEMENTED
Google SSO                        IMPLEMENTED
Invite-Only Access                IMPLEMENTED
Admin / Member Roles              IMPLEMENTED
Break-Glass Admin                 IMPLEMENTED
Public Upcoming Masses            IMPLEMENTED
Reusable File Picker              IMPLEMENTED
```

The application is now deployed publicly with authentication, authorization, administrative access controls, and production security configuration in place.

---

# Project Goals

The Choir Music System provides a central library for choir resources and simplifies preparation for Mass.

The main workflow is:

```text
Music Library
      │
      ├──────────────────────┐
      │                      │
      ▼                      ▼
Presentation Library     Mass Templates
      │                      │
      └──────────┬───────────┘
                 │
                 ▼
             Create Mass
                 │
                 ▼
             Plan Mass
                 │
        ┌────────┴────────┐
        │                 │
        ▼                 ▼
   Music Plan      Presentation Plan
        │                 │
        ▼                 ▼
   Music Pack      Generated PowerPoint
      PDF                PPTX
                           │
                           ▼
                 Optional Final PowerPoint
```

The system is designed so songs and presentation content are maintained once and reused across many Masses.

---

# Core Features

## Dashboard

The dashboard provides an overview of the current choir library and upcoming Mass preparation.

Current dashboard functionality includes:

* Music Library statistics
* Mass statistics
* Mass Template statistics
* Presentation Library statistics
* Music readiness information
* Upcoming Masses
* Next Mass summary
* Recent music
* Quick access to common actions
* Mass preparation status
* Venue visibility for multi-church planning

The dashboard is intended to provide a quick operational view of the choir's preparation work.

---

# Music Library

The Music Library is the central repository for choir songs and music sheets.

Current functionality includes:

* Upload individual PDF music sheets
* Bulk upload existing PDF music sheets
* Automatic title detection from PDFs
* Maintain song metadata
* Edit song information
* Replace existing PDFs
* Archive music
* Search the library
* Filter music by Mass part
* Preview PDF music sheets
* Maintain presentation lyrics
* Generate an individual song PowerPoint
* Upload, replace, download, and remove a custom song PowerPoint
* Use a custom song PowerPoint during Mass generation when one is available
* Reuse songs across multiple Masses
* Identify songs with incomplete metadata

PDF music sheets remain the source documents used for generated choir music packs.

---

# Song Information

A Song may contain:

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
OneLicenseNumber
Publisher
CopyrightText
PresentationLyrics
CustomPresentationFileName
CustomPresentationPath
CustomPresentationUpdatedDate
IsActive
CreatedDate
UpdatedDate
```

Song information is stored in the database while physical PDFs and optional custom song PowerPoints are stored separately in file storage.

---

# Custom Song PowerPoints

A song may optionally have a manually prepared `.pptx` file. This allows choir members to use presentation content that cannot be represented adequately by the standard lyrics-based generator.

Custom song PowerPoints are stored under:

```text
Storage/SongPresentations/
```

When a Mass PowerPoint is generated, the custom song PowerPoint is used when one exists; otherwise the application generates the song slides from `PresentationLyrics`. The custom slides are merged into the Mass presentation while reconnecting them to the destination template and Mass background behaviour.

The Music Library supports uploading, replacing, downloading, and removing the custom PowerPoint while keeping fresh lyrics-based song generation available.

---

# Presentation Lyrics

Songs can contain lyrics specifically prepared for PowerPoint generation.

Presentation lyrics use explicit slide markers.

Example:

```text
[SLIDE:TITLED]
Amazing grace,
how sweet the sound...

[SLIDE]
I once was lost,
but now am found...
```

The supported markers are:

```text
[SLIDE:TITLED]
```

Creates a slide using:

```text
Song - Title + Lyrics
```

and:

```text
[SLIDE]
```

Creates a continuation slide using:

```text
Song - Lyrics
```

This gives the choir direct control over where lyric slides are divided.

Songs without presentation lyrics can still generate a titled song slide.

---

# Mass Management

A Mass represents a planned church service.

Current functionality includes:

* Create a Mass
* Edit a Mass
* Delete a Mass
* Duplicate an existing Mass
* Plan music
* Plan presentation content
* Select a PowerPoint background
* Generate a choir music pack
* Generate a PowerPoint presentation
* Save a Mass as a reusable Mass Template

---

# Mass Information

A Mass contains information including:

```text
Id
Name
MassDate
Venue
MassIntroduction
Notes
PresentationBackgroundPath
CreatedDate
UpdatedDate
```

## Mass Name

The Mass Name is the primary title of the Mass.

It is also used as the PowerPoint title.

Example:

```text
Sunday Mass
```

or:

```text
Feast of the Holy Cross
```

---

# Multi-Church / Venue Support

The Choir Music System is designed to support Masses across multiple churches and venues.

Each Mass can therefore have a Venue.

Example:

```text
Mass Name
Sunday Mass

Venue
Holy Cross Church
```

Venue information is intentionally visible in important planning areas such as the dashboard and Mass listings so that similarly named Masses at different churches can be easily distinguished.

Venue is informational and is not used to automatically change presentation behaviour.

For example, the application must not contain presentation logic such as:

```text
If Venue = Holy Cross
    automatically add Holy Cross Safety
```

Church-specific content should instead be managed through the Presentation Library and Mass Presentation Order.

---

# Mass Introduction

Mass Introduction is public presentation content associated with the Mass.

It is separate from internal planning Notes.

Example:

```text
Welcome to our celebration of the
Feast of the Holy Cross.
```

During PowerPoint generation:

```text
PowerPoint Title       ← Mass.Name

PowerPoint Date        ← Mass.MassDate

PowerPoint Subtitle    ← Mass.MassIntroduction
```

Mass Notes are not displayed on the title slide.

---

# Mass Notes

Mass Notes are intended for internal planning information.

Examples may include:

```text
Choir call time 8:30 AM

Use alternate Gloria arrangement

Check microphone before Mass
```

Notes should not automatically appear in generated presentations.

---

# Mass Parts

The system supports common Mass parts including:

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

The application architecture allows additional Mass parts to be supported without redesigning the core system.

---

# Mass Music Planning

The Music Plan allows songs from the Music Library to be selected for each Mass part.

Current functionality includes:

* Multiple songs per Mass part
* Search while planning
* Display useful song metadata
* Preserve selected songs
* Preserve manual presentation ordering
* Show selected songs while Mass-part sections are collapsed
* Insert newly selected songs near the appropriate Mass part
* Reuse the same song for multiple Mass parts
* Open source music-sheet PDFs where available

Music planning and presentation ordering are related but intentionally separate.

Changing a song selection should not unnecessarily destroy the manually arranged Presentation Order.

---

# Mass Presentation Planning

Each Mass has a unified Presentation Order.

The Presentation Order controls the actual sequence used when generating the PowerPoint.

It can contain:

```text
Mass Title
Songs
Presentation Library Items
```

Example:

```text
1. Holy Cross Safety
2. Gathering Song
3. Welcome
4. Mass Title
5. Entrance Song
6. Gloria
7. Psalm Response
8. Alleluia
9. Offertory
10. Holy
11. Memorial Acclamation
12. Amen
13. Communion
14. Recessional
```

Items can be reordered using drag and drop.

The PowerPoint generator follows this order directly.

---

# Mass Title

Mass Title is a system-generated Presentation Order item.

It is not a Presentation Library item.

The Mass Title can be moved within the Presentation Order just like other presentation content.

This allows content such as:

```text
Safety Notice
Prelude Song
Welcome
```

to appear before the Mass title if required.

The Mass Title uses the PowerPoint template layout:

```text
Title
```

The mapping is:

```text
Template Title       ← Mass.Name
Template Date        ← Mass.MassDate
Template Subtitle    ← Mass.MassIntroduction
```

---

# Presentation Library

The Presentation Library stores reusable non-song presentation content.

Examples include:

* Psalm responses
* Prayers
* Creed text
* Congregational responses
* Notices
* Safety information
* Custom presentation content

Presentation Library items can be reused across many Masses.

---

# Presentation Item Information

A Presentation Library item may contain:

```text
Id
Title
Type
Language
PresentationText
PowerPointLayout
IsActive
CreatedDate
UpdatedDate
```

Supported language classifications include:

```text
English
Tagalog
Bilingual
Other
```

Example presentation types include:

```text
Psalm Response
Creed
Prayer
Response
Notice
Custom
```

---

# Presentation Slide Markers

Presentation Library content can also use explicit slide markers.

The normal layouts are:

```text
Presentation - Title + Text

Presentation - Text
```

This allows longer presentation content to be deliberately divided across multiple slides.

---

# Custom PowerPoint Layouts

Presentation Library items can optionally select a specific layout from the PowerPoint template.

If no custom layout is selected, the application uses the normal presentation layouts.

If a custom layout is selected, the PowerPoint generator uses that template layout for the presentation item.

Available layouts are discovered from the configured PowerPoint template rather than being permanently hard-coded into the Presentation Library interface.

---

# Holy Cross Safety

The PowerPoint template currently includes a custom layout:

```text
HC Safety
```

Holy Cross safety information is handled as a normal reusable Presentation Library item.

For example:

```text
Title
Holy Cross Safety

Type
Notice

PowerPoint Layout
HC Safety
```

It can then be manually added to the Mass Presentation Order and positioned wherever required.

It is deliberately not automatically inserted based on Venue.

This keeps venue information and presentation content independent.

---

# PowerPoint Generation

The application generates `.pptx` presentations using the configured choir PowerPoint template.

PowerPoint generation currently supports:

* Mass title slides
* Mass date
* Mass introduction
* Songs
* Song lyrics
* Explicit lyric slide markers
* Presentation Library items
* Explicit presentation slide markers
* Divider slides between consecutive items in the unified Presentation Plan
* Custom song PowerPoint merging
* Custom PowerPoint layouts
* Background images
* Background transparency
* User-controlled Presentation Order
* Template fonts and positioning
* Reusable presentation content

PowerPoint generation uses the Open XML SDK.

---

# PowerPoint Template

The current PowerPoint template is stored at:

```text
Storage/PowerPointTemplates/Template.pptx
```

The template is intentionally maintained with the application.

Known template layouts include:

```text
Title
Song - Title + Lyrics
Song - Lyrics
Presentation - Title + Text
Presentation - Text
Divider
HC Safety
```

The template is currently designed for a widescreen 16:9 presentation.

---

# PowerPoint Background Behaviour

A Mass can have a selected presentation background.

The current presentation rules are:

```text
Title
Background transparency: 0%

Divider
Background transparency: 0%

Song slides
Background transparency: 85%

Presentation Library slides
Background transparency: 85%

Custom Presentation layouts
Background transparency: 85%
```

This keeps text readable while still showing the selected Mass background.

Custom layouts such as `HC Safety` use the same Mass background behaviour as other Presentation Library content.

---

# Generated PowerPoint Files

Generated presentations are treated as temporary output.

The intended workflow is:

```text
Generate temporary PPTX
        ↓
Read generated file
        ↓
Return browser download
        ↓
Delete temporary file
```

Generated PowerPoint files should not need to remain permanently on the server. Generating a fresh presentation never overwrites or removes an uploaded Final PowerPoint for the Mass.

## Final Mass PowerPoint

A Mass can optionally store a manually edited Final PowerPoint. The intended workflow is:

```text
Presentation Plan
       ↓
Generate Fresh PowerPoint
       ↓
Download and edit in PowerPoint
       ↓
Upload as Final PowerPoint
       ↓
Download the approved/final Mass deck
```

Final Mass PowerPoints are stored under:

```text
Storage/MassPresentations/
```

The Final PowerPoint is independent of generated output. Users can continue to generate a fresh PowerPoint from the current Mass plan at any time. The stored Final PowerPoint changes only when it is explicitly replaced or removed.

The user-facing Mass PowerPoint filename follows the general format:

```text
{Venue}-{Mass Name}-{yyyyMMdd}.pptx
```

Example:

```text
Holy Cross Church-Sunday Mass-20260906.pptx
```

Temporary internal filenames may contain unique identifiers to prevent collisions, but those identifiers should not be exposed as the browser download filename.

---

# Music Pack PDF Generation

The application can generate a combined PDF containing the music required for a Mass.

The workflow is:

```text
Mass
  ↓
Music Plan
  ↓
Selected Songs
  ↓
Find Source PDFs
  ↓
Remove Duplicate PDFs
  ↓
Merge PDFs
  ↓
Music Pack
```

PdfSharpCore is used for PDF merging.

---

# Duplicate Music-Sheet Handling

One PDF may contain several parts of a Mass.

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

The generated choir music pack should include:

```text
Mass of Creation.pdf
```

only once.

The generated PDF preserves the appropriate music order while avoiding duplicate source documents.

---

# Mass Templates

Mass Templates allow common Mass structures to be reused.

A template can contain an ordered combination of:

```text
Mass Title
Songs
Presentation Library Items
```

Templates have their own Presentation Order.

Items can be reordered using drag and drop.

A Mass can also be saved as a new Mass Template.

---

# Mass Template Workflow

Example:

```text
Mass Template
      │
      ├── Mass Title
      ├── Entrance Song
      ├── Gloria
      ├── Psalm Response
      ├── Creed
      ├── Holy
      ├── Communion
      └── Recessional
              │
              ▼
         Create Mass
              │
              ▼
      Copy Template Plan
              │
              ▼
      Edit for this Mass
```

The template stores references to reusable Song and Presentation Library records rather than duplicating their content.

---

# MassPlanItem

`MassPlanItem` represents an item in the unified Mass Presentation Order.

Conceptually it contains:

```text
Id
MassId
ItemType
SongId
PresentationItemId
MassPart
DisplayOrder
```

Supported item types include:

```text
MassTitle
Song
Presentation
```

Song and Presentation references are nullable because a system item such as `MassTitle` does not require either.

---

# Presentation Ordering

Presentation ordering uses numeric `DisplayOrder` values.

The application commonly uses increments such as:

```text
10
20
30
40
50
```

This keeps ordering simple while allowing items to be inserted and reordered.

The saved Presentation Order is the authoritative order used during PowerPoint generation.

---

# Technology Stack

## Backend

* ASP.NET Core 10
* C#
* Razor Pages

## Database

* Entity Framework Core
* SQLite

The architecture should remain portable to another Entity Framework Core provider if required in the future.

Possible future database providers include:

* PostgreSQL
* Microsoft SQL Server

## Frontend

* Razor Pages
* HTML
* CSS
* Bootstrap
* Bootstrap Icons
* JavaScript
* Chart.js

A separate frontend framework such as React or Angular is not currently required.

## PDF

* PdfPig
* PdfSharpCore

PdfPig is used for PDF metadata/title extraction.

PdfSharpCore is used for generated choir music packs.

## PowerPoint

* DocumentFormat.OpenXml

The Open XML SDK is used for cross-platform PowerPoint generation.

## Development

* Visual Studio Code
* Git
* GitHub
* .NET CLI
* Entity Framework CLI
* Docker

---

# Supported Platforms

Development should remain cross-platform.

The application is intended to work across:

```text
macOS
Windows
Linux / Docker
```

The project should avoid unnecessary operating-system-specific code.

Use cross-platform .NET functionality such as:

```csharp
Path.Combine(...)
```

instead of manually constructing operating-system-specific paths.

Do not hard-code paths such as:

```text
C:\ChoirMusic\
```

or:

```text
/Users/username/ChoirMusic/
```

---

# Application Architecture

The application uses a relatively simple architecture.

```text
Browser
   │
   ▼
ASP.NET Core Razor Pages
   │
   ▼
Application Services
   │
   ├───────────────┐
   │               │
   ▼               ▼
Entity Framework   File Storage
   │               │
   ▼               ▼
ChoirDbContext    Storage/
   │
   ▼
SQLite
```

The architecture deliberately keeps database access and physical file storage separate.

---

# Database Portability

SQLite is currently used because it is lightweight, cross-platform, and simple to deploy.

Normal application database access should go through:

```text
Application
     ↓
Entity Framework Core
     ↓
ChoirDbContext
     ↓
Database Provider
```

This allows another database provider to be considered later.

Possible providers include:

```text
Entity Framework Core
        │
        ├── SQLite
        │
        ├── PostgreSQL
        │
        └── SQL Server
```

Avoid database-specific SQL wherever practical.

Entity Framework Core migrations are used to manage schema changes.

---

# Legacy Database Table Names

The application originally used the model names:

```text
MusicSheet
MassMusicSheet
```

The C# domain models were later renamed to:

```text
Song
MassSong
```

For compatibility with the existing database, the physical SQLite table names remain:

```text
MusicSheets
MassMusicSheets
```

and the existing foreign-key column remains:

```text
MusicSheetId
```

Entity Framework Core mappings allow the application to use the newer C# model names without requiring physical database table renames.

This is intentional.

---

# File Storage

Music PDFs and other application files are stored separately from the database.

The current storage structure includes:

```text
Data/
└── choir.db

Storage/
├── MusicSheets/
├── SongPresentations/
├── MassPresentations/
├── PowerPointTemplates/
│   └── Template.pptx
├── Backgrounds/
├── Generated/
├── Temp/
└── Backups/
```

Some folders may only contain runtime-generated content depending on the environment.

---

# Music Sheet Storage

Existing music PDFs are currently stored under:

```text
Storage/MusicSheets/
```

The physical music-sheet location should not be unnecessarily changed because existing database records reference these files.

The database stores metadata and file paths.

The PDF itself is stored on the filesystem.

---

# Generated File Storage

Generated output may temporarily use:

```text
Storage/Generated/
```

or another configured temporary location.

Generated files are reproducible and should generally not be treated as permanent application data.

Where possible, generated downloads should be removed after being returned to the browser.

---

# PowerPoint Template Storage

PowerPoint templates are stored under:

```text
Storage/PowerPointTemplates/
```

The current application template is:

```text
Storage/PowerPointTemplates/Template.pptx
```

Unlike normal generated or uploaded runtime data, the main PowerPoint template is intentionally tracked with the application source because presentation generation depends on its layouts.

---

# Background Storage

Uploaded PowerPoint background images are stored under:

```text
Storage/Backgrounds/
```

A selected background path can be associated with a Mass and applied during PowerPoint generation.

---

# Reusable File Picker

File-upload screens use a shared custom file-picker component rather than relying on inconsistent native browser file-input styling. The shared component is used for music PDFs, custom song PowerPoints, Mass backgrounds, Final Mass PowerPoints, PowerPoint template uploads, and backup restore uploads.

---

# Backup & Restore

A complete Choir Music System backup must include both the database and physical application files.

The SQLite database alone is not sufficient because uploaded PDFs, backgrounds, templates, and other files are stored outside the database.

Likewise, backing up only the files is not sufficient because Mass plans and metadata are stored in the database.

---

# Backup Contents

The backup process includes the important application data from:

```text
Data/choir.db

Storage/

Songs/
```

Generated and backup working folders can be excluded where appropriate because generated output can be recreated.

The database and file storage should be considered one logical backup set.

---

# SQLite-Safe Backups

SQLite may use:

```text
choir.db
choir.db-wal
choir.db-shm
```

while the application is running.

The backup process must therefore create a consistent SQLite backup rather than simply copying an active database file without considering SQLite's current state.

---

# Restore

Restore must restore the database and associated physical files together.

Conceptually:

```text
Backup ZIP
    │
    ├── Database
    │
    ├── Music PDFs
    │
    ├── Backgrounds
    │
    ├── Templates
    │
    └── Other application files
             │
             ▼
      Restore Application
```

Backup and Restore is a sensitive administrative capability and is restricted to users with the Admin role.

---

# Project Structure

The application structure is broadly:

```text
choir-music-system/
│
├── Data/
│   ├── ChoirDbContext.cs
│   └── choir.db
│
├── Models/
│   ├── Song.cs
│   ├── Mass.cs
│   ├── MassSong.cs
│   ├── MassPlanItem.cs
│   ├── PresentationItem.cs
│   ├── MassTemplate.cs
│   ├── MassTemplateItem.cs
│   └── AppUser.cs
│
├── Services/
│   ├── PowerPointService.cs
│   └── PDF / storage services
│
├── Pages/
│   ├── Masses/
│   ├── MassTemplates/
│   ├── MusicLibrary/
│   ├── PresentationLibrary/
│   ├── Account/
│   ├── Admin/
│   └── Index.cshtml
│
├── Storage/
│   ├── MusicSheets/
│   ├── SongPresentations/
│   ├── MassPresentations/
│   ├── PowerPointTemplates/
│   ├── Backgrounds/
│   ├── Generated/
│   ├── Temp/
│   └── Backups/
│
├── wwwroot/
│   └── css/
│       └── site.css
│
├── Migrations/
├── appsettings.json
├── Program.cs
├── Dockerfile
├── docker-compose.yml
├── deploy.sh
└── README.md
```

Exact runtime folders may vary as the application continues to evolve.

---

# Installation

## macOS

### Install Git

```bash
brew install git
```

Verify:

```bash
git --version
```

### Install Visual Studio Code

Install Visual Studio Code for macOS.

Verify:

```bash
code --version
```

If `code` is unavailable from Terminal, use the Visual Studio Code Command Palette:

```text
Shell Command: Install 'code' command in PATH
```

### Install .NET 10 SDK

Install the .NET 10 SDK.

Verify:

```bash
dotnet --version
dotnet --list-sdks
```

### Install SQLite

```bash
brew install sqlite
```

Verify:

```bash
sqlite3 --version
```

### Install Entity Framework CLI

```bash
dotnet tool install --global dotnet-ef
```

If already installed:

```bash
dotnet tool update --global dotnet-ef
```

Verify:

```bash
dotnet ef --version
```

---

# Windows Development

## Install Git

Install Git for Windows.

Verify:

```powershell
git --version
```

## Install Visual Studio Code

Install Visual Studio Code for Windows.

Verify:

```powershell
code --version
```

## Install .NET 10 SDK

Install the .NET 10 SDK.

Verify:

```powershell
dotnet --version
dotnet --list-sdks
```

## Install SQLite

SQLite can be installed using Windows Package Manager:

```powershell
winget install SQLite.SQLite
```

Verify:

```powershell
sqlite3 --version
```

## Install Entity Framework CLI

```powershell
dotnet tool install --global dotnet-ef
```

If already installed:

```powershell
dotnet tool update --global dotnet-ef
```

Verify:

```powershell
dotnet ef --version
```

---

# Restore Project Dependencies

After cloning the repository:

```bash
dotnet restore
```

Apply outstanding database migrations:

```bash
dotnet ef database update
```

---

# HTTPS Development Certificate

For local HTTPS development:

```bash
dotnet dev-certs https --trust
```

macOS may request permission to add the certificate to the system keychain.

Windows may display a certificate confirmation prompt.

---

# Running Locally

From the project directory:

```bash
dotnet watch
```

The application will display the development URL.

Stop the application using:

```text
Ctrl + C
```

---

# Database Migrations

When the data model changes, create an Entity Framework Core migration.

Example:

```bash
dotnet ef migrations add MigrationName
```

Apply it:

```bash
dotnet ef database update
```

After significant changes, run the project's clean-build process:

```bash
./clean-build.sh
```

---

# Git Workflow

The source repository is:

```text
https://github.com/butchdc/choir-music-system.git
```

The main development branch is:

```text
main
```

Before committing changes:

```bash
git status
```

Commit:

```bash
git add .
git commit -m "Describe changes"
git push
```

When continuing development on another computer:

```bash
git pull
dotnet restore
dotnet ef database update
```

Then run the application normally.

---

# Source Control and Runtime Data

Source code and database migrations should be stored in Git.

Runtime application data should generally not be committed.

This includes:

```text
SQLite database
Uploaded music PDFs
Uploaded backgrounds
Generated PDFs
Generated PowerPoints
Temporary files
Backups
```

The main PowerPoint template is an intentional exception.

It is required for presentation generation and is tracked with the source code.

---

# Secrets

Passwords, API keys, production credentials, and other secrets must never be committed to Git.

Development connection strings that contain no credentials may be stored in normal configuration where appropriate.

Production secrets should be supplied through secure environment configuration or another appropriate secrets mechanism.

Production authentication and break-glass secrets are supplied outside source control through environment configuration.

---

# Docker Deployment

The application supports Docker-based deployment.

The production application directory is currently expected to be similar to:

```text
/docker/choir-music-system
```

The Docker deployment exposes the application internally through a configured port.

Current deployment mapping:

```text
5105:8080
```

Persistent application data should be mounted from the host so that rebuilding the container does not delete the database or uploaded files.

Conceptually:

```text
Docker Host
    │
    ├── Application Container
    │       └── ASP.NET Core
    │
    ├── Data/
    │       └── choir.db
    │
    └── Storage/
            ├── MusicSheets/
            ├── Backgrounds/
            └── Other persistent files
```

Database migrations are applied as part of the application's deployment/startup process.

---

# Reverse Proxy

A reverse proxy can be placed in front of the application for public deployment.

The intended architecture is:

```text
Internet
   │
   ▼
HTTPS
   │
   ▼
Reverse Proxy
   │
   ▼
Docker Host
   │
   ▼
Choir Music System
```

Nginx Proxy Manager can be used for this purpose.

Public deployment should not proceed until the application's security phase is completed.

---

# Development Principles

The project follows several core principles.

## Cross-Platform

Development and execution should remain portable across macOS, Windows, and Linux/Docker environments.

## Simple Architecture

Avoid unnecessary architectural complexity.

The system should remain understandable and maintainable for a small choir application.

## Database Independence

Application logic should use Entity Framework Core rather than depending directly on SQLite-specific functionality wherever practical.

## Storage Independence

File handling should remain sufficiently separated from application logic to allow local storage to be replaced with another storage provider in the future if required.

## Reusable Content

Songs and Presentation Library items should be maintained once and reused across multiple Masses and templates.

## Presentation Order Is Authoritative

The manually arranged Mass Presentation Order determines the generated PowerPoint sequence.

## Venue Is Informational

Venue identifies where the Mass is taking place.

Venue should not be used as hidden application logic for inserting church-specific presentation content.

## No Hard-Coded Paths

Use configuration and cross-platform path handling.

## No Secrets in Source Control

Credentials and secrets must remain outside Git.

## Generated Content Is Disposable

Generated PDF and PowerPoint files should generally be reproducible from source data and should not need permanent server storage.

---

# Completed Development Phases

## Foundation

Completed:

* ASP.NET Core 10 Razor Pages project
* SQLite configuration
* Entity Framework Core
* Database migrations
* Cross-platform development
* Git/GitHub workflow
* Local file storage
* Docker deployment foundation

---

## Music Library

Completed:

* Individual PDF upload
* Bulk PDF upload
* PDF title detection
* Song metadata
* Editing
* PDF replacement
* Search
* Mass-part filtering
* PDF preview
* Song archiving/deletion workflow
* Presentation lyrics
* Song PowerPoint generation
* Readiness indicators

---

## Mass Planning

Completed:

* Create Mass
* Edit Mass
* Delete Mass
* Duplicate Mass
* Mass Name
* Mass Date
* Venue
* Mass Introduction
* Internal Notes
* PowerPoint background selection
* Multiple songs per Mass part
* Song searching
* Collapsible Mass-part planning
* Selected-song preview
* Ordered song insertion
* Unified Presentation Order

---

## Presentation Library

Completed:

* Reusable Presentation Library
* Presentation types
* Language classification
* Presentation text
* Slide markers
* Custom PowerPoint layouts
* PowerPoint layout discovery
* Holy Cross Safety custom-layout support

---

## Mass Templates

Completed:

* Create Mass Templates
* Edit Mass Templates
* Template Presentation Order
* Songs in templates
* Presentation Library items in templates
* Mass Title support
* Drag-and-drop ordering
* Create Mass from template
* Save Mass as template

---

## Music Pack Generation

Completed core functionality:

* Read selected songs for a Mass
* Preserve music order
* Deduplicate repeated source PDFs
* Merge source PDFs
* Generate combined choir music pack

Generated music packs are reproducible from the Mass plan and Music Library.

---

## PowerPoint Generation

Completed core functionality:

* Open XML PowerPoint generation
* Existing choir PowerPoint template
* Mass Title
* Mass Introduction
* Date
* Songs
* Song lyrics
* Explicit slide markers
* Presentation Library items
* Custom layouts
* Mass-part dividers
* Selected backgrounds
* Background transparency
* Unified Presentation Order
* Temporary generated files
* Friendly download filenames

---

## Backup & Restore

Completed core functionality:

* Database backup
* Application file backup
* ZIP-based backup package
* Restore database and files
* Include important top-level application data
* Exclude reproducible generated output where appropriate
* SQLite-safe backup handling

Backup & Restore is restricted to authorized administrators.

---

# Current Milestone

The core functional application is now substantially complete.

```text
                    CHOIR MUSIC SYSTEM

                           │
          ┌────────────────┴────────────────┐
          │                                 │
          ▼                                 ▼
    Music Library                  Presentation Library
          │                                 │
          └────────────────┬────────────────┘
                           │
                           ▼
                     Mass Templates
                           │
                           ▼
                       Create Mass
                           │
                           ▼
                       Plan Mass
                           │
               ┌───────────┴───────────┐
               │                       │
               ▼                       ▼
           Music Plan          Presentation Order
               │                       │
               ▼                       ▼
          Music Pack          Generated PowerPoint
              PDF                     PPTX
                                        │
                                        ▼
                              Optional Final PowerPoint
```

The core security and public deployment phase is now implemented. Future work can focus on security maintenance, usability, and additional choir functionality.

---

# Security & Public Deployment

The security and public deployment phase has been implemented and tested. Normal application functionality now requires an authenticated user unless a page or endpoint is explicitly allowed to be anonymous.

---

# Authentication

Google SSO is the primary authentication method. Successful Google authentication alone does not grant application access: the Google account email must also exist as an active user in the application's invite-only user list.

A temporary external authentication cookie is used while the Google identity is validated. After validation, the application creates its own authentication cookie containing the user's application identity and role.

Static assets required by the login page are explicitly available anonymously so the authentication screen can load its CSS, icons, and presentation resources while the application remains protected.

---

# Invite-Only Access

Application access is controlled through the `AppUsers` database table.

An invited user contains information including:

```text
Email
NormalizedEmail
DisplayName
Role
IsActive
InvitedAt
LastLoginAt
InvitedBy
```

Google accounts that authenticate successfully but do not have an active matching application user are denied access.

Administrators can invite users through User Management. An invitation currently means adding the Google email address to the application allow-list; the application does not send an invitation email.

---

# Public Login and Upcoming Masses

The sign-in page is intentionally available without authentication and provides the Google sign-in entry point. It also displays a limited Upcoming Masses view containing public event information only: Mass name, date, time, and venue.

Music plans, presentation plans, songs, notes, OneLicense information, uploaded files, and administration functions remain behind authentication.

---

# Authorization

Two roles are implemented:

```text
Admin
Member
```

Administrators have normal choir functionality plus sensitive administration functions. Current Admin-only functionality includes User Management, Backup & Restore, PowerPoint template management, break-glass hash generation, and destructive Mass deletion.

Admin authorization is enforced server-side through the `AdminOnly` policy. Administrative navigation items are hidden from Members, but navigation visibility is not relied upon as the security control.

Members retain normal choir planning functionality including the Music Library, Masses, Presentation Library, Mass Templates, and generation workflows.

The application also prevents an administrator from disabling or demoting the last active Admin account.

---

# User Management

Admin User Management supports:

* View application users
* Invite a Google account
* Assign `Admin` or `Member`
* Activate or deactivate users
* Review last-login information
* Protect the last active Admin from being disabled or demoted

Deactivating an application user prevents future Google sign-ins from creating an application session.

---

# Break-Glass Administrator

The application has an independent break-glass administrator for emergency recovery. This account is deliberately separate from Google SSO and the `AppUsers` table so access can be recovered if normal Google/Admin access is unavailable.

Break-glass configuration uses:

```text
Security:BreakGlass:Enabled
Security:BreakGlass:Username
Security:BreakGlass:PasswordHash
```

The plaintext password is not stored in source control or the application database. Only a secure ASP.NET Core password hash is stored in protected configuration.

A successful break-glass login creates a temporary Admin identity. The emergency session is non-persistent, cannot be refreshed, and expires after 30 minutes.

The emergency login is rate limited. Failed and successful break-glass login events are logged without recording passwords or password hashes.

The break-glass account is not managed through normal User Management.

---

# Session and Cookie Security

The main application authentication cookie uses:

```text
HttpOnly        Enabled
SameSite        Lax
Secure          Always
Sliding         Enabled
Lifetime        8 hours
```

The temporary Google external-authentication cookie is also `HttpOnly`, `SameSite=Lax`, and restricted to secure HTTPS transport.

---

# HTTPS and Reverse Proxy

Production is deployed through Nginx Proxy Manager.

```text
Browser
   │
   ▼
HTTPS
   │
   ▼
Nginx Proxy Manager
   │
   ▼
Docker-hosted ASP.NET Core application
```

The application processes forwarded proxy headers so ASP.NET Core understands that the original public request used HTTPS. This allows authentication redirects such as the Google OAuth `/signin-google` callback to use the correct public HTTPS scheme.

The production Google OAuth client must contain the exact public HTTPS callback URL as an authorized redirect URI.

---

# Security Headers

Browser security headers are applied by the application, including:

```text
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: camera=(), microphone=(), geolocation=()
```

HSTS is enabled outside the Development environment.

A Content Security Policy has not yet been introduced and can be reviewed separately against the application's frontend resources.

---

# Rate Limiting and Security Logging

The emergency break-glass login is protected by ASP.NET Core rate limiting using a fixed window by source IP. Rejected requests return HTTP `429 Too Many Requests`.

Break-glass authentication also records security-relevant events through ASP.NET Core logging, including failed login attempts, successful emergency logins, configuration problems, and source IP information. Passwords and password hashes are never logged.

Broader rate limiting and application-wide audit history remain possible future enhancements.

---

# Backup Security

Backup & Restore is restricted to the `Admin` role.

The restore implementation includes safeguards such as ZIP file requirement, safe archive extraction/path validation, SQLite integrity validation, a safety backup before restore, and controlled replacement of application database and file storage.

Because restore can replace live application data, authorization is enforced on the server rather than relying only on navigation visibility.

---

# Secrets and Production Configuration

Passwords, OAuth credentials, password hashes, API keys, and other production secrets must never be committed to Git.

Local development uses .NET User Secrets for sensitive authentication configuration.

Production Docker deployment supplies secrets through environment configuration. Current production security configuration includes:

```text
Authentication__Google__ClientId
Authentication__Google__ClientSecret
Security__BreakGlass__Enabled
Security__BreakGlass__Username
Security__BreakGlass__PasswordHash
```

Docker Compose references environment values rather than embedding secret values in source control. A server-side `.env` file may provide these values and must remain excluded from Git with restrictive filesystem permissions.

---

# Production Error Handling

Outside Development, the application uses its production error handler and HSTS configuration. Detailed stack traces, database details, filesystem paths, credentials, and configuration values should remain in server-side diagnostics rather than being displayed publicly.

---

# State-Changing Requests and File Security

ASP.NET Core Razor Pages antiforgery protection applies to normal form-based state-changing operations. Sensitive actions should continue to be reviewed as the application evolves.

The application handles music PDFs, presentation resources, backgrounds, generated documents, and backup archives. Existing backup restore handling includes safe archive path validation. Application data such as the SQLite database, backup archives, temporary files, and configuration must not be exposed as unrestricted public static files.

Additional upload validation can be added if the application's trust model or user population changes.

---

# Dependency and Deployment Security

The application should continue to keep .NET/ASP.NET Core, Entity Framework Core, Google authentication, Open XML SDK, PdfPig, PdfSharpCore, frontend libraries, and Docker base images maintained.

Dependency vulnerability review remains an ongoing maintenance responsibility rather than a one-time deployment task.

---

# Production Security Architecture

```text
                         INTERNET
                            │
                            ▼
                          HTTPS
                            │
                            ▼
                   Nginx Proxy Manager
                            │
                            ▼
                 Google SSO + Invite Check
                            │
                    ┌───────┴───────┐
                    ▼               ▼
                  ADMIN           MEMBER
                    │               │
                    └───────┬───────┘
                            ▼
                   Choir Music System
                            │
                    ┌───────┴───────┐
                    ▼               ▼
                  SQLite        File Storage

Emergency recovery:
Break-Glass Login → Temporary Admin Session → Choir Music System
```

---

# Security Status

The core public-deployment security phase is complete.

```text
Google SSO                         COMPLETE
Invite-only access                 COMPLETE
Admin / Member roles               COMPLETE
Admin User Management              COMPLETE
Admin-only Backup & Restore        COMPLETE
Last-active-Admin protection       COMPLETE
Break-glass administrator          COMPLETE
Hashed emergency password          COMPLETE
30-minute emergency session        COMPLETE
Emergency rate limiting            COMPLETE
Emergency security logging         COMPLETE
Secure authentication cookies      COMPLETE
HTTPS reverse-proxy handling       COMPLETE
Security headers                   COMPLETE
Production secret configuration    COMPLETE
Public Docker deployment           COMPLETE
```

Ongoing security maintenance includes dependency updates, security testing after significant changes, log review, production configuration review, and revisiting controls as the application or user population grows.

---

# Next Development Phase

With the core application and public-deployment security controls implemented, development can return to functional improvements and operational refinement.

Potential priorities include:

```text
1. Stabilization and end-to-end regression testing
2. README and operational documentation maintenance
3. Security testing and dependency maintenance
4. Improved audit/activity history
5. Focused UI and usability improvements
6. Additional choir workflow enhancements
```

Security changes should continue to be implemented incrementally and tested so existing Mass planning, PDF, PowerPoint, template, authentication, and backup workflows remain operational.

---

# Future Enhancements

Potential future improvements may include:

* Additional user roles
* Per-church permissions
* Cloud file storage
* PostgreSQL or SQL Server
* Additional PowerPoint templates
* Additional presentation layouts
* More configurable Mass parts
* Improved audit logging
* Activity history
* Additional backup options
* MuseScore integration
* Additional reporting
* Mobile-focused improvements

These are optional future enhancements and are not required for the current core system.

---

# Project Direction

The Choir Music System has moved beyond the original PDF-only prototype.

It now provides an integrated workflow for:

```text
Music
+
Presentation Content
+
Mass Planning
+
Reusable Templates
+
PDF Generation
+
PowerPoint Generation
+
Custom / Final PowerPoint Workflow
+
Backup & Restore
+
Multi-Church Planning
```

The application is now publicly deployed with authentication and authorization controls protecting these capabilities.

The project should continue to favour:

```text
Simple
Secure
Cross-platform
Maintainable
Reusable
Choir-focused
```

over unnecessary complexity.