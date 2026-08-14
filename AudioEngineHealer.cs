using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AudioCinematicBoost
{
    public class AudioHealthStatus
    {
        public bool IsFullyHooked { get; set; }
        public int TotalEndpoints { get; set; }
        public int HookedEndpoints { get; set; }
        public bool ConfigFileExists { get; set; }
        public bool EqualizerApoInstalled { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }

    public static class AudioEngineHealer
    {
        public const string ApoClsid = "{EC1CC9CE-FAED-4822-828A-82A81A6F018F}";
        public static readonly string ApoInstallDir = @"C:\Program Files\EqualizerAPO";
        public static readonly string ConfigPath = @"C:\Program Files\EqualizerAPO\config\config.txt";

        private const string MmDevicesBaseKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio";

        public static AudioHealthStatus PerformHealthAudit()
        {
            var status = new AudioHealthStatus();
            status.EqualizerApoInstalled = Directory.Exists(ApoInstallDir) || File.Exists(Path.Combine(ApoInstallDir, "EqualizerAPO.dll"));
            status.ConfigFileExists = File.Exists(ConfigPath);

            if (!status.EqualizerApoInstalled)
            {
                status.IsFullyHooked = false;
                status.StatusMessage = "Equalizer APO is not installed on this system.";
                return status;
            }

            int total = 0;
            int hooked = 0;

            CheckDeviceTree("Render", ref total, ref hooked);
            CheckDeviceTree("Capture", ref total, ref hooked);

            status.TotalEndpoints = total;
            status.HookedEndpoints = hooked;

            if (total > 0 && hooked > 0)
            {
                status.IsFullyHooked = true;
                status.StatusMessage = $"Audio engine healthy: Equalizer APO attached ({hooked}/{total} devices).";
            }
            else if (total > 0 && hooked == 0)
            {
                status.IsFullyHooked = false;
                status.StatusMessage = "Windows Update unhooked Equalizer APO from audio devices.";
            }
            else
            {
                status.IsFullyHooked = status.ConfigFileExists;
                status.StatusMessage = status.ConfigFileExists ? "Audio configuration file active." : "Configuration file missing.";
            }

            return status;
        }

        private static void CheckDeviceTree(string direction, ref int total, ref int hooked)
        {
            try
            {
                using RegistryKey? baseKey = Registry.LocalMachine.OpenSubKey($@"{MmDevicesBaseKey}\{direction}");
                if (baseKey == null) return;

                foreach (string deviceGuid in baseKey.GetSubKeyNames())
                {
                    using RegistryKey? fxKey = baseKey.OpenSubKey($@"{deviceGuid}\FxProperties");
                    if (fxKey != null)
                    {
                        total++;
                        bool isDeviceHooked = false;

                        foreach (string valName in fxKey.GetValueNames())
                        {
                            string? val = fxKey.GetValue(valName)?.ToString();
                            if (val != null && val.IndexOf(ApoClsid, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isDeviceHooked = true;
                                break;
                            }
                        }

                        if (isDeviceHooked)
                        {
                            hooked++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning registry: {ex.Message}");
            }
        }

        public static bool RepairAudioEngine(out string logDetails)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine($"--- Audio Engine Diagnostics Started at {DateTime.Now} ---");

            try
            {
                // 1. Verify directory & config file structure
                if (!Directory.Exists(ApoInstallDir))
                {
                    log.AppendLine($"ERROR: Equalizer APO directory not found at '{ApoInstallDir}'.");
                    logDetails = log.ToString();
                    return false;
                }

                string configDir = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                    log.AppendLine($"Created config directory: {configDir}");
                }

                // Restore config.txt if missing
                if (!File.Exists(ConfigPath) || new FileInfo(ConfigPath).Length == 0)
                {
                    File.WriteAllText(ConfigPath, AudioProcessor.GetCinematicConfig());
                    log.AppendLine("Restored config.txt with Cinematic EQ preset.");
                }
                else
                {
                    log.AppendLine("config.txt is intact and accessible.");
                }

                // 2. Launch Configurator.exe or DeviceSelector.exe if endpoints are completely unhooked
                string configuratorPath = Path.Combine(ApoInstallDir, "Configurator.exe");
                if (!File.Exists(configuratorPath))
                {
                    configuratorPath = Path.Combine(ApoInstallDir, "DeviceSelector.exe");
                }

                if (File.Exists(configuratorPath))
                {
                    log.AppendLine($"Launching {Path.GetFileName(configuratorPath)} for safe device registration...");
                    var psi = new ProcessStartInfo
                    {
                        FileName = configuratorPath,
                        WorkingDirectory = ApoInstallDir,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                    log.AppendLine("Device Selector opened. Please select your audio output/mic and click OK to enable audio enhancement.");
                    logDetails = log.ToString();
                    return true;
                }
                else
                {
                    log.AppendLine("Configurator/DeviceSelector executable not found.");
                    logDetails = log.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.AppendLine($"ERROR during repair: {ex.Message}");
                logDetails = log.ToString();
                return false;
            }
        }
    }
}
