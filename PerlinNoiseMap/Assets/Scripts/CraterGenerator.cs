﻿using System;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

public static class CraterGenerator
{
    public static float[,] GenerateCrater(int chunkSize, float craterSize, float craterIntensity, float posX, float posY, float ellipseX, float ellipseY, bool weightenedAngle)
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
                distanceX = ellipseX*((centerX- posX) - (i)) * (centerX - (i));
                distanceY = ellipseY*((centerY- posY) - j) * (centerY - j);

                if (weightenedAngle)
                {
                    distanceX = ellipseX * ((j * posX) - i) * ((j * posX) - i);
                    //j always gets one full chunksize before i has a full one. thats why chunksize must be 
                    distanceY = ellipseY * ((chunkSize - i) - j) * ((chunkSize - i) - j);
                }

                // multiplicate for line graph 
                distanceToCenter = Mathf.Sqrt(distanceX + distanceY);

                //number shows how big the crater will be
                distanceToCenter2 = distanceToCenter / (craterSize*10f);

                map[i, j] = IntensityOfCrater(distanceToCenter2, craterIntensity);
            }
        }

        return map;
    }

    static float IntensityOfCrater(float value, float a)
    {
        return (Mathf.Pow(value, a) * value) / value;
    }

    static float Fourir(float bob, float a, float b)
    {
        float value = 0;
        for (int i = 0; i < a; i++)
        {
            value = (float)((Math.PI / 10) - Mathf.Sin(i) / i * (Mathf.Cos(i * bob) * b));
        }

        return value;
    }
}
