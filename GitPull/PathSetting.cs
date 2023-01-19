namespace GitPull;

public class PathSetting : JsonHelper<PathSetting>
{
    public List<PathInfo> PathInfo = new();
}