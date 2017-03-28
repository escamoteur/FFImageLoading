using FFImageLoading.Forms;
using FFImageLoading.Work;
using FFImageLoading.Extensions;
using FFImageLoading.Forms.Args;
using System;
using System.ComponentModel;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Linq;
using FFImageLoading.Helpers;

#if WINDOWS_UWP
using FFImageLoading.Forms.WinUWP;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;

#elif SILVERLIGHT
using FFImageLoading.Forms.WinSL;
using Xamarin.Forms.Platform.WinPhone;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#else
using FFImageLoading.Forms.WinRT;
using Xamarin.Forms.Platform.WinRT;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
#endif

#if WINDOWS_UWP
[assembly: ExportRenderer(typeof(CachedImage), typeof(CachedImageRenderer))]
namespace FFImageLoading.Forms.WinUWP
#elif SILVERLIGHT
[assembly: Xamarin.Forms.ExportRenderer(typeof(CachedImage), typeof(CachedImageRenderer))]
namespace FFImageLoading.Forms.WinSL
#else
[assembly: ExportRenderer(typeof(CachedImage), typeof(CachedImageRenderer))]
namespace FFImageLoading.Forms.WinRT
#endif
{
    /// <summary>
    /// CachedImage Implementation
    /// </summary>
#if SILVERLIGHT
    public class CachedImageRenderer : ViewRenderer<CachedImage, Image>, IDisposable
#else
    public class CachedImageRenderer : ViewRenderer<CachedImage, Image>
#endif
    {
        private bool _measured;
        private IScheduledWork _currentTask;
		private bool _isDisposed = false;

        /// <summary>
        ///   Used for registration with dependency service
        /// </summary>
        public static void Init()
        {
        }

        public override Xamarin.Forms.SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var bitmapSource = Control.Source as BitmapSource;

            if (bitmapSource == null)
                return new Xamarin.Forms.SizeRequest();

            _measured = true;

            return new Xamarin.Forms.SizeRequest(new Xamarin.Forms.Size()
            {
                Width = bitmapSource.PixelWidth,
                Height = bitmapSource.PixelHeight
            });
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CachedImage> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            if (Control == null)
            {
                Image control = new Image()
                {
                    Stretch = GetStretch(Xamarin.Forms.Aspect.AspectFill),
                };
                control.ImageOpened += OnImageOpened;
                SetNativeControl(control);

				Control.HorizontalAlignment = HorizontalAlignment.Center;
				Control.VerticalAlignment = VerticalAlignment.Center;
            }

            if (e.NewElement != null)
            {
				e.NewElement.InternalReloadImage = new Action(ReloadImage);
				e.NewElement.InternalCancel = new Action(Cancel);
				e.NewElement.InternalGetImageAsJPG = new Func<GetImageAsJpgArgs, Task<byte[]>>(GetImageAsJpgAsync);
				e.NewElement.InternalGetImageAsPNG = new Func<GetImageAsPngArgs, Task<byte[]>>(GetImageAsPngAsync);
            }

			if (e.OldElement != null && Control != null && !_isDisposed)
			{
				Control.ImageOpened -= OnImageOpened;
			}

            UpdateSource();
            UpdateAspect();
        }

#if SILVERLIGHT
        public void Dispose()
        {
			if (_isDisposed)
				return;

            if (Control != null)
            {
                Control.ImageOpened -= OnImageOpened;
            }
			_isDisposed = true;
        }
#else
		protected override void Dispose(bool disposing)
        {
			if (_isDisposed)
				return;
			
            if (Control != null)
            {
                Control.ImageOpened -= OnImageOpened;
            }

			_isDisposed = true;
            base.Dispose(disposing);
        }
#endif

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == CachedImage.SourceProperty.PropertyName)
            {
                UpdateSource();
            }
            else
            {
                if (!(e.PropertyName == CachedImage.AspectProperty.PropertyName))
                    return;

                UpdateAspect();
            }
        }

        private void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_measured)
            {
                ((Xamarin.Forms.IVisualElementController)Element)?.InvalidateMeasure(Xamarin.Forms.Internals.InvalidationTrigger.RendererReady);
            }
        }

        private async void UpdateSource()
        {
            Element.SetIsLoading(true);

            Xamarin.Forms.ImageSource source = Element.Source;

            Cancel();
            TaskParameter imageLoader = null;

            var ffSource = await ImageSourceBinding.GetImageSourceBinding(source, Element).ConfigureAwait(false);

            if (ffSource == null)
            {
                if (Control != null)
                    Control.Source = null;

                ImageLoadingFinished(Element);
            }
            else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Url)
            {
                imageLoader = ImageService.Instance.LoadUrl(ffSource.Path, Element.CacheDuration);
            }
            else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.CompiledResource)
            {
                imageLoader = ImageService.Instance.LoadCompiledResource(ffSource.Path);
            }
            else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.ApplicationBundle)
            {
                imageLoader = ImageService.Instance.LoadFileFromApplicationBundle(ffSource.Path);
            }
            else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Filepath)
            {
                imageLoader = ImageService.Instance.LoadFile(ffSource.Path);
            }
            else if (ffSource.ImageSource == FFImageLoading.Work.ImageSource.Stream)
            {
                imageLoader = ImageService.Instance.LoadStream(ffSource.Stream);
            }

            if (imageLoader != null)
            {
				// CustomKeyFactory
				if (Element.CacheKeyFactory != null)
				{
					var bindingContext = Element.BindingContext;
					imageLoader.CacheKey(Element.CacheKeyFactory.GetKey(source, bindingContext));
				}

                // LoadingPlaceholder
                if (Element.LoadingPlaceholder != null)
                {
					var placeholderSource = await ImageSourceBinding.GetImageSourceBinding(Element.LoadingPlaceholder, Element).ConfigureAwait(false);
                    if (placeholderSource != null)
                        imageLoader.LoadingPlaceholder(placeholderSource.Path, placeholderSource.ImageSource);
                }

                // ErrorPlaceholder
                if (Element.ErrorPlaceholder != null)
                {
					var placeholderSource = await ImageSourceBinding.GetImageSourceBinding(Element.ErrorPlaceholder, Element).ConfigureAwait(false);
                    if (placeholderSource != null)
                        imageLoader.ErrorPlaceholder(placeholderSource.Path, placeholderSource.ImageSource);
                }

				// Enable vector image source
				var vect1 = Element.Source as IVectorImageSource;
				var vect2 = Element.LoadingPlaceholder as IVectorImageSource;
				var vect3 = Element.ErrorPlaceholder as IVectorImageSource;
				if (vect1 != null)
				{
					imageLoader.WithCustomDataResolver(vect1.GetVectorDataResolver());
				}
				if (vect2 != null)
				{
					imageLoader.WithCustomLoadingPlaceholderDataResolver(vect2.GetVectorDataResolver());
				}
				if (vect3 != null)
				{
					imageLoader.WithCustomErrorPlaceholderDataResolver(vect3.GetVectorDataResolver());
				}
				if (Element.CustomDataResolver != null)
				{
					imageLoader.WithCustomDataResolver(Element.CustomDataResolver);
					imageLoader.WithCustomLoadingPlaceholderDataResolver(Element.CustomDataResolver);
					imageLoader.WithCustomErrorPlaceholderDataResolver(Element.CustomDataResolver);
				}

				// Downsample
				if (Element.DownsampleToViewSize && (Element.Width > 0 || Element.Height > 0))
				{
					if (Element.Height > Element.Width)
					{
						imageLoader.DownSampleInDip(height: (int)Element.Height);
					}
					else
					{
						imageLoader.DownSampleInDip(width: (int)Element.Width);
					}
				}
				else if (Element.DownsampleToViewSize && (Element.WidthRequest > 0 || Element.HeightRequest > 0))
				{
					if (Element.HeightRequest > Element.WidthRequest)
					{
						imageLoader.DownSampleInDip(height: (int)Element.HeightRequest);
					}
					else
					{
						imageLoader.DownSampleInDip(width: (int)Element.WidthRequest);
					}
				}
				else if ((int)Element.DownsampleHeight != 0 || (int)Element.DownsampleWidth != 0)
				{
					if (Element.DownsampleHeight > Element.DownsampleWidth)
					{
						if (Element.DownsampleUseDipUnits)
							imageLoader.DownSampleInDip(height: (int)Element.DownsampleHeight);
						else
							imageLoader.DownSample(height: (int)Element.DownsampleHeight);
					}
					else
					{
						if (Element.DownsampleUseDipUnits)
							imageLoader.DownSampleInDip(width: (int)Element.DownsampleWidth);
						else
							imageLoader.DownSample(width: (int)Element.DownsampleWidth);
					}
				}

                // RetryCount
                if (Element.RetryCount > 0)
                {
                    imageLoader.Retry(Element.RetryCount, Element.RetryDelay);
                }

				if (Element.BitmapOptimizations.HasValue)
					imageLoader.BitmapOptimizations(Element.BitmapOptimizations.Value);

                // FadeAnimation
                if (Element.FadeAnimationEnabled.HasValue)
                    imageLoader.FadeAnimation(Element.FadeAnimationEnabled.Value);

                // TransformPlaceholders
                if (Element.TransformPlaceholders.HasValue)
                    imageLoader.TransformPlaceholders(Element.TransformPlaceholders.Value);

                // Transformations
                if (Element.Transformations != null && Element.Transformations.Count > 0)
                {
                    imageLoader.Transform(Element.Transformations);
                }

                imageLoader.WithPriority(Element.LoadingPriority);
                if (Element.CacheType.HasValue)
                {
                    imageLoader.WithCache(Element.CacheType.Value);
                }

				if (Element.LoadingDelay.HasValue)
				{
					imageLoader.Delay(Element.LoadingDelay.Value);
				}

                var element = Element;

                imageLoader.Finish((work) =>
                {
                    element.OnFinish(new CachedImageEvents.FinishEventArgs(work));
                    ImageLoadingFinished(element);
                });

                imageLoader.Success((imageInformation, loadingResult) =>
                    element.OnSuccess(new CachedImageEvents.SuccessEventArgs(imageInformation, loadingResult)));

                imageLoader.Error((exception) =>
                    element.OnError(new CachedImageEvents.ErrorEventArgs(exception)));

				imageLoader.DownloadStarted((downloadInformation) =>
					element.OnDownloadStarted(new CachedImageEvents.DownloadStartedEventArgs(downloadInformation)));

				imageLoader.DownloadProgress((progress) =>
					element.OnDownloadProgress(new CachedImageEvents.DownloadProgressEventArgs(progress)));

				imageLoader.FileWriteFinished((fileWriteInfo) =>
					element.OnFileWriteFinished(new CachedImageEvents.FileWriteFinishedEventArgs(fileWriteInfo)));

                _currentTask = imageLoader.Into(Control);
            }
        }

        private void UpdateAspect()
        {
            Control.Stretch = GetStretch(Element.Aspect);
        }

        private static Stretch GetStretch(Xamarin.Forms.Aspect aspect)
        {
            switch (aspect)
            {
                case Xamarin.Forms.Aspect.AspectFill:
                    return Stretch.UniformToFill;
                case Xamarin.Forms.Aspect.Fill:
                    return Stretch.Fill;
                default:
                    return Stretch.Uniform;
            }
        }

        private void ImageLoadingFinished(CachedImage element)
        {
			MainThreadDispatcher.Instance.Post(() =>
			{
            	if (element != null && !_isDisposed)
				{
					var elCtrl = element as Xamarin.Forms.IVisualElementController;

					if (elCtrl != null)
					{
                        ((Xamarin.Forms.IVisualElementController)Element)?.InvalidateMeasure(Xamarin.Forms.Internals.InvalidationTrigger.RendererReady);
                    }

                    element.SetIsLoading(false);
                }
			});
        }

		private void ReloadImage()
		{
			UpdateSource();
		}

		private void Cancel()
		{
			var taskToCancel = _currentTask;
			if (taskToCancel != null && !taskToCancel.IsCancelled)
			{
				taskToCancel.Cancel();
			}
		}

		private Task<byte[]> GetImageAsJpgAsync(GetImageAsJpgArgs args)
        {
			return GetImageAsByteAsync(BitmapEncoder.JpegEncoderId, args.Quality, args.DesiredWidth, args.DesiredHeight);
        }

		private Task<byte[]> GetImageAsPngAsync(GetImageAsPngArgs args)
        {
			return GetImageAsByteAsync(BitmapEncoder.PngEncoderId, 90, args.DesiredWidth, args.DesiredHeight);
        }

        private async Task<byte[]> GetImageAsByteAsync(Guid format, int quality, int desiredWidth, int desiredHeight)
        {
            if (Control == null || Control.Source == null)
                return null;

            var bitmap = Control.Source as WriteableBitmap;

            if (bitmap == null)
                return null;

            byte[] pixels = null;
            uint pixelsWidth = (uint)bitmap.PixelWidth;
            uint pixelsHeight = (uint)bitmap.PixelHeight;

            if (desiredWidth != 0 || desiredHeight != 0)
            {
                double widthRatio = (double)desiredWidth / (double)bitmap.PixelWidth;
                double heightRatio = (double)desiredHeight / (double)bitmap.PixelHeight;

                double scaleRatio = Math.Min(widthRatio, heightRatio);

                if (desiredWidth == 0)
                    scaleRatio = heightRatio;

                if (desiredHeight == 0)
                    scaleRatio = widthRatio;

                uint aspectWidth = (uint)((double)bitmap.PixelWidth * scaleRatio);
                uint aspectHeight = (uint)((double)bitmap.PixelHeight * scaleRatio);

                using (var tempStream = new InMemoryRandomAccessStream())
                {
                    byte[] tempPixels = await GetBytesFromBitmapAsync(bitmap);

                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, tempStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                        pixelsWidth, pixelsHeight, 96, 96, tempPixels);
                    await encoder.FlushAsync();
                    tempStream.Seek(0);

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(tempStream);
                    BitmapTransform transform = new BitmapTransform()
                    {
                        ScaledWidth = aspectWidth,
                        ScaledHeight = aspectHeight,
                        InterpolationMode = BitmapInterpolationMode.Linear
                    };
                    PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        transform,
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.DoNotColorManage);

                    pixels = pixelData.DetachPixelData();
                    pixelsWidth = aspectWidth;
                    pixelsHeight = aspectHeight;
                }
            }
            else
            {
                pixels = await GetBytesFromBitmapAsync(bitmap);
            }

            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(format, stream);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    pixelsWidth, pixelsHeight, 96, 96, pixels);
                await encoder.FlushAsync();
                stream.Seek(0);

                var bytes = new byte[stream.Size];
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

                return bytes;
            }
        }

        private async Task<byte[]> GetBytesFromBitmapAsync(WriteableBitmap bitmap)
        {
#if SILVERLIGHT
            using (var ms = new MemoryStream())
            {
                bitmap.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                return ms.ToArray();
            }
#else
            byte[] tempPixels;
            using (var sourceStream = bitmap.PixelBuffer.AsStream())
            {
                tempPixels = new byte[sourceStream.Length];
				await sourceStream.ReadAsync(tempPixels, 0, tempPixels.Length).ConfigureAwait(false);
            }

            return tempPixels;
#endif
        }
    }
}
