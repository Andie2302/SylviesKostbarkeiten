using SylviesKostbarkeiten.io;
using SylviesKostbarkeiten.models;

namespace SylviesKostbarkeiten;

public static class Tools
{
    public static string DesktopPfad => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static string GetDesktopDatei(string dateiName) => Path.Combine(DesktopPfad, dateiName);
    public static List<KassenGruppenInfo> LeseMenueDatei(string menueDateiPfad) => File.Exists(menueDateiPfad) ? new MenueParser().Parse(menueDateiPfad) : [];
}