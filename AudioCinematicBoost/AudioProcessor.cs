using System;
using System.IO;
using System.Text;

namespace AudioCinematicBoost
{
    public static class AudioProcessor
    {
        // 👇 EQUALIZERAPO DEFAULT CONFIG PATH
        private static readonly string ConfigFile = @"C:\Program Files\EqualizerAPO\config\config.txt";

        public static float BassMultiplier { get; set; } = 1.0f;
        public static float ClarityMultiplier { get; set; } = 1.0f;

        // 🔥 YOUR PERFECT CINEMATIC CONFIG (Dynamic)
        public static string GetCinematicConfig()
        {
            // 40 Hz - Sub Bass
            float subBassGain = 20.0f * BassMultiplier; 
            // 120 Hz - Punch Bass
            float punchBassGain = 15.0f * BassMultiplier; 

            // 4000 Hz - Clarity (Increased to 12 dB as requested)
            float clarityGain = 12.0f * ClarityMultiplier; 
            // 1500 Hz - Vocal Protect (Increased to 4 dB as requested)
            float vocalGain = 4.0f * ClarityMultiplier; 

            // 8000 Hz - High-end sparkle (Set to 4 as requested)
            float sparkle8k = 4.0f * ClarityMultiplier;

            return $@"# --- SPOTY CINEMATIC AUDIO PROCESSOR (PERFECT MATCH) ---

# 1. Limiter (Reduced cut to increase overall loudness)
Preamp: -3.0 dB

# 2. Sub Bass (Matches your 'lowshelf' 40Hz with Max Multiplier)
Filter: ON LS Fc 40 Hz Gain {subBassGain.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} dB

# 3. Punch Bass (Matches your 'peaking' 120Hz with Q 1.0)
Filter: ON PK Fc 120 Hz Gain {punchBassGain.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} dB Q 1.0

# 4. Vocal Protect (Boosts mid-vocals for clarity)
Filter: ON PK Fc 1500 Hz Gain {vocalGain.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} dB Q 1.0

# 5. Clarity (Matches your 'highshelf' 4000Hz)
Filter: ON HS Fc 4000 Hz Gain {clarityGain.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} dB

# 6. 10-Band EQ (Added 8000Hz sparkle for extra clarity)
GraphicEQ: 31 0; 62 0; 125 0; 250 0; 500 0; 1000 0; 2000 0; 4000 0; 8000 {sparkle8k.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}; 16000 0";
        }

        // 🔇 FLAT/BYPASS CONFIG (disables all processing)
        public static readonly string FlatConfig = 
@"# --- FLAT / BYPASS ---
Preamp: 0.0 dB

# Turn off all filters
Filter: OFF ALL

# Flat 10-Band EQ
GraphicEQ: 31 0; 62 0; 125 0; 250 0; 500 0; 1000 0; 2000 0; 4000 0; 8000 0; 16000 0";

        public static bool ApplyCinematic()
        {
            try
            {
                // Ensure the config directory exists
                string? dir = Path.GetDirectoryName(ConfigFile);
                if (dir != null && !Directory.Exists(dir))
                    return false;

                File.WriteAllText(ConfigFile, GetCinematicConfig(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply Cinematic: {ex.Message}");
                return false;
            }
        }

        public static bool ApplyFlat()
        {
            try
            {
                File.WriteAllText(ConfigFile, FlatConfig, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply Flat: {ex.Message}");
                return false;
            }
        }

        // Checks if the current config matches the Cinematic one (simple heuristic)
        public static bool IsCinematicActive()
        {
            try
            {
                if (!File.Exists(ConfigFile)) return false;
                string content = File.ReadAllText(ConfigFile);
                return content.Contains("SPOTY CINEMATIC AUDIO PROCESSOR");
            }
            catch { return false; }
        }
    }
}