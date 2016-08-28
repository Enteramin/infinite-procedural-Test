using UnityEngine;
using System.Collections;

public static class CraterGenerator
{
    //substracts from noise so landmass is fully sorrounded
    public static float[,] GenerateCrater(int chunkSize, float craterSize, float moda, float modb)
    {
        float[,] map = new float[chunkSize, chunkSize];

        int centerX = 124;
        int centerY = 124;

        float distanceX;
        float distanceY;

        float distanceToCenter;
        float distanceToCenter2;

        //float distanceToCenter;

        //i and j is coordinate of a point inside the square map
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                ////take the coordinates and make them in a range from -1 to 1
                float x = i / (float)chunkSize * 2 - 1;
                float y = j / (float)chunkSize * 2 - 1;

                distanceX = (centerX - i) * (centerX - i);
                distanceY = (centerY - j) * (centerY - j);

                distanceToCenter = Mathf.Sqrt(distanceX + distanceY);

                //number shows how big the crater will be
                distanceToCenter2 = distanceToCenter / craterSize;

                //map[i, j] = Evaluate(distanceToCenter, moda, modb);
                //map[i, j] = (distanceToCenter * (Evaluate(distanceToCenter, moda, modb) * 2 - 1)) - distanceToCenter;
                map[i, j] = Evaluate(distanceToCenter2, moda, modb);
            }
        }

        return map;
    }

    //modulates the values so its not linear but a graph
    static float Evaluate(float value, float a, float b)
    {
        //float a = 3f;
        //float b = 2.2f;
        // x^a / ( x^a + (b-bx)^a )
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    static float Logar(float value)
    {
        return Mathf.Log(value, 2.1f) + Mathf.Log(value, 1.1f);
    }
}
