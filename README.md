# 🎧 AudioCinematicBoost (Cinema Playback & Volume Boost Edition)

> **Real-Time Dynamic Cinema Bass & Master Volume Booster for Windows**

AudioCinematicBoost is a high-performance, lightweight Windows System Tray application built in **C# (.NET / WinForms)** that interfaces directly with **Equalizer APO** to deliver deep sub-bass, crystal-clear dialogue, and supercharged volume boost above 100% for quiet movies, games, and streaming audio.

---

## 🌟 Key Features

### 🔊 Speakers & Headphones Audio Enhancement (Stage: Post-Mix)
* **Sub Bass & Punch Bass**: Deep sub-bass boost (40Hz) and punchy mid-bass (120Hz) calibrated for powerful cinematic rumble without muddiness.
* **🔊 PC Audio Booster (Above 100%)**: Boosts overall PC volume above Windows 100% (from 100% up to 300% / +20 dB boost) so dialogue and quiet audio in movies, Netflix, YouTube, and games is loud and crystal clear.
* **Built-in Cinema Clarity & Dialogue Protect**: Maintains reference dialogue clarity (1.5kHz vocal protection, 4kHz high-shelf clarity, and 8kHz high-end sparkle) so speech always cuts through action scenes.
* **Real-Time Smooth Adjustment**: Debounced sliders for instant volume & bass adjustments without audio stutter or disk locking.
* **Persistent Settings**: Bass and Volume Boost preferences are automatically saved in `%LOCALAPPDATA%\AudioCinematicBoost\audio_settings.ini` and restored on startup.

### 🛡️ Windows Update Self-Healing Engine
* **Post-Windows Update Audit**: Scans 64-bit Windows Audio endpoint registry (`MMDevices\Audio\Render`) to verify Equalizer APO sAPO hook integrity.
* **1-Click Repair Audio Engine**: Automatically detects Equalizer APO installation path dynamically from the registry and launches `Configurator.exe` / `DeviceSelector.exe` with administrative elevation if Windows Update ever unhooks your audio drivers.
* **Atomic Safe Writes**: Updates `config.txt` safely with thread-safe file locking and automatic directory creation.

---

## 🛠️ Prerequisites
* **Windows 10 / 11** (64-bit)
* **Equalizer APO** installed — [Download Equalizer APO on SourceForge](https://sourceforge.net/projects/equalizerapo/)

---

## 🚀 Quick Start / Usage

### Running the App
1. Run `AudioCinematicBoost.exe`.
2. The app will launch into your Windows System Tray (near the taskbar clock).
3. **Double-click** the tray icon to quickly toggle Cinematic Audio & Volume Boost **ON / OFF**.
4. **Right-click** the tray icon to:
   * **Toggle Cinematic Audio (Active / Disabled)**
   * **Adjust Bass & Volume Boost...** (Customize Sub/Punch Bass and PC Volume Boost sliders)
   * **🔧 Repair Audio Engine (Post-Windows Update)**
   * **Exit**

### Command-Line Arguments
* `AudioCinematicBoost.exe /start` — Enables Cinematic Audio & Volume Boost (launches in tray, or activates existing instance)
* `AudioCinematicBoost.exe /stop` — Disables audio boost (reverts to flat bypass) and exits cleanly

---

## 🔨 Building from Source
To rebuild `AudioCinematicBoost.exe` at any time:
* Double-click or run `build.bat` in the terminal.
* The build script automatically locates the Roslyn C# compiler / Visual Studio Build Tools / MSBuild and compiles an optimized, standalone executable.

---

## 📜 License
MIT License. Free to use, modify, and distribute.
