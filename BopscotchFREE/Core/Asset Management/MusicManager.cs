using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

namespace Leda.Core.Asset_Management
{
    public sealed class MusicManager
    {
        private static MusicManager _instance = null;
        private static MusicManager Instance { get { if (_instance == null) { _instance = new MusicManager(); } return _instance; } }

        public static bool Initialized { get { return Instance.InstanceInitialized; } }
        public static bool Available { get { return Instance.CheckMediaPlayerIsAvailable(); } }
        public static bool Muted { get { return Instance.InstanceMuted; } set { Instance.InstanceSetMutedState(value); } }
        public static void Initialize() { Instance.AttemptInitialization(); }
        public static void AddTune(string tuneName, Song tuneToAdd) { Instance.InstanceAddTune(tuneName, tuneToAdd); }
        public static void PlayMusic(string tuneName) { Instance.InstancePlayTune(tuneName, false); }
        public static void PlayLoopedMusic(string tuneName) { Instance.InstancePlayTune(tuneName, true); }
        public static void StopMusic() { Instance.InstanceStopTune(); }

        public static void HandleGameObscured() { Instance.InstanceSetPausedState(true); }
        public static void HandleGameUnobscured() { Instance.InstanceSetPausedState(false); }

        private Dictionary<string, Song> _tunes = null;
        private string _lastSongName;
        private bool _lastSongWasLooped;
        private bool InstanceInitialized { get; set; }
        private bool InstanceMuted { get; set; }

        private MusicManager()
        {
            _tunes = new Dictionary<string, Song>();
            InstanceInitialized = false;
            InstanceMuted = false;
        }

        private void AttemptInitialization()
        {
            //try { MediaPlayer.IsMuted = false; InstanceInitialized = true; }
            //catch (Exception ex) { InstanceInitialized = false; }

            InstanceInitialized = true;
        }

        private bool CheckMediaPlayerIsAvailable()
        {
            if (InstanceInitialized) { return XnaMediaPlayer.GameHasControl; }

            return false;
        }

        private void InstanceAddTune(string tuneName, Song tuneToAdd)
        {
            if (!_tunes.ContainsKey(tuneName)) { _tunes.Add(tuneName, tuneToAdd); }
            else { _tunes[tuneName] = tuneToAdd; }
        }

        private void InstancePlayTune(string tuneName, bool loop)
        {
            if ((InstanceInitialized) && (XnaMediaPlayer.GameHasControl))
            {
				if ((!InstanceMuted) && (_tunes.ContainsKey(tuneName)))
                {
                    XnaMediaPlayer.IsRepeating = loop;
                    XnaMediaPlayer.Play(_tunes[tuneName]);
                }

                _lastSongName = tuneName;
                _lastSongWasLooped = loop;
            }
        }

        private void InstanceStopTune()
        {
            if ((InstanceInitialized) && (XnaMediaPlayer.GameHasControl))
            {
                _lastSongName = "";
                _lastSongWasLooped = false;
                XnaMediaPlayer.Stop();
            }
        }

        private void InstanceSetMutedState(bool mute)
        {
            InstanceMuted = mute;

            if ((InstanceInitialized) && (XnaMediaPlayer.GameHasControl))
            {
                if (mute)
                {
                    XnaMediaPlayer.Stop();
                }
                else
                {
                    if ((XnaMediaPlayer.State == MediaState.Stopped) && (!string.IsNullOrEmpty(_lastSongName)))
                    {
                        InstancePlayTune(_lastSongName, _lastSongWasLooped);
                    }
                }
            }
        }

        private void InstanceSetPausedState(bool paused)
        {
            if ((InstanceInitialized) && (XnaMediaPlayer.GameHasControl))
            {
                if ((paused) && (XnaMediaPlayer.State == MediaState.Playing)) { XnaMediaPlayer.Pause(); }
                else if ((!paused) && (XnaMediaPlayer.State == MediaState.Paused)) { XnaMediaPlayer.Resume(); }
            }
        }
    }
}
