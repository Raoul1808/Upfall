using System;

namespace Upfall;

// Easings functions pulled from https://easings.net/
public static class Easings
{
    public static double InOutQuad(double x)
    {
        return x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2;
    }

    public static double OutQuart(double x)
    {
        return 1.0 - Math.Pow(1 - x, 4);
    }
}
