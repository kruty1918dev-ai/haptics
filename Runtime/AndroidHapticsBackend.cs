#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace Kruty1918.Haptics
{
    sealed class AndroidHapticsBackend : IHapticsBackend
    {
        AndroidJavaObject _vibrator;
        int _sdk;
        bool _supported;

        public AndroidHapticsBackend()
        {
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    _sdk = version.GetStatic<int>("SDK_INT");
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (_sdk >= 31)
                    {
                        using (var manager = activity.Call<AndroidJavaObject>("getSystemService", "vibrator_manager"))
                            if (manager != null) _vibrator = manager.Call<AndroidJavaObject>("getDefaultVibrator");
                    }
                    else _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                _supported = _sdk >= 26 && _vibrator != null && _vibrator.Call<bool>("hasVibrator");
            }
            catch (Exception e) { Disable(e); }
        }

        public bool IsSupported => _supported;
        public void Play(HapticCue cue)
        {
            if (!_supported) return;
            try
            {
                using (var effects = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    // Platform presets respect device tuning on Android 10+.
                    if (_sdk >= 29 && cue != HapticCue.Warning)
                    {
                        var key = cue == HapticCue.Selection ? "EFFECT_TICK"
                            : cue == HapticCue.Success ? "EFFECT_DOUBLE_CLICK" : "EFFECT_CLICK";
                        using (var effect = effects.CallStatic<AndroidJavaObject>("createPredefined", effects.GetStatic<int>(key)))
                            _vibrator.Call("vibrate", effect);
                    }
                    else if (cue == HapticCue.Success || cue == HapticCue.Warning)
                    {
                        long[] times = cue == HapticCue.Success ? new long[] { 0, 18, 55, 24 }
                            : new long[] { 0, 12, 45, 12 };
                        int[] amplitudes = { 0, 70, 0, 90 };
                        using (var effect = effects.CallStatic<AndroidJavaObject>("createWaveform", times, amplitudes, -1))
                            _vibrator.Call("vibrate", effect);
                    }
                    else
                    {
                        long duration = cue == HapticCue.Selection ? 8L : cue == HapticCue.Light ? 12L : 22L;
                        int amplitude = cue == HapticCue.Confirm ? 110 : 65;
                        using (var effect = effects.CallStatic<AndroidJavaObject>("createOneShot", duration, amplitude))
                            _vibrator.Call("vibrate", effect);
                    }
                }
            }
            catch (Exception e) { Disable(e); }
        }
        public void Cancel()
        {
            if (!_supported) return;
            try { _vibrator.Call("cancel"); } catch (Exception e) { Disable(e); }
        }
        void Disable(Exception e)
        {
            _supported = false;
            Debug.LogWarning("[Haptics] Device backend unavailable: " + e.Message);
        }
        public void Dispose() { _vibrator?.Dispose(); _vibrator = null; _supported = false; }
    }
}
#endif
