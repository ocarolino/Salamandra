using NAudio.Wave;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
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
        private WaveOutEvent? outputDevice;
        private AudioFileReader? audioFileReader;

        private PlaybackStopType playbackStopType;

        public int OutputDevice { get; set; }
        public SoundEngineState State { get; set; }
        public float Volume
        {
            get
            {
                if (this.audioFileReader != null)
                    return this.audioFileReader.Volume;

                return 1;
            }
            set
            {
                if (this.audioFileReader != null)
                    this.audioFileReader.Volume = value;
            }
        }
        public double PositionInSeconds
        {
            get => this.audioFileReader != null ? this.audioFileReader.CurrentTime.TotalSeconds : 0;

            set
            {
                if (this.audioFileReader != null)
                    this.audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }
        public double TotalLengthInSeconds
        {
            get => this.audioFileReader != null ? this.audioFileReader.TotalTime.TotalSeconds : 0;
        }

        public SoundEngine()
        {
            this.OutputDevice = 0;
            this.State = SoundEngineState.Stopped;
        }

        public void EnumerateDevices()
        {
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                Debug.WriteLine($"{n}: {caps.ProductName}");
            }
        }

        public void PlayAudioFile(string filename, float volume = 1)
        {
            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent() { DeviceNumber = 0 };
                this.outputDevice.PlaybackStopped += WaveOutEvent_PlaybackStopped;
            }

            if (this.audioFileReader == null)
            {
                this.audioFileReader = new AudioFileReader(filename) { Volume = volume };
                this.outputDevice.Init(this.audioFileReader);
            }

            this.playbackStopType = PlaybackStopType.ReachedEndOfFile;
            this.State = SoundEngineState.Playing;
            this.outputDevice.Play();
        }

        public void TogglePlayPause()
        {
            if (this.State == SoundEngineState.Stopped)
                return;

            if (this.State == SoundEngineState.Playing)
            {
                this.outputDevice?.Pause();
                this.State = SoundEngineState.Paused;
            }
            else
            {
                this.outputDevice?.Play();
                this.State = SoundEngineState.Playing;
            }
        }

        private void WaveOutEvent_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            this.outputDevice?.Dispose();
            this.outputDevice = null;
            this.audioFileReader?.Dispose();
            this.audioFileReader = null;

            this.State = SoundEngineState.Stopped;

            SoundStopped?.Invoke(this, new SoundStoppedEventArgs() { PlaybackStopType = this.playbackStopType });
        }

        public void Stop()
        {
            this.playbackStopType = PlaybackStopType.StoppedByRequest;
            this.outputDevice?.Stop();
        }

        public event OnSoundStopped? SoundStopped;
    }

    public delegate void OnSoundStopped(object? sender, SoundStoppedEventArgs e);
}
