#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace AudioCinematicBoost
{
    public static class AudioProcessor
    {
        private const string MarkerText = "SPOTY CINEMATIC AUDIO PROCESSOR";
        private static readonly object FileLock = new object();

        private static string? _cachedInstallDir;
        private static string? _cachedConfigFile;

        public static float BassMultiplier { get; set; } = 1.0f;
        public static float VolumeMultiplier { get; set; } = 1.0f;

        private static string SettingsFilePath
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AudioCinematicBoost");
                try
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
                catch { }
                return Path.Combine(folder, "audio_settings.ini");
            }
        }

        static AudioProcessor()
        {
            LoadSettings();
        }

        public static string InstallDirectory
        {
            get
            {
                if (_cachedInstallDir != null) return _cachedInstallDir;

                try
                {
                    using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\EqualizerAPO");
                    if (key != null)
                    {
                        string? path = key.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            _cachedInstallDir = path!;
                            return _cachedInstallDir;
                        }
                    }
                }
                catch { }

                _cachedInstallDir = @"C:\Program Files\EqualizerAPO";
                return _cachedInstallDir;
            }
        }

        public static string ConfigFile
        {
            get
            {
                if (_cachedConfigFile != null) return _cachedConfigFile;

                try
                {
                    using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\EqualizerAPO");
                    if (key != null)
                    {
                        string? cfgDir = key.GetValue("ConfigPath")?.ToString();
                        if (!string.IsNullOrEmpty(cfgDir) && Directory.Exists(cfgDir))
                        {
                            _cachedConfigFile = Path.Combine(cfgDir!, "config.txt");
                            return _cachedConfigFile;
                        }
                    }
                }
                catch { }

                _cachedConfigFile = Path.Combine(InstallDirectory, "config", "config.txt");
                return _cachedConfigFile;
            }
        }

        // 🔥 Pure Playback Cinematic Config (Speakers & Headphones only)
        public static string GetCinematicConfig()
        {
            // Bass: Sub-bass (40Hz) & Punch bass (120Hz)
            float subBassGain = 20.0f * BassMultiplier; 
            float punchBassGain = 15.0f * BassMultiplier; 

            // Dialogue & Clarity: Reference cinema clarity levels
            const float clarityGain = 12.0f; 
            const float vocalGain = 4.0f; 
            const float sparkle8k = 4.0f;

            // PC Audio Booster (Preamp gain above 100%):
            // Normal 100% (1.0) = -3.0 dB limiter headroom baseline
            // Each 100% increase adds +10.0 dB boost (e.g. 200% = +7.0 dB, 300% = +17.0 dB)
            float extraBoostDb = (VolumeMultiplier - 1.0f) * 10.0f;
            float totalPreamp = -3.0f + extraBoostDb;

            return $@"# --- {MarkerText} ---

# --- PLAYBACK AUDIO (SPEAKERS / HEADPHONES) ---
Stage: post-mix

# 1. Master Volume & Limiter Headroom (PC Audio Booster above 100%)
Preamp: {totalPreamp.ToString("0.0", CultureInfo.InvariantCulture)} dB

# 2. Sub Bass (Low-shelf 40Hz)
Filter: ON LS Fc 40 Hz Gain {subBassGain.ToString("0.0", CultureInfo.InvariantCulture)} dB

# 3. Punch Bass (Peaking 120Hz with Q 1.0)
Filter: ON PK Fc 120 Hz Gain {punchBassGain.ToString("0.0", CultureInfo.InvariantCulture)} dB Q 1.0

# 4. Vocal Protect (Boosts mid-vocals for clarity)
Filter: ON PK Fc 1500 Hz Gain {vocalGain.ToString("0.0", CultureInfo.InvariantCulture)} dB Q 1.0

# 5. Clarity (High-shelf 4000Hz)
Filter: ON HS Fc 4000 Hz Gain {clarityGain.ToString("0.0", CultureInfo.InvariantCulture)} dB

# 6. High Sparkle (8000Hz Sheen)
GraphicEQ: 31 0; 62 0; 125 0; 250 0; 500 0; 1000 0; 2000 0; 4000 0; 8000 {sparkle8k.ToString("0.0", CultureInfo.InvariantCulture)}; 16000 0
";
        }

        // 🔇 FLAT / BYPASS CONFIG (Disables all playback processing)
        public static readonly string FlatConfig = 
@"# --- FLAT / BYPASS ---
Stage: post-mix
Preamp: 0.0 dB
Filter: OFF ALL
GraphicEQ: 31 0; 62 0; 125 0; 250 0; 500 0; 1000 0; 2000 0; 4000 0; 8000 0; 16000 0
";

        public static bool ApplyCinematic()
        {
            SaveSettings();
            return SafeWriteFile(GetCinematicConfig());
        }

        public static bool ApplyFlat()
        {
            return SafeWriteFile(FlatConfig);
        }

        private static bool SafeWriteFile(string content)
        {
            lock (FileLock)
            {
                string targetPath = ConfigFile;
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        string? dir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.WriteAllText(targetPath, content, Encoding.UTF8);
                        return true;
                    }
                    catch (IOException)
                    {
                        // File locked by EqualizerAPO watcher, wait 20ms and retry
                        System.Threading.Thread.Sleep(20);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to write config: {ex.Message}");
                        return false;
                    }
                }
                return false;
            }
        }

        // Accurately checks whether cinematic config is active in config.txt
        public static bool IsCinematicActive()
        {
            try
            {
                string path = ConfigFile;
                if (!File.Exists(path)) return false;
                string content = File.ReadAllText(path);
                return content.Contains(MarkerText);
            }
            catch 
            { 
                return false; 
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string content = $"Bass={BassMultiplier.ToString("0.00", CultureInfo.InvariantCulture)}\nVolume={VolumeMultiplier.ToString("0.00", CultureInfo.InvariantCulture)}";
                File.WriteAllText(SettingsFilePath, content, Encoding.UTF8);
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                string path = SettingsFilePath;
                if (!File.Exists(path)) return;

                foreach (string line in File.ReadAllLines(path))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                        {
                            if (key.Equals("Bass", StringComparison.OrdinalIgnoreCase))
                                BassMultiplier = Math.Max(0f, Math.Min(2.0f, val));
                            else if (key.Equals("Volume", StringComparison.OrdinalIgnoreCase) || key.Equals("VolumeBoost", StringComparison.OrdinalIgnoreCase))
                                VolumeMultiplier = Math.Max(1.0f, Math.Min(3.0f, val));
                            else if (key.Equals("Clarity", StringComparison.OrdinalIgnoreCase))
                                VolumeMultiplier = 1.0f; // Default normal volume for migrated users
                        }
                    }
                }
            }
            catch { }
        }
    }
}