using System.IO;

namespace BasketballRulesCms.App.Services;

public static class AppPaths
{
    // inicijalizacija baza direktorijuma i relativnih putanja
    public static string AppBaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public static string DataDirectory => Path.Combine(AppBaseDirectory, "Data");
    public static string UsersXmlRelativePath => Path.Combine("Data", "users.xml");
    public static string RulesXmlRelativePath => Path.Combine("Data", "rules.xml");

    public static string ImagesDirectory => Path.Combine(DataDirectory, "images");
    public static string RtfDirectory => Path.Combine(DataDirectory, "rtf");

    // kreiranje potrebnih foldera pri pokretanju
    public static void EnsureDataFolders()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(ImagesDirectory);
        Directory.CreateDirectory(RtfDirectory);
    }

    // konverzije putanja (relativna <-> apsolutna)
    public static string ToAbsolutePath(string relativePath)
        => Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(AppBaseDirectory, relativePath);

    public static string ToRelativePath(string absolutePath)
    {
        var baseDir = AppBaseDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (absolutePath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return absolutePath.Substring(baseDir.Length);
        return absolutePath;
    }
}

