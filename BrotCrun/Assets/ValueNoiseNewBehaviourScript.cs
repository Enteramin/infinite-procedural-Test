using UnityEngine;
using System.Collections;

public class ValueNoise
{

    public readonly int WIDTH;
    public readonly int HEIGHT;

    //2Dim array für alle unterschiedlichen Werte zwischen 0 und 1
    private float[,] heightMap;
    public int octaves = 8;
    //wie viele Punkte in breite und höhe in der Oktave generiert werden sollen
    public int startFrequencyX = 3;
    public int startFrequencyY = 3;
    //zufallszahlen die generiert werden
    public float alpha = 20;



    public ValueNoise(int width, int height)
    {
        WIDTH = width;
        HEIGHT = height;

        heightMap = new float[width,height];
    }

    //frequence die sich über die einzelnen oktaven ändern 
    public void calculate()
    {
        int currentFrequencyX = startFrequencyX;
        int currentFrequencyY = startFrequencyY;
        float currentAlpha = alpha;

        //wie viele oktaven durchgegangen werden
        for (int oc = 0; oc < octaves; oc++)
        {
            // in jeder weiteren Oktave doppelt so viele Punkte aber nur mit der hälfte der Werte von der Oktave davor
            if (oc > 0)
            {
                currentFrequencyX *= 2;
                currentFrequencyY *= 2;
                currentAlpha /= 2;
            }

            //für die konkreten Punkte die nicht zwischen drin sind auch zuffalswerte Generieren
            //Zufallspunkte auserhalb des Randes generieren mit +1, damit Rand auch berechnet wird
            float[,] discretePoints = new float[currentFrequencyX+1,currentFrequencyY+1];
            for (int i = 0; i < currentFrequencyX + 1; i++)
            {
                for (int k = 0; k < currentFrequencyY + 1; k++)
                {
                    discretePoints[i, k] = UnityEngine.Random.Range(-currentAlpha, currentAlpha);
                }
            }


            //über gesamten ValueNoise drüberiterieren(über die einzelnen Pixel)
            //for schleifen haben jeweilige punkte koordinaten
            for (int i = 0; i < WIDTH; i++)
            {
                for (int k = 0; k < HEIGHT; k++)
                {
                    float currentX = i/(float) WIDTH*currentFrequencyX;
                    float currentY = k/(float) HEIGHT*currentFrequencyY;
                    int indexX = (int) currentX;
                    int indexY = (int) currentY;

                    //Berechnen der jeweiligen Gewichte. erst zwischen zwei punkten und dann dieses Gewicht zwischen zwei anderen Punkte um ein dreieck zu erhalten
                    //t(das hintere teil) ist nur prozentsatz. ist es ganz links = 0, ganz rechts = 1
                    float w0 = interpolate(discretePoints[indexX, indexY], discretePoints[indexX + 1, indexY], currentX - indexX);
                    float w1 = interpolate(discretePoints[indexX, indexY + 1], discretePoints[indexX + 1, indexY + 1], currentX - indexX);

                    //ergebnis der Interpolation ausrechnen
                    float w = interpolate(w0, w1, currentY - indexY);
                    //ergebnis auf die jeweilige heightmap addieren
                    heightMap[i, k] += w;
                }
            }
        }

        normalize();

    }

    //WErte normaliesieren (zwischen 0 und 1 bringen)
    private void normalize()
    {
        //was ist der min wert
        float min = float.MaxValue;
        //min mit max wert initialisieren und schauen obs im array was kleineres gibt
        for (int i = 0; i < WIDTH; i++)
        {
            for (int k = 0; k < HEIGHT; k++)
            {
                if (heightMap[i, k] < min)
                {
                    min = heightMap[i, k];
                }
            }
        }

        //min wert garantiert auf der 0
        for (int i = 0; i < WIDTH; i++)
        {
            for (int k = 0; k < HEIGHT; k++)
            {
                heightMap[i, k] -= min;
            }
        }


        float max = float.MinValue;
        for (int i = 0; i < WIDTH; i++)
        {
            for (int k = 0; k < HEIGHT; k++)
            {
                if (heightMap[i, k] < max)
                {
                    max = heightMap[i, k];
                }
            }
        }

        //max garantiert auf 1
        for (int i = 0; i < WIDTH; i++)
        {
            for (int k = 0; k < HEIGHT; k++)
            {
                heightMap[i, k] /= max;
            }
        }

    }

    private float interpolate(float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    public float[,] getHeightmap()
    {
        return heightMap;
    }
}
