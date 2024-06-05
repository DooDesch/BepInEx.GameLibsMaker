using System.Xml;
using System.Xml.Linq;

namespace EnoPM.BepInEx.GameLibsMaker;

internal sealed class PropsFileMaker
{
    private const string GameLibsOutputDirectoryVariableName = "GameLibsOutputDirectory";
    private static readonly XNamespace Ns = "http://schemas.microsoft.com/developer/msbuild/2003";
    private static readonly XmlWriterSettings WriterSettings = new()
    {
        Indent = true,
        IndentChars = "    ",
        OmitXmlDeclaration = true
    };
    
    private readonly XElement _itemGroup;
    private readonly XElement _project;
    private readonly string _outputDirectory;
    private readonly string _outputFileName;
    internal string OutputFileName => Path.Combine(_outputDirectory, _outputFileName);
    
    internal PropsFileMaker(string outputDirectory, string outputFileName)
    {
        _outputDirectory = new DirectoryInfo(outputDirectory).FullName;
        _outputFileName = outputFileName;
        var propertyGroup = new XElement(Ns + "PropertyGroup", new XElement(Ns + GameLibsOutputDirectoryVariableName, _outputDirectory));
        _itemGroup = new XElement(Ns + "ItemGroup");
        _project = new XElement(Ns + "Project", propertyGroup, _itemGroup);
    }

    internal void AddReference(string name, string path)
    {
        var filePath = new FileInfo(path).FullName.Replace(_outputDirectory, $"$({GameLibsOutputDirectoryVariableName})");
        var hintPath = new XElement(Ns + "HintPath", filePath);
        var include = new XAttribute("Include", name);
        var reference = new XElement(Ns + "Reference", include, hintPath);
        _itemGroup.Add(reference);
    }
    
    internal void Save()
    {
        var document = new XDocument(_project);
        using var writer = XmlWriter.Create(OutputFileName, WriterSettings);
        document.Save(writer);
    }
}