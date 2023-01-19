namespace GitPull;

public class MainClass
{
    private static bool _up2date;
    private static StringBuilder _logs = new();

    public static void CallMe()
    {
        Console.Write("Enter Y for show UpToDate: ");
        var up2date = Console.ReadKey();
        if (up2date.Key == ConsoleKey.Y)
            _up2date = true;
        Console.WriteLine("");
        Console.Clear();

        var sw = new Stopwatch();
        sw.Start();
        PullThemAll();
        sw.Stop();

        Console.WriteLine("================= Done =================");
        Console.WriteLine(ToTime(sw.Elapsed));
        Console.WriteLine("========================================");

        Console.ReadKey();
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
                try
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
                catch (Exception ex)
                {
                    WriteOutput($"Error at {path} - In Process {min}");
                    WriteOutput(ex.Message);
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
            using var repo = new Repository(path);
            foreach (Submodule submodule in repo.Submodules)
            {
                var subrepoPath = Path.Combine(repo.Info.WorkingDirectory, submodule.Path);

                using var subRepo = new Repository(subrepoPath);
                Branch remoteBranch = subRepo.Branches["origin/master"];
                subRepo.Reset(ResetMode.Hard, remoteBranch.Tip);
            }
            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
            var pullResult = Commands.Pull(repo, signature, new PullOptions());
            return pullResult;
        }
        catch
        {
            return null;
        }
    }

    private static List<List<string>> GetPath()
    {
        return (from t in Program.PathSetting.PathInfo where Directory.Exists(t.Path) select GetDirectory(t.Path, t.Depth, true)).ToList();
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
        return directory.Contains("#Archived") || directory.Contains("#Remove") || directory.Contains("#My Project") || directory.EndsWith("Logs");
    }

    //https://stackoverflow.com/a/4423615
    private static string ToTime(TimeSpan span)
    {
        var day = span.Duration().Days > 0
            ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, "
            : string.Empty;

        var hours = span.Duration().Hours > 0
            ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, "
            : string.Empty;

        var min = span.Duration().Minutes > 0
            ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, "
            : string.Empty;

        var sec = span.Duration().Seconds > 0
            ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}"
            : string.Empty;

        var ms = span.Duration().Milliseconds > 0
            ? $"{span.Milliseconds:0} ms{(span.Milliseconds == 1 ? string.Empty : "ms")}"
            : string.Empty;

        var formatted = day + hours + min + sec;

        if (formatted.EndsWith(", ")) formatted = formatted[..^2];

        if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

        return formatted;
    }

    private static string ToTimeOld(TimeSpan time)
    {
        var result = "";
        var type = 0;

        if (time.TotalHours > 0)
        {
            result += time.TotalHours + ":";
            type = 3;
        }

        if (time.TotalMinutes > 0)
        {
            result += time.TotalMinutes + ":";
            if (type == 0)
                type = 2;
        }

        if (time.Seconds > 0)
        {
            result += time.TotalSeconds;
            if (type == 0)
                type = 1;
        }

        switch (type)
        {
            case 3:
                result += " hr";
                break;

            case 2:
                result += " min";
                break;

            case 1:
                result += " sec";
                break;

            case 0:
                result += time.TotalMilliseconds + " ms";
                break;
        }

        return result;
    }
}