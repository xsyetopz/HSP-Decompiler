using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HspDecompiler.Gui.Resources;
using HspDecompiler.Gui.ViewModels;

namespace HspDecompiler.Gui.Views;

public partial class MainWindow : Window
{
    private static readonly string[] s_hspFilePatterns = ["*.ax", "*.exe", "*.dpm"];
    private static readonly string[] s_allFilePatterns = ["*"];

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Title = Strings.WindowTitle;

        MenuItem? menuOpen = this.FindControl<MenuItem>("MenuOpen");
        MenuItem? menuExit = this.FindControl<MenuItem>("MenuExit");
        MenuItem? menuAbout = this.FindControl<MenuItem>("MenuAbout");
        if (menuOpen != null)
        {
            menuOpen.Click += OnMenuOpen;
        }

        if (menuExit != null)
        {
            menuExit.Click += (_, _) => Close();
        }

        if (menuAbout != null)
        {
            menuAbout.Click += OnMenuAbout;
        }

        MenuItem? menuFile = this.FindControl<MenuItem>("MenuFile");
        MenuItem? menuHelp = this.FindControl<MenuItem>("MenuHelp");
        if (menuFile != null)
        {
            menuFile.Header = Strings.MenuFile;
        }

        if (menuOpen != null)
        {
            menuOpen.Header = Strings.MenuOpen;
        }

        if (menuExit != null)
        {
            menuExit.Header = Strings.MenuExit;
        }

        if (menuHelp != null)
        {
            menuHelp.Header = Strings.MenuHelp;
        }

        if (menuAbout != null)
        {
            menuAbout.Header = Strings.MenuAbout;
        }

        DataGrid? dataGrid = this.FindControl<DataGrid>("FileGrid");
        if (dataGrid != null && dataGrid.Columns.Count >= 4)
        {
            dataGrid.Columns[0].Header = Strings.ColumnFilename;
            dataGrid.Columns[1].Header = Strings.ColumnEncrypted;
            dataGrid.Columns[2].Header = Strings.ColumnOffset;
            dataGrid.Columns[3].Header = Strings.ColumnFileSize;
        }
    }

    private async void OnMenuOpen(object? sender, RoutedEventArgs e)
    {
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await StorageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = Strings.OpenFileTitle,
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType(Strings.HspFilesFilter)
                        {
                            Patterns = s_hspFilePatterns,
                        },
                        new FilePickerFileType(Strings.AllFilesFilter)
                        {
                            Patterns = s_allFilePatterns,
                        },
                    },
                }
            )
            .ConfigureAwait(false);
        if (files.Count > 0)
        {
            string? path = files[0].TryGetLocalPath();
            if (path != null && DataContext is MainWindowViewModel vm)
            {
                await vm.ProcessFileAsync(path).ConfigureAwait(false);
            }
        }
    }

    private void OnMenuAbout(object? sender, RoutedEventArgs e)
    {
        var about = new AboutDialog();
        about.ShowDialog(this);
    }

    private void OnDragOver(object? sender, DragEventArgs e) =>
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files))
        {
            return;
        }

        var files = e.Data.GetFiles()?.ToList();
        if (files == null || files.Count == 0)
        {
            return;
        }

        string? path = files[0].TryGetLocalPath();
        if (path != null && DataContext is MainWindowViewModel vm)
        {
            await vm.ProcessFileAsync(path).ConfigureAwait(false);
        }
    }
}
