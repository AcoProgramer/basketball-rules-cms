using System.IO;

namespace BasketballRulesCms.App.Services;

public sealed class ImageService
{
    public string CopyIntoDataImages(string sourceFilePath)
    {
        AppPaths.EnsureDataFolders();
        Directory.CreateDirectory(AppPaths.ImagesDirectory);

        var ext = Path.GetExtension(sourceFilePath);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var destAbs = Path.Combine(AppPaths.ImagesDirectory, fileName);
        File.Copy(sourceFilePath, destAbs, overwrite: true);

        return AppPaths.ToRelativePath(destAbs);
    }
}

