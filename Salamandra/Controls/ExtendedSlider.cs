using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Salamandra.Controls
{
    public class ExtendedSlider : Slider
    {
        #region Drag Command Properties
        public ICommand? DragStartedCommand
        {
            get => (ICommand)GetValue(DragStartedCommandProperty);
            set => SetValue(DragStartedCommandProperty, value);
        }

        public ICommand? DragCompletedCommand
        {
            get { return (ICommand)GetValue(DragCompletedCommandProperty); }
            set { SetValue(DragCompletedCommandProperty, value); }
        }

        public static readonly DependencyProperty DragStartedCommandProperty = DependencyProperty.RegisterAttached(
            "DragStartedCommand", typeof(ICommand), typeof(ExtendedSlider), new PropertyMetadata(null));


        public static readonly DependencyProperty DragCompletedCommandProperty = DependencyProperty.RegisterAttached(
            "DragCompletedCommand", typeof(ICommand), typeof(ExtendedSlider), new PropertyMetadata(null));
        #endregion

        #region Tooltip Properties
        private ToolTip? _autoToolTip;
        private ToolTip? AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    var field = typeof(Slider).GetField(nameof(_autoToolTip), BindingFlags.NonPublic | BindingFlags.Instance);
                    _autoToolTip = field?.GetValue(this) as ToolTip;
                }

                return _autoToolTip;
            }
        }

        public string CustomToolTipContent
        {
            get => (string)GetValue(CustomToolTipContentProperty);
            set => SetValue(CustomToolTipContentProperty, value);
        }

        public static readonly DependencyProperty CustomToolTipContentProperty = DependencyProperty.Register(
            nameof(CustomToolTipContent), typeof(string), typeof(ExtendedSlider), new PropertyMetadata(default(string)));
        #endregion

        private void FormatAutoToolTipContent()
        {
            if (!string.IsNullOrEmpty(CustomToolTipContent))
                AutoToolTip!.Content = CustomToolTipContent;
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);

            this.DragStartedCommand?.Execute(null);

            FormatAutoToolTipContent();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);

            FormatAutoToolTipContent();
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);

            this.DragCompletedCommand?.Execute(null);
        }
    }
}
