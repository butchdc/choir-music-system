using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authorization;

namespace choir_music_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class BackupModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public BackupModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? BackupFile { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
    }

    // =========================================================
    // EXPORT
    // =========================================================

    public async Task<IActionResult> OnPostExportAsync()
    {
        var projectRoot = _environment.ContentRootPath;

        var databasePath = Path.Combine(
            projectRoot,
            "Data",
            "choir.db");

        if (!System.IO.File.Exists(databasePath))
        {
            StatusMessage = "Database file was not found.";
            return RedirectToPage();
        }

        var tempFolder = Path.Combine(
            Path.GetTempPath(),
            $"choir-backup-{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempFolder);

        string? zipPath = null;

        try
        {
            // -------------------------------------------------
            // Database snapshot
            // -------------------------------------------------

            var backupDataFolder = Path.Combine(
                tempFolder,
                "Data");

            Directory.CreateDirectory(
                backupDataFolder);

            var backupDatabasePath = Path.Combine(
                backupDataFolder,
                "choir.db");

            BackupSqliteDatabase(
                databasePath,
                backupDatabasePath);

            // -------------------------------------------------
            // Storage
            // -------------------------------------------------

            var storageSource = Path.Combine(
                projectRoot,
                "Storage");

            var storageDestination = Path.Combine(
                tempFolder,
                "Storage");

            if (Directory.Exists(storageSource))
            {
                CopyDirectory(
                    storageSource,
                    storageDestination,
                    excludeGenerated: true,
                    excludeBackups: true);
            }

            // -------------------------------------------------
            // Songs
            // -------------------------------------------------

            var songsSource = Path.Combine(
                projectRoot,
                "Songs");

            var songsDestination = Path.Combine(
                tempFolder,
                "Songs");

            if (Directory.Exists(songsSource))
            {
                CopyDirectory(
                    songsSource,
                    songsDestination,
                    excludeGenerated: false,
                    excludeBackups: false);
            }

            // -------------------------------------------------
            // Create ZIP
            // -------------------------------------------------

            var timestamp =
                DateTime.Now.ToString(
                    "yyyyMMdd-HHmmss");

            zipPath = Path.Combine(
                Path.GetTempPath(),
                $"choir-backup-{timestamp}-{Guid.NewGuid():N}.zip");

            ZipFile.CreateFromDirectory(
                tempFolder,
                zipPath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);

            var bytes =
                await System.IO.File
                    .ReadAllBytesAsync(zipPath);

            return File(
                bytes,
                "application/zip",
                $"choir-backup-{timestamp}.zip");
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Export failed: {ex.Message}";

            return RedirectToPage();
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(
                    tempFolder,
                    true);
            }

            if (!string.IsNullOrWhiteSpace(zipPath) &&
                System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }
        }
    }

    // =========================================================
    // IMPORT
    // =========================================================

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (BackupFile is null ||
            BackupFile.Length == 0)
        {
            StatusMessage =
                "Please select a backup file.";

            return RedirectToPage();
        }

        if (!string.Equals(
            Path.GetExtension(BackupFile.FileName),
            ".zip",
            StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage =
                "Only ZIP backup files are supported.";

            return RedirectToPage();
        }

        var projectRoot =
            _environment.ContentRootPath;

        var databasePath = Path.Combine(
            projectRoot,
            "Data",
            "choir.db");

        var tempZip = Path.Combine(
            Path.GetTempPath(),
            $"choir-upload-{Guid.NewGuid():N}.zip");

        var extractFolder = Path.Combine(
            Path.GetTempPath(),
            $"choir-restore-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            extractFolder);

        try
        {
            // -------------------------------------------------
            // Save uploaded ZIP
            // -------------------------------------------------

            await using (var stream =
                new FileStream(
                    tempZip,
                    FileMode.Create,
                    FileAccess.Write))
            {
                await BackupFile.CopyToAsync(
                    stream);
            }

            // -------------------------------------------------
            // Extract safely
            // -------------------------------------------------

            ExtractZipSafely(
                tempZip,
                extractFolder);

            // -------------------------------------------------
            // Validate imported database
            // -------------------------------------------------

            var importedDatabase = Path.Combine(
                extractFolder,
                "Data",
                "choir.db");

            if (!System.IO.File.Exists(
                importedDatabase))
            {
                StatusMessage =
                    "The backup does not contain Data/choir.db.";

                return RedirectToPage();
            }

            ValidateSqliteDatabase(
                importedDatabase);

            // -------------------------------------------------
            // Safety backup BEFORE restore
            // -------------------------------------------------

            CreateSafetyBackup(
                projectRoot,
                databasePath);

            // -------------------------------------------------
            // Restore database
            // -------------------------------------------------

            RestoreSqliteDatabase(
                importedDatabase,
                databasePath);

            // -------------------------------------------------
            // Restore Storage as a true replacement
            // while preserving local Generated + Backups
            // -------------------------------------------------

            var importedStorage = Path.Combine(
                extractFolder,
                "Storage");

            var targetStorage = Path.Combine(
                projectRoot,
                "Storage");

            if (Directory.Exists(importedStorage))
            {
                var generatedPath = Path.Combine(
                    targetStorage,
                    "Generated");

                var backupsPath = Path.Combine(
                    targetStorage,
                    "Backups");

                var tempGenerated = Path.Combine(
                    Path.GetTempPath(),
                    $"choir-generated-{Guid.NewGuid():N}");

                var tempBackups = Path.Combine(
                    Path.GetTempPath(),
                    $"choir-backups-{Guid.NewGuid():N}");

                // Temporarily preserve Generated.
                if (Directory.Exists(generatedPath))
                {
                    Directory.Move(
                        generatedPath,
                        tempGenerated);
                }

                // Temporarily preserve Backups.
                if (Directory.Exists(backupsPath))
                {
                    Directory.Move(
                        backupsPath,
                        tempBackups);
                }

                try
                {
                    if (Directory.Exists(targetStorage))
                    {
                        Directory.Delete(
                            targetStorage,
                            true);
                    }

                    Directory.CreateDirectory(
                        targetStorage);

                    CopyDirectory(
                        importedStorage,
                        targetStorage,
                        excludeGenerated: true,
                        excludeBackups: true);
                }
                finally
                {
                    if (Directory.Exists(tempGenerated))
                    {
                        Directory.Move(
                            tempGenerated,
                            generatedPath);
                    }

                    if (Directory.Exists(tempBackups))
                    {
                        Directory.Move(
                            tempBackups,
                            backupsPath);
                    }
                }
            }

            // -------------------------------------------------
            // Restore Songs as a true replacement
            // -------------------------------------------------

            var importedSongs = Path.Combine(
                extractFolder,
                "Songs");

            var targetSongs = Path.Combine(
                projectRoot,
                "Songs");

            if (Directory.Exists(targetSongs))
            {
                Directory.Delete(
                    targetSongs,
                    true);
            }

            if (Directory.Exists(importedSongs))
            {
                CopyDirectory(
                    importedSongs,
                    targetSongs,
                    excludeGenerated: false,
                    excludeBackups: false);
            }

            StatusMessage =
                "Backup restored successfully.";

            return RedirectToPage();
        }
        catch (InvalidDataException ex)
        {
            StatusMessage =
                $"Invalid backup: {ex.Message}";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Restore failed: {ex.Message}";

            return RedirectToPage();
        }
        finally
        {
            if (System.IO.File.Exists(
                tempZip))
            {
                System.IO.File.Delete(
                    tempZip);
            }

            if (Directory.Exists(
                extractFolder))
            {
                Directory.Delete(
                    extractFolder,
                    true);
            }
        }
    }

    // =========================================================
    // SQLITE BACKUP
    // =========================================================

    private static void BackupSqliteDatabase(
        string sourceDatabasePath,
        string destinationDatabasePath)
    {
        if (System.IO.File.Exists(
            destinationDatabasePath))
        {
            System.IO.File.Delete(
                destinationDatabasePath);
        }

        using var sourceConnection =
            new SqliteConnection(
                $"Data Source={sourceDatabasePath}");

        using var destinationConnection =
            new SqliteConnection(
                $"Data Source={destinationDatabasePath}");

        sourceConnection.Open();
        destinationConnection.Open();

        sourceConnection.BackupDatabase(
            destinationConnection);
    }

    // =========================================================
    // SQLITE RESTORE
    // =========================================================

    private static void RestoreSqliteDatabase(
        string backupDatabasePath,
        string liveDatabasePath)
    {
        using var sourceConnection =
            new SqliteConnection(
                $"Data Source={backupDatabasePath};Mode=ReadOnly");

        using var destinationConnection =
            new SqliteConnection(
                $"Data Source={liveDatabasePath}");

        sourceConnection.Open();
        destinationConnection.Open();

        sourceConnection.BackupDatabase(
            destinationConnection);
    }

    // =========================================================
    // SQLITE VALIDATION
    // =========================================================

    private static void ValidateSqliteDatabase(
        string databasePath)
    {
        try
        {
            using var connection =
                new SqliteConnection(
                    $"Data Source={databasePath};Mode=ReadOnly");

            connection.Open();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                "PRAGMA integrity_check;";

            var result =
                command.ExecuteScalar()?.ToString();

            if (!string.Equals(
                result,
                "ok",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Database integrity check failed: {result}");
            }
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"The backup database is not valid: {ex.Message}",
                ex);
        }
    }

    // =========================================================
    // SAFETY BACKUP
    // =========================================================

    private static void CreateSafetyBackup(
        string projectRoot,
        string databasePath)
    {
        var backupRoot = Path.Combine(
            projectRoot,
            "Storage",
            "Backups");

        Directory.CreateDirectory(
            backupRoot);

        var tempFolder = Path.Combine(
            Path.GetTempPath(),
            $"choir-before-restore-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            tempFolder);

        try
        {
            // -------------------------------------------------
            // Database
            // -------------------------------------------------

            if (System.IO.File.Exists(
                databasePath))
            {
                var dataFolder = Path.Combine(
                    tempFolder,
                    "Data");

                Directory.CreateDirectory(
                    dataFolder);

                BackupSqliteDatabase(
                    databasePath,
                    Path.Combine(
                        dataFolder,
                        "choir.db"));
            }

            // -------------------------------------------------
            // Storage
            // -------------------------------------------------

            var storageSource = Path.Combine(
                projectRoot,
                "Storage");

            if (Directory.Exists(
                storageSource))
            {
                var storageDestination =
                    Path.Combine(
                        tempFolder,
                        "Storage");

                CopyDirectory(
                    storageSource,
                    storageDestination,
                    excludeGenerated: true,
                    excludeBackups: true);
            }

            // -------------------------------------------------
            // Songs
            // -------------------------------------------------

            var songsSource = Path.Combine(
                projectRoot,
                "Songs");

            if (Directory.Exists(
                songsSource))
            {
                var songsDestination =
                    Path.Combine(
                        tempFolder,
                        "Songs");

                CopyDirectory(
                    songsSource,
                    songsDestination,
                    excludeGenerated: false,
                    excludeBackups: false);
            }

            // -------------------------------------------------
            // Create safety ZIP
            // -------------------------------------------------

            var timestamp =
                DateTime.Now.ToString(
                    "yyyyMMdd-HHmmss");

            var backupPath = Path.Combine(
                backupRoot,
                $"before-restore-{timestamp}.zip");

            ZipFile.CreateFromDirectory(
                tempFolder,
                backupPath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);
        }
        finally
        {
            if (Directory.Exists(
                tempFolder))
            {
                Directory.Delete(
                    tempFolder,
                    true);
            }
        }
    }

    // =========================================================
    // DIRECTORY COPY
    // =========================================================

    private static void CopyDirectory(
        string sourceDir,
        string destinationDir,
        bool excludeGenerated,
        bool excludeBackups)
    {
        Directory.CreateDirectory(
            destinationDir);

        foreach (var file in
                 Directory.GetFiles(sourceDir))
        {
            var targetFile = Path.Combine(
                destinationDir,
                Path.GetFileName(file));

            System.IO.File.Copy(
                file,
                targetFile,
                true);
        }

        foreach (var directory in
                 Directory.GetDirectories(sourceDir))
        {
            var name =
                Path.GetFileName(directory);

            if (excludeGenerated &&
                string.Equals(
                    name,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (excludeBackups &&
                string.Equals(
                    name,
                    "Backups",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            CopyDirectory(
                directory,
                Path.Combine(
                    destinationDir,
                    name),
                excludeGenerated,
                excludeBackups);
        }
    }

    // =========================================================
    // SAFE ZIP EXTRACTION
    // =========================================================

    private static void ExtractZipSafely(
        string zipPath,
        string destinationFolder)
    {
        var destinationRoot =
            Path.GetFullPath(
                destinationFolder) +
            Path.DirectorySeparatorChar;

        using var archive =
            ZipFile.OpenRead(zipPath);

        foreach (var entry in archive.Entries)
        {
            var destinationPath =
                Path.GetFullPath(
                    Path.Combine(
                        destinationFolder,
                        entry.FullName));

            if (!destinationPath.StartsWith(
                destinationRoot,
                StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The backup contains an invalid file path.");
            }

            if (string.IsNullOrEmpty(
                entry.Name))
            {
                Directory.CreateDirectory(
                    destinationPath);

                continue;
            }

            var directory =
                Path.GetDirectoryName(
                    destinationPath);

            if (!string.IsNullOrWhiteSpace(
                directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            entry.ExtractToFile(
                destinationPath,
                overwrite: true);
        }
    }
}