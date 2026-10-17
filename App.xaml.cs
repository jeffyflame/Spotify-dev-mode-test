using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace XboxGlassMusic
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            EnteredBackground += OnEnteredBackground;
            LeavingBackground += OnLeavingBackground;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
            if (rootFrame == null)
            {
                rootFrame = new Windows.UI.Xaml.Controls.Frame();
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null) rootFrame.Navigate(typeof(MainPage), e.Arguments);
            Window.Current.Activate();
        }

        private bool _isInBackgroundMode;

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            _isInBackgroundMode = true;
            // Keep this handler lightweight. The backgroundMediaPlayback manifest capability
            // allows active MediaPlayer audio to continue while the app is not foregrounded.
        }

        private void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            _isInBackgroundMode = false;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            // Background media playback is enabled by the manifest capability. Do not pause
            // the MediaPlayer here; doing so would stop playback when the app is backgrounded.
        }
    }
}
