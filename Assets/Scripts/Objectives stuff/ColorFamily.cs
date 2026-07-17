using UnityEngine;

public enum ColorFamily
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,
    Pink,
    Brown,
    White,
    Gray,
    Black
}

public static class ColorFamilyUtil
{
    public static ColorFamily GetFamily(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);

        if (v <= 0.12f) return ColorFamily.Black;
        if (s <= 0.15f && v >= 0.85f) return ColorFamily.White;
        if (s <= 0.20f) return ColorFamily.Gray;
        if (h >= 0.06f && h <= 0.13f && v < 0.55f) return ColorFamily.Brown;
        if (h < 0.03f || h >= 0.96f) return ColorFamily.Red;
        if (h < 0.08f) return ColorFamily.Orange;
        if (h < 0.17f) return ColorFamily.Yellow;
        if (h < 0.42f) return ColorFamily.Green;
        if (h < 0.68f) return ColorFamily.Blue;
        if (h < 0.83f) return ColorFamily.Purple;
        return ColorFamily.Pink;
    }
}