using Files.App.ViewModels;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Files.App.UserControls
{
    public sealed partial class FileIcon : UserControl
    {
        private SelectedItemsPropertiesViewModel viewModel;

        public SelectedItemsPropertiesViewModel ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;

                if (value is null)
                {
                    return;
                }

                if (ViewModel?.CustomIconSource is not null)
                {
                    CustomIconImageSource = new SvgImageSource(ViewModel.CustomIconSource);
                }
            }
        }

        private double itemSize;

        public double ItemSize
        {
            get => itemSize;
            set
            {
                itemSize = value;
                LargerItemSize = itemSize + 2.0;
            }
        }

        private double LargerItemSize { get; set; }

        private static DependencyProperty FileIconImageSourceProperty { get; } = DependencyProperty.Register(nameof(FileIconImageSource), typeof(BitmapImage), typeof(FileIcon), null);

        private BitmapImage FileIconImageSource
        {
            get => GetValue(FileIconImageSourceProperty) as BitmapImage;
            set => SetValue(FileIconImageSourceProperty, value);
        }

        public static DependencyProperty FileIconImageDataProperty { get; } = DependencyProperty.Register(nameof(FileIconImageData), typeof(byte[]), typeof(FileIcon), null);

        public byte[] FileIconImageData
        {
            get => GetValue(FileIconImageDataProperty) as byte[];
            set
            {
                SetValue(FileIconImageDataProperty, value);
                if (value is not null)
                {
                    UpdateImageSourceAsync();
                }
            }
        }

        private SvgImageSource CustomIconImageSource { get; set; }

        public FileIcon()
        {
            this.InitializeComponent();
        }

        public async void UpdateImageSourceAsync()
        {
            if (FileIconImageData is not null)
            {
                FileIconImageSource = new BitmapImage();
                using InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(FileIconImageData.AsBuffer());
                stream.Seek(0);
                await FileIconImageSource.SetSourceAsync(stream);
            }
        }
    }
}