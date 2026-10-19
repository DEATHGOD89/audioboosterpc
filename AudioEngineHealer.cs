#nullable enable
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
        private const string MmDevicesBaseKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio";

        public static string ApoInstallDir => AudioProcessor.InstallDirectory;
        public static string ConfigPath => AudioProcessor.ConfigFile;

        public static AudioHealthStatus PerformHealthAudit()
        {
            var status = new AudioHealthStatus();
            string installDir = ApoInstallDir;
            string configPath = ConfigPath;

            status.EqualizerApoInstalled = Directory.Exists(installDir) || File.Exists(Path.Combine(installDir, "EqualizerAPO.dll"));
            status.ConfigFileExists = File.Exists(configPath);

            if (!status.EqualizerApoInstalled)
            {
                status.IsFullyHooked = false;
                status.StatusMessage = "Equalizer APO is not installed on this system.";
                return status;
            }

            int total = 0;
            int hooked = 0;

            // Audit Playback (Render) endpoints exclusively (Speakers / Headphones)
            CheckDeviceTree("Render", ref total, ref hooked);

            status.TotalEndpoints = total;
            status.HookedEndpoints = hooked;

            if (total > 0 && hooked > 0)
            {
                status.IsFullyHooked = true;
                status.StatusMessage = $"Audio engine healthy: Equalizer APO attached ({hooked}/{total} playback devices).";
            }
            else if (total > 0 && hooked == 0)
            {
                status.IsFullyHooked = false;
                status.StatusMessage = "Windows Update unhooked Equalizer APO from playback devices.";
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
                // Explicitly use 64-bit registry view to support both 32-bit and 64-bit runtime execution
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                                       .OpenSubKey($@"{MmDevicesBaseKey}\{direction}");
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
                string installDir = ApoInstallDir;
                string configPath = ConfigPath;

                // 1. Verify directory & config file structure
                if (!Directory.Exists(installDir))
                {
                    log.AppendLine($"ERROR: Equalizer APO directory not found at '{installDir}'.");
                    logDetails = log.ToString();
                    return false;
                }

                string? configDir = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                    log.AppendLine($"Created config directory: {configDir}");
                }

                // Restore config.txt if missing or empty
                if (!File.Exists(configPath) || new FileInfo(configPath).Length == 0)
                {
                    File.WriteAllText(configPath, AudioProcessor.GetCinematicConfig());
                    log.AppendLine("Restored config.txt with Cinematic EQ preset.");
                }
                else
                {
                    log.AppendLine("config.txt is intact and accessible.");
                }

                // 2. Launch Configurator.exe or DeviceSelector.exe if endpoints need re-hooking
                string configuratorPath = Path.Combine(installDir, "Configurator.exe");
                if (!File.Exists(configuratorPath))
                {
                    configuratorPath = Path.Combine(installDir, "DeviceSelector.exe");
                }

                if (File.Exists(configuratorPath))
                {
                    log.AppendLine($"Launching {Path.GetFileName(configuratorPath)} for safe device registration...");
                    var psi = new ProcessStartInfo
                    {
                        FileName = configuratorPath,
                        WorkingDirectory = installDir,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                    log.AppendLine("Equalizer APO Configurator opened. Please select your playback device and click OK.");
                    logDetails = log.ToString();
                    return true;
                }
                else
                {
                    log.AppendLine("Configurator/DeviceSelector executable not found in install directory.");
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
