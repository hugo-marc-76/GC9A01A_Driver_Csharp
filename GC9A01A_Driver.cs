using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using SkiaSharp;

namespace GC9A01A
{
    /// <summary>
    /// Classe LCD_1inch28 pour pilotage d'un écran GC9A01A en mode SPI.
    /// Utilise SkiaSharp pour le rendu des images.
    /// </summary>
    public class GC9A01A_Driver : IDisposable
    {
        // Dimensions de l'écran
        public int Width { get; } = 240;
        public int Height { get; } = 240;

        // Broches utilisées
        private readonly int _rstPin; // Broche de Reset
        private readonly int _dcPin;  // Broche Data/Command
        private readonly int _blPin;  // Broche Backlight

        private readonly SpiDevice _spi;
        private readonly GpioController _gpio;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="spi">Instance de SpiDevice configurée.</param>
        /// <param name="gpio">Instance de GpioController.</param>
        /// <param name="rstPin">Numéro de broche Reset.</param>
        /// <param name="dcPin">Numéro de broche Data/Command.</param>
        /// <param name="blPin">Numéro de broche Backlight.</param>
        public GC9A01A_Driver(SpiDevice spi, GpioController gpio, int rstPin, int dcPin, int blPin)
        {
            _spi = spi ?? throw new ArgumentNullException(nameof(spi));
            _gpio = gpio ?? throw new ArgumentNullException(nameof(gpio));
            _rstPin = rstPin;
            _dcPin = dcPin;
            _blPin = blPin;

            _gpio.OpenPin(_dcPin, PinMode.Output);
            _gpio.OpenPin(_rstPin, PinMode.Output);
            _gpio.OpenPin(_blPin, PinMode.Output);
        }

        #region Communication SPI/GPIO

        /// <summary>
        /// Envoie une commande via SPI (met la broche DC à LOW).
        /// </summary>
        private void SendCommand(byte cmd)
        {
            _gpio.Write(_dcPin, PinValue.Low);
            _spi.Write(new byte[] { cmd });
        }

        /// <summary>
        /// Envoie des données via SPI (met la broche DC à HIGH).
        /// </summary>
        private void SendData(byte[] data)
        {
            _gpio.Write(_dcPin, PinValue.High);
            _spi.Write(data);
        }

        #endregion

        #region Initialisation et commandes d'écran

        /// <summary>
        /// Réalise un reset matériel de l'écran.
        /// </summary>
        public void Reset()
        {
            _gpio.Write(_rstPin, PinValue.High);
            Thread.Sleep(10);
            _gpio.Write(_rstPin, PinValue.Low);
            Thread.Sleep(10);
            _gpio.Write(_rstPin, PinValue.High);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Initialise l'écran en envoyant la séquence d'initialisation.
        /// </summary>
        public void Init()
        {
            ModuleInit();
            Reset();

            // Séquence d'initialisation adaptée (voir documentation/versions Python/Adafruit)
            SendCommand(0xEF);
            SendCommand(0xEB); SendData(new byte[] { 0x14 });
            SendCommand(0xFE);
            SendCommand(0xEF);
            SendCommand(0xEB); SendData(new byte[] { 0x14 });
            SendCommand(0x84); SendData(new byte[] { 0x40 });
            SendCommand(0x85); SendData(new byte[] { 0xFF });
            SendCommand(0x86); SendData(new byte[] { 0xFF });
            SendCommand(0x87); SendData(new byte[] { 0xFF });
            SendCommand(0x88); SendData(new byte[] { 0x0A });
            SendCommand(0x89); SendData(new byte[] { 0x21 });
            SendCommand(0x8A); SendData(new byte[] { 0x00 });
            SendCommand(0x8B); SendData(new byte[] { 0x80 });
            SendCommand(0x8C); SendData(new byte[] { 0x01 });
            SendCommand(0x8D); SendData(new byte[] { 0x01 });
            SendCommand(0x8E); SendData(new byte[] { 0xFF });
            SendCommand(0x8F); SendData(new byte[] { 0xFF });
            SendCommand(0xB6); SendData(new byte[] { 0x00, 0x20 });
            SendCommand(0x36); SendData(new byte[] { 0x08 });
            SendCommand(0x3A); SendData(new byte[] { 0x05 });
            SendCommand(0x90); SendData(new byte[] { 0x08, 0x08, 0x08, 0x08 });
            SendCommand(0xBD); SendData(new byte[] { 0x06 });
            SendCommand(0xBC); SendData(new byte[] { 0x00 });
            SendCommand(0xFF); SendData(new byte[] { 0x60, 0x01, 0x04 });
            SendCommand(0xC3); SendData(new byte[] { 0x13 });
            SendCommand(0xC4); SendData(new byte[] { 0x13 });
            SendCommand(0xC9); SendData(new byte[] { 0x22 });
            SendCommand(0xBE); SendData(new byte[] { 0x11 });
            SendCommand(0xE1); SendData(new byte[] { 0x10, 0x0E });
            SendCommand(0xDF); SendData(new byte[] { 0x21, 0x0C, 0x02 });
            SendCommand(0xF0); SendData(new byte[] { 0x45, 0x09, 0x08, 0x08, 0x26, 0x2A });
            SendCommand(0xF1); SendData(new byte[] { 0x43, 0x70, 0x72, 0x36, 0x37, 0x6F });
            SendCommand(0xF2); SendData(new byte[] { 0x45, 0x09, 0x08, 0x08, 0x26, 0x2A });
            SendCommand(0xF3); SendData(new byte[] { 0x43, 0x70, 0x72, 0x36, 0x37, 0x6F });
            SendCommand(0xED); SendData(new byte[] { 0x1B, 0x0B });
            SendCommand(0xAE); SendData(new byte[] { 0x77 });
            SendCommand(0xCD); SendData(new byte[] { 0x63 });
            SendCommand(0xE8); SendData(new byte[] { 0x34 });
            SendCommand(0x62); SendData(new byte[] { 0x18, 0x0D, 0x71, 0xED, 0x70, 0x70, 0x18, 0x0F, 0x71, 0xEF, 0x70, 0x70 });
            SendCommand(0x63); SendData(new byte[] { 0x18, 0x11, 0x71, 0xF1, 0x70, 0x70, 0x18, 0x13, 0x71, 0xF3, 0x70, 0x70 });
            SendCommand(0x64); SendData(new byte[] { 0x28, 0x29, 0xF1, 0x01, 0xF1, 0x00, 0x07 });
            SendCommand(0x66); SendData(new byte[] { 0x3C, 0x00, 0xCD, 0x67, 0x45, 0x45, 0x10, 0x00, 0x00, 0x00 });
            SendCommand(0x67); SendData(new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x00, 0x01, 0x54, 0x10, 0x32, 0x98 });
            SendCommand(0x74); SendData(new byte[] { 0x10, 0x85, 0x80, 0x00, 0x00, 0x4E, 0x00 });
            SendCommand(0x98); SendData(new byte[] { 0x3E, 0x07 });
            SendCommand(0x35); // TEON
            SendCommand(0x21); // Inversion OFF
            SendCommand(0x11); // Sleep Out
            Thread.Sleep(120);
            SendCommand(0x29); // Display On
            Thread.Sleep(20);
        }

