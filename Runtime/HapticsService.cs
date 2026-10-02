using System;
using UnityEngine;

namespace Kruty1918.Haptics
{
    /// <summary>Main-thread service. Finite cues only; never queues input feedback.</summary>
    public sealed class HapticsService : IDisposable
    {
        readonly IHapticsBackend _backend;
        readonly Func<double> _clock;
        double _nextAllowed;
        HapticCue _lastCue;
        bool _enabled = true, _suspended, _disposed, _hasPlayed;

        public HapticsService(IHapticsBackend backend = null, Func<double> clock = null)
        {
            _backend = backend ?? CreateBackend();
            _clock = clock ?? (() => Time.realtimeSinceStartupAsDouble);
        }

        public bool IsSupported => !_disposed && _backend.IsSupported;
        public bool Enabled
        {
            get => _enabled;
            set { if (_enabled == value) return; _enabled = value; if (!value) Cancel(); }
        }
        public bool Suspended
        {
            get => _suspended;
            set { if (_suspended == value) return; _suspended = value; if (value) Cancel(); }
        }

        /// <returns>True only if a cue reached the device backend.</returns>
        public bool Play(HapticCue cue)
        {
            if (_disposed || !_enabled || _suspended || !_backend.IsSupported) return false;
            if (cue < HapticCue.Selection || cue > HapticCue.Success) return false;
            var now = _clock();
            // Result feedback can replace a recent UI tick; repeated results cannot spam.
            if (_hasPlayed && now < _nextAllowed && Priority(cue) <= Priority(_lastCue)) return false;
            _backend.Play(cue);
            if (!_backend.IsSupported) return false;
            _lastCue = cue;
            _hasPlayed = true;
            _nextAllowed = now + (cue == HapticCue.Success ? .45 : cue == HapticCue.Warning ? .3 : .08);
            return true;
        }

        static int Priority(HapticCue cue) => cue == HapticCue.Success ? 3
            : cue == HapticCue.Warning ? 2 : cue == HapticCue.Confirm ? 1 : 0;

        public void Cancel()
        {
            if (_disposed) return;
            _backend.Cancel();
            _hasPlayed = false;
            _nextAllowed = 0;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Cancel();
            _disposed = true;
            _backend.Dispose();
        }

        static IHapticsBackend CreateBackend()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidHapticsBackend();
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosHapticsBackend();
#else
            return new NoHapticsBackend();
#endif
        }
    }

    sealed class NoHapticsBackend : IHapticsBackend
    {
        public bool IsSupported => false;
        public void Play(HapticCue cue) { }
        public void Cancel() { }
        public void Dispose() { }
    }
}
