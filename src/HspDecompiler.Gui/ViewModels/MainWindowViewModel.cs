using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HspDecompiler.Core.DpmToAx;
using HspDecompiler.Core.Pipeline;
using HspDecompiler.Gui.Resources;
using HspDecompiler.Gui.Services;

namespace HspDecompiler.Gui.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _logText = "";

        [ObservableProperty]
        private bool _isProcessing;

        public ObservableCollection<DpmFileEntry> DpmFiles { get; } = new ObservableCollection<DpmFileEntry>();

        public async Task ProcessFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            IsProcessing = true;
            LogText = "";
            DpmFiles.Clear();

            try
            {
                var logger = new GuiLogger(this);
                var progress = new GuiProgressReporter();
                var pipeline = new DecompilerPipeline(logger, progress);

                string dictPath = Path.Combine(AppContext.BaseDirectory, "Dictionary.csv");
                if (!pipeline.Initialize(dictPath))
                {
                    AppendLog(Strings.FailedToLoadDictionary);
                    return;
                }

                var options = new DecompilerOptions
                {
                    InputPath = filePath,
                    OutputDirectory = Path.GetDirectoryName(filePath),
                    DictionaryPath = dictPath,
                    AllowDecryption = true,
                    SkipEncrypted = false
                };

                var result = await pipeline.RunAsync(options, CancellationToken.None);

                if (!result.Success)
                    AppendLog(string.Format(Strings.ErrorFormat, result.ErrorMessage));
            }
            catch (Exception ex)
            {
                AppendLog(string.Format(Strings.ErrorFormat, ex.Message));
            }
            finally
            {
                IsProcessing = false;
            }
        }

        internal void AppendLog(string line)
        {
            LogText += line + Environment.NewLine;
        }
    }
}
