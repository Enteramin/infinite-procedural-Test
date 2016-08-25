using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        //reference mapGen to target
        MapGenerator mapGen =(MapGenerator) target;

        //autoupdate: if any value changed
        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapIneditor();
            }
        }
        //Generate Button
        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapIneditor();
        }
    }
}
