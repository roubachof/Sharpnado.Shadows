using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sharpnado.Presentation.Forms.Effects;
using Sharpnado.Shades;
using Sharpnado.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListItemCustomEasing : ContentView
    {
        private double _expandedHeight;
        private double _initialHeight;

        private static int globalCounter = 0;

        private int _counter = 0;

        public ListItemCustomEasing()
        {
            InitializeComponent();

            TapCommandEffect.SetTap(ButtonExpand, new Command(OnClickedExpander));

            _counter = globalCounter++;
        }

        private void OnClickedExpander()
        {
            if (_initialHeight == 0)
            {
                _initialHeight = Container.Height - 15;
                _expandedHeight = _initialHeight * 2;
            }

            bool isExpanded = ButtonExpand.Rotation == 180;

            var from = isExpanded ? _expandedHeight : _initialHeight;
            var to = isExpanded ? _initialHeight : _expandedHeight;

            Easing easing = Easing.SpringOut;
            uint length = 500;
            switch (_counter)
            {
                case 0:
                    easing = new Easing (t => 9 * t * t * t - 13.5 * t * t + 5.5 * t);
                    EasingLabel.Text = "Hula Hoop Easing";
                    break;
                case 1:
                    easing = new Easing (t => 1 - Math.Cos (10 * Math.PI * t) * Math.Exp (-5 * t));
                    EasingLabel.Text = "Shaking Easing";
                    break;
                case 2:
                    easing = Easing.SpringOut;
                    EasingLabel.Text = "Spring Out Easing";
                    break;
                case 3:
                    easing = Easing.SpringIn;
                    EasingLabel.Text = "Spring In Easing";
                    break;
            }

            TaskMonitor.Create(
                async () =>
                    {
                        var easingLabelTask = EasingLabel.RotateTo(isExpanded ? 0 : 360, length, easing);
                        var rotateTask = ButtonExpand.RotateTo(isExpanded ? 0 : 180, length, easing);
                        var heightAnimation = Container.DoubleTo(
                            from,
                            to,
                            h => Container.HeightRequest = h,
                            length,
                            easing);

                        await Task.WhenAll(rotateTask, heightAnimation, easingLabelTask);

                        if (isExpanded)
                        {
                            EasingLabel.Text = "Show more details";
                        }

                        ButtonExpand.Rotation = isExpanded ? 0 : 180;
                        EasingLabel.Rotation = isExpanded ? 0 : 360;
                    });
        }
    }

    public static class ViewExtensions
    {
        public static Task<bool> DoubleTo(this VisualElement self, double from, double to, Action<double> callback, uint length = 250, Easing easing = null)
        {
            Func<double, double> transform = (t) => from + (t * (to - from));
            return DoubleAnimation(self, "DoubleTo", transform, callback, length, easing);
        }

        public static void CancelAnimation(this VisualElement self)
        {
            self.AbortAnimation("DoubleTo");
        }

        static Task<bool> DoubleAnimation(VisualElement element, string name, Func<double, double> transform, Action<double> callback, uint length, Easing easing)
        {
            easing = easing ?? Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate<double>(name, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }
    }
}