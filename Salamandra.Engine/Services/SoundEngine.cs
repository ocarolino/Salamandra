using NAudio.Wave;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class SoundEngine
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;

        private PlaybackStopType playbackStopType;

        public SoundEngine()
        {
        }

        public void PlayAudioFile(string filename)
        {
            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent();
                this.outputDevice.PlaybackStopped += WaveOutEvent_PlaybackStopped;
            }

            if (this.audioFileReader == null)
            {
                this.audioFileReader = new AudioFileReader(filename);
                this.outputDevice.Init(this.audioFileReader);
            }

            this.playbackStopType = PlaybackStopType.ReachedEndOfFile;
            this.outputDevice.Play();
        }

        private void WaveOutEvent_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("SoundEngine: Start WaveOutEvent_PlaybackStopped");
            this.outputDevice.Dispose();
            this.outputDevice = null;
            this.audioFileReader.Dispose();
            this.audioFileReader = null;

            SoundStopped?.Invoke(this, new SoundStoppedEventArgs() { PlaybackStopType = this.playbackStopType });
            Debug.WriteLine("SoundEngine: End WaveOutEvent_PlaybackStopped");
        }

        public void Stop()
        {
            this.playbackStopType = PlaybackStopType.StoppedByRequest;
            this.outputDevice?.Stop();
        }

        public event OnSoundStopped SoundStopped;
    }

    public delegate void OnSoundStopped(object? sender, SoundStoppedEventArgs e);
}
