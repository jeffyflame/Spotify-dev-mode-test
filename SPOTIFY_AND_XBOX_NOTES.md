# Spotify and Xbox platform verification

## Spotify is not an arbitrary audio CDN

Do not implement scraping, stream URL extraction, DRM bypass, or private endpoints. Spotify's public Web API is useful for permitted metadata and account functions; it is not a general audio streaming API for a third-party custom player. Spotify playback control endpoints are subject to current eligibility, account tier, device support, and developer policy. A custom UWP app does not automatically become a Spotify Connect receiver.

Recommended next step: create a Spotify developer app, review current policy, and implement Authorization Code with PKCE only for a clearly defined permitted use case. Use a client ID; do not embed a client secret. Do not treat OAuth success as audio playback support.

## Background audio

This project uses `Windows.Media.Playback.MediaPlayer` and exposes `SystemMediaTransportControls`. These are standard Windows media APIs. However, Xbox's lifecycle, supported API surface, and Dev Mode app behavior can vary by console OS and SDK. Leaving the app, launching a game, suspending, or terminating it may stop playback. This code intentionally makes no guarantee that music continues while another game is active.

For actual background playback, verify the supported Xbox UWP background audio model for the precise target SDK and console build. Do not blindly add desktop-only APIs or assume a desktop MSIX background task works on Xbox.

## Device test checklist

- Does the app package deploy and launch on the target console?
- Can the file picker access a test audio file?
- Does playback survive opening the Xbox guide?
- Does playback survive switching to another app?
- Does playback survive launching a game?
- Do hardware / Xbox media controls appear and work?
- What happens after suspension, resume, and app termination?
- Does the target OS accept the selected architecture and manifest capabilities?
