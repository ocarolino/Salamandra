using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Salamandra.UserControls
{
    /// <summary>
    /// Interação lógica para LoadingSpinner.xam
    /// </summary>
    public partial class LoadingSpinner : UserControl
    {
        public string LoadingText
        {
            get { return (string)GetValue(LoadingTextProperty); }
            set { SetValue(LoadingTextProperty, value); }
        }

        public static readonly DependencyProperty LoadingTextProperty = DependencyProperty.Register("LoadingText", typeof(string),
            typeof(LoadingSpinner), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                LoadingTextChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        private static void LoadingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingSpinner spinner)
            {
                spinner.UpdateText();
            }
        }

        public LoadingSpinner()
        {
            InitializeComponent();
        }

        private void UpdateText()
        {
            TextField.Text = LoadingText;
        }
    }
}
