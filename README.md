# XboxGlassMusic

A native UWP / C# / XAML starter for Xbox One / Xbox Series consoles running Developer Mode. The interface uses a dark, layered "Liquid Glass" visual language and is designed for controller navigation.

## Current scope

- Native UWP project targeting the Universal Windows Platform.
- Controller-friendly navigation and focus visuals.
- Liquid-glass-inspired translucent cards, gradient backdrop, large album-art placeholder, transport controls.
- Local file selection via `FileOpenPicker` (choose audio files from locations exposed to the app).
- `MediaPlayer` + `MediaPlaybackList` playback plumbing and `SystemMediaTransportControls` integration.
- Spotify integration notes and explicit boundaries.

## Important limitations

This is a **source project**, not a prebuilt `.appx`/`.msix`. A real Xbox package must be built and signed in Visual Studio with the Windows/Xbox SDK tooling and then tested on the target console. This environment cannot compile or validate an Xbox package.

**Spotify:** Spotify's Web API is primarily for metadata and account features. It does not provide arbitrary raw audio streams for a custom player. Spotify Connect playback control generally requires Spotify Premium and a compatible Spotify Connect device; it does not turn a custom UWP app into a Spotify playback endpoint. Spotify does not provide a generally supported Xbox Dev Mode UWP playback SDK. This starter therefore does not scrape Spotify, extract protected streams, or pretend Spotify playback is implemented. See [Spotify developer documentation](https://developer.spotify.com/documentation/web-api) and [Spotify developer terms](https://developer.spotify.com/terms).

**Background playback:** UWP audio/background execution behavior differs by Windows build and Xbox console environment. `MediaPlayer` and `SystemMediaTransportControls` provide Windows media plumbing, but continued playback after leaving this app is **not guaranteed on Xbox** by this starter. Verify on the target OS build. If Xbox suspends or terminates the app, the player cannot keep playing. This is not a replacement for the official Spotify Xbox app.

## Build in Visual Studio

1. On a Windows PC, install Visual Studio 2022 with **Universal Windows Platform development** and the Windows 10/11 SDK components available to your setup.
2. Open `XboxGlassMusic.sln`.
3. Set configuration to `Release` and target `x64` for a typical Xbox console target. Select the appropriate Windows SDK and minimum version installed.
4. Restore NuGet packages if prompted.
5. Build the `XboxGlassMusic` project.
6. Use the package creation/publishing wizard to create a package. For a console Dev Mode deployment, use the packaging/signing options appropriate to your Xbox developer environment.
7. Enable Xbox Developer Mode, connect to the Device Portal shown on the console, and deploy the generated package using the supported deployment workflow.
8. Test controller navigation, file access, playback, media controls, suspension/resume, and playback after switching to another app/game.

The exact packaging options depend on your Visual Studio and Xbox SDK versions. If the project template does not offer an Xbox-compatible target on your machine, install the supported Xbox extensions/tooling for your console workflow; don't assume a generic desktop MSIX will run on Xbox.

## Local audio

Use the **Open audio** button and choose a file. The picker is configured for a broad set of common audio extensions. Xbox file access and picker locations vary by OS and app capability; test with files accessible to the app.

## Spotify integration roadmap

A legitimate metadata/account integration could use Spotify Authorization Code with PKCE and the Web API, subject to Spotify app settings and current developer policy. Add a client ID and a registered redirect URI only after creating your own Spotify developer app. Never embed a client secret in a client app. This metadata/auth flow is not audio streaming. For playback, use only a Spotify-supported playback path/device/API permitted for your account and platform. Do not ship credentials in source control.

## Project structure

- `MainPage.xaml` — Liquid Glass UI
- `MainPage.xaml.cs` — controller-friendly commands and local audio playback
- `App.xaml` / `App.xaml.cs` — UWP entry point
- `Package.appxmanifest` — package identity and media/library capabilities
- `XboxGlassMusic.csproj` — UWP project


## Build automatically with GitHub Actions

The workflow at `.github/workflows/build-appx.yml` builds an x64 UWP package on a Windows GitHub-hosted runner when you push code to `main` or manually run the workflow.

1. Create a GitHub repository named `XboxGlassMusic` under your account.
2. Upload/push the contents of this folder, including `.github/workflows/build-appx.yml`, to the repository's `main` branch.
3. Open the repository's **Actions** tab and select **Build XboxGlassMusic package**.
4. Choose **Run workflow**. Wait for the job to finish.
5. Download the `XboxGlassMusic-x64-appx` artifact from the successful run. It contains the generated package and `XboxGlassMusic.cer`.

The workflow creates a fresh temporary signing certificate every run. The `.cer` is public certificate material; the PFX/private key is not uploaded. Since the certificate changes on every run, use the matching certificate from the same artifact as the package. Dev Mode package trust and installation steps vary with the console OS/Device Portal version; import/trust the certificate only using the supported Xbox Dev Mode workflow for your device.

**Important:** this is a CI build pipeline, not proof the package will install or run on every Xbox OS. GitHub-hosted Windows images may not include the precise Xbox-specific SDK/extensions. If the build fails on SDK/targeting-pack or manifest errors, inspect the Actions log and use a runner with the supported Xbox tooling. The workflow does not bypass platform requirements.

## Spotify and background audio status

The UI currently links to Spotify's official Web API documentation; it does **not** authenticate to Spotify or stream Spotify tracks. Spotify Web API access is not a general-purpose audio-streaming entitlement. A supported Spotify playback integration needs to be designed around Spotify's current developer rules and compatible playback devices; this UWP app is not a Spotify Connect receiver.

The player uses `MediaPlayer` and `SystemMediaTransportControls`, but continuous audio while another game runs is **not yet guaranteed or implemented as a verified Xbox background-audio solution**. Validate the Xbox-supported background task/audio model against the actual target console OS and SDK before adding a task extension. The workflow packages the current source; it cannot grant unsupported Spotify playback or Xbox background privileges.
