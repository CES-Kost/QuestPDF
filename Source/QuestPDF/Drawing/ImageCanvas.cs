﻿using System.Collections.Generic;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace QuestPDF.Drawing
{
    internal class ImageCanvas : SkiaCanvasBase
    {
        private DocumentSettings Settings { get; }
        private SKSurface Surface { get; set; }

        internal ICollection<byte[]> Images { get; } = new List<byte[]>();
        
        public ImageCanvas(DocumentSettings settings)
        {
            Settings = settings;
        }
        
        public override void BeginDocument()
        {
            
        }

        public override void EndDocument()
        {
            Canvas?.Dispose();
            Surface?.Dispose();
        }

        public override void BeginPage(Size size)
        {
            var scalingFactor = Settings.ImageRasterDpi / (float) PageSizes.PointsPerInch;
            var imageInfo = new SKImageInfo((int) (size.Width * scalingFactor), (int) (size.Height * scalingFactor));
            
            Surface = SKSurface.Create(imageInfo);
            Canvas = Surface.Canvas;
            
            Canvas.Scale(scalingFactor);
        }

        public override void EndPage()
        {
            Canvas.Save();
            var image = Surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).ToArray();
            Images.Add(image);
            
            Canvas.Dispose();
            Surface.Dispose();
        }
    }
}