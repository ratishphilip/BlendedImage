using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using CompositionProToolkit;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BlendedImage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;
        private CanvasGeometry _leftGeometry;
        private CanvasGeometry _rightGeometry;
        private float _topOffset;
        private float _splitValue = 50f;
        private float _blurRadius = 100f;
        private IGaussianMaskSurface _leftGaussianSurface;
        private IGaussianMaskSurface _rightGaussianSurface;
        private const float GridWidth = 1138f;
        private const float GridHeight = 640f;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = Window.Current.Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            var size = new Vector2(GridWidth, GridHeight);
            _topOffset = -GridHeight / 2f;

            // Create the left image visual
            var leftImageVisual = _compositor.CreateSpriteVisual();
            leftImageVisual.Size = size;

            // Left Image Surface
            var leftImageSurface = await _generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/Image1.jpg"), size.ToSize(), ImageSurfaceOptions.Default);

            // Left Mask Geometry

            _leftGeometry = CanvasGeometry.CreateRectangle(_generator.Device, new Rect(0f, 0f, 2f * (GridWidth * _splitValue) / 100f, 2f * GridHeight));
            var leftOffset = -(GridWidth * _splitValue) / 100f;
            // Left Masked Brush
            var leftMaskedBrush = _compositor.CreateMaskBrush();
            leftMaskedBrush.Source = _compositor.CreateSurfaceBrush(leftImageSurface);
            _leftGaussianSurface = _generator.CreateGaussianMaskSurface(size.ToSize(), _leftGeometry, new Vector2(leftOffset, _topOffset), _blurRadius);
            leftMaskedBrush.Mask = _compositor.CreateSurfaceBrush(_leftGaussianSurface);
            leftImageVisual.Brush = leftMaskedBrush;

            // Create the right image visual
            var rightImageVisual = _compositor.CreateSpriteVisual();
            rightImageVisual.Size = size;

            // Right Image Surface
            var rightImageSurface = await _generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/Image2.jpg"), size.ToSize(), ImageSurfaceOptions.Default);

            // Right Mask Geometry
            _rightGeometry = CanvasGeometry.CreateRectangle(_generator.Device, new Rect(0, 0, 2f * (GridWidth * (100f - _splitValue)) / 100, 2f * GridHeight));

            // Right Masked Brush
            var rightMaskedBrush = _compositor.CreateMaskBrush();
            rightMaskedBrush.Source = _compositor.CreateSurfaceBrush(rightImageSurface);
            _rightGaussianSurface = _generator.CreateGaussianMaskSurface(size.ToSize(), _rightGeometry, new Vector2(-leftOffset, _topOffset), _blurRadius);
            rightMaskedBrush.Mask = _compositor.CreateSurfaceBrush(_rightGaussianSurface);
            rightImageVisual.Brush = rightMaskedBrush;

            var visualContainer = _compositor.CreateContainerVisual();
            visualContainer.Size = size;
            visualContainer.Children.InsertAtTop(leftImageVisual);
            visualContainer.Children.InsertAtTop(rightImageVisual);

            ElementCompositionPreview.SetElementChildVisual(ImageGrid, visualContainer);
        }

        private void OnSplitValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _splitValue = (float)e.NewValue;
            Redraw();
        }

        private void OnBlurRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _blurRadius = (float)e.NewValue;
            Redraw();
        }

        private void Redraw()
        {
            if (_leftGeometry == null || _rightGeometry == null)
            {
                return;
            }

            var leftOffset = -(GridWidth * _splitValue) / 100f;
            _leftGeometry = CanvasGeometry.CreateRectangle(_generator.Device, new Rect(0f, 0f, 2f * (GridWidth * _splitValue) / 100f, 2f * GridHeight));
            _leftGaussianSurface.Redraw(_leftGeometry, new Vector2(leftOffset, _topOffset), _blurRadius);
            _rightGeometry = CanvasGeometry.CreateRectangle(_generator.Device, new Rect(0, 0, 2f * (GridWidth * (100f - _splitValue)) / 100, 2f * GridHeight));
            _rightGaussianSurface.Redraw(_rightGeometry, new Vector2(-leftOffset, _topOffset), _blurRadius);
        }
    }
}
