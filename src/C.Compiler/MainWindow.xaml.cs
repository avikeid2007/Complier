using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using C.Compiler.Controls;
using C.Compiler.Models;
using C.Compiler.Services;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.Graphics;
using Windows.UI;

using WinRT.Interop;

namespace C.Compiler
{
    public sealed partial class MainWindow : Window
    {
        private readonly FileService _fileService;
        private readonly CompilerService _compilerService = new();
        private readonly SettingsService _settingsService = new();
        private readonly ObservableCollection<string> _messages = new();
        private readonly List<CompilerError> _currentErrors = new();

        private string _lastSearchText = string.Empty;
        private bool _lastSearchCaseSensitive;
        private string? _lastCompiledExePath;

        // Menu system state
        private readonly Popup[] _menuPopups;
        private readonly Button[] _menuButtons;
        private int _activeMenuIndex = -1;
        private bool _menuBarActive;

        public MainWindow()
        {
            InitializeComponent();

            _fileService = new FileService(this);

            // Collect popup and button references
            _menuPopups = new Popup[]
            {
                MenuPopup0, MenuPopup1, MenuPopup2, MenuPopup3, MenuPopup4,
                MenuPopup5, MenuPopup6, MenuPopup7, MenuPopup8, MenuPopup9, MenuPopup10
            };
            _menuButtons = new Button[]
            {
                SysMenuBtn, FileMenuBtn, EditMenuBtn, SearchMenuBtn, RunMenuBtn,
                CompileMenuBtn, DebugMenuBtn, ProjectMenuBtn, OptionsMenuBtn, WindowMenuBtn, HelpMenuBtn
            };

            // Window setup — extend into title bar to remove Windows chrome
            Title = "Turbo C";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(MenuBarGrid); // Menu bar doubles as drag region

            // Style the caption buttons to match gray menu bar
            var appWindow = GetAppWindow();
            if (appWindow.TitleBar != null)
            {
                appWindow.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 170, 170, 170);
                appWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, 0, 0, 0);
                appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 0, 170, 0);
                appWindow.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
                appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 0, 128, 0);
                appWindow.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 255, 255, 255);
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 170, 170, 170);
                appWindow.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 85, 85, 85);
            }
            appWindow.Resize(new SizeInt32(1024, 768));

            // Message list
            MessageList.ItemsSource = _messages;

            // Editor
            var doc = _fileService.CreateNew();
            ActiveEditor.Document = doc;
            ActiveEditor.CursorMoved += OnCursorMoved;
            ActiveEditor.ContentChanged += OnContentChanged;
            ActiveEditor.CloseRequested += OnEditorCloseRequested;
            ActiveEditor.FocusEditor();

            // Load settings + detect compiler
            _ = InitAsync();
        }

        private AppWindow GetAppWindow()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        private async Task InitAsync()
        {
            await _settingsService.LoadAsync();
            _compilerService.Settings = _settingsService.Settings.Compiler;

            if (_compilerService.DetectCompiler())
                AddMessage($"Compiler detected: {_compilerService.DetectedCompilerType} at {_compilerService.DetectedCompilerPath}");
            else
                AddMessage("No C compiler detected. Configure in Options > Compiler.");
        }

        // ═══════════════════════════════════════
        // CUSTOM MENU SYSTEM
        // ═══════════════════════════════════════

        private void MenuTitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int index))
            {
                if (_activeMenuIndex == index && _menuPopups[index].IsOpen)
                {
                    CloseAllMenus();
                }
                else
                {
                    OpenMenu(index);
                }
            }
        }

        private void MenuTitle_Hover(object sender, PointerRoutedEventArgs e)
        {
            // Only switch menus on hover if a menu is already open
            if (!_menuBarActive) return;

            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int index))
            {
                if (index != _activeMenuIndex)
                {
                    OpenMenu(index);
                }
            }
        }

        private void OpenMenu(int index)
        {
            // Close any open menu first
            for (int i = 0; i < _menuPopups.Length; i++)
            {
                _menuPopups[i].IsOpen = false;
                _menuButtons[i].Background = new SolidColorBrush(Colors.Transparent);
            }

            // Position the popup below the button
            var btn = _menuButtons[index];
            var transform = btn.TransformToVisual(RootGrid);
            var pos = transform.TransformPoint(new Windows.Foundation.Point(0, btn.ActualHeight));

            _menuPopups[index].HorizontalOffset = pos.X;
            _menuPopups[index].VerticalOffset = pos.Y;
            _menuPopups[index].IsOpen = true;

            // Highlight the active menu title (green like TC3)
            btn.Background = new SolidColorBrush(Color.FromArgb(255, 0, 170, 0));

            _activeMenuIndex = index;
            _menuBarActive = true;
        }

        private void CloseAllMenus()
        {
            for (int i = 0; i < _menuPopups.Length; i++)
            {
                _menuPopups[i].IsOpen = false;
                _menuButtons[i].Background = new SolidColorBrush(Colors.Transparent);
            }
            _activeMenuIndex = -1;
            _menuBarActive = false;
        }

        private void MenuPopup_Closed(object sender, object e)
        {
            // When light-dismiss closes a popup, reset state
            if (sender is Popup closedPopup)
            {
                int index = Array.IndexOf(_menuPopups, closedPopup);
                if (index >= 0)
                {
                    _menuButtons[index].Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            // Check if any popup is still open
            bool anyOpen = false;
            for (int i = 0; i < _menuPopups.Length; i++)
            {
                if (_menuPopups[i].IsOpen) { anyOpen = true; break; }
            }
            if (!anyOpen)
            {
                _activeMenuIndex = -1;
                _menuBarActive = false;
            }
        }

        // ═══════════════════════════════════════
        // KEYBOARD ACCELERATORS
        // ═══════════════════════════════════════

        private void Accel_Save(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { FileSave_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Open(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { FileOpen_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Make(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { Make_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Compile(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { Compile_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Run(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { RunProgram_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Quit(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { Close(); e.Handled = true; }
        private void Accel_UserScreen(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { UserScreen_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Find(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { SearchFind_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_Replace(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { SearchReplace_Click(s, new RoutedEventArgs()); e.Handled = true; }
        private void Accel_GoToLine(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { GoToLine_Click(s, new RoutedEventArgs()); e.Handled = true; }

        private void Accel_Escape(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e)
        {
            // Close any open dialog or menu
            if (FindDialogOverlay.Visibility == Visibility.Visible) { FindDialogOverlay.Visibility = Visibility.Collapsed; e.Handled = true; return; }
            if (GoToLineDialogOverlay.Visibility == Visibility.Visible) { GoToLineDialogOverlay.Visibility = Visibility.Collapsed; e.Handled = true; return; }
            if (CompilerDialogOverlay.Visibility == Visibility.Visible) { CompilerDialogOverlay.Visibility = Visibility.Collapsed; e.Handled = true; return; }
            if (AboutDialogOverlay.Visibility == Visibility.Visible) { AboutDialogOverlay.Visibility = Visibility.Collapsed; e.Handled = true; return; }
            CloseAllMenus();
            e.Handled = true;
        }

        // ═══════════════════════════════════════
        // MESSAGE HELPERS
        // ═══════════════════════════════════════

        private void AddMessage(string message) => _messages.Add(message);
        private void ClearMessages() { _messages.Clear(); _currentErrors.Clear(); }

        // ═══════════════════════════════════════
        // EDITOR EVENTS
        // ═══════════════════════════════════════

        private void OnCursorMoved(object? sender, (int Line, int Column) pos) => StatusLineCol.Text = $"{pos.Line}:{pos.Column}";
        private void OnContentChanged(object? sender, EventArgs e) { }
        private void OnEditorCloseRequested(object? sender, EventArgs e) => ActiveEditor.Document = _fileService.CreateNew();

        // ═══════════════════════════════════════
        // FILE MENU HANDLERS
        // ═══════════════════════════════════════

        private void FileNew_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); ActiveEditor.Document = _fileService.CreateNew(); }

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            var doc = await _fileService.OpenFileAsync();
            if (doc != null) { ActiveEditor.Document = doc; AddMessage($"Loaded: {doc.FilePath}"); }
        }

        private async void FileSave_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ActiveEditor.Document.Content = ActiveEditor.GetText();
            if (await _fileService.SaveAsync(ActiveEditor.Document))
                AddMessage($"Saved: {ActiveEditor.Document.FilePath}");
        }

        private async void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ActiveEditor.Document.Content = ActiveEditor.GetText();
            if (await _fileService.SaveAsAsync(ActiveEditor.Document))
                AddMessage($"Saved as: {ActiveEditor.Document.FilePath}");
        }

        private void ChangeDir_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); AddMessage($"Current directory: {Environment.CurrentDirectory}"); }

        private void DosShell_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = true,
                WorkingDirectory = ActiveEditor.Document.FilePath != null
                    ? Path.GetDirectoryName(ActiveEditor.Document.FilePath) ?? Environment.CurrentDirectory
                    : Environment.CurrentDirectory
            });
        }

        private void Quit_Click(object sender, RoutedEventArgs e) { Close(); }

        // ═══════════════════════════════════════
        // EDIT MENU HANDLERS
        // ═══════════════════════════════════════

        private void EditUndo_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); }
        private void EditRedo_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); }
        private void EditCut_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); }
        private void EditCopy_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); }
        private void EditPaste_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); }

        // ═══════════════════════════════════════
        // SEARCH MENU — inline dialog overlays
        // ═══════════════════════════════════════

        private void SearchFind_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            DlgFindSearchBox.Text = _lastSearchText;
            DlgFindReplaceBox.Text = string.Empty;
            FindDialogOverlay.Visibility = Visibility.Visible;
            DlgFindSearchBox.Focus(FocusState.Programmatic);
        }

        private void SearchReplace_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            DlgFindSearchBox.Text = _lastSearchText;
            FindDialogOverlay.Visibility = Visibility.Visible;
            DlgFindReplaceBox.Focus(FocusState.Programmatic);
        }

        private void DlgFind_OK(object sender, RoutedEventArgs e)
        {
            _lastSearchText = DlgFindSearchBox.Text;
            _lastSearchCaseSensitive = DlgFindCaseCheck.IsChecked == true;
            bool wholeWord = DlgFindWholeWordCheck.IsChecked == true;
            FindDialogOverlay.Visibility = Visibility.Collapsed;

            if (!ActiveEditor.Find(_lastSearchText, _lastSearchCaseSensitive, wholeWord))
                AddMessage($"Search string not found: {_lastSearchText}");
        }

        private void DlgFind_ReplaceAll(object sender, RoutedEventArgs e)
        {
            string search = DlgFindSearchBox.Text;
            string replace = DlgFindReplaceBox.Text;
            bool caseSensitive = DlgFindCaseCheck.IsChecked == true;
            FindDialogOverlay.Visibility = Visibility.Collapsed;

            int count = ActiveEditor.ReplaceAll(search, replace, caseSensitive);
            AddMessage($"Replaced {count} occurrences.");
        }

        private void DlgFind_Cancel(object sender, RoutedEventArgs e) => FindDialogOverlay.Visibility = Visibility.Collapsed;

        private void SearchAgain_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            if (!string.IsNullOrEmpty(_lastSearchText))
            {
                if (!ActiveEditor.Find(_lastSearchText, _lastSearchCaseSensitive, false))
                    AddMessage($"Search string not found: {_lastSearchText}");
            }
        }

        private void GoToLine_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            DlgGoToLineBox.Text = ActiveEditor.Document.CursorLine.ToString();
            GoToLineDialogOverlay.Visibility = Visibility.Visible;
            DlgGoToLineBox.Focus(FocusState.Programmatic);
            DlgGoToLineBox.SelectAll();
        }

        private void DlgGoToLine_OK(object sender, RoutedEventArgs e)
        {
            GoToLineDialogOverlay.Visibility = Visibility.Collapsed;
            if (int.TryParse(DlgGoToLineBox.Text, out int line) && line > 0)
                ActiveEditor.GoToLine(line);
        }

        private void DlgGoToLine_Cancel(object sender, RoutedEventArgs e) => GoToLineDialogOverlay.Visibility = Visibility.Collapsed;

        private void NextError_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            if (_currentErrors.Count > 0)
            {
                var error = _currentErrors[0];
                ActiveEditor.GoToLine(error.Line);
                _currentErrors.RemoveAt(0);
                _currentErrors.Add(error);
            }
        }

        // ═══════════════════════════════════════
        // COMPILE MENU
        // ═══════════════════════════════════════

        private async void Compile_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); await CompileCurrentFileAsync(compileOnly: true); }
        private async void Make_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); await CompileCurrentFileAsync(compileOnly: false); }

        private async Task CompileCurrentFileAsync(bool compileOnly)
        {
            var doc = ActiveEditor.Document;
            doc.Content = ActiveEditor.GetText();

            if (doc.IsNewFile)
            {
                if (!await _fileService.SaveAsAsync(doc)) { AddMessage("Compilation cancelled — file not saved."); return; }
            }
            else if (doc.IsDirty)
            {
                await _fileService.SaveAsync(doc);
            }

            ClearMessages();
            AddMessage($"Compiling {doc.FilePath}...");

            var result = compileOnly
                ? await _compilerService.CompileAsync(doc.FilePath!)
                : await _compilerService.MakeAsync(doc.FilePath!);

            if (result.Success)
            {
                AddMessage("Compilation successful.");
                if (!compileOnly) { _lastCompiledExePath = result.OutputPath; AddMessage($"Output: {result.OutputPath}"); }
            }
            else
            {
                AddMessage("Compilation failed:");
                _currentErrors.Clear();
                foreach (var error in result.Errors) { AddMessage(error.ToString()); _currentErrors.Add(error); }
                if (result.Errors.Count == 0 && !string.IsNullOrWhiteSpace(result.RawOutput))
                    foreach (var line in result.RawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                        AddMessage(line.TrimEnd());
                if (_currentErrors.Count > 0) ActiveEditor.GoToLine(_currentErrors[0].Line);
            }
        }

        // ═══════════════════════════════════════
        // RUN MENU
        // ═══════════════════════════════════════

        private async void RunProgram_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            await CompileCurrentFileAsync(compileOnly: false);
            if (_lastCompiledExePath != null && File.Exists(_lastCompiledExePath))
            {
                AddMessage($"Running: {_lastCompiledExePath}");
                _compilerService.RunExecutable(_lastCompiledExePath);
            }
            else
            {
                AddMessage("No executable to run. Compile your program first.");
            }
        }

        private void UserScreen_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); AddMessage("User Screen: Switch to the console window to see program output."); }

        // ═══════════════════════════════════════
        // OPTIONS MENU — inline dialog
        // ═══════════════════════════════════════

        private void OptionsCompiler_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            var s = _compilerService.Settings;
            DlgCompilerPath.Text = s.CompilerPath;
            DlgIncludeDirs.Text = s.IncludeDirectories;
            DlgLibDirs.Text = s.LibraryDirectories;
            DlgOutputDir.Text = s.OutputDirectory;
            DlgFlags.Text = s.AdditionalFlags;
            CompilerDialogOverlay.Visibility = Visibility.Visible;
        }

        private async void DlgCompiler_OK(object sender, RoutedEventArgs e)
        {
            CompilerDialogOverlay.Visibility = Visibility.Collapsed;
            _compilerService.Settings = new CompilerSettings
            {
                CompilerPath = DlgCompilerPath.Text.Trim(),
                CompilerType = "auto",
                IncludeDirectories = DlgIncludeDirs.Text.Trim(),
                LibraryDirectories = DlgLibDirs.Text.Trim(),
                OutputDirectory = DlgOutputDir.Text.Trim(),
                AdditionalFlags = DlgFlags.Text.Trim()
            };
            _settingsService.Settings.Compiler = _compilerService.Settings;
            await _settingsService.SaveAsync();
            AddMessage("Compiler options saved.");
        }

        private void DlgCompiler_Cancel(object sender, RoutedEventArgs e) => CompilerDialogOverlay.Visibility = Visibility.Collapsed;

        // ═══════════════════════════════════════
        // WINDOW MENU
        // ═══════════════════════════════════════

        private void WindowList_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); AddMessage($"Window 1: {ActiveEditor.Document.FileName}"); }
        private void ToggleMessageWindow_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            MessagePanel.Visibility = MessagePanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        // ═══════════════════════════════════════
        // ABOUT — inline overlay
        // ═══════════════════════════════════════

        private void About_Click(object sender, RoutedEventArgs e) { CloseAllMenus(); AboutDialogOverlay.Visibility = Visibility.Visible; }
        private void DlgAbout_OK(object sender, RoutedEventArgs e) => AboutDialogOverlay.Visibility = Visibility.Collapsed;
    }
}
