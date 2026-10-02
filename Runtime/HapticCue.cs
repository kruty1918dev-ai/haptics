namespace Kruty1918.Haptics
{
    public enum HapticCue { Selection, Light, Confirm, Warning, Success }

    /// <summary>Implement this boundary to inject a device or a recording test backend.</summary>
    public interface IHapticsBackend : System.IDisposable
    {
        bool IsSupported { get; }
        void Play(HapticCue cue);
        void Cancel();
    }
}
