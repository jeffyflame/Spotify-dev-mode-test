using System;
using System.Collections.Generic;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGlassMusic
{
    public sealed partial class MainPage : Page
    {
        private readonly MediaPlayer _player = new MediaPlayer();
        private readonly List<StorageFile> _queue = new List<StorageFile>();
        private int _currentIndex = -1;

        public MainPage()
        {
            InitializeComponent();
            _player.CommandManager.IsEnabled = true;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;
            _player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            var smtc = _player.SystemMediaTransportControls;
            smtc.IsEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.ButtonPressed += Smtc_ButtonPressed;
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += AcceleratorKeyActivated;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OpenButton.Focus(FocusState.Programmatic);
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary,
                ViewMode = PickerViewMode.List
            };
            foreach (var ext in new[] { ".mp3", ".m4a", ".aac", ".wav", ".wma", ".flac", ".mp4" })
                picker.FileTypeFilter.Add(ext);

            // FileOpenPicker needs the active CoreWindow on UWP.
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            _queue.Clear();
            _queue.Add(file);
            _currentIndex = 0;
            await PlayCurrentAsync();
        }

        private async System.Threading.Tasks.Task PlayCurrentAsync()
        {
            if (_currentIndex < 0 || _currentIndex >= _queue.Count) return;
            var file = _queue[_currentIndex];
            _player.Source = MediaSource.CreateFromStorageFile(file);
            TrackTitle.Text = file.DisplayName;
            TrackArtist.Text = "Local audio · XboxGlassMusic";
            StatusText.Text = "Loading · " + file.Name;
            var props = await file.Properties.GetMusicPropertiesAsync();
            if (!string.IsNullOrWhiteSpace(props.Artist)) TrackArtist.Text = props.Artist;
            _player.Play();
            PlayButton.Content = "Ⅱ";
            StatusText.Text = "Playing · " + file.Name;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                _player.Pause();
                PlayButton.Content = "▶";
            }
            else if (_player.Source != null)
            {
                _player.Play();
                PlayButton.Content = "Ⅱ";
            }
            else
            {
                OpenButton.Focus(FocusState.Programmatic);
                StatusText.Text = "Choose an audio file first.";
            }
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_queue.Count == 0) return;
            _currentIndex = Math.Max(0, _currentIndex - 1);
            await PlayCurrentAsync();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_queue.Count == 0) return;
            _currentIndex = Math.Min(_queue.Count - 1, _currentIndex + 1);
            await PlayCurrentAsync();
        }

        private void SpotifyButton_Click(object sender, RoutedEventArgs e)
        {
            // Metadata/API auth can be added with a developer-owned Spotify app.
            // This link does not claim to start Spotify playback.
            Windows.System.Launcher.LaunchUriAsync(new Uri("https://developer.spotify.com/documentation/web-api"));
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (_currentIndex + 1 < _queue.Count)
                {
                    _currentIndex++;
                    await PlayCurrentAsync();
                }
                else
                {
                    PlayButton.Content = "▶";
                    StatusText.Text = "Playback finished.";
                }
            });
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                StatusText.Text = "Playback error: " + args.ErrorMessage;
            });
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayButton.Content = sender.PlaybackState == MediaPlaybackState.Playing ? "Ⅱ" : "▶";
                if (sender.NaturalDuration.TotalSeconds > 0)
                {
                    ProgressSlider.Maximum = sender.NaturalDuration.TotalSeconds;
                    ProgressSlider.Value = sender.Position.TotalSeconds;
                    DurationText.Text = FormatTime(sender.NaturalDuration);
                    ElapsedText.Text = FormatTime(sender.Position);
                }
            });
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play: _player.Play(); break;
                case SystemMediaTransportControlsButton.Pause: _player.Pause(); break;
                case SystemMediaTransportControlsButton.Next: NextButton_Click(this, null); break;
                case SystemMediaTransportControlsButton.Previous: PreviousButton_Click(this, null); break;
            }
        }

        private void AcceleratorKeyActivated(Windows.UI.Core.CoreDispatcher sender, Windows.UI.Core.AcceleratorKeyEventArgs args)
        {
            if (args.EventType != Windows.UI.Core.CoreAcceleratorKeyEventType.KeyDown) return;
            if (args.VirtualKey == VirtualKey.Space && _player.Source != null)
            {
                if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) _player.Pause();
                else _player.Play();
                args.Handled = true;
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            return ((int)time.TotalMinutes).ToString() + ":" + time.Seconds.ToString("D2");
        }
    }
}
