using System;
using System.Reflection;

// using Plugin.SimpleAudioPlayer; // Not yet migrated to MAUI
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ShadowsSample.Maui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BeCreative : ShadowsElement
    {
        // private ISimpleAudioPlayer _audioPlayer; // Not yet migrated to MAUI

        public BeCreative()
        {
            InitializeComponent();
        }

        public void OnAppearing()
        {
            // Audio player functionality temporarily disabled - needs MAUI-compatible audio library
            // if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.Tizen)
            // {
            //     // fail to load stream on UWP
            //     return;
            // }
            //
            // if (_audioPlayer == null)
            // {
            //     var assembly = typeof(App).GetTypeInfo().Assembly;
            //     var stream = assembly.GetManifestResourceStream($"{nameof(ShadowsSample)}.original.mp3");
            //     _audioPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            //     _audioPlayer.Load(stream);
            // }
        }

        public override void OnIsCompactChanged()
        {
            if (IsCompact)
            {
                Description.Height = 0;
            }
        }

        private bool ShouldRepeat => false; // _audioPlayer != null && _audioPlayer.IsPlaying;

        private void CatOnTapped(object sender, EventArgs e)
        {
            // Audio functionality temporarily disabled
            // if (ShouldRepeat)
            // {
            //     _audioPlayer.Stop();
            //     return;
            // }

            Task.Run(
                async () =>
                    {
                        // _audioPlayer?.Play();

                        await NyanBackground.FadeTo(1, 250, Easing.Linear);

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
                            finished: async (value, end) =>
                                {
                                    if (!ShouldRepeat)
                                    {
                                        await NyanBackground.FadeTo(0, 250, Easing.Linear);
                                    }
                                });
                    });
        }
    }
}