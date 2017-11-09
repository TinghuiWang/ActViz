using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ActViz.Controls
{
    public class SliderValueChangeCompletedEventArgs : RoutedEventArgs
    {
        private readonly double _value;

        public double Value { get { return _value; } }

        public SliderValueChangeCompletedEventArgs(double value)
        {
            _value = value;
        }
    }
    public delegate void SlideValueChangeCompletedEventHandler(object sender, SliderValueChangeCompletedEventArgs args);

    public class DateSlider : Slider
    {
        public event SlideValueChangeCompletedEventHandler ValueChangeCompleted;
        private bool _dragging = false;

        protected void OnValueChangeCompleted(double value)
        {
            if (ValueChangeCompleted != null)
            {
                ValueChangeCompleted(this, new SliderValueChangeCompletedEventArgs(value));
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var thumb = base.GetTemplateChild("HorizontalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += ThumbOnDragStarted;
                thumb.DragCompleted += ThumbOnDragCompleted;
            }
            thumb = base.GetTemplateChild("VerticalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += ThumbOnDragStarted;
                thumb.DragCompleted += ThumbOnDragCompleted;
            }
        }

        private void ThumbOnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _dragging = false;
            OnValueChangeCompleted(this.Value);
        }

        private void ThumbOnDragStarted(object sender, DragStartedEventArgs e)
        {
            _dragging = true;
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if (!_dragging)
            {
                OnValueChangeCompleted(newValue);
            }
        }
    }
}
