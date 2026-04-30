// ColorSpectrum.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace Fluid_Simulator.Core.ColorManagement
{
    public class ColorSpectrum
    {
        public static Color ValueToColor(double value)
        {
            // Clamp the input value between 0 and 1
            value = Math.Max(0, Math.Min(1, value));

            // Map the value to the wavelength range (380nm to 750nm)
            double wavelength = 450 + value * (750 - 450);

            return WavelengthToColor(wavelength);
        }

        private static Color WavelengthToColor(double wavelength)
        {
            double gamma = 0.8;
            double intensityMax = 255;
            double factor;
            double red, green, blue;

            if (wavelength >= 380 && wavelength < 440)
            {
                red = -(wavelength - 440) / (440 - 380);
                green = 0.0;
                blue = 1.0;
            }
            else if (wavelength >= 440 && wavelength < 490)
            {
                red = 0.0;
                green = (wavelength - 440) / (490 - 440);
                blue = 1.0;
            }
            else if (wavelength >= 490 && wavelength < 510)
            {
                red = 0.0;
                green = 1.0;
                blue = -(wavelength - 510) / (510 - 490);
            }
            else if (wavelength >= 510 && wavelength < 580)
            {
                red = (wavelength - 510) / (580 - 510);
                green = 1.0;
                blue = 0.0;
            }
            else if (wavelength >= 580 && wavelength < 645)
            {
                red = 1.0;
                green = -(wavelength - 645) / (645 - 580);
                blue = 0.0;
            }
            else if (wavelength >= 645 && wavelength <= 750)
            {
                red = 1.0;
                green = 0.0;
                blue = 0.0;
            }
            else
            {
                red = 0.0;
                green = 0.0;
                blue = 0.0;
            }

            // Let the intensity fall off near the vision limits
            if (wavelength >= 380 && wavelength < 420)
                factor = 0.3 + 0.7 * (wavelength - 380) / (420 - 380);
            else if (wavelength >= 645 && wavelength <= 750)
                factor = 0.3 + 0.7 * (750 - wavelength) / (750 - 645);
            else
                factor = 1.0;

            int r = AdjustIntensity(red, factor, gamma, intensityMax);
            int g = AdjustIntensity(green, factor, gamma, intensityMax);
            int b = AdjustIntensity(blue, factor, gamma, intensityMax);

            return new Color(r, g, b);
        }

        private static int AdjustIntensity(double color, double factor, double gamma, double intensityMax)
        {
            if (color == 0.0)
                return 0;
            return (int)Math.Round(intensityMax * Math.Pow(color * factor, gamma));
        }
    }
}
