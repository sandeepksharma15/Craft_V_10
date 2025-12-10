using System.Security.Cryptography;

namespace Craft.Utilities.Helpers;

public static class FileHelper
{
    public static string GetUniqueFileName(string directory, string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var uniqueName = fileName;
        var counter = 1;

        while (File.Exists(Path.Combine(directory, uniqueName)))
        {
            uniqueName = $"{name}_{counter}{extension}";
            counter++;
        }

        return uniqueName;
    }

    public static long GetFileSize(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return new FileInfo(path).Length;
    }

    public static string GetFileHash(string path, HashAlgorithmName algorithm = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var algorithmName = algorithm == default ? HashAlgorithmName.SHA256 : algorithm;

        using var stream = File.OpenRead(path);
        using var hashAlgorithm = HashAlgorithm.Create(algorithmName.Name!)!;
        var hash = hashAlgorithm.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }

    public static bool IsFileLocked(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    public static void EnsureDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        Directory.CreateDirectory(path);
    }

    public static IEnumerable<string> GetFilesRecursive(string directory, string pattern = "*.*")
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        return Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);
    }

    public static string GetRelativePath(string fromPath, string toPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(fromPath);
        ArgumentException.ThrowIfNullOrEmpty(toPath);

        return Path.GetRelativePath(fromPath, toPath);
    }

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Where(c => !invalidChars.Contains(c)));
    }

    public static string GetReadableFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
