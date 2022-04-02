using Microsoft.Win32;
using Salamandra.Commands;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public ICommand AddFilesToPlaylistCommand { get; set; }

        public MainViewModel()
        {
            this.SoundEngine = new SoundEngine();
            this.PlaylistManager = new PlaylistManager();

            LoadCommands();
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommand(p => AddFilesToPlaylist(), p => true);
        }

        private void AddFilesToPlaylist()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var item in openFileDialog.FileNames)
                {
                    SoundFileTrack soundFileTrack = new SoundFileTrack(item, Path.GetFileNameWithoutExtension(item));
                    this.PlaylistManager.AddTrack(soundFileTrack);
                }

                if (this.PlaylistManager.CurrentTrack == null)
                    this.PlaylistManager.UpdateNextTrack();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
