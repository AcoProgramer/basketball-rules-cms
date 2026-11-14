using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BasketballRulesCms.App.Services;

public sealed class XmlFileStore<T> where T : class, new()
{
    private readonly XmlSerializer _serializer = new(typeof(T));
    private readonly string _relativePath;

    public XmlFileStore(string relativePath)
    {
        _relativePath = relativePath;
    }

    public string AbsolutePath => AppPaths.ToAbsolutePath(_relativePath);

    public T LoadOrCreate(Func<T> factory)
    {
        AppPaths.EnsureDataFolders();

        var abs = AbsolutePath;
        if (!File.Exists(abs))
        {
            var doc = factory();
            Save(doc);
            return doc;
        }

        using var stream = File.OpenRead(abs);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        var result = _serializer.Deserialize(reader) as T;
        return result ?? factory();
    }

    public void Save(T document)
    {
        AppPaths.EnsureDataFolders();

        var abs = AbsolutePath;
        Directory.CreateDirectory(Path.GetDirectoryName(abs)!);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            NewLineHandling = NewLineHandling.Entitize,
        };

        using var stream = File.Create(abs);
        using var writer = XmlWriter.Create(stream, settings);
        _serializer.Serialize(writer, document);
    }
}

