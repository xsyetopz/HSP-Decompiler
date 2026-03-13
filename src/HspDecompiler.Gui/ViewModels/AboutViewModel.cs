using System.Reflection;
using HspDecompiler.Gui.Resources;

namespace HspDecompiler.Gui.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        public string Title => Strings.WindowTitle;
        public string Copyright => "Kitsutsuki (Original) / HSP Decompiler Contributors";
    }
}
