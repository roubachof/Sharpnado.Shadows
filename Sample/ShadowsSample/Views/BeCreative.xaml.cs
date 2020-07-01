using System;
using System.Reflection;

using Plugin.SimpleAudioPlayer;
using Sharpnado.Presentation.Forms.Helpers;
using Sharpnado.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BeCreative : ShadowsElement
    {
        private ISimpleAudioPlayer _audioPlayer;

        public BeCreative()
        {
            InitializeComponent();
        }

        public void OnAppearing()
        {
            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.Tizen)
            {
                // fail to load stream on UWP
                return;
            }

            if (_audioPlayer == null)
            {
                var assembly = typeof(App).GetTypeInfo().Assembly;
                var stream = assembly.GetManifestResourceStream($"{nameof(ShadowsSample)}.original.mp3");
                _audioPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
                _audioPlayer.Load(stream);
            }
        }

        public override void OnIsCompactChanged()
        {
            if (IsCompact)
            {
                Description.Height = 0;
            }
        }

        private bool ShouldRepeat => _audioPlayer != null && _audioPlayer.IsPlaying;

        private void CatOnTapped(object sender, EventArgs e)
        {
            if (ShouldRepeat)
            {
                _audioPlayer.Stop();
                return;
            }

            TaskMonitor.Create(
                async () =>
                    {
                        _audioPlayer?.Play();

                        await NyanBackground.AnimateTo(0, 1, "BackgroundFadeIn", (ve, v) => ve.Opacity = v);

                        var parentAnimation = new Animation();
                        foreach (var shade in CatShadows.Shades)
                        {
                            var offsetFirstAnimation = new Animation(
                                v => shade.Offset = new Point(v, shade.Offset.Y),
                                shade.Offset.X,
                                -shade.Offset.X);
                            parentAnimation.Add(0, 0.25, offsetFirstAnimation);

                            var offsetSecondAnimation = new Animation(
                                v => shade.Offset = new Point(shade.Offset.X, v),
                                shade.Offset.Y,
                                -shade.Offset.Y);
                            parentAnimation.Add(0.25, 0.5, offsetSecondAnimation);

                            var offsetThirdAnimation = new Animation(
                                v => shade.Offset = new Point(v, shade.Offset.Y),
                                -shade.Offset.X,
                                shade.Offset.X);
                            parentAnimation.Add(0.5, 0.75, offsetThirdAnimation);

                            var offsetFourthAnimation = new Animation(
                                v => shade.Offset = new Point(shade.Offset.X, v),
                                -shade.Offset.Y,
                                shade.Offset.Y);
                            parentAnimation.Add(0.75, 1, offsetFourthAnimation);
                        }

                        parentAnimation.Commit(
                            CatShadows,
                            "CatShadowsAnimation",
                            length: 1000,
                            repeat: () => ShouldRepeat,
                            finished: (value, end) =>
                                {
                                    if (!ShouldRepeat)
                                    {
                                        TaskMonitor.Create(
                                            NyanBackground.AnimateTo(
                                                1,
                                                0,
                                                "BackgroundFadeIn",
                                                (ve, v) => ve.Opacity = v));
                                    }
                                });
                    });
        }
    }
}