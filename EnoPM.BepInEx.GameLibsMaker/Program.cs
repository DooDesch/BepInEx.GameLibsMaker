using System.Diagnostics;
using EnoPM.BepInEx.GameLibsMaker.Extensions;
using Mono.Cecil;

namespace EnoPM.BepInEx.GameLibsMaker;

internal static class Program
{
    private delegate bool ArgumentValidator(string input, out string errorMessage);
    private static readonly DefaultAssemblyResolver Resolver = new();
    private static readonly string ConfigFilePath = Path.Combine(Directory.GetCurrentDirectory(), $".{Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName)}");
    
    private static void Main(string[] args)
    {
        var globalExecution = Stopwatch.StartNew();
        var isManuallyStarted = Environment.GetCommandLineArgs().Length <= 1;
        try
        {
            InfoMessage("Welcome and thank you for using GameLibsMaker");
            if (args.Length == 2)
            {
                RunWithArguments(args);
            }
            else if (File.Exists(ConfigFilePath))
            {
                RunWithConfigFile();
            }
            else
            {
                if (args.Length > 0)
                {
                    ErrorMessage("Invalid arguments count", "You must specify 2 arguments (game directory & output directory)");
                }
                else
                {
                    RunWithReadLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
        }
        globalExecution.Stop();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[GameLibsMaker] Total execution time {MathF.Round((float)globalExecution.Elapsed.TotalMilliseconds, 2)} ms.");
        Console.ResetColor();
        if (isManuallyStarted)
        {
            Console.ReadKey();
        }
    }

    private static void RunWithArguments(string[] arguments)
    {
        var gameDirectory = arguments[0];
        if (!IsValidGameDirectory(gameDirectory, out var gameDirectoryError))
        {
            ErrorMessage("Invalid game directory", gameDirectoryError);
            return;
        }
        var outputDirectory = arguments[1];
        if (!IsValidOutputDirectory(outputDirectory, out var outputDirectoryError))
        {
            ErrorMessage("Invalid output directory", outputDirectoryError);
            return;
        }
        Run(gameDirectory, outputDirectory);
    }

    private static void RunWithConfigFile()
    {
        var fileContent = File.ReadAllText(ConfigFilePath);
        var args = fileContent.Split('\n');
        if (args.Length < 2)
        {
            ErrorMessage("Invalid config file", "Config file must contains 2 lines: game directory and output directory");
            return;
        }
        RunWithArguments(args.Select(x => x.Trim()).ToArray());
    }
    
    private static void RunWithReadLine()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[Configuration required]");
        Console.ResetColor();
        Run(ValidatedAsk("Game directory:", IsValidGameDirectory), ValidatedAsk("Output directory:", IsValidOutputDirectory));
    }

    private static void Run(string gameDirectoryPath, string outputDirectoryPath)
    {
        var managedDirectory = GetManagedDirectory(gameDirectoryPath);
        if (managedDirectory == null) return;
        var totalStopwatch = Stopwatch.StartNew();
        var libraryCount = 0;
        if (managedDirectory.FullName.ToLowerInvariant().Replace("/", string.Empty).Replace(@"\", string.Empty) == outputDirectoryPath.ToLowerInvariant().Replace("/", string.Empty).Replace(@"\", string.Empty))
        {
            ErrorMessage("Invalid output directory", "Output directory cannot be managed directory");
            return;
        }
        Resolver.AddSearchDirectory(managedDirectory.FullName);
        var propsFile = new PropsFileMaker(outputDirectoryPath, "GameLibs.props");

        var files = managedDirectory.GetFiles("*.dll");
        foreach (var file in files)
        {
            // Skip if file starts with "mscore", "netstandard", "unity" or "system"
            if (file.Name.StartsWith("mscore") || file.Name.StartsWith("netstandard") || file.Name.StartsWith("unity") || file.Name.StartsWith("system"))
            {
                continue;
            }
            var stopwatch = Stopwatch.StartNew();
            InfoMessage($"Processing assembly {file.Name}...");
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(file.FullName, new ReaderParameters
            {
                AssemblyResolver = Resolver
            });
            assemblyDefinition.Publicize();
            assemblyDefinition.Strip();
            var assemblyOutputPath = Path.Combine(outputDirectoryPath, assemblyDefinition.MainModule.Name);
            assemblyDefinition.Write(assemblyOutputPath);
            stopwatch.Stop();
            SuccessMessage($"Library {file.Name} created in {MathF.Round((float)stopwatch.Elapsed.TotalMilliseconds, 2)}ms.");
            libraryCount++;
            propsFile.AddReference(assemblyDefinition.Name.Name, assemblyOutputPath);
        }
        
        InfoMessage($"Creating references props file...");
        propsFile.Save();
        SuccessMessage($"References props file created: {propsFile.OutputFileName}");
        totalStopwatch.Stop();
        InfoMessage($"{libraryCount} libraries created in {MathF.Round((float)totalStopwatch.Elapsed.TotalSeconds, 2)} seconds.");
    }

    private static DirectoryInfo? GetManagedDirectory(string gameDirectoryPath)
    {
        var gameDirectory = new DirectoryInfo(gameDirectoryPath);
        var gameDataDirectory = gameDirectory.GetDirectories().FirstOrDefault(x => x.Name.EndsWith("_Data"));
        if (gameDataDirectory == null)
        {
            ErrorMessage("Invalid game directory", $"Unable to find game data directory in {gameDirectory.FullName}");
            return null;
        }
        if (gameDataDirectory.GetDirectories().FirstOrDefault(x => x.Name == "il2cpp_data") != null)
        {
            // Game is a il2Cpp game
            var bepInExDirectory = gameDirectory.GetDirectories().FirstOrDefault(x => x.Name == "BepInEx");
            if (bepInExDirectory == null)
            {
                ErrorMessage("Invalid game directory", $"Unable to find BepInEx directory in {gameDirectory.FullName}");
                return null;
            }
            var interopDirectory = bepInExDirectory.GetDirectories().FirstOrDefault(x => x.Name == "interop");
            if (interopDirectory == null)
            {
                ErrorMessage("Invalid game directory", $"Unable to find interop directory in {bepInExDirectory.FullName}");
                return null;
            }
            return interopDirectory;
        }
        var managedDirectory = gameDataDirectory.GetDirectories().FirstOrDefault(x => x.Name == "Managed");
        if (managedDirectory == null)
        {
            ErrorMessage("Invalid game directory", $"Unable to find managed directory in {gameDataDirectory.FullName}");
            return null;
        }
        return managedDirectory;
    }

    private static void ErrorMessage(string category, string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"[{category}]");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    private static void SuccessMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    private static void InfoMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static string ValidatedAsk(string question, ArgumentValidator validator)
    {
        var isValidInput = false;
        var input = string.Empty;
        var errorMessage = string.Empty;

        while (!isValidInput || input == null)
        {
            if (errorMessage != string.Empty)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{question} ");
            Console.ResetColor();
            input = Console.ReadLine();
            isValidInput = input != null && validator(input, out errorMessage);
        }

        return input;
    }

    private static bool IsValidGameDirectory(string path, out string errorMessage)
    {
        if (path == string.Empty)
        {
            errorMessage = string.Empty;
            return false;
        }
        if (!Directory.Exists(path))
        {
            errorMessage = $"Directory not found: |{path}|";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }
    
    private static bool IsValidOutputDirectory(string path, out string errorMessage)
    {
        if (path == string.Empty)
        {
            errorMessage = string.Empty;
            return false;
        }
        if (!Directory.Exists(path))
        {
            errorMessage = $"Directory not found: |{path}|";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }
}