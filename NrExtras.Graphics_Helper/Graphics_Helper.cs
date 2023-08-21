using System.Drawing;
using System.Reflection;

namespace NrExtras.Graphics_Helper
{
    public static class Graphics_Helper
    {
        /// <summary>
        /// draw rectangles to image
        /// </summary>
        /// <param name="inImagePath">image path</param>
        /// <param name="rects">list of rectangles to draw</param>
        /// <param name="color">Line color</param>
        /// <param name="rectangleLineWidth_pixels">Line width in pixels. default 1</param>
        /// <returns>Bitmap object</returns>
        public static Bitmap DrawRectanglesToImage(string inImagePath, List<Rectangle> rects, Color color, int rectangleLineWidth_pixels = 1)
        {
            if (File.Exists(inImagePath) == false) throw new Exception("Image not found");

            try
            {
                Bitmap out_bitmap = new Bitmap(inImagePath);
                Pen thick_pen = new Pen(color, rectangleLineWidth_pixels);

                //draw rectangles
                foreach (Rectangle rect in rects)
                    using (Graphics gr = Graphics.FromImage(out_bitmap))
                        gr.DrawRectangle(thick_pen, rect); //draw rectangle

                //return result
                return out_bitmap;

            }
            catch (Exception ex)
            {
                throw new Exception("Error drawing rectangls. Error: " + ex.Message);
            }
        }

        /// <summary>
        /// get all colors from known color names
        /// </summary>
        /// <returns>list of colors</returns>
        public static List<Color> ColorStructToList()
        {
            return typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                                .Select(c => (Color)c.GetValue(null, null))
                                .ToList();
        }

        /// <summary>
        /// get x random colors
        /// </summary>
        /// <param name="nColors">num of colors to pick</param>
        /// <returns>list of found colors</returns>
        /// <exception cref="Exception">incase we need more colors then known color names we have</exception>
        public static List<Color> GetRandomColors(int nColors)
        {
            //get all colors
            List<Color> colors = ColorStructToList();
            //check that we didn't exceed the colors count
            if (nColors > colors.Count) throw new Exception("Max different colors is " + colors.Count + ". we cannot exceed it");
            //return only nColors
            return RandomCollection.RandomCollection.GetRandomElements(colors, nColors);
        }
    }
}