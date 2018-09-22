namespace Improbable.Gdk.BuildSystem.Configuration
{
    [System.Flags]
    public enum SpatialBuildPlatforms
    {
        Windows64 = 1 << 2,
        Linux = 1 << 3,
        OSX = 1 << 4
    }
}
