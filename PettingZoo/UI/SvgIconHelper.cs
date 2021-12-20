using System;
using System.Windows;
using System.Windows.Media;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers.Utils;
using SharpVectors.Renderers.Wpf;

namespace PettingZoo.UI
{
    public static class SvgIconHelper
    {
        public static ImageSource LoadFromResource(string resourceName)
        {
            var streamInfo = Application.GetResourceStream(new Uri($"/PettingZoo;component{resourceName}", UriKind.Relative));
            if (streamInfo == null)
                throw new ArgumentException($"Resource '{resourceName}' not found");

            // Basically the code used in FileSvgConverter, but that only supports outputting to a file not returning the Drawing
            var wpfDrawingSettings = new WpfDrawingSettings
            {
                IncludeRuntime = true,
                TextAsGeometry = true
            };
            var wpfRenderer = new WpfDrawingRenderer(wpfDrawingSettings);
            var wpfWindow = new WpfSvgWindow(0, 0, wpfRenderer);

            wpfWindow.LoadDocument(streamInfo.Stream, wpfDrawingSettings);
            wpfRenderer.InvalidRect = SvgRectF.Empty;
            wpfRenderer.Render(wpfWindow.Document);

            if (wpfRenderer.Drawing == null)
                throw new ArgumentException($"Resource '{resourceName}' is not a valid SVG");
            
            return new DrawingImage(wpfRenderer.Drawing);
        }
    }
}
