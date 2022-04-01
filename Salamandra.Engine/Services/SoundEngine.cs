using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class SoundEngine
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;

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

            this.outputDevice.Play();
        }

        private void WaveOutEvent_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            this.outputDevice.Dispose();
            this.outputDevice = null;
            this.audioFileReader.Dispose();
            this.audioFileReader = null;
        }

        public void Stop()
        {
            this.outputDevice?.Stop();
        }
    }
}
