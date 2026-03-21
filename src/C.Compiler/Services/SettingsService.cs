using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using C.Compiler.Models;

namespace C.Compiler.Services
{
    public class SettingsService
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TurboC-IDE");

        private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

        public AppSettings Settings { get; private set; } = new();

        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = await File.ReadAllTextAsync(SettingsFile);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Settings, options);
                await File.WriteAllTextAsync(SettingsFile, json);
            }
            catch
            {
                // Silently fail settings save
            }
        }
    }

    public class AppSettings
    {
        public CompilerSettings Compiler { get; set; } = new();
        public int TabSize { get; set; } = 4;
        public bool AutoIndent { get; set; } = true;
        public string LastDirectory { get; set; } = string.Empty;
    }
}
