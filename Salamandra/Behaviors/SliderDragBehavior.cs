using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Salamandra.Behaviors
{
    public class SliderDragBehavior
    {
        // ToDo: Esse código funciona, mas ele fede! Dá pra resumir os event handlers.
        public static readonly DependencyProperty DragCompletedCommandProperty = DependencyProperty.RegisterAttached(
            "DragCompletedCommand", typeof(ICommand), typeof(SliderDragBehavior), new PropertyMetadata(
                default(ICommand), OnDragCompletedCommandChanged));

        public static readonly DependencyProperty DragStartedCommandProperty = DependencyProperty.RegisterAttached(
            "DragStartedCommand", typeof(ICommand), typeof(SliderDragBehavior), new PropertyMetadata(
        default(ICommand), OnDragStartedCommandChanged));

        private static void SliderOnLoaded_DragCompleted(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider == null)
            {
                return;
            }
            slider.Loaded -= SliderOnLoaded_DragCompleted;

            Track track = slider.Template.FindName("PART_Track", slider) as Track;
            if (track == null)
            {
                return;
            }
            track.Thumb.DragCompleted += (dragCompletedSender, dragCompletedArgs) =>
            {
                ICommand command = GetDragCompletedCommand(slider);
                command.Execute(null);
            };
        }

        private static void SliderOnLoaded_DragStarted(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider == null)
            {
                return;
            }
            slider.Loaded -= SliderOnLoaded_DragStarted;

            Track track = slider.Template.FindName("PART_Track", slider) as Track;
            if (track == null)
            {
                return;
            }
            track.Thumb.DragStarted += (dragStartedSender, dragStartedArgs) =>
            {
                ICommand command = GetDragStartedCommand(slider);
                command.Execute(null);
            };
        }

        private static void OnDragStartedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider slider = d as Slider;

            if (slider == null)
            {
                return;
            }

            if (e.NewValue is ICommand)
            {
                slider.Loaded += SliderOnLoaded_DragStarted;
            }
        }

        public static void SetDragStartedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragStartedCommandProperty, value);
        }

        public static ICommand GetDragStartedCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragStartedCommandProperty);
        }

        private static void OnDragCompletedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider slider = d as Slider;
            if (slider == null)
            {
                return;
            }

            if (e.NewValue is ICommand)
            {
                slider.Loaded += SliderOnLoaded_DragCompleted;
            }
        }

        public static void SetDragCompletedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragCompletedCommandProperty, value);
        }

        public static ICommand GetDragCompletedCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragCompletedCommandProperty);
        }
    }
}
