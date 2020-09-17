using System.Runtime.CompilerServices;

using Sharpnado.Presentation.Forms;
using Sharpnado.Presentation.Forms.Effects;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShadowsSample.CustomViews
{
    [ContentProperty(nameof(CustomView))]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Toolbar : ContentView
    {
        public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(
            nameof(ShowBackButton),
            typeof(bool),
            typeof(Toolbar),
            defaultValue: false,
            propertyChanged: ShowBackButtonPropertyChanged);

        public static readonly BindableProperty ForegroundColorProperty = BindableProperty.Create(
            nameof(ForegroundColor),
            typeof(Color),
            typeof(Toolbar));

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(Toolbar),
            string.Empty);

        public static readonly BindableProperty CustomViewProperty = BindableProperty.Create(
            nameof(CustomView),
            typeof(View),
            typeof(Toolbar));

        public static readonly BindableProperty UseSafeAreaMarginProperty = BindableProperty.Create(
            nameof(UseSafeAreaMargin),
            typeof(bool),
            typeof(Toolbar),
            true);

        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(Toolbar),
            string.Empty,
            propertyChanged: SubtitlePropertyChanged);

        public static readonly BindableProperty BackTapCommandProperty = BindableProperty.Create(
            nameof(BackTapCommand),
            typeof(TaskLoaderCommand),
            typeof(Toolbar),
            null,
            propertyChanged: BackTapCommandPropertyChanged);

        public Toolbar()
        {
            InitializeComponent();

            UpdateUseSafeAreaMargin();
        }

        public View CustomView
        {
            get => (View)GetValue(CustomViewProperty);
            set => SetValue(CustomViewProperty, value);
        }

        public bool ShowBackButton
        {
            get => (bool)GetValue(ShowBackButtonProperty);
            set => SetValue(ShowBackButtonProperty, value);
        }

        public bool UseSafeAreaMargin
        {
            get => (bool)GetValue(UseSafeAreaMarginProperty);
            set => SetValue(UseSafeAreaMarginProperty, value);
        }

        public Color ForegroundColor
        {
            get => (Color)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public TaskLoaderCommand BackTapCommand
        {
            get => (TaskLoaderCommand)GetValue(BackTapCommandProperty);
            set => SetValue(BackTapCommandProperty, value);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(CustomView))
            {
                CustomViewRoot.Content = CustomView;
            }

            if (propertyName == nameof(UseSafeAreaMargin))
            {
                UpdateUseSafeAreaMargin();
            }
        }

        private void UpdateUseSafeAreaMargin()
        {
            if (UseSafeAreaMargin)
            {
                return;
            }

            SafeAreaRowDefinition.RemoveBinding(RowDefinition.HeightProperty);
            SafeAreaRowDefinition.Height = new GridLength(0);
        }

        private static void ShowBackButtonPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var toolbar = (Toolbar)bindable;
            toolbar.UpdateShowBackButton();
        }

        private static void SubtitlePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var toolbar = (Toolbar)bindable;
            toolbar.UpdateSubtitle();
        }

        private static void BackTapCommandPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var toolbar = (Toolbar)bindable;
            TapCommandEffect.SetTap(toolbar.BackButton, (TaskLoaderCommand)newvalue);
        }

        private void UpdateShowBackButton()
        {
            ButtonColumnDefinition.Width = ShowBackButton ? ResourcesHelper.GetResource<double>("HeightToolbar") : 0;

            if (ShowBackButton)
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    TitleLabel.HorizontalOptions = LayoutOptions.Center;
                }

                TitleLabel.Margin = new Thickness(
                    0,
                    TitleLabel.Margin.Top,
                    TitleLabel.Margin.Right,
                    TitleLabel.Margin.Bottom);
            }
            else
            {
                TitleLabel.Margin = new Thickness(
                     30,
                     TitleLabel.Margin.Top,
                     TitleLabel.Margin.Right,
                     TitleLabel.Margin.Bottom);
            }
        }

        private void UpdateSubtitle()
        {
            SubtitleRowDefinition.Height = string.IsNullOrEmpty(Subtitle) ? 0 : GridLength.Auto;
        }
    }
}