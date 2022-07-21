using LibGit2Sharp;
using System.Diagnostics;
using System.Text;

namespace GitPull;

public class MainClass
{
    private static ListInfo _path = new();
    private static bool _up2date;
    private static StringBuilder _logs = new();
    public MainClass()
    {
        UpdatePatch();
        //Console.Write("Enter Y for Pull: ");
        //var result = Console.ReadKey();
        //Console.WriteLine("");
        Console.Write("Enter Y for show UpToDate: ");
        var up2date = Console.ReadKey();
        if (up2date.Key == ConsoleKey.Y)
            _up2date = true;
        //if (result.Key != ConsoleKey.Y) return;
        Console.WriteLine("");
        Console.Clear();
        PullThemAll();
    }

    private static void PullThemAll()
    {
        _logs = new StringBuilder();
        var listPath = GetPath();
        var min = 0;
        var max = 0;
        max += listPath.Sum(t => t.Count);

        var header = false;
        foreach (var paths in listPath)
        {
            if (_up2date)
            {
                WriteOutput("================= " + paths[0] + " =================");
                header = true;
            }
            foreach (var path in paths)
            {
                min++;
                Console.Title = $"In process {min}/{max}";
                var result = GitPull(path);
                if (result == null)
                {
                    Debug.WriteLine("It's null");
                    continue;
                }

                if (result.Status == MergeStatus.FastForward)
                {
                    if (!header)
                    {
                        WriteOutput("================= " + paths[0] + " =================");
                        header = true;
                    }
                    WriteOutput(path);
                    WriteOutput(result.Status);
                    WriteOutput(result.Commit);
                    WriteOutput("");
                }
                else if (result.Status == MergeStatus.UpToDate && !_up2date)
                {
                    Debug.WriteLine(path);
                    Debug.WriteLine(result.Status);
                }
                else
                {
                    if (!header)
                    {
                        WriteOutput("================= " + paths[0] + " =================");
                        header = true;
                    }
                    WriteOutput(path);
                    WriteOutput(result.Status);
                    WriteOutput(result.Commit);
                    WriteOutput("");
                }
            }

            header = false;
        }

        WriteLogs(_logs);
    }

    private static void WriteLogs(StringBuilder logs)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var file = Path.Combine(path, date + ".txt");
        for (var i = 0; ; i++)
        {
            if (File.Exists(file))
            {
                file = Path.Combine(path, date + "_" + i + ".txt");
                continue;
            }
            break;
        }
        File.AppendAllText(file, logs.ToString());
    }

    private static void WriteOutput(object value)
    {
        Console.WriteLine(value);
        _logs.AppendLine(value.ToString());
    }

    private static MergeResult? GitPull(string path)
    {
        try
        {
            var repo = new Repository(path);
            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
            var pullResult = Commands.Pull(repo, signature, new PullOptions());
            return pullResult;
        }
        catch
        {
            return null;
        }
    }

    private static void UpdatePatch() // Config Path
    {
        _path.PathInfo = new List<PathInfo>
            {
                new()
                {
                    Path = @"D:\Android",
                    Depth = 0
                },
                new()
                {
                    Path = @"D:\Github\#Android",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#C-Sharp",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#Deobfuscator",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#Games",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#Injector",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#Maple\#Cheat",
                    Depth = 0
                },
                new()
                {
                    Path = @"D:\Github\#Maple\#Old",
                    Depth = 0
                },
                new()
                {
                    Path = @"D:\Github\#Memory",
                    Depth = 1
                },
                new()
                {
                    Path = @"D:\Github\#Protector",
                    Depth = 1
                },
                new()
                {
                    Path=@"D:\Github\#RO\#rAthenaServer",
                    Depth = 0
                },
                new()
                {
                    Path = @"D:\Github",
                    Depth = 1
                }
            };
    }

    private static List<List<string>> GetPath()
    {
        return (from t in _path.PathInfo where !t.Path.Contains("#Remove") select GetDirectory(t.Path, t.Depth, true)).ToList();
    }

    private static List<string> GetDirectory(string root, int depth, bool except)
    {
        var folders = new List<string> { root };
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            if (except)
            {
                if (IsExcept(directory))
                {
                    continue;
                }
            }
            folders.Add(directory);
            if (depth > 0)
            {
                var result = GetDirectory(directory, depth - 1, except);
                folders.AddRange(result);
            }
        }

        return folders;
    }

    private static bool IsExcept(string directory)
    {
        if (directory.Contains("#Archived") || directory.Contains("#Remove") || directory.Contains("#My Project") || directory.EndsWith("Logs"))
        {
            return true;
        }

        return false;
    }
}

public class PathInfo
{
    public string Path { get; set; }
    public int Depth { get; set; }
}

public class ListInfo
{
    public List<PathInfo> PathInfo { get; set; }
}