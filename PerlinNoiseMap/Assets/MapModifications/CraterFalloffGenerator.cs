﻿using UnityEngine;

public static class CraterFalloffGenerator
{

    //substracts from noise so landmass is fully sorrounded
    public static float[,] GenerateCraterFalloff(int chunkSize, float craterSize, float craterIntensity, float posX, float posY, float ellipseX, float ellipseY, bool weightenedAngle, float modb)
    {
        float[,] map = new float[chunkSize, chunkSize];

        //radius
        int centerX = chunkSize / 2;
        int centerY = chunkSize / 2;

        float distanceX;
        float distanceY;

        //float ellipseX, float ellipseY, bool weightenedAngle

        float distanceToCenter;
        float distanceToCenter2;


        //float distanceToCenter;
        //i and j is coordinate of a point inside the square map
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                //one of them mult with 20 for ellipse
                distanceX = ellipseX * (centerX - posX - i) * (centerX - posX - i);
                distanceY = ellipseY * (centerY - posY - j) * (centerY - posY - j);

                if (weightenedAngle)
                {
                    distanceX = ellipseX * ((j * posX) - i) * ((j * posX) - i);
                    //j always gets one full chunksize before i has a full one. thats why chunksize must be 
                    distanceY = ellipseY * (chunkSize - i - j) * (chunkSize - i - j);
                }

                distanceX /= Mathf.Pow((float)chunkSize * 2, 2);
                distanceY /= Mathf.Pow((float)chunkSize * 2, 2);

                // multiplicate for line graph 
                distanceToCenter = Mathf.Sqrt(distanceX + distanceY);

                //number shows how big the crater will be
                distanceToCenter2 = distanceToCenter / Mathf.Abs(craterSize);

                //for calculating the other masks: The result of dividing with cratersize cannot exceed 1 at the white area
                if (distanceToCenter2 > 1)
                    distanceToCenter2 = 1;

                if (distanceToCenter2 < 0)
                    distanceToCenter2 = 0;

                //-1 for inverse so low numbers are outside
                map[i, j] = IntensityOfCrater(distanceToCenter2, craterIntensity, modb);
            }
        }

        return map;
    }

    static float IntensityOfCrater(float value, float intensity, float modb)
    {
        return (Mathf.Pow(value, intensity) * value) / value;
    }

    static float Fourir(float bob, float a, float b)
    {
        float value = 0;
        for (int i = 0; i < a; i++)
        {
            value = (float)((Mathf.PI / 10) - Mathf.Sin(i) / i * (Mathf.Cos(i * bob) * b));
        }

        return value;
    }

    static float Wsin(float value, float intensity, float modb)
    {
        return -((Mathf.Sin(value)*intensity) /value);
    }
}
