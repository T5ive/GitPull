namespace GitPull;

internal static class Program
{
    internal static PathSetting PathSetting = new();

    [STAThread]
    private static void Main(string[] args)
    {
        PathSetting = PathSetting.Load();
        MainClass.CallMe();
    }
}