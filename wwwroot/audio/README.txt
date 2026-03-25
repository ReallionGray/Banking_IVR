Place prerecorded prompt audio files here for Twilio <Play> fallback.

Expected structure:
wwwroot/audio/pidgin/welcome.aiff
wwwroot/audio/pidgin/menu.aiff
wwwroot/audio/pidgin/enter-recipient.aiff
wwwroot/audio/pidgin/invalid-account.aiff
wwwroot/audio/pidgin/enter-amount.aiff
wwwroot/audio/pidgin/enter-pin.aiff
wwwroot/audio/pidgin/transfer-cancelled.aiff
wwwroot/audio/pidgin/invalid-selection.aiff
wwwroot/audio/pidgin/invalid-pin.aiff
wwwroot/audio/pidgin/thank-you.aiff
wwwroot/audio/pidgin/missing-phone.aiff

Repeat the same filenames under:
wwwroot/audio/yo/
wwwroot/audio/ig/
wwwroot/audio/ha/

When a file exists, the IVR will prefer <Play> for that prompt instead of Twilio <Say>.
Twilio supports AIFF playback, so `.aiff` is used here for direct generation on macOS.
