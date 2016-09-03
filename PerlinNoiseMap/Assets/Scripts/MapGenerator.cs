using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum TypeOfTerrain { own, Earthlike, Moonlike };
    public TypeOfTerrain terrainType;

    public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap, Crater, CraterRing, CraterFalloff, CraterStripes, CraterQuadFalloff, CraterSinW, CraterSidedParable, CraterPseudoRnd, CraterMod1, CraterMod2 };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 239; //less than 255^2: w-1 = 240: 240 has properties of 2,4,6,8,10,12: -2 because of border vertices
    [Range(0, 6)] //makes it to slider
    public int LOD; //lod only for editor
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance; //decreas in amplitude of octaves. how small these features the whole map changes
    public float lacunarity; //frequencys of octaves. increases number of small features

    public int seed;
    public Vector2 offset;

    [Range(0, 100)]
    public int craterProbability;

    public float craterSize;
    [Range(0, 10)] //exponent: everything above or below will crash unity
    public float craterIntensity;
    public float modb;

    public Vector2 position;
    public Vector2 ellipse;
    public bool weightenedAngle;

    public bool cleanChunks;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool isFlatshaded;

    public bool autoUpdate;

    public RockTypes[] rockLevels;

    public CraterType[] craters;

    float[,] falloffMap; //stores the falloffMap
    float[,] craterMap; //stores craterMap
    float[,] craterRing;
    float[,] craterFalloff;
    float[,] craterStripes;
    float[,] craterQuadFalloff;
    float[,] craterSinW;
    float[,] craterSidedParable;
    float[,] craterPseudoRnd;
    float[,] craterMod1;
    float[,] craterMod2;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //use FallowMap
    void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
        //craterMap = CraterGenerator.GenerateCrater(mapChunkSize + 2);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>(); //reference to mapdisplay, gives different options
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, LOD, isFlatshaded),
                TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
        else if (drawMode == DrawMode.Crater)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterGenerator.GenerateCrater(mapChunkSize, craterSize, craterIntensity, position.x, position.y,
                    ellipse.x, ellipse.y, weightenedAngle)));
        }
        else if (drawMode == DrawMode.CraterRing)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterRingGenerator.GenerateCraterRing(mapChunkSize, craterSize + craters[0].ringWidth,
            craterIntensity + craterSize + craters[0].ringWeight, position.x, position.y, ellipse.x, ellipse.y, weightenedAngle, modb)));
        }
        else if (drawMode == DrawMode.CraterFalloff)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterFalloffGenerator.GenerateCraterFalloff(mapChunkSize, craterSize + craters[0].ringWidth + craters[0].falloffstart,
            craterIntensity + craterSize + craters[0].falloffIntensity, position.x, position.y, ellipse.x, ellipse.y, weightenedAngle, modb)));
        }
        else if (drawMode == DrawMode.CraterStripes)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterStripesGenerator.GenerateCraterStripes(mapChunkSize, craterSize, craterIntensity, modb)));
        }
        else if (drawMode == DrawMode.CraterQuadFalloff)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterQuadFalloffGenerator.GenerateCraterQuadFalloff(mapChunkSize, craterSize, craterIntensity, modb)));
        }
        else if (drawMode == DrawMode.CraterSinW)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterSinW.GenerateCraterSinW(mapChunkSize, craterSize, craterIntensity, position.x, position.y,
                    ellipse.x, ellipse.y, modb)));
        }
        else if (drawMode == DrawMode.CraterSidedParable)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterSidedParable.GenerateCraterSidedParable(1,2)));
        }
        else if (drawMode == DrawMode.CraterPseudoRnd)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(CraterPseudoRndGenerator.GenerateCraterPseudoRnd(mapChunkSize, craterSize, craterIntensity, modb)));
        }
        else if (drawMode == DrawMode.CraterMod1)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterModification1.GenerateCraterModification1(mapChunkSize, craterSize, craterIntensity,
                    position.x, position.y, ellipse.x, ellipse.y, weightenedAngle)));
        }
        else if (drawMode == DrawMode.CraterMod2)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(CraterModification2.GenerateCraterModification2(mapChunkSize, craterSize, craterIntensity,
                    position.x, position.y, ellipse.x, ellipse.y, weightenedAngle)));
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
                if (cleanChunks)
                {
                    //after test endcomment
                    //noiseMap[x, y] = craterMap[x, y];

                    if (craterRing[x, y] * craters[0].ringIntensity >= 0.8f)
                        craterRing[x, y] = 0.8f;

                    if (craterRing[x, y] * craters[0].ringIntensity <= 0.6f)
                        craterRing[x, y] = 0.6f;

                    //if (craterFalloff[x, y] >= 0.8f)
                    //    craterFalloff[x, y] = 0.8f;

                    //if (craterFalloff[x, y] <= 0.6f)
                    //    craterFalloff[x, y] = 0.6f;

                    noiseMap[x, y] = craterMap[x, y] + craterFalloff[x,y] - (craterRing[x, y] * craters[0].ringIntensity) - ((craterStripes[x, y] * craters[0].stripeIntensity) - craterMap[x, y]);
                }
                //while looping through noiseMap. use falloff map
                else if (craterProbability >= rndFall)
                {
                    noiseMap[x, y] = (craterMap[x, y] - noiseMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
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


        return new MapData(noiseMap, colourMap);
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

        //runs the falloffmap even when games not run
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);

        craterMap = CraterGenerator.GenerateCrater(mapChunkSize, craterSize, craterIntensity, position.x, position.y, ellipse.x, ellipse.y, weightenedAngle);

        craterRing = CraterRingGenerator.GenerateCraterRing(mapChunkSize, craterSize + craters[0].ringWidth,
            craterIntensity + craterSize + craters[0].ringWeight, position.x, position.y, ellipse.x, ellipse.y, weightenedAngle, modb);

        craterFalloff = CraterFalloffGenerator.GenerateCraterFalloff(mapChunkSize, craterSize + craters[0].ringWidth + craters[0].falloffstart,
            craterIntensity + craterSize + craters[0].falloffIntensity, position.x, position.y, ellipse.x, ellipse.y, weightenedAngle, modb);

        craterStripes = CraterStripesGenerator.GenerateCraterStripes(mapChunkSize, craterSize, craterIntensity, modb);

        craterQuadFalloff = CraterQuadFalloffGenerator.GenerateCraterQuadFalloff(mapChunkSize, craterSize, craterIntensity, modb);

        craterSinW = CraterSinW.GenerateCraterSinW(mapChunkSize, craterSize, craterIntensity, position.x, position.y, ellipse.x, ellipse.y, modb);

        craterSidedParable = CraterSidedParable.GenerateCraterSidedParable(1, 2);

        craterPseudoRnd = CraterPseudoRndGenerator.GenerateCraterPseudoRnd(mapChunkSize, craterSize, craterIntensity, modb);

        craterMod1 = CraterModification1.GenerateCraterModification1(mapChunkSize, craterSize, craterIntensity, position.x, position.y, ellipse.x,
            ellipse.y, weightenedAngle);

        craterMod2 = CraterModification2.GenerateCraterModification2(mapChunkSize, craterSize, craterIntensity, position.x, position.y, ellipse.x,
            ellipse.y, weightenedAngle);

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
    public string craterName;
    public float ringWeight;
    public float ringWidth;
    public float ringIntensity;
    public float stripeIntensity;
    public float falloffstart;
    public float falloffIntensity;
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
