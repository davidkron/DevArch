﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Presentation.Coloring
{
    namespace ColoringAlgorithms
    {
        public abstract class ColorDataWithDepth
        {
            public int Depth;
        }
        public class Hsl : ColorDataWithDepth, IColorData
        {
            private readonly AdvancedColor _color;

            public Hsl(double h, double s, double l, int depth)
            {
                Depth = depth;
                if (h > 1)
                    h = h - 1;
                _color = new AdvancedColor(h, s, l);
            }

            public double H => _color.H;
            public double S => _color.S;
            public double L => _color.L;

            public AdvancedColor GetColor()
            {
                return _color;
            }
        }

        public class ColorRange : ColorDataWithDepth, IColorData
        {
            private readonly AdvancedColor _color;
            public readonly double Bottom;
            public readonly double Top;

            public ColorRange(double top, double bottom, double s, double l, int depth)
            {
                Depth = depth;
                _color = new AdvancedColor((bottom + top) / 2, s, l);
                Top = top;
                Bottom = bottom;
            }

            public double S => _color.S;
            public double L => _color.L;

            public AdvancedColor GetColor()
            {
                return _color;
            }
        }

        public class WeightedDistinct : PalletteAlgorithm<Hsl>
        {
            protected override Hsl GetSubColorImplementation(Hsl parentData)
            {
                var newLight = parentData.L*1.05;
                return new Hsl(parentData.H, parentData.S, newLight, parentData.Depth + 1);
            }

            protected override Hsl GetStartingColorDataImplementation()
            {
                return new Hsl(0, 0.4, 0.05, 0);
            }

            protected override IEnumerable<Hsl> GetDistinctColorsImplementation(Hsl parentData, int slices)
            {
                var newLight = parentData.L*1.05 + 1/12.0;
                if (parentData.Depth == 0)
                {
                    var range = Enumerable.Range(0, slices);
                    var hues = range.Select(r => r*(1/(double) slices));
                    return
                        hues.Select(
                            hue =>
                                new Hsl(hue, parentData.S, newLight, parentData.Depth + 1));
                }

                if (parentData.Depth > 2)
                {
                    return Enumerable.Repeat(GetSubColorImplementation(parentData), slices);
                }

                var sizePerSlice = 1/(double) slices;
                var newHue = parentData.H + sizePerSlice/2.0;
                var ranges = new List<Hsl>();
                for (var slice = 0; slice < slices; slice++)
                {
                    ranges.Add(new Hsl(newHue, parentData.S, newLight, parentData.Depth + 1));
                    newHue += sizePerSlice;
                }
                return ranges;
            }
        }

        public class EvenDistinct : WeightedDistinct
        {
            protected override IEnumerable<Hsl> GetDistinctColorsImplementation(Hsl parentData, int slices)
            {
                var newLight = parentData.L*1.05 + 1/12.0;
                IEnumerable<double> hues;
                IEnumerable<int> range;
                if (parentData.Depth == 0)
                {
                    range = Enumerable.Range(0, slices);
                    hues = range.Select(r => r*(1/(double) slices));
                    return
                        hues.Select(
                            hue =>
                                new Hsl(hue, parentData.S, newLight, parentData.Depth + 1));
                }

                range = Enumerable.Range(1, slices);
                hues = range.Select(r => parentData.H + r*(1/((double) slices + 1)));
                return hues.Select(
                    hue =>
                        new Hsl(hue, parentData.S, newLight, parentData.Depth + 1));
            }
        }



        public class HueRangeDivisor : PalletteAlgorithm<ColorRange>
        {
            private readonly int _height;

            public HueRangeDivisor(int height)
            {
                _height = height;
            }

            protected override ColorRange GetSubColorImplementation(ColorRange parentData)
            {
                var sizeLeft = parentData.Top - parentData.Bottom;
                var newLight = GetNextLightLevel(parentData);//parentData.L * 1.05 + sizeLeft / 12;
                return new ColorRange(parentData.Top, parentData.Bottom, parentData.S, newLight,parentData.Depth+1);
            }

            protected override ColorRange GetStartingColorDataImplementation()
            {
                return new ColorRange(1, 0, 0.4, 0.1, 0);
            }

            protected override IEnumerable<ColorRange> GetDistinctColorsImplementation(ColorRange parentData, int slices)
            {
                var sizeLeft = parentData.Top - parentData.Bottom;
                var sizePerSlice = sizeLeft / slices;
                var newLight = GetNextLightLevel(parentData);
                var newBottom = parentData.Bottom;
                var ranges = new List<ColorRange>();
                for (var slice = 0; slice < slices; slice++)
                {
                    var newTop = newBottom + sizePerSlice;
                    ranges.Add(new ColorRange(newTop,newBottom, parentData.S, newLight,parentData.Depth +1));
                    newBottom = newTop;
                }
                return ranges;
            }

            private double GetNextLightLevel(ColorRange parentData)
            {
                var progress = (parentData.Depth + 1.0)/_height;
                var newLight = progress * 0.5 + 0.1;//0.1 + Math.Pow(progress,0.4) * 0.35;
                return newLight;
            }
        }
    }
}