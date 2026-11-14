using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace BasketballRulesCms.App.Services;

// upravljanje RTF dokumentima: u?itavanje u FlowDocument, ?uvanje iz FlowDocument i brojanje re?i
public sealed class RtfService
{
    // u?itavanje RTF fajla u FlowDocument
    public FlowDocument LoadToFlowDocument(string relativePath)
    {
        var abs = AppPaths.ToAbsolutePath(relativePath);
        var doc = new FlowDocument();
        if (!File.Exists(abs))
            return doc;

        using var stream = File.OpenRead(abs);
        var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        range.Load(stream, DataFormats.Rtf);
        return doc;
    }

    // ?uvanje FlowDocument-a kao RTF fajl
    public void SaveFlowDocument(string relativePath, FlowDocument doc)
    {
        AppPaths.EnsureDataFolders();
        var abs = AppPaths.ToAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(abs)!);

        using var stream = File.Create(abs);
        var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        range.Save(stream, DataFormats.Rtf);
    }

    // brojanje re?i u FlowDocument-u
    public static int CountWords(FlowDocument doc)
    {
        var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        return text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

