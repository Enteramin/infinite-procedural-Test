﻿using UnityEngine;
using System.Collections;

public static class CraterModification1
{

    //Pseudorandomly sets Dots with sinus functions
    public static float[,] GenerateCraterModification1(int chunkSize, float craterSize, float moda, float modb)
    {
        float[,] map = new float[chunkSize, chunkSize];

        int centerX = 124;
        int centerY = 124;

        float distanceX;
        float distanceY;

        float distanceToCenter;
        float distanceToCenter2;

        int stripes = 3;
        float sawtooth;
        float triangle;
        float square;

        //float distanceToCenter;

        //i and j is coordinate of a point inside the square map
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                //chunkSize /= 2;
                ////take the coordinates and make them in a range from -1 to 1
                float x = i / (float)chunkSize * 2 - 1;
                float y = j / (float)chunkSize * 2 - 1;

                distanceX = (centerX - i) * (centerX - i);
                distanceY = (centerY - j) * (centerY - j);

                // Oval
                float g = 10 * (centerY - j) * (centerY - j);

                distanceToCenter = Mathf.Sqrt(distanceX + distanceY);

                //sawtooth = Mathf.Pow(distanceX / distanceY, 1);
                //triangle = Mathf.Abs((float)(2.0 * sawtooth - 1)) * Mathf.Sin(sawtooth);
                //square = Mathf.SmoothStep(triangle, 9f, 1.5f);

                float x1 = centerX + (distanceToCenter * Mathf.Sin((float)(0.3 * y * Mathf.PI)) + distanceX * Mathf.Tan(distanceX * modb));
                float y1 = (float)(Mathf.Sin((x)) + Mathf.Sin(distanceY));
                sawtooth = Mathf.Sin(Mathf.Pow(distanceX / distanceY, 1));

                sawtooth = Mathf.Pow(distanceToCenter, 1);
                triangle = Mathf.Abs((float)(2.0 * sawtooth - 1)) * Mathf.Sin(sawtooth);
                square = Mathf.SmoothStep(triangle, 9f, 1.5f);

                //number shows how big the crater will be
                distanceToCenter2 = triangle / craterSize;


                //map[i, j] = Evaluate(distanceToCenter2, moda, modb);
                //map[i, j] = (distanceToCenter * (Evaluate(distanceToCenter, moda, modb) * 2 - 1)) - distanceToCenter;
                //map[i, j] = Evaluate(distanceToCenter2, moda, modb) / 2; // / 2 to be under max 1
                //map[i, j] = Logar(distanceToCenter2) / 2;
                //map[i, j] = Logar(distanceToCenter2, moda, modb)/2;
                map[i, j] = Testering(distanceToCenter2, moda, modb);
            }
        }

        return map;
    }

    static float Testering(float value, float a, float b)
    {
        return Mathf.Pow(value, a) * Mathf.Sin(value * a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    //modulates the values so its not linear but a graph
    static float Evaluate(float value, float a, float b)
    {
        //float a = 3f;
        //float b = 2.2f;
        // x^a / ( x^a + (b-bx)^a )
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    static float Sin(float value, float a, float b)
    {
        return (Mathf.PI * b) - ((Mathf.Sin(value) * a) / value);
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

    static float Sq(float value, float a, float b)
    {
        return Mathf.Pow(value, a) + value * b;
    }
}
