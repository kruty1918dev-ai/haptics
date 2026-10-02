# Kruty1918 Haptics

Standalone Unity UPM package, with no Quiet Camp dependency. Android API 26+
uses VibrationEffect (device presets on API 29+, VibratorManager on API 31+).
iOS uses UIKit feedback generators. Editor, desktop and WebGL are silent no-ops.
Call the service on Unity's main thread. iOS hardware availability is best-effort
(iPhone model and user settings can suppress feedback).

```csharp
using Kruty1918.Haptics;
var haptics = new HapticsService();
haptics.Enabled = savedPreference;
haptics.Play(HapticCue.Confirm);
// Forward app pause/focus changes; cancel on scene transitions as appropriate.
haptics.Suspended = true;
haptics.Dispose();
```

Cues: Selection, Light, Confirm, Warning, Success. Feedback is finite, never queued,
and rate-limited (80 ms for taps, 300 ms warning, 450 ms success). Higher priority
results can replace recent taps. Disabling, suspending and disposing cancel vibration.
Inject IHapticsBackend and an optional clock for tests. Android VIBRATE permission
is merged from the included .androidlib; no runtime permission prompt is needed.

Install in Packages/manifest.json:

```json
"com.kruty1918.haptics": "https://github.com/kruty1918dev-ai/haptics.git#v1.0.0"
```

Add `Kruty1918.Haptics` to your assembly references. To run package tests, add
`com.kruty1918.haptics` to the project's `testables`, then run EditMode tests.

MIT license. Native Android reference:
https://developer.android.com/develop/ui/views/haptics/haptics-apis
