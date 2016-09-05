using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        MixedHeightMap,
        ColourMap,
        Mesh,
        Crater,
        CraterRing,
        CraterFalloff,
        CraterStripes,
        CraterSinW,
        CraterQuadFalloff,
        CraterSidedParable,
        CraterCentralPeak,
        CraterTerrace,
        CraterPseudoRnd,
    };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 239; //less than 255^2: w-1 = 240: 240 has properties of 2,4,6,8,10,12: -2 because of border vertices
    [Range(0, 6)] //makes it to slider
    public int MeshDetails; //lod only for editor

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    [Range(0,1f)]
    public float terrainHeight;
    public bool cleanChunks;
    public bool isFlatshaded;

    [Header("Perlin Noise Settings")]
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance; //decreas in amplitude of octaves. how small these features the whole map changes
    public float lacunarity; //frequencys of octaves. increases number of small features

    public int seed;
    public Vector2 offset;

    [Header("Single Crater Settings")]
    [Range(0, 100)]
    public int craterProbability;

    [Range(0, 2)]
    public float craterSize;
    [Range(0, 10)] //exponent: everything above or below will crash unity
    public float craterIntensity;
    [Range(0, 100)]
    public float NoiseIntensity;

    public Vector2 position;
    public Vector2 ellipse;

    public RockTypes[] rockLevels;

    public CraterType[] craters;

    float[,] craterMap; //stores craterMap
    float[,] craterRing;
    float[,] craterFalloff;
    float[,] craterStripes;
    float[,] craterQuadFalloff;
    float[,] craterSinW;
    float[,] craterSidedParable;
    float[,] craterPseudoRnd;
    float[,] craterCentralPeak;
    float[,] craterTerrace;

    [Header("Randomizer")]
    public bool activateRandomizer;
    public enum TypeOfTerrain
    {
        ownType,
        Earthlike,
        Moonlike
    };
    public TypeOfTerrain terrainType;

    [Header("Settings for Own Type")]
    public float size;
    public float age;
    public float angle;
    public float terrainBrittleness;
    public Vector2 tectonicPlateMovement;
    
    public bool autoUpdate;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //use FallowMap
    void Awake()
    {
        //falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
        //craterMap = CraterGenerator.GenerateCrater(mapChunkSize + 2);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>(); //reference to mapdisplay, gives different options
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, Vector2.zero + offset, normalizeMode)));
        }
        if (drawMode == DrawMode.MixedHeightMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, MeshDetails, isFlatshaded),
                TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Crater)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterGenerator.GenerateCrater(mapChunkSize, craterSize, craterIntensity, position.x, position.y,
                    ellipse.x, ellipse.y)));
        }
        else if (drawMode == DrawMode.CraterRing)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterRingGenerator.GenerateCraterRing(mapChunkSize, craterSize + craters[0].ringWidth,
            craterIntensity + craterSize + craters[0].ringWeight, position.x, position.y, ellipse.x, ellipse.y)));
        }
        else if (drawMode == DrawMode.CraterFalloff)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterFalloffGenerator.GenerateCraterFalloff(mapChunkSize, craterSize + craters[0].ringWidth + craters[0].falloffstart,
            craterIntensity + craterSize + craters[0].falloffWeight, position.x, position.y, ellipse.x, ellipse.y)));
        }
        else if (drawMode == DrawMode.CraterStripes)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterStripesGenerator.GenerateCraterStripes(mapChunkSize, craters[0].stripeSin, craters[0].stripeQuantity)));
        }
        else if (drawMode == DrawMode.CraterSinW)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterSinW.GenerateCraterSinW(mapChunkSize, craters[0].sinWCentress, position.x, position.y, ellipse.x, ellipse.y, craters[0].sinWQuantity)));
        }
        else if (drawMode == DrawMode.CraterQuadFalloff)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterQuadFalloffGenerator.GenerateCraterQuadFalloff(mapChunkSize, craters[0].lineStart, craters[0].lineDirectNSFW)));
        }
        else if (drawMode == DrawMode.CraterSidedParable)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterSidedParable.GenerateCraterSidedParable(mapChunkSize, craters[0].diagScale, craters[0].diagParable, craters[0].diagDirection)));
        }
        else if (drawMode == DrawMode.CraterCentralPeak)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterCentralPeak.GenerateCreaterCentralPeak(mapChunkSize, craters[0].centralSize, craters[0].centralPeakness, position.x + craters[0].centralPosition.x, position.y + craters[0].centralPosition.y, ellipse.x,
            ellipse.y)));
        }
        else if (drawMode == DrawMode.CraterTerrace)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterTerrace.GenerateCraterTerrace(mapChunkSize, craters[0].terraceSize, craters[0].terracePeakness, position.x + craters[0].terracePosition.x, position.y + craters[0].terracePosition.y, ellipse.x,
            ellipse.y)));
        }
        else if (drawMode == DrawMode.CraterPseudoRnd)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterPseudoRndGenerator.GenerateCraterPseudoRnd(mapChunkSize, craters[0].pseudoDensity, craters[0].pseudoPeaks, craters[0].pseudoVal)));
        }
    }

    //Threadening
    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback); //centre so its not always the same chunk
        };

        new Thread(threadStart).Start();
    }

    //Threadening
    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue) //lock: so no other thread cann access this thread
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate  //Meshdata Thread
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    //Gets the Height Map from GeneratteTerrainmesh
    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod, isFlatshaded);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0) //as long there are threads
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue(); //Dequeue: the next thing in the queue
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] map = new float[mapChunkSize + 2, mapChunkSize + 2];

        //fetching 2D NoiseMap from the Noise Class
        // +2 for the border vertices. generates 1 extra noise value on left and right side
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);
        
        System.Random rnd = new System.Random(); //Random percentage to get a crater
        int rndFall = rnd.Next(0, 100);

        //generate 1D Colormap from 2D Noisemap
        Color[] colourMap = new Color[(mapChunkSize + 2) * (mapChunkSize + 2)];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float ringMask = Mathf.Clamp01(craters[0].ringIntensity * craterRing[x, y]);
                float falloffMask = Mathf.Clamp01(craters[0].falloffIntensity * craterFalloff[x, y]);
                float stripeMask = Mathf.Clamp01(craters[0].stripeIntensity * craterStripes[x, y] - craterRing[x, y] * craters[0].stripeWidth);
                float sinusMask = Mathf.Clamp01(craters[0].sinWIntensity * craterSinW[x, y] - craterRing[x, y] * craters[0].sinWWidth);
                float lineMask = Mathf.Clamp01((craters[0].lineIntensity * (craterQuadFalloff[x, y] - craterFalloff[x, y] * 10)));
                float centralMask =
                    Mathf.Clamp01(-(craterCentralPeak[x, y] - 1) - (craterMap[x, y] - sinusMask) * craters[0].centralIntensity);
                float terraceMask = Mathf.Clamp01((((craterTerrace[x, y])) - craterMap[x, y]) * craters[0].terraceIntensity);
                float pseudoMask = Mathf.Clamp01(craters[0].pseudoIntensity*craterPseudoRnd[x, y]);

                    if (falloffMask > terrainHeight)
                        falloffMask = terrainHeight;

                    if (centralMask > craters[0].centralHeight)
                        centralMask = craters[0].centralHeight;

                    if (terraceMask > craters[0].terraceHeight)
                        terraceMask = craters[0].terraceHeight;

                //if(Mathf.Clamp01(-(craterTerrace[x, y] - 1) - (craterMap[x, y]) * craters[0].terraceIntensity) > craters[0].terraceHeight)
                //    craterMap[x, y] = craters[0].terraceHeight;

                //map[x, y] = craterMap[x, y] -
                //            Mathf.Clamp01(craters[0].ringIntensity*craterRing[x, y]) -
                //            Mathf.Clamp01(craters[0].falloffIntensity*craterFalloff[x, y]) -
                //Mathf.Clamp01(craters[0].stripeIntensity*craterStripes[x, y] - craterRing[x, y]*craters[0].stripeWidth) +
                //Mathf.Clamp01(craters[0].sinWIntensity*craterSinW[x, y] - craterRing[x, y]*craters[0].sinWWidth) +
                //Mathf.Clamp01((craters[0].lineIntensity*craterQuadFalloff[x, y] - craterFalloff[x, y]*10)) +
                //Mathf.Clamp01((-(craterCentralPeak[x, y] - 1) - (craterMap[x, y]) * craters[0].centralIntensity));
                //Mathf.Clamp01((-(craterTerrace[x, y])-1) - (craterMap[x, y]) * craters[0].terraceIntensity);

                //terrainheight if cant be changed so terrainHeight has to be changed here
                map[x, y] = (-(terrainHeight) + 1) + Mathf.Abs(noiseMap[x, y] / 100 * NoiseIntensity);

                if (cleanChunks)
                {
                    map[x, y] = craterMap[x, y] - ringMask - falloffMask - stripeMask + sinusMask + lineMask + centralMask - terraceMask - pseudoMask;
                }
                //while looping through noiseMap. use falloff map
                else if (craterProbability >= rndFall)
                {
                    map[x, y] = (craterMap[x, y] - ringMask - falloffMask - stripeMask + sinusMask + lineMask + centralMask - terraceMask) + Mathf.Abs(noiseMap[x, y] / 100 * NoiseIntensity);
                }
                float currentHeight = map[x, y];
                for (int i = 0; i < rockLevels.Length; i++)
                {
                    //sections where we actual assigning the colors
                    if (currentHeight >= rockLevels[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = rockLevels[i].colour; //if its greater, then assign the color
                    }
                    else
                    {
                        break; //only break if its less then the rockLevels height
                    }
                }
            }
        }
        return new MapData(map, colourMap);
    }

    //call automatical whenever one of scripts variables changes in its vector
    void OnValidate()
    {
        //wont allow to drop values below that number
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        //Dont forget to change Draw Mapeditor, too
        craterMap = CraterGenerator.GenerateCrater(mapChunkSize, craterSize, craterIntensity, position.x, position.y, ellipse.x, ellipse.y);

        craterRing = CraterRingGenerator.GenerateCraterRing(mapChunkSize, craterSize + craters[0].ringWidth,
            craterIntensity + craterSize + craters[0].ringWeight, position.x, position.y, ellipse.x, ellipse.y);

        craterFalloff = CraterFalloffGenerator.GenerateCraterFalloff(mapChunkSize, craterSize + craters[0].ringWidth + craters[0].falloffstart,
            craterIntensity + craterSize + craters[0].falloffWeight, position.x, position.y, ellipse.x, ellipse.y);

        craterStripes = CraterStripesGenerator.GenerateCraterStripes(mapChunkSize, craters[0].stripeSin, craters[0].stripeQuantity);

        craterSinW = CraterSinW.GenerateCraterSinW(mapChunkSize, craters[0].sinWCentress, position.x, position.y, ellipse.x, ellipse.y, craters[0].sinWQuantity);

        craterQuadFalloff = CraterQuadFalloffGenerator.GenerateCraterQuadFalloff(mapChunkSize, craters[0].lineStart, craters[0].lineDirectNSFW);

        craterSidedParable = CraterSidedParable.GenerateCraterSidedParable(mapChunkSize, craters[0].diagScale, craters[0].diagParable, craters[0].diagDirection);

        craterCentralPeak = CraterCentralPeak.GenerateCreaterCentralPeak(mapChunkSize, craters[0].centralSize, craters[0].centralPeakness, position.x + craters[0].centralPosition.x, position.y + craters[0].centralPosition.y, ellipse.x,
            ellipse.y);

        craterTerrace = CraterTerrace.GenerateCraterTerrace(mapChunkSize, craters[0].terraceSize, craters[0].terracePeakness, position.x + craters[0].terracePosition.x, position.y + craters[0].terracePosition.y, ellipse.x,
            ellipse.y);

        craterPseudoRnd = CraterPseudoRndGenerator.GenerateCraterPseudoRnd(mapChunkSize, craters[0].pseudoDensity, craters[0].pseudoPeaks, craters[0].pseudoVal);
    }

    //Holds callback data and Mapdata info. Make it Generic <T> so it can handle both mesh data and mapdata
    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback; //structs should be unreadably after creations
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}

