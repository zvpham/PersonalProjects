using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ResourceManagerHelper : MonoBehaviour
{
    public ResourceManager resourceManager;
    public void Compile()
    {

        for (int i = 0; i < resourceManager.heroes.Count; i++)
        {
            resourceManager.heroes[i].heroIndex = i;
        }

        for (int i = 0; i < resourceManager.mercenaries.Count; i++)
        {
            resourceManager.mercenaries[i].mercenaryIndex = i;
        }
        EditorUtility.SetDirty(resourceManager);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ResourceManagerHelper))]
public class WallSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ResourceManagerHelper me = (ResourceManagerHelper)target;
        if (GUILayout.Button("Compile"))
        {
            me.Compile();
        }
        DrawDefaultInspector();
    }
}
#endif