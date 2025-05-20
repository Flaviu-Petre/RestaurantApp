using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace RestaurantApp.UI.Infrastructure
{
    public static class ImageHelper
    {
        public static string GetApplicationImageDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string imageDir = Path.Combine(baseDir, "Images", "Dishes");

            if (!Directory.Exists(imageDir))
            {
                Directory.CreateDirectory(imageDir);
            }

            return imageDir;
        }

        public static string SaveImageToAppStorage(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                return null;

            try
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                string destDir = GetApplicationImageDirectory();
                string destPath = Path.Combine(destDir, uniqueFileName);

                File.Copy(sourceFilePath, destPath, true);

                return destPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }

        public static BitmapImage LoadImageSource(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(imagePath, UriKind.Absolute);
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                return null;
            }
        }
    }
}