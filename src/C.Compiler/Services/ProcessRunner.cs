using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace C.Compiler.Services
{
    public class ProcessRunner
    {
        public class ProcessResult
        {
            public int ExitCode { get; set; }
            public string StandardOutput { get; set; } = string.Empty;
            public string StandardError { get; set; } = string.Empty;
            public bool Success => ExitCode == 0;
        }

        public async Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, int timeoutMs = 30000)
        {
            var result = new ProcessResult();

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            using var process = new Process { StartInfo = startInfo };

            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) stdoutBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) stderrBuilder.AppendLine(e.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = await Task.Run(() => process.WaitForExit(timeoutMs));

                if (!exited)
                {
                    process.Kill(true);
                    result.ExitCode = -1;
                    result.StandardError = "Compilation timed out.";
                    return result;
                }

                // Ensure async output handlers have completed
                process.WaitForExit();

                result.ExitCode = process.ExitCode;
                result.StandardOutput = stdoutBuilder.ToString();
                result.StandardError = stderrBuilder.ToString();
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.StandardError = $"Failed to run process: {ex.Message}";
            }

            return result;
        }

        public void RunInConsole(string exePath, string? workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{exePath}\" & pause\"",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = workingDirectory ?? System.IO.Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory
            };

            Process.Start(startInfo);
        }
    }
}
