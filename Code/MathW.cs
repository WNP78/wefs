using System;

namespace wefs;

public static class MathW
{
    public static float MoveTowards(float a, float b, float t)
    {
        float d = b - a;
        if (MathF.Abs(d) > t)
            return a + MathF.Sign(d) * t;
        else
            return b;
    }
}
