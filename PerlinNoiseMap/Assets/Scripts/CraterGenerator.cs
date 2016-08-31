using System;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

public static class CraterGenerator
{
    public static float[,] GenerateCrater(int chunkSize, float craterSize, float moda, float modb)
    {
        float[,] map = new float[chunkSize, chunkSize];

        int centerX = 124;
        int centerY = 124;

        float distanceX;
        float distanceY;

        float distanceToCenter;
        float distanceToCenter2;

        int stripes = 240;
        float sawtooth;
        float triangle;
        float square;

            //float distanceToCenter;

            //i and j is coordinate of a point inside the square map
            for (int i = 0; i < chunkSize; i++)
            {
                stripes = (i + 1)*5;
            for (int j = 0; j < chunkSize; j++)
            {
                //chunkSize /= 2;
                ////take the coordinates and make them in a range from -1 to 1
                float x = i / (float)chunkSize * 2 - 1;
                float y = j / (float)chunkSize * 2 - 1;

                distanceX = (centerX - i) * (centerX - i);
                distanceY = (centerY - j) * (centerY - j);

                float x0 = (centerX - i) * (centerX - i);
                float y0 = (centerY - j) * (centerY - j ) ;

                distanceToCenter = Mathf.Sqrt(distanceX + distanceY);

                float x1 = centerX + (distanceToCenter * Mathf.Sin((float) (0.3*y0*Mathf.PI)) + distanceX * Mathf.Tan(distanceX * modb));
                float y1 = (float) (Mathf.Sin((x)) + Mathf.Sin(distanceY));
                float x2 = distanceToCenter * Mathf.Cos((float)(0.3 * x)) - distanceX * 0.5f;
                float y2 = distanceToCenter * Mathf.Sin((float)(0.3 * x)) - distanceY * 0.5f;
                float line = (x1 * y1) -(x2 * y2) * 20;

                float distanceToCenter3 = Mathf.Sqrt(x1 + Mathf.Tan(y1));

                sawtooth = Mathf.Sqrt(Mathf.Cos(Mathf.Pow((y0) / x0 + 1, 1)));
                triangle = Mathf.Abs((float)(2.0 * sawtooth - 1)) * Mathf.Sin(sawtooth);
                square = Mathf.SmoothStep(triangle, 9f, 1.5f);

                float dot = (x + y)/distanceToCenter;

                float f =  (centerX - i) * (centerX - i);
                float g = (centerY - j) * 20*(centerY - j);

                float h = f*g * stripes ;

                float x3 = distanceX + (stripes*Mathf.Cos(i*Mathf.PI/180));
                float y3 = distanceX + (stripes * Mathf.Sin(i * Mathf.PI / 180));

                //number shows how big the crater will be
                distanceToCenter2 = sawtooth / craterSize;
                
                //map[i, j] = Evaluate(distanceToCenter2, moda, modb);
                //map[i, j] = (distanceToCenter * (Evaluate(distanceToCenter, moda, modb) * 2 - 1)) - distanceToCenter;
                //map[i, j] = Evaluate(distanceToCenter2, moda, modb) / 2; // / 2 to be under max 1
                //map[i, j] = Logar(distanceToCenter2) / 2;
                //map[i, j] = Logar(distanceToCenter2, moda, modb)/2;
                if(Mathf.Sin(distanceToCenter) == distanceToCenter2)
                map[i, j] = Evaluate(distanceToCenter, moda, modb);
            }
        }

        return map;
    }

    static float Testering(float value, float a, float b)
    {
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) - Mathf.Pow(b - b * value, a)) ;
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
        return (Mathf.PI*b)-((Mathf.Sin(value)*a)/value);
    }

    static float Fourir(float bob, float a, float b)
    {
        float value = 0;
        for (int i = 0; i < a; i++)
        {
            value = (float) ((Math.PI/10) - Mathf.Sin(i)/i*(Mathf.Cos(i*bob)*b));
        }

        return value;
    }

    static float Sq(float value, float a, float b)
    {
        return Mathf.Pow(value, a)+value*b;
    }
}
