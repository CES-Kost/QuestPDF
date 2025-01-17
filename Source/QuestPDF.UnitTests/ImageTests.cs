﻿using System;
using System.Linq;
using System.Net.Mime;
using FluentAssertions;
using NUnit.Framework;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.UnitTests.TestEngine;
using SkiaSharp;
using ImageElement = QuestPDF.Elements.Image;
using DocumentImage = QuestPDF.Infrastructure.Image;

namespace QuestPDF.UnitTests
{
    [TestFixture]
    public class ImageTests
    {
        [Test]
        public void Measure_TakesAvailableSpaceRegardlessOfSize()
        {
            TestPlan
                .For(x => new ImageElement
                {
                    DocumentImage = GenerateDocumentImage(400, 300)
                })
                .MeasureElement(new Size(300, 200))
                .CheckMeasureResult(SpacePlan.FullRender(300, 200));
        }
        
        [Test]
        public void Draw_TakesAvailableSpaceRegardlessOfSize()
        {
            TestPlan
                .For(x => new ImageElement
                {
                    DocumentImage = GenerateDocumentImage(400, 300)
                })
                .DrawElement(new Size(300, 200))
                .ExpectCanvasDrawImage(new Position(0, 0), new Size(300, 200))
                .CheckDrawResult();
        }
        
        [Test]
        public void Fluent_RecognizesImageProportions()
        {
            var image = GenerateSkiaImage(600, 200).Encode(SKEncodedImageFormat.Png, 100).ToArray();
            
            TestPlan
                .For(x =>
                {
                    var container = new Container();
                    container.Image(image);
                    return container;
                })
                .MeasureElement(new Size(300, 200))
                .CheckMeasureResult(SpacePlan.FullRender(300, 100));;
        }
        
        [Test]
        public void UsingSharedImageShouldNotDrasticallyIncreaseDocumentSize()
        {
            var placeholderImage = Placeholders.Image(1000, 200);

            var documentWithSingleImageSize = GetDocumentSize(container =>
            {
                container.Image(placeholderImage);
            });

            var documentWithMultipleImagesSize = GetDocumentSize(container =>
            {
                container.Column(column =>
                {
                    foreach (var i in Enumerable.Range(0, 100))
                        column.Item().Image(placeholderImage);
                });
            });

            var documentWithSingleImageUsedMultipleTimesSize = GetDocumentSize(container =>
            {
                container.Column(column =>
                {
                    var sharedImage = DocumentImage.FromBinaryData(placeholderImage).DisposeAfterDocumentGeneration();
                    
                    foreach (var i in Enumerable.Range(0, 100))
                        column.Item().Image(sharedImage);
                });
            });

            (documentWithMultipleImagesSize / (float)documentWithSingleImageSize).Should().BeInRange(90, 100);
            (documentWithSingleImageUsedMultipleTimesSize / (float)documentWithSingleImageSize).Should().BeInRange(1f, 1.5f);
        }
        
        [Test]
        public void ImageShouldNotBeScaledAboveItsNativeResolution()
        {
            var image = Placeholders.Image(200, 200);

            // TODO
        }
        
        private static int GetDocumentSize(Action<IContainer> container)
        {
            return Document
                .Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Content().Element(container);
                    });
                })
                .GeneratePdf()
                .Length;
        }
        
        SKImage GenerateSkiaImage(int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);
            return surface.Snapshot();
        }
        
        DocumentImage GenerateDocumentImage(int width, int height)
        {
            var skiaImage = GenerateSkiaImage(width, height);
            return DocumentImage.FromSkImage(skiaImage);
        }
    }
}