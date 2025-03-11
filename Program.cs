using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using System.Threading;
using GC9A01A;
using SkiaSharp;

namespace Exemple
{
    class Program
    {
        // Configuration des broches sur Raspberry Pi
        const int RST_PIN = 27;   // Reset
        const int DC_PIN = 25;    // Data/Command
        const int BL_PIN = 18;    // Backlight
        const int SPI_BUS = 0;
        const int SPI_DEVICE = 0; // CE0

        /// <summary>
        /// Dessine un cadran de niveau d'essence sur le canvas SkiaSharp.
        /// fuelLevel est un nombre compris entre 0 (0%) et 1 (100%).
        /// </summary>
        public static void DrawFuelGauge(SKCanvas canvas, float fuelLevel)
        {
            int centerX = 120;
            int centerY = 120;
            int radius = 100;
            // Cercle extérieur
            using (var paint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 4, IsStroke = true, IsAntialias = true })
            {
                canvas.DrawCircle(centerX, centerY, radius, paint);
            }
            // Arc du cadran (240° de balayage, de 150° à -90°)
            float startAngle = 150;
            float sweepAngle = 240;
            using (var paint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 8, IsStroke = true, IsAntialias = true })
            {
                SKRect arcRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
                canvas.DrawArc(arcRect, startAngle, sweepAngle, false, paint);
            }
            // Repères le long de l'arc
            using (var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsStroke = true, IsAntialias = true })
            {
                int numMarkers = 10;
                for (int i = 0; i <= numMarkers; i++)
                {
                    float angle = startAngle + sweepAngle * i / numMarkers;
                    float rad = angle * (float)Math.PI / 180f;
                    float x1 = centerX + radius * (float)Math.Cos(rad);
                    float y1 = centerY + radius * (float)Math.Sin(rad);
                    float x2 = centerX + (radius - 10) * (float)Math.Cos(rad);
                    float y2 = centerY + (radius - 10) * (float)Math.Sin(rad);
                    canvas.DrawLine(x1, y1, x2, y2, paint);
                }
            }
            // Pointeur indiquant le niveau
            float pointerAngle = startAngle + sweepAngle * fuelLevel;
            float pointerRad = pointerAngle * (float)Math.PI / 180f;
            float pointerLength = radius - 20;
            float xPointer = centerX + pointerLength * (float)Math.Cos(pointerRad);
            float yPointer = centerY + pointerLength * (float)Math.Sin(pointerRad);
            using (var paint = new SKPaint { Color = SKColors.Red, StrokeWidth = 4, IsStroke = true, IsAntialias = true })
            {
                canvas.DrawLine(centerX, centerY, xPointer, yPointer, paint);
            }
            // Petit cercle central
            using (var paint = new SKPaint { Color = SKColors.Black, IsStroke = false, IsAntialias = true })
            {
                canvas.DrawCircle(centerX, centerY, 5, paint);
            }
            // Texte indiquant "Fuel" et le pourcentage
            using (var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 20 })
            {
                canvas.DrawText("Fuel", centerX - 20, centerY + radius + 30, paint);
                string percentage = $"{(int)(fuelLevel * 100)}%";
                canvas.DrawText(percentage, centerX - 15, centerY + radius + 55, paint);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Initialisation de l'écran...");
            var gpio = new GpioController();
            var spiSettings = new SpiConnectionSettings(SPI_BUS, SPI_DEVICE)
            {
                ClockFrequency = 10_000_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            using var spi = SpiDevice.Create(spiSettings);
            using var disp = new GC9A01A_Driver(spi, gpio, RST_PIN, DC_PIN, BL_PIN);

            disp.Init();
            disp.Clear();
            disp.SetBacklight(50);

            // Création d'un SKBitmap pour dessiner le cadran
            using SKBitmap bitmap = new SKBitmap(disp.Width, disp.Height, SKColorType.Rgb565, SKAlphaType.Opaque);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                // Par exemple, 60% de niveau de carburant
                float fuelLevel = 0.6f;
                DrawFuelGauge(canvas, fuelLevel);
            }

            disp.ShowImage(bitmap);
            Console.WriteLine("Cadran affiché, pause de 5 secondes...");
            Thread.Sleep(5000);

            Console.WriteLine("Fin du test, fermeture du module.");
        }
    }
}