        /// <summary>
        /// Efface l'écran (remplit de noir).
        /// </summary>
        public void Clear() => FillScreen(0x0000);

        /// <summary>
        /// Remplit l'écran avec une couleur donnée (en RGB565).
        /// </summary>
        public void FillScreen(ushort color)
        {
            SetWindow(0, 0, (ushort)Width, (ushort)Height);
            int totalPixels = Width * Height;
            byte hi = (byte)(color >> 8);
            byte lo = (byte)(color & 0xFF);
            byte[] pixels = new byte[totalPixels * 2];
            for (int i = 0; i < totalPixels; i++)
            {
                pixels[i * 2] = hi;
                pixels[i * 2 + 1] = lo;
            }
            const int chunkSize = 4096;
            for (int offset = 0; offset < pixels.Length; offset += chunkSize)
            {
                int len = Math.Min(chunkSize, pixels.Length - offset);
                byte[] chunk = new byte[len];
                Array.Copy(pixels, offset, chunk, 0, len);
                SendData(chunk);
            }
        }

        /// <summary>
        /// Configure la fenêtre d'écriture de l'écran.
        /// </summary>
        public void SetWindow(ushort x, ushort y, ushort w, ushort h)
        {
            ushort x2 = (ushort)(x + w - 1);
            ushort y2 = (ushort)(y + h - 1);
            SendCommand(0x2A); // CASET
            SendData(new byte[] {
                (byte)(x >> 8), (byte)(x & 0xFF),
                (byte)(x2 >> 8), (byte)(x2 & 0xFF)
            });
            SendCommand(0x2B); // RASET
            SendData(new byte[] {
                (byte)(y >> 8), (byte)(y & 0xFF),
                (byte)(y2 >> 8), (byte)(y2 & 0xFF)
            });
            SendCommand(0x2C); // RAMWR
        }

        /// <summary>
        /// Contrôle le rétroéclairage. Si dutyCycle > 0, la LED est activée.
        /// </summary>
        public void SetBacklight(int dutyCycle)
        {
            _gpio.Write(_blPin, dutyCycle > 0 ? PinValue.High : PinValue.Low);
        }

        /// <summary>
        /// Affiche un SKBitmap sur l'écran en convertissant chaque pixel en RGB565.
        /// </summary>
        public void ShowImage(SKBitmap bmp)
        {
            if (bmp.Width != Width || bmp.Height != Height)
                throw new ArgumentException($"L'image doit être de taille {Width}x{Height}.");
            byte[] buffer = new byte[Width * Height * 2];
            int index = 0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    SKColor color = bmp.GetPixel(x, y);
                    ushort pixel = (ushort)(((color.Red & 0xF8) << 8) |
                                              ((color.Green & 0xFC) << 3) |
                                              (color.Blue >> 3));
                    buffer[index++] = (byte)(pixel >> 8);
                    buffer[index++] = (byte)(pixel & 0xFF);
                }
            }
            SetWindow(0, 0, (ushort)Width, (ushort)Height);
            const int chunkSize = 4096;
            for (int offset = 0; offset < buffer.Length; offset += chunkSize)
            {
                int len = Math.Min(chunkSize, buffer.Length - offset);
                byte[] chunk = new byte[len];
                Array.Copy(buffer, offset, chunk, 0, len);
                SendData(chunk);
            }
        }

        private void ModuleInit()
        {
            Console.WriteLine("ModuleInit: configuration SPI et GPIO déjà effectuée.");
        }

        #endregion

        #region IDisposable Support

        public void Dispose()
        {
            _spi?.Dispose();
            _gpio?.Dispose();
        }

        #endregion
    }
}
