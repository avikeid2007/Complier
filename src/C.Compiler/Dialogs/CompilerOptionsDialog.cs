using C.Compiler.Models;

using Microsoft.UI.Xaml.Controls;

namespace C.Compiler.Dialogs
{
    public sealed partial class CompilerOptionsDialog : ContentDialog
    {
        public CompilerOptionsDialog()
        {
            InitializeComponent();
        }

        public void LoadSettings(CompilerSettings settings)
        {
            CompilerPathBox.Text = settings.CompilerPath;
            IncludeDirsBox.Text = settings.IncludeDirectories;
            LibDirsBox.Text = settings.LibraryDirectories;
            OutputDirBox.Text = settings.OutputDirectory;
            FlagsBox.Text = settings.AdditionalFlags;

            // Select compiler type
            for (int i = 0; i < CompilerTypeCombo.Items.Count; i++)
            {
                if (CompilerTypeCombo.Items[i] is ComboBoxItem item &&
                    item.Tag?.ToString() == settings.CompilerType)
                {
                    CompilerTypeCombo.SelectedIndex = i;
                    break;
                }
            }
            if (CompilerTypeCombo.SelectedIndex < 0)
                CompilerTypeCombo.SelectedIndex = 0;
        }

        public CompilerSettings GetSettings()
        {
            string compilerType = "auto";
            if (CompilerTypeCombo.SelectedItem is ComboBoxItem selected)
                compilerType = selected.Tag?.ToString() ?? "auto";

            return new CompilerSettings
            {
                CompilerPath = CompilerPathBox.Text.Trim(),
                CompilerType = compilerType,
                IncludeDirectories = IncludeDirsBox.Text.Trim(),
                LibraryDirectories = LibDirsBox.Text.Trim(),
                OutputDirectory = OutputDirBox.Text.Trim(),
                AdditionalFlags = FlagsBox.Text.Trim()
            };
        }
    }
}
