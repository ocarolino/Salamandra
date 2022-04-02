using Microsoft.Win32;
using Salamandra.Commands;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Salamandra.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public SoundEngine SoundEngine { get; set; }
        public PlaylistManager PlaylistManager { get; set; }

        public SoundFileTrack? SelectedTrack { get; set; }

        public ICommand? AddFilesToPlaylistCommand { get; set; }
        public ICommand? RemoveTracksFromPlaylistCommand { get; set; }

        public MainViewModel()
        {
            this.SoundEngine = new SoundEngine();
            this.PlaylistManager = new PlaylistManager();

            LoadCommands();
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommand(p => AddFilesToPlaylist(), p => true);
            this.RemoveTracksFromPlaylistCommand = new RelayCommand(p => RemoveTracksFromPlaylist(p), p => true);
        }

        private void AddFilesToPlaylist()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                List<SoundFileTrack> tracks = new List<SoundFileTrack>();

                foreach (var item in openFileDialog.FileNames)
                {
                    SoundFileTrack soundFileTrack = new SoundFileTrack(item, Path.GetFileNameWithoutExtension(item));
                    tracks.Add(soundFileTrack);
                }

                this.PlaylistManager.AddTracks(tracks);
            }
        }

        private void RemoveTracksFromPlaylist(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<SoundFileTrack> tracks = ((System.Collections.IList)items).Cast<SoundFileTrack>().ToList();

            this.PlaylistManager.RemoveTracks(tracks);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
