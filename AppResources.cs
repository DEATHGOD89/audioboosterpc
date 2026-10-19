#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    public static class AppResources
    {
        private static Icon? _cachedIcon;
        private static Image? _cachedLogo;

        public static Icon AppIcon
        {
            get
            {
                if (_cachedIcon != null) return _cachedIcon;

                try
                {
                    // 1. Load from embedded manifest resource
                    var asm = Assembly.GetExecutingAssembly();
                    using Stream? stream = asm.GetManifestResourceStream("app.ico");
                    if (stream != null)
                    {
                        _cachedIcon = new Icon(stream);
                        return _cachedIcon;
                    }
                }
                catch { }

                try
                {
                    // 2. Load from disk if running in development directory
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] candidates = {
                        Path.Combine(exeDir, "app.ico"),
                        Path.Combine(exeDir, "assets", "app.ico")
                    };
                    foreach (var path in candidates)
                    {
                        if (File.Exists(path))
                        {
                            _cachedIcon = new Icon(path);
                            return _cachedIcon;
                        }
                    }
                }
                catch { }

                try
                {
                    // 3. Fallback: extract associated icon from current executable
                    Icon? assoc = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                    if (assoc != null)
                    {
                        _cachedIcon = assoc;
                        return _cachedIcon;
                    }
                }
                catch { }

                // 4. Default fallback
                _cachedIcon = SystemIcons.Application;
                return _cachedIcon;
            }
        }

        public static Image LogoImage
        {
            get
            {
                if (_cachedLogo != null) return _cachedLogo;

                try
                {
                    // 1. Load from embedded manifest resource
                    var asm = Assembly.GetExecutingAssembly();
                    using Stream? stream = asm.GetManifestResourceStream("logo.png");
                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        ms.Position = 0;
                        _cachedLogo = Image.FromStream(ms);
                        return _cachedLogo;
                    }
                }
                catch { }

                try
                {
                    // 2. Load from disk
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] candidates = {
                        Path.Combine(exeDir, "assets", "logo.png"),
                        Path.Combine(exeDir, "logo.png")
                    };
                    foreach (var path in candidates)
                    {
                        if (File.Exists(path))
                        {
                            _cachedLogo = Image.FromFile(path);
                            return _cachedLogo;
                        }
                    }
                }
                catch { }

                // 3. Fallback: convert AppIcon to Bitmap
                _cachedLogo = AppIcon.ToBitmap();
                return _cachedLogo;
            }
        }
    }
}
