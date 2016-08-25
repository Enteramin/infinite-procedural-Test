using UnityEngine;
using System.Collections;
using UnityEditor;

//extends Editor
[CustomEditor(typeof(paintTerrain))]
public class TerrainBuilder: Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //target
        paintTerrain myScript = (paintTerrain) target;
        if (GUILayout.Button("Generate Terrain")) //puts button on inspector gui
        {
            myScript.Start(); //start must public
        }
    }

}
