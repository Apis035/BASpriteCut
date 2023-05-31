using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace BlueArchiveSpriteCut
{
    class Program
    {
        // Folder path
        static string
            CurrentDir = Directory.GetCurrentDirectory(),
            SpineDir   = CurrentDir + "/spine/",
            ResultDir  = CurrentDir + "/Character/";

        enum Angle
        {
            Angle0,
            Angle90,
            Angle180,
            Angle270
        }

        struct Atlas
        {
            public string  PartName;
            public Angle   Rotate;
            public Vector2 Position,
                           Size,
                           Origin, /* not needed */
                           Offset; /* not needed */
            public int     Index;  /* not needed */
        }

        static void Main(string[] args)
        {
            string spineName, atlasFile, spriteFile, charName;

            // Iterate through every sprite file
            foreach (var dir in Directory.GetDirectories(SpineDir))
            {
                // Skip L2D sprites
                if (!dir.Contains("_spr"))
                    continue;

                // Get filename
                spineName  = new DirectoryInfo(dir).Name;
                spriteFile = $"{dir}/{spineName}.png";
                atlasFile  = $"{dir}/{spineName}.atlas";
                charName   = FirstToUpper(spineName.Replace("_spr", ""));

                // Check file existence
                if (!File.Exists(spriteFile) || !File.Exists(atlasFile))
                    continue;

                Console.WriteLine($"Processing {charName}...");

                // Create directory for the result
                Directory.CreateDirectory(ResultDir + charName);

                // Read atlas file
                string[] lines = File.ReadAllLines(atlasFile);

                List<Atlas> atlasList = new List<Atlas>();

                for (int i = 6; i < lines.Length;)
                {
                    Atlas atlas;
                    atlas.PartName = lines[i++];
                    atlas.Rotate   = GetRotateMode(lines[i++].Substring(9));
                    atlas.Position = ReadVector2(lines[i++], "xy");
                    atlas.Size     = ReadVector2(lines[i++], "size");
                    atlas.Origin   = ReadVector2(lines[i++], "orig");
                    atlas.Offset   = ReadVector2(lines[i++], "offset");
                    atlas.Index    = int.Parse(lines[i++].Replace("  index: ", ""));

                    atlasList.Add(atlas);
                }

                // Crop
                foreach (var atlas in atlasList)
                {
                    Rectangle cropArea = new Rectangle(
                        (int)atlas.Position.X,
                        (int)atlas.Position.Y,
                        (int)atlas.Size.X,
                        (int)atlas.Size.Y);

                    if (atlas.Rotate != Angle.Angle0)
                    {
                        cropArea.Width  = (int)atlas.Size.Y;
                        cropArea.Height = (int)atlas.Size.X;
                    }

                    Bitmap bitmap = new Bitmap(spriteFile);
                    Bitmap canvas = new Bitmap(cropArea.Width, cropArea.Height);

                    using (Graphics g = Graphics.FromImage(canvas))
                        g.DrawImage(bitmap, -cropArea.X, -cropArea.Y);

                    // Rotate image
                    switch (atlas.Rotate)
                    {
                        case Angle.Angle90:  canvas.RotateFlip(RotateFlipType.Rotate90FlipNone);  break;
                        case Angle.Angle180: canvas.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                        case Angle.Angle270: canvas.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                    };

                    // Save result
                    canvas.Save($"{ResultDir + charName}/{atlas.PartName}.png");

                    // Free memory
                    bitmap.Dispose();
                    canvas.Dispose();
                }
            }
            Console.WriteLine("Finished processing atlas sprite.");
            Console.Read();
        }

        static string FirstToUpper(string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        static Angle GetRotateMode(string str)
        {
            return str.Equals("true") ? Angle.Angle90
                 : str.Equals("180")  ? Angle.Angle180
                 : str.Equals("270")  ? Angle.Angle270
                 : Angle.Angle0;
        }

        static Vector2 ReadVector2(string str, string keyname)
        {
            int[] val = Array.ConvertAll(
                str.Replace($"{keyname}:", "").Replace(" ", "").Split(','),
                int.Parse);

            return new Vector2(val[0], val[1]);
        }
    }
}