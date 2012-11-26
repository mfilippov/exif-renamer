using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace exif_renamer
{
    public sealed class Program
    {
        static void Main()
        {
            foreach (var file in Directory.GetFileSystemEntries(Directory.GetCurrentDirectory(), "*.jpg"))
            {
                var newName = string.Format(CultureInfo.InvariantCulture, "{0}.jpg", GetCreationDate(file));
                while (File.Exists(newName))
                {
                    newName = string.Format(CultureInfo.InvariantCulture, "{0}-1.jpg", Path.GetFileNameWithoutExtension(newName));
                }
                File.Move(Path.GetFileName(file), newName);
                Console.WriteLine("Procesed file:{0}", Path.GetFileName(file));
            }
        }

        private static string GetCreationDate(string fullName)
        {
            Image img = new Bitmap(fullName);
            DateTime result;
            try
            {
                var bytes = img.GetPropertyItem(36868).Value; // EXIF creation date field.
                var encoding = new ASCIIEncoding();
                try
                {
                    var str = encoding.GetString(bytes).Trim().Replace("\0", string.Empty);
                    result = DateTime.ParseExact(str, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture,
                                                            "Invalid EXIF info in file: {0}", fullName));
                }
            }
            catch (ArgumentException)
            {
                var creationTime = File.GetCreationTime(fullName);
                var modificationTime = File.GetLastWriteTime(fullName);
                result = creationTime > modificationTime ? modificationTime : creationTime;
            }
            finally
            {
                img.Dispose();
            }

            return result.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        }
    }
}
