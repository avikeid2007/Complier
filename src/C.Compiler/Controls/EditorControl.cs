using System;
using System.Collections.Generic;
using System.Text;

using C.Compiler.Models;
using C.Compiler.Services;

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.UI;

namespace C.Compiler.Controls
{
    public sealed partial class EditorControl : UserControl
    {
        private readonly SyntaxHighlighter _highlighter = new();
        private EditorDocument _document = new();
        private bool _isUpdating;
        private bool _isHighlighting;
        private int _windowNumber = 1;

        public event EventHandler? CloseRequested;
        public event EventHandler? ContentChanged;
        public event EventHandler<(int Line, int Column)>? CursorMoved;

        public EditorDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                LoadDocument();
            }
        }

        public int WindowNumber
        {
            get => _windowNumber;
            set
            {
                _windowNumber = value;
                WindowNumberText.Text = value.ToString();
            }
        }

        public EditorControl()
        {
            InitializeComponent();
            SetEditorDefaults();
        }

        private void SetEditorDefaults()
        {
            CodeEditor.Document.SetText(TextSetOptions.None, string.Empty);

            // Set default character formatting
            var format = CodeEditor.Document.GetDefaultCharacterFormat();
            format.ForegroundColor = Color.FromArgb(255, 255, 255, 85); // Yellow
            format.Size = 14;
            format.Name = "Consolas";
            CodeEditor.Document.SetDefaultCharacterFormat(format);
        }

        private void LoadDocument()
        {
            _isUpdating = true;
            TitleText.Text = _document.DisplayTitle;

            CodeEditor.Document.SetText(TextSetOptions.None, _document.Content);
            ApplySyntaxHighlighting();

            _isUpdating = false;
            UpdateLineNumbers();
        }

        public string GetText()
        {
            CodeEditor.Document.GetText(TextGetOptions.None, out string text);
            // RichEditBox appends a trailing \r, remove it
            return text.TrimEnd('\r', '\n');
        }

        public void SetText(string text)
        {
            _isUpdating = true;
            CodeEditor.Document.SetText(TextSetOptions.None, text);
            _document.Content = text;
            ApplySyntaxHighlighting();
            _isUpdating = false;
            UpdateLineNumbers();
        }

        public void GoToLine(int lineNumber)
        {
            string text = GetText();
            int pos = 0;
            int currentLine = 1;

            foreach (char c in text)
            {
                if (currentLine >= lineNumber) break;
                if (c == '\r') currentLine++;
                pos++;
            }

            var range = CodeEditor.Document.GetRange(pos, pos);
            range.ScrollIntoView(PointOptions.Start);
            CodeEditor.Document.Selection.SetRange(pos, pos);
            CodeEditor.Focus(FocusState.Programmatic);
        }

        public void FocusEditor()
        {
            CodeEditor.Focus(FocusState.Programmatic);
        }

        public bool Find(string searchText, bool caseSensitive, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText)) return false;

            string text = GetText();
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int startFrom = CodeEditor.Document.Selection.EndPosition;
            int index = text.IndexOf(searchText, startFrom, comparison);

            // Wrap around
            if (index < 0)
                index = text.IndexOf(searchText, 0, comparison);

            if (index >= 0)
            {
                CodeEditor.Document.Selection.SetRange(index, index + searchText.Length);
                return true;
            }

            return false;
        }

        public int ReplaceAll(string searchText, string replaceText, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(searchText)) return 0;

            string text = GetText();
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int count = 0;
            int index;
            var sb = new StringBuilder(text);
            int searchFrom = 0;

            while ((index = text.IndexOf(searchText, searchFrom, comparison)) >= 0)
            {
                count++;
                searchFrom = index + searchText.Length;
            }

            if (count > 0)
            {
                string newText = caseSensitive
                    ? text.Replace(searchText, replaceText, StringComparison.Ordinal)
                    : text.Replace(searchText, replaceText, StringComparison.OrdinalIgnoreCase);
                SetText(newText);
                _document.IsDirty = true;
                _document.Content = newText;
                TitleText.Text = _document.DisplayTitle;
            }

            return count;
        }

        private void ApplySyntaxHighlighting()
        {
            if (_isHighlighting) return;
            _isHighlighting = true;

            try
            {
                string text = GetText();
                var tokens = _highlighter.Tokenize(text);

                // Set all text to default color first
                var fullRange = CodeEditor.Document.GetRange(0, text.Length);
                var defaultFormat = fullRange.CharacterFormat;
                defaultFormat.ForegroundColor = Color.FromArgb(255, 255, 255, 85); // Yellow default
                defaultFormat.Bold = FormatEffect.Off;
                defaultFormat.Name = "Consolas";
                defaultFormat.Size = 14;

                foreach (var token in tokens)
                {
                    if (token.Start + token.Length > text.Length) continue;

                    var range = CodeEditor.Document.GetRange(token.Start, token.Start + token.Length);
                    var format = range.CharacterFormat;
                    format.ForegroundColor = token.Color;
                    format.Bold = token.Bold ? FormatEffect.On : FormatEffect.Off;
                    format.Name = "Consolas";
                    format.Size = 14;
                }
            }
            finally
            {
                _isHighlighting = false;
            }
        }

        private void UpdateLineNumbers()
        {
            string text = GetText();
            int lineCount = 1;
            foreach (char c in text)
            {
                if (c == '\r') lineCount++;
            }

            var sb = new StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                sb.AppendLine(i.ToString());
            }
            LineNumbers.Text = sb.ToString().TrimEnd();
        }

        private (int Line, int Column) GetCursorPosition()
        {
            string text = GetText();
            int pos = CodeEditor.Document.Selection.StartPosition;
            if (pos > text.Length) pos = text.Length;

            int line = 1, col = 1;
            for (int i = 0; i < pos && i < text.Length; i++)
            {
                if (text[i] == '\r')
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col++;
                }
            }
            return (line, col);
        }

        private void CodeEditor_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _isHighlighting) return;

            string text = GetText();
            _document.Content = text;
            _document.IsDirty = true;
            TitleText.Text = _document.DisplayTitle;

            UpdateLineNumbers();
            ApplySyntaxHighlighting();

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CodeEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;

            var (line, col) = GetCursorPosition();
            _document.CursorLine = line;
            _document.CursorColumn = col;
            CursorMoved?.Invoke(this, (line, col));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle zoom (could toggle visibility of other panels)
        }
    }
}
