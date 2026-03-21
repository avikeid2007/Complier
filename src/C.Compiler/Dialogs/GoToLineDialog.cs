using Microsoft.UI.Xaml.Controls;

namespace C.Compiler.Dialogs
{
    public sealed partial class GoToLineDialog : ContentDialog
    {
        public int LineNumber { get; private set; }

        public GoToLineDialog()
        {
            InitializeComponent();
        }

        public GoToLineDialog(int currentLine) : this()
        {
            LineNumberBox.Text = currentLine.ToString();
            LineNumberBox.SelectAll();
        }

        public bool TryGetLineNumber(out int lineNumber)
        {
            return int.TryParse(LineNumberBox.Text, out lineNumber) && lineNumber > 0;
        }
    }
}
