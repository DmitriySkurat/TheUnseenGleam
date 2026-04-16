public static class GameResolutions
{
    public static readonly (int width, int height)[] resolutions =
    {
        (1280,  720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };

    public static int DefaultIndex = 1; // 1920x1080

    public static string ToLabel(int width, int height) => $"{width}x{height}";
}
