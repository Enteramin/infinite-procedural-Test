using UnityEngine;
using System.Collections;
using System.Xml;
using JetBrains.Annotations;

public class paintTerrain : MonoBehaviour {

    //class inside class. stores index of the texture which heights the texture starts
    [System.Serializable] //exposes elements in the expector
    public class SplatHeights
    {
        public int textureIndex;
        public int startingHeight;
        //distance for textures to allow to overlap
        public int overlap;
    }

    TerrainData terraindata;
    float[,] newHeightData;

    // alpha channel per texture index
    public SplatHeights[] splatHeights;

    [Header("Perlin Noise Settings")]
    [Range(0.000f, 0.01f)]
    public float bumpiness;
    [Range(0.000f, 1.000f)]
    public float damp;

    [Header("Mountain Settings")]
    public int numMountains;
    [Range(0.001f, 0.5f)]
    public float heightChange;
    [Range(0.0001f, 0.05f)]
    public float sideSlope;

    [Header("Hole Settings")]
    public int numHoles;
    [Range(0.0f, 1.0f)]
    public float holeDepth; // max depth
    [Range(0.001f, 0.5f)]
    public float holeChange; //first initial hole. and than goes up
    [Range(0.0001f, 0.05f)]
    public float holeSlope;

    //normalize. goas to entire array and adds up. and goes again and divide total. 
    void normalize(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    //no sharp cutoffs. floating between textures
    public float map(float value, float sMin, float sMax, float mMin, float mMax)
    {
        return (value - sMin*(mMax - mMin)/(sMax - sMin) + mMin);
    }

    //recursive function
    void Mountain(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terraindata.alphamapWidth) return; //off x range of mape
        if (y <= 0 || y >= terraindata.alphamapHeight) return; //off y range of map
        if (height <= 0) return; //if hit lowest level
        if (newHeightData[x, y] >= height) return; //if run into higher elevation
        newHeightData[x, y] = height;
        Mountain(x-1, y, height-Random.Range(0.001f,slope),slope);
        Mountain(x+1, y, height-Random.Range(0.001f,slope),slope);
        Mountain(x, y-1, height-Random.Range(0.001f,slope),slope);
        Mountain(x, y+1, height-Random.Range(0.001f,slope),slope);
    }

    void Hole(int x, int y, float height, float slope)
    {
        if (x <= 0 || x >= terraindata.alphamapWidth) return; //off x range of mape
        if (y <= 0 || y >= terraindata.alphamapHeight) return; //off y range of map
        if (height <= holeDepth) return; //if hit lowest level
        if (newHeightData[x, y] <= height) return; //if run into higher elevation
        newHeightData[x, y] = height;
        Hole(x - 1, y, height + Random.Range(slope, slope+0.01f), slope);
        Hole(x + 1, y, height + Random.Range(slope, slope+0.01f), slope);
        Hole(x, y - 1, height + Random.Range(slope, slope+0.01f), slope);
        Hole(x, y + 1, height + Random.Range(slope, slope+0.01f), slope);
    }

    //void ApplyRiver()
    //{
    //    for (int i = 0; i < numRivvers; i++)
    //    {
    //        int cx = Random.Range(10, terraindata.alphamapWidth - 10);
    //        int cy = Random.Range(10, terraindata.alphamapHeight - 10);
    //        int xdir = Random.Range(-1, 2);
    //        int ydir = Random.Range(-1, 2);
    //        while (cy >= 0 && cy < terraindata.alphamapHeight && cx > 0 && cx < terraindata.alphamapWidth)
    //        {
    //            RiverCrawler(cx, cy, newHeightData[cx, cy] - digDepth, bankSlope);

    //            if (Random.Range(0, 50) < 5)
    //                xdir = Random.Range(-1, 2);

    //            if (Random.Range(0, 50) < 5)
    //                ydir = Random.Range(0, 2);

    //            cx = cx + xdir;
    //            cy = cy + ydir;
    //        }

    //    }
    //}

    void ApplyHoles()
    {
        for (int i = 0; i < numHoles; i++)
        {
            //height data needs to be between 0 and 1
            int xpos = Random.Range(10, terraindata.alphamapWidth - 10); //10 from the edges
            int ypos = Random.Range(10, terraindata.alphamapHeight - 10);
            float newHeight = newHeightData[xpos, ypos] - holeChange;
            Hole(xpos, ypos, newHeight, holeSlope); //sideslope, how steep its going down

        }
    }



    void ApplyMountains()
    {
        for (int i = 0; i < numMountains; i++)
        {
            //height data needs to be between 0 and 1
            int xpos = Random.Range(10, terraindata.alphamapWidth - 10); //10 from the edges
            int ypos = Random.Range(10, terraindata.alphamapHeight - 10);
            float newHeight = newHeightData[xpos, ypos] + heightChange;
            Mountain(xpos, ypos, newHeight, sideSlope); //sideslope, how steep its going down

        }
    }
    
    void ApplyPerlin()
    {
        for (int y = 0; y < terraindata.alphamapHeight; y++)
        {
            for (int x = 0; x < terraindata.alphamapWidth; x++)
            {
                newHeightData[x, y] = Mathf.PerlinNoise(x*bumpiness, y*bumpiness)*damp;
            }
        }
    }

    //creates all splat maps and stores textures in the terrain
    public void Start()
    {
        //holds all the height data in it
        terraindata = Terrain.activeTerrain.terrainData;
        float[,,] splatmapData = new float[terraindata.alphamapWidth,terraindata.alphamapWidth,terraindata.alphamapLayers];

        //stores all the heightvalues for the terrain. height is determent by the number that is in the settings for the terrain
        newHeightData = new float[terraindata.alphamapWidth, terraindata.alphamapHeight];

        ApplyPerlin();
        ApplyMountains();
        ApplyHoles();
        //ApplyRiver();
        terraindata.SetHeights(0, 0, newHeightData);

        // 2 for loops for vertex 
        for (int y = 0; y < terraindata.alphamapHeight; y++)
        {
            for (int x = 0; x < terraindata.alphamapWidth; x++)
            {
                //gets heights with. y,x because splatmaps is inverse
                float terrainHeigt = terraindata.GetHeight(y, x);

                float [] splat = new float[splatHeights.Length];
                
                for (int i = 0; i < splatHeights.Length; i++)
                {
                    float thisNoise = map(Mathf.PerlinNoise(x*0.03f, y*0.03f),0f,1f, 0.5f, 1f);
                    float thisHeightStart = splatHeights[i].startingHeight * thisNoise - splatHeights[i].overlap * thisNoise;

                    float nextHeightStart = 0;
                    if (i != splatHeights.Length - 1)
                    {
                        nextHeightStart = splatHeights[i + 1].startingHeight * thisNoise + splatHeights[i + 1].overlap * thisNoise;
                    }



                    if (i == splatHeights.Length - 1 && terrainHeigt >= thisHeightStart) //last texture 
                        splat[i] = 1;
                    else if (terrainHeigt >= thisHeightStart && terrainHeigt <= nextHeightStart) // if its greater than that before and smaller then the next to stop overlapping
                        //fully turn on if height for the texture is given
                        splat[i] = 1;
                }

                normalize(splat);

                for (int j = 0; j < splatHeights.Length; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }

            terraindata.SetAlphamaps(0,0,splatmapData);
        }

    }

    

}
