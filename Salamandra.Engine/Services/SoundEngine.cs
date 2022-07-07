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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class SoundEngine
    {
        private WaveOutEvent? outputDevice;
        private AudioFileReaderExtended? audioFileReader;

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

        public static string[] SupportedAudioFormats = { ".wav", ".mp3", ".wma", ".ogg", ".flac" };

        #region Volume
        public float VolumeMin { get; set; } = 0;
        public float VolumeMax { get; set; } = 1;
        public float VolumeStep { get; set; } = 0.1f;
        public float VolumeLargeStep { get; set; } = 0.2f;
        public float VolumeSmallStep { get; set; } = 0.05f;
        #endregion

        public SoundEngine()
        {
            this.OutputDevice = 0;
            this.State = SoundEngineState.Stopped;
        }

        public List<SoundOutputDevice> EnumerateDevices()
        {
            List<SoundOutputDevice> devices = new List<SoundOutputDevice>();

            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                devices.Add(new SoundOutputDevice() { DeviceIndex = n, Name = caps.ProductName });
            }

            return devices;
        }

        public void PlayAudioFile(string filename, float volume = 1)
        {
            if (!File.Exists(filename))
            {
                throw new SoundEngineFileException("File not found.");
            }

            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent() { DeviceNumber = this.OutputDevice };
                this.outputDevice.PlaybackStopped += WaveOutEvent_PlaybackStopped;
            }

            try
            {
                if (this.audioFileReader == null)
                {
                    this.audioFileReader = new AudioFileReaderExtended(filename) { Volume = volume };
                    this.outputDevice.Init(this.audioFileReader);
                }

                this.playbackStopType = PlaybackStopType.ReachedEndOfFile;
                this.State = SoundEngineState.Playing;
                this.outputDevice.Play();
            }
            catch (MmException ex)
            {
                UnloadSoundDevices();

                throw new SoundEngineDeviceException(ex.Message);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is COMException)
            {
                UnloadSoundDevices();

                throw new SoundEngineFileException(ex.Message);
            }
            catch (Exception ex)
            {
                UnloadSoundDevices();

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
            UnloadSoundDevices();

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

        private void UnloadSoundDevices()
        {
            this.outputDevice?.Dispose();
            this.outputDevice = null;
            this.audioFileReader?.Dispose();
            this.audioFileReader = null;

            this.State = SoundEngineState.Stopped;
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
