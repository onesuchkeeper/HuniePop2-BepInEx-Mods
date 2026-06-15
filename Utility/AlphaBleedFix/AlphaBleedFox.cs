using System.Drawing;
using System.Drawing.Imaging;

class AlphaBleedFix
{
    static void Main(string[] args)
    {
        args = [
            "C:\\Git\\HuniePop2-BepInEx-Mods\\Unity\\AssetBundle Generator\\Assets\\HpUltimate\\UiTextues",
            "C:\\Git\\HuniePop2-BepInEx-Mods\\Unity\\AssetBundle Generator\\Assets\\HpUltimate\\UiTextues_Fixed",
        ];

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AlphaBleedFix <inputDir> <outputDir> [iterations]");
            return;
        }

        string inputDir = args[0];
        string outputDir = args[1];
        int iterations = args.Length >= 3 ? int.Parse(args[2]) : 4;

        Directory.CreateDirectory(outputDir);

        foreach (var file in Directory.GetFiles(inputDir, "*.png"))
        {
            Console.WriteLine($"Processing {Path.GetFileName(file)}");

            using var bmp = new Bitmap(file);
            using var fixedBmp = FixAlphaBleed(bmp, iterations);

            string outPath = Path.Combine(outputDir, Path.GetFileName(file));
            fixedBmp.Save(outPath, ImageFormat.Png);
        }

        Console.WriteLine("Done.");
    }

    static Bitmap FixAlphaBleed(Bitmap src, int iterations)
    {
        int w = src.Width;
        int h = src.Height;

        var current = new Bitmap(src);
        var next = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        for (int iter = 0; iter < iterations; iter++)
        {
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = current.GetPixel(x, y);

                // Preserve everything that is not fully transparent
                if (c.A != 0)
                {
                    next.SetPixel(x, y, c);
                    continue;
                }

                // Look for nearest opaque neighbor (8-connected)
                Color found = FindNeighborColor(current, x, y, w, h);

                next.SetPixel(x, y, found);
            }

            current.Dispose();
            current = (Bitmap)next.Clone();
        }

        return current;
    }

    static Color FindNeighborColor(Bitmap bmp, int x, int y, int w, int h)
    {
        for (int r = 1; r <= 2; r++)
        {
            for (int oy = -r; oy <= r; oy++)
            for (int ox = -r; ox <= r; ox++)
            {
                int nx = x + ox;
                int ny = y + oy;

                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    continue;

                Color c = bmp.GetPixel(nx, ny);
                if (c.A > 0)
                    return Color.FromArgb(0, c.R, c.G, c.B);
            }
        }

        return Color.FromArgb(0, 0, 0, 0);
    }
}