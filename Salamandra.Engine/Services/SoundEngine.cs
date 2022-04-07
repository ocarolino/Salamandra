using NAudio;
using NAudio.Wave;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Events;
using Salamandra.Engine.Exceptions;
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
            if (!File.Exists(filename))
            {
                throw new SoundEngineFileException("File not found.");
            }

            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent() { DeviceNumber = 0 };
                this.outputDevice.PlaybackStopped += WaveOutEvent_PlaybackStopped;
            }

            try
            {
                if (this.audioFileReader == null)
                {
                    this.audioFileReader = new AudioFileReader(filename) { Volume = volume };
                    this.outputDevice.Init(this.audioFileReader);
                }

                this.playbackStopType = PlaybackStopType.ReachedEndOfFile;
                this.State = SoundEngineState.Playing;
                this.outputDevice.Play();
            }
            catch (MmException ex)
            {
                throw new SoundEngineDeviceException(ex.Message);
            }
            catch (IOException ex)
            {
                throw new SoundEngineFileException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

            if (e.Exception == null)
                SoundStopped?.Invoke(this, new SoundStoppedEventArgs() { PlaybackStopType = this.playbackStopType });
            else
            {
                if (e.Exception is IOException fileException)
                    SoundError?.Invoke(this, new SoundErrorEventArgs() { SoundErrorException = new SoundEngineFileException(fileException.Message) });
                else if (e.Exception is MmException deviceException)
                    SoundError?.Invoke(this, new SoundErrorEventArgs() { SoundErrorException = new SoundEngineDeviceException(deviceException.Message) });
                else
                    SoundError?.Invoke(this, new SoundErrorEventArgs() { SoundErrorException = e.Exception });
            }
        }

        public void Stop()
        {
            this.playbackStopType = PlaybackStopType.StoppedByRequest;
            this.outputDevice?.Stop();
        }

        public event OnSoundStopped? SoundStopped;

        public event OnSoundError? SoundError;
    }

    public delegate void OnSoundStopped(object? sender, SoundStoppedEventArgs e);

    public delegate void OnSoundError(object? sender, SoundErrorEventArgs e);
}
