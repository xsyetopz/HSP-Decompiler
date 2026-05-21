using System.Reflection;
using HspDecompiler.Gui.Resources;

namespace HspDecompiler.Gui.ViewModels;

public class AboutViewModel : ViewModelBase
{
    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    public static string Title => Strings.WindowTitle;
    public static string Copyright => "Kitsutsuki (Original) / HSP Decompiler Contributors";
}
