namespace C.Compiler.Models
{
    public class EditorDocument
    {
        public string FileName { get; set; } = "NONAME.C";
        public string? FilePath { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsDirty { get; set; }
        public int CursorLine { get; set; } = 1;
        public int CursorColumn { get; set; } = 1;

        public string DisplayTitle => IsDirty ? $"*{FileName}" : FileName;

        public bool IsNewFile => FilePath == null;
    }
}
