using NUnit.Framework;
using UnityEditor;
namespace Kruty1918.Haptics.Tests
{
    public class HapticsServiceTests
    {
        sealed class Recorder : IHapticsBackend
        {
            public bool IsSupported { get; set; } = true;
            public int Plays, Cancels, Disposals;
            public void Play(HapticCue cue) => Plays++;
            public void Cancel() => Cancels++;
            public void Dispose() => Disposals++;
        }
        [Test] public void CooldownDropsRepeatsButResultOverridesTick()
        {
            var device = new Recorder(); double now = 10;
            using (var service = new HapticsService(device, () => now))
            {
                Assert.IsTrue(service.Play(HapticCue.Selection));
                Assert.IsFalse(service.Play(HapticCue.Selection));
                Assert.IsTrue(service.Play(HapticCue.Confirm));
                Assert.IsTrue(service.Play(HapticCue.Success));
                Assert.IsFalse(service.Play(HapticCue.Success));
                now += .5;
                Assert.IsTrue(service.Play(HapticCue.Selection));
                Assert.AreEqual(4, device.Plays);
            }
        }
        [Test] public void DisableSuspendAndDisposeCancelAndPreventPlayback()
        {
            var device = new Recorder();
            var service = new HapticsService(device, () => 1);
            service.Enabled = false;
            Assert.IsFalse(service.Play(HapticCue.Confirm));
            service.Enabled = true; service.Suspended = true;
            Assert.IsFalse(service.Play(HapticCue.Success));
            service.Suspended = false;
            Assert.IsTrue(service.Play(HapticCue.Success));
            service.Dispose(); service.Dispose();
            Assert.IsFalse(service.Play(HapticCue.Success));
            Assert.AreEqual(3, device.Cancels);
            Assert.AreEqual(1, device.Disposals);
        }
        [Test] public void UnsupportedDeviceAndInvalidCueDoNothing()
        {
            var device = new Recorder { IsSupported = false };
            using (var service = new HapticsService(device))
            {
                Assert.IsFalse(service.Play(HapticCue.Warning));
                device.IsSupported = true;
                Assert.IsFalse(service.Play((HapticCue)99));
                Assert.AreEqual(0, device.Plays);
            }
        }

        [Test] public void NativePluginsAreEnabledOnlyForTheirMobilePlatform()
        {
            var android = AssetImporter.GetAtPath(
                "Packages/com.kruty1918.haptics/Runtime/Plugins/Android/Haptics.androidlib") as PluginImporter;
            Assert.IsNotNull(android, "Android library must import as a native plugin.");
            Assert.IsFalse(android.GetCompatibleWithAnyPlatform());
            Assert.IsTrue(android.GetCompatibleWithPlatform(BuildTarget.Android));
            Assert.IsFalse(android.GetCompatibleWithEditor());
            var ios = AssetImporter.GetAtPath(
                "Packages/com.kruty1918.haptics/Runtime/Plugins/iOS/KrutyHaptics.mm") as PluginImporter;
            Assert.IsNotNull(ios);
            Assert.IsFalse(ios.GetCompatibleWithAnyPlatform());
            Assert.IsTrue(ios.GetCompatibleWithPlatform(BuildTarget.iOS));
            Assert.IsFalse(ios.GetCompatibleWithEditor());
        }
    }
}
