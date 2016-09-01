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

        int radius = chunkSize / 2;
        int radiusSquared = radius * radius;
        int x4;
        int y4;
        int dSquared;
        float f2 = 0;
        float g2 = 0;


        //float distanceToCenter;

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

                    f2 = (float) Mathf.Round((255*Mathf.Sqrt(dSquared))/radius);

                    //1.0 anzahl stripes
                    g2 = (float) (180*(1+Mathf.Atan2(y4, x4)/Mathf.PI*1.0));

                    //drehen
                    g2 += 90;
                    //if (g2 > 360)
                    //{
                    //    g2 -= 360;
                    //}
                }

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
                //if (Mathf.Sin(distanceToCenter) == distanceToCenter2)
                
                if(Mathf.Sin(g2) > 0)
                    map[i, j] = Mathf.Sqrt(f2 / g2 ) * Mathf.Pow(Mathf.Sin(g2), 1.0f);
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
