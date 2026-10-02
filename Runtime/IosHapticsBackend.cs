#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
namespace Kruty1918.Haptics
{
    sealed class IosHapticsBackend : IHapticsBackend
    {
        [DllImport("__Internal")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool KrutyHapticsSupported();
        [DllImport("__Internal")] static extern void KrutyHapticsPlay(int cue);
        public bool IsSupported => KrutyHapticsSupported();
        public void Play(HapticCue cue) => KrutyHapticsPlay((int)cue);
        // UIKit generators issue finite impulses, with no queued or looping vibration.
        public void Cancel() { }
        public void Dispose() { }
    }
}
#endif
