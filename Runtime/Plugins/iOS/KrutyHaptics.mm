#import <UIKit/UIKit.h>
extern "C" bool KrutyHapticsSupported() {
    // iPhone hardware supports UIKit tactile feedback; iPads do not.
    return UIDevice.currentDevice.userInterfaceIdiom == UIUserInterfaceIdiomPhone;
}
extern "C" void KrutyHapticsPlay(int cue) {
    if (!KrutyHapticsSupported()) return;
    if (cue == 0) {
        UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
        [generator prepare]; [generator selectionChanged];
    } else if (cue == 3 || cue == 4) {
        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator prepare];
        [generator notificationOccurred:cue == 4 ? UINotificationFeedbackTypeSuccess : UINotificationFeedbackTypeWarning];
    } else {
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:
            cue == 1 ? UIImpactFeedbackStyleLight : UIImpactFeedbackStyleMedium];
        [generator prepare]; [generator impactOccurred];
    }
}
