namespace C.Compiler.Models
{
    public class CompilerError
    {
        public string FilePath { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public CompilerErrorSeverity Severity { get; set; }

        public override string ToString()
        {
            string prefix = Severity == CompilerErrorSeverity.Error ? "Error" : "Warning";
            return $"{System.IO.Path.GetFileName(FilePath)} {Line}: {prefix} - {Message}";
        }
    }

    public enum CompilerErrorSeverity
    {
        Warning,
        Error
    }
}
