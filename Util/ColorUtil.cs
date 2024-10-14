using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Util
{
    // Thanks ChatGPT for pretty color inversion function
    internal class ColorUtil
    {
        public static (double H, double S, double L) RgbToHsl(int r, int g, int b)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));

            double h = 0, s, l = (max + min) / 2.0;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (max == rNorm)
                {
                    h = (gNorm - bNorm) / d + (gNorm < bNorm ? 6 : 0);
                }
                else if (max == gNorm)
                {
                    h = (bNorm - rNorm) / d + 2;
                }
                else
                {
                    h = (rNorm - gNorm) / d + 4;
                }

                h /= 6;
            }

            return (h * 360, s, l);
        }

        // Convert HSL back to RGB
        public static (int R, int G, int B) HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            h /= 360.0;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                Func<double, double, double, double> hue2rgb = (p, q, t) =>
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                    if (t < 1 / 3.0) return q;
                    if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                    return p;
                };

                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                r = hue2rgb(p, q, h + 1 / 3.0);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3.0);
            }

            return ((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        // Get adjusted inverse color with lower saturation and higher contrast
        public static (int R, int G, int B) GetAdjustedInverseColor(int r, int g, int b)
        {
            // Invert the RGB values
            int invR = 255 - r;
            int invG = 255 - g;
            int invB = 255 - b;

            // Convert inverted RGB to HSL
            var (h, s, l) = RgbToHsl(invR, invG, invB);

            // Check if the color is grayscale
            bool isGray = s == 0;

            if (!isGray)
            {
                // Adjust lightness to increase contrast for non-gray colors
                l = l > 0.5 ? 0.3 : 0.7;

                // Reduce saturation by half for non-grayscale
                s *= 0.5;
            }
            else
            {
                // For grayscale colors, apply stronger contrast
                if (l > 0.5)
                {
                    l = 0.2; // Push gray towards dark
                }
                else
                {
                    l = 0.8; // Push gray towards light
                }
            }

            // Convert back to RGB
            var (newR, newG, newB) = HslToRgb(h, s, l);

            return (newR, newG, newB);
        }
    }
}
