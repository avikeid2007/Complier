using Microsoft.UI.Xaml.Controls;

namespace C.Compiler.Dialogs
{
    public sealed partial class FindReplaceDialog : ContentDialog
    {
        public string SearchText => SearchBox.Text;
        public string ReplaceText => ReplaceBox.Text;
        public bool CaseSensitive => CaseSensitiveCheck.IsChecked == true;
        public bool WholeWord => WholeWordCheck.IsChecked == true;

        public FindReplaceDialog()
        {
            InitializeComponent();
        }

        public void SetInitialSearch(string text)
        {
            SearchBox.Text = text;
        }
    }
}
