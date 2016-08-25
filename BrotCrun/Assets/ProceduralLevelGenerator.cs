using UnityEngine;
using System.Collections;

public class ProceduralLevelGenerator : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData td;

	// Use this for initialization
	void Start ()
	{
	    terrain = this.GetComponent<Terrain>();
        //über td kann direkt die höhen des jeweiligen Terrain modifizieren
	    td = terrain.terrainData;

        ValueNoise vn = new ValueNoise(td.heightmapWidth, td.heightmapHeight);
        vn.calculate();

        //valueNoise in die terreinDatareinbekommen
        td.SetHeights(0,0, vn.getHeightmap());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
