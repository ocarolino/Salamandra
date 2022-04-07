using System;
using System.Collections.Generic;
using System.Linq;
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
        #region Dependency Properties
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

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);

            this.DragStartedCommand?.Execute(null);
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);

            this.DragCompletedCommand?.Execute(null);
        }
    }
}
