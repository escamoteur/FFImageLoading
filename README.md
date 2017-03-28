# FFImageLoading - Fast & Furious Image Loading [![AppVeyor][ci-img]][ci-link]

Library to load images quickly & easily on Xamarin.iOS, Xamarin.Android, Xamarin.Forms and Windows (UWP, WinRT).

*Authors: Daniel Luberda, Fabien Molinet. If you would like to help maintaining the project, just let us know!*

| iOS / Android / Windows (UWP / WinRT) | Xamarin.Forms |
|:--------------------------------:|:-------------:|
| [![NuGet][ffil-img]][ffil-link] [![NuGet][preffil-img]][preffil-link] | [![NuGet][forms-img]][forms-link] [![NuGet][preforms-img]][preforms-link] |
| [![][demo-droid-img]][demo-droid-src] [![][demo-win-img]][demo-win-src] | [![][demo-forms-img]][demo-forms-src] |

| Addon | iOS / Android / Windows | Xamarin.Forms |
|:-----:|:-----------------------:|:-------------:|
| Transformations | [![NuGet][trans-img]][trans-link] [![NuGet][pretrans-img]][pretrans-link] | [![NuGet][trans-img]][trans-link] [![NuGet][pretrans-img]][pretrans-link] |
| SVG support | [![NuGet][svg-img]][svg-link] [![NuGet][presvg-img]][presvg-link] | [![NuGet][svgforms-img]][svgforms-link] [![NuGet][presvgforms-img]][presvgforms-link] |

[![NuGet][ffimageloading]][ffimageloading_large]

## Features

- Xamarin.iOS, Xamarin.Android, Xamarin.Forms and Windows (WinRT, UWP) support
- Configurable disk and memory caching
- Multiple image views using the same image source (url, path, resource) will use only one bitmap which is cached in memory (less memory usage)
- Deduplication of similar download/load requests. *(If 100 similar requests arrive at same time then one real loading will be performed while 99 others will wait).*
- Error and loading placeholders support
- Images can be automatically downsampled to specified size (less memory usage)
- Fluent API which is inspired by Picasso naming
- SVG & WebP support
- Image loading Fade-In animations support
- Can retry image downloads (RetryCount, RetryDelay)
- Android bitmap optimization. Saves 50% of memory by trying not to use transparency channel when possible.
- Transformations support
  - BlurredTransformation
  - CircleTransformation, RoundedTransformation, CornersTransformation, CropTransformation
  - ColorSpaceTransformation, GrayscaleTransformation, SepiaTransformation, TintTransformation
  - FlipTransformation, RotateTransformation
  - Supports custom transformations (native platform `ITransformation` implementations)

## Documentation

https://github.com/luberda-molinet/FFImageLoading/wiki

[what-is-this]: various_images_and_image_links

[ci-img]: https://img.shields.io/appveyor/ci/daniel-luberda/ffimageloading.svg
[ci-link]: https://ci.appveyor.com/project/daniel-luberda/ffimageloading

[ffil-img]: https://img.shields.io/nuget/v/Xamarin.FFImageLoading.svg
[ffil-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading
[forms-img]: https://img.shields.io/nuget/v/Xamarin.FFImageLoading.Forms.svg
[forms-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Forms
[trans-img]: https://img.shields.io/nuget/v/Xamarin.FFImageLoading.Transformations.svg
[trans-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Transformations
[svg-img]: https://img.shields.io/nuget/v/Xamarin.FFImageLoading.Svg.svg
[svg-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Svg
[svgforms-img]: https://img.shields.io/nuget/v/Xamarin.FFImageLoading.Svg.Forms.svg
[svgforms-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Svg.Forms

[preffil-img]: https://img.shields.io/nuget/vpre/Xamarin.FFImageLoading.svg
[preffil-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading
[preforms-img]: https://img.shields.io/nuget/vpre/Xamarin.FFImageLoading.Forms.svg
[preforms-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Forms
[pretrans-img]: https://img.shields.io/nuget/vpre/Xamarin.FFImageLoading.Transformations.svg
[pretrans-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Transformations
[presvg-img]: https://img.shields.io/nuget/vpre/Xamarin.FFImageLoading.Svg.svg
[presvg-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Svg
[presvgforms-img]: https://img.shields.io/nuget/vpre/Xamarin.FFImageLoading.Svg.Forms.svg
[presvgforms-link]: https://www.nuget.org/packages/Xamarin.FFImageLoading.Svg.Forms

[ffimageloading_large]: https://raw.githubusercontent.com/luberda-molinet/FFImageLoading/master/samples/Screenshots/ffimageloading_large.png
[ffimageloading]: https://raw.githubusercontent.com/luberda-molinet/FFImageLoading/master/samples/Screenshots/ffimageloading.png

[demo-forms-img]: https://img.shields.io/badge/examples-xamarin.forms-orange.svg
[demo-forms-src]: https://github.com/luberda-molinet/FFImageLoading/tree/master/samples/ImageLoading.Forms.Sample
[demo-droid-img]: https://img.shields.io/badge/examples-android-orange.svg
[demo-droid-src]: https://github.com/luberda-molinet/FFImageLoading/tree/master/samples/ImageLoading.Sample
[demo-win-img]: https://img.shields.io/badge/examples-win-orange.svg
[demo-win-src]: https://github.com/luberda-molinet/FFImageLoading/tree/master/samples/Simple.WinPhone.Sample
[dev-nugets-img]: https://img.shields.io/badge/nugets-dev-yellow.svg
[dev-nugets]: https://github.com/luberda-molinet/FFImageLoading/wiki/Dev-NuGet-packages

