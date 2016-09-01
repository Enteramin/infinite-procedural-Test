using UnityEngine;
using System.Collections;

public static class CraterRingGenerator
{

    //substracts from noise so landmass is fully sorrounded
    public static float[,] GenerateCraterRing(int chunkSize, float craterSize)
    {
        //direction: 0 = top, 1 = right, 2 = bottom, 3 = left, 4 = all sides
        float[,] map = new float[chunkSize, chunkSize];

        float x;
        float y;
        float value = 0;
        int direction = 4; //space holder

        int radius = chunkSize / 2;
        int radiusSquared = radius * radius;
        int x4;
        int y4;
        int dSquared;
        float f2 = 0;
        float g2 = 0;

        //i and j is coordinate of a point inside the square map
        for (int i = 0; i < chunkSize; i++)
        {

            y4 = chunkSize - 1 - i - radius;

            for (int j = 0; j < chunkSize; j++)
            {
                x4 = j - radius;
                dSquared = x4 * x4 + y4 * y4;

                if (dSquared <= radiusSquared)
                {
                    //f2 = (float) Math.Round((255*Math.Sqrt(dSquared))/radius);

                    //g2 = (float) Math.Round(180*(1 + Math.Atan2(y4, x4)/Math.PI));

                    // / radius for uneven round intensity
                    f2 = (float)Mathf.Round((255 * Mathf.Sqrt(dSquared)) / radius);

                    g2 = (float)(180 * (1 + Mathf.Atan2(y4, x4) / Mathf.PI));

                    //drehen
                    g2 += 90;
                    if (g2 > 360)
                    {
                        g2 -= 360;
                    }
                }

                //take the coordinates and make them in a range from -1 to 1
                x = i / (float)chunkSize * 2 - 1;
                y = j / (float)chunkSize * 2 - 1;

                //get the value to use for map, find out which one, x or y, is closest to the edge of the square. which one is closer to 1
                //float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                if (direction == 0)
                    value = -y;

                if (direction == 1)
                    value = -x;

                if (direction == 2)
                    value = y;

                if (direction == 3)
                    value = x;

                if (direction == 4)
                    value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                //map[i, j] = Evaluate(value);

                //Radial symetrical sinus curve. Everytime sinus is above 0 the line gets drawn
                //multiplicating with Sin g2 so values between 0.0 to 1.0 to 0.0 gets drawn too and edges are softer
                if (Mathf.Sin(g2) > 0)
                    map[i, j] = Mathf.Sqrt(f2 / g2) * Mathf.Pow(Mathf.Sin(g2), 1.0f);
            }
        }

        return map;
    }

    //modulates the values so its not linear but a graph
    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;
        // x^a / ( x^a + (b-bx)^a )
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}