namespace Improbable.Gdk.BuildSystem.Configuration
{
    [System.Flags]
    public enum SpatialBuildPlatforms
    {
        Windows64 = 1 << 0,
        Linux = 1 << 1,
        OSX = 1 << 2,
    }
}