//terrain rockLevels for different color for each height.
[System.Serializable]
public struct RockTypes
{
    public string name; // water, grass, rock, etc.
    public float height;
    public Color colour; // color for terrain
}

[System.Serializable]
public struct CraterType
{
    public int craterNr;
    [Header("Ring")]
    public float ringWeight;
    public float ringWidth;
    public float ringIntensity;
    [Header("Falloff")]
    public float falloffstart;
    public float falloffWeight;
    public float falloffIntensity;
    [Header("Stripes")]
    [Range(0,0.5f)]
    public float stripeIntensity;
    public float stripeSin;
    public float stripeQuantity;
    public float stripeWidth;
    public float sinWIntensity;
    public float sinWCentress;
    public float sinWQuantity;
    public float sinWWidth;
    [Header("Direction Modifier")]
    [Range(0,0)]
    public int lineDirectNSFW; //linedirection does not work atm
    public float lineStart;
    public float lineIntensity;
    [Range(0, 6)]
    public int diagDirection;
    public float diagIntensity;
    public float diagScale;
    public float diagParable;
    [Header("Central Peak")]
    public float centralIntensity;
    [Range(0, 100)]
    public float centralNoise;
    public float centralHeight;
    public float centralSize;
    public float centralPeakness;
    public Vector2 centralPosition;
    [Header("Terrace")]
    public float terraceIntensity;
    [Range(0,100)]
    public float terraceNoise;
    public float terraceHeight;
    public float terraceSize;
    public float terracePeakness;
    public Vector2 terracePosition;
    [Header("Brittleness")]
    public float pseudoIntensity;
    public float pseudoPeaks;
    public float pseudoDensity;
    public float pseudoVal;
}

//Gets the heightMap and Colourmap from the GenerateMapMethod
public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}

//Threadening
/*
spread the calculation of the map data and the mash data over multiple frames.
EndlessTerrain will gets Mapgenerator data. But it wont get immediately because its generated over multiple frames

    MapGenerator: RequestMapdata will take Action<MapData>callback
    2. MapDataThread: add mapData + callback queue. 
    3. outside of the method in the update methode, for(queue. Count>0) as long item is in the queue we call map data

    EndlessTerrain: RequestMapData(OnMapDataReceived


*/
