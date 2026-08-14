# 🎧 AudioCinematicBoost v2.0 (Future-Proof & Self-Healing Edition)

> **Real-Time Dynamic Cinema Audio & Professional Microphone Enhancement Engine for Windows**

AudioCinematicBoost is a lightweight Windows System Tray application built in **C# (.NET / WinForms)** that interfaces directly with **EqualizerAPO** to deliver cinema-grade sound and studio-quality microphone voice processing.

---

## 🌟 Key Features

### 🔊 Speakers & Headphones Audio Enhancement
* **Sub Bass & Punch Bass**: Deep sub-bass boost (40Hz) and punchy mid-bass (120Hz).
* **Vocal Protection & Clarity**: High-shelf clarity at 4kHz & vocal warmth at 1.5kHz.
* **8kHz Sparkle**: High-end sheen for dynamic music, gaming, and movie immersion.
* **Limiter Headroom**: Built-in limiter to prevent digital clipping & distortion.

### 🎙️ Microphone Voice Processing & Noise Cleanup
* **Preamp Sensitivity Boost**: +12 dB gain boost so soft speech is captured effortlessly on Discord/Zoom.
* **Electrical Ground Hum Filter**: High-pass filter at 75Hz to eliminate 50Hz/60Hz mains hum.
* **Notch Filters**: Notch cuts at 120Hz & 180Hz to eliminate power supply electrical current buzz.
* **USB Static & Coil Whine Filter**: High-shelf cut at 9000Hz to eliminate electronic current hiss.
* **Vocal Clarity & Energy**: Boosts core speech frequencies (1.5kHz & 3kHz) so your voice cuts through background noise.

### 🛡️ Windows Update Self-Healing Engine
* **Post-Windows Update Healing**: Automatically detects when a Windows Update unhooks Equalizer APO from your sound devices.
* **Device Selector Auto-Launcher**: Includes a **1-Click Repair Audio Engine** feature that launches `DeviceSelector.exe` / `Configurator.exe` with proper environment paths for instant re-hooking.
* **Safe Real-Time Presets**: Updates `config.txt` dynamically without killing audio services or crashing Spotify.

---

## 🛠️ Prerequisites
* **Windows 10 / 11** (64-bit)
* **Equalizer APO** — [Download Equalizer APO on SourceForge](https://sourceforge.net/projects/equalizerapo/)

---

## 🚀 Quick Start / Usage

1. Clone or download this repository.
2. Run `AudioCinematicBoost.exe` as Administrator.
3. The app will launch in your System Tray (near the Windows clock).
4. Right-click the tray icon to:
   * **Toggle Cinematic Audio ON / OFF**
   * **Adjust EQ Settings...** (Customize Bass, Sound Clarity, and Microphone Gain trackbars)
   * **🔧 Repair Audio Engine (Post-Windows Update)**

---

## 📜 License
MIT License. Free to use, modify, and distribute.
