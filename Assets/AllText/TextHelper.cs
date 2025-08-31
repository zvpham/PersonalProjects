using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextHelper : MonoBehaviour
{
    public TextAsset textAsset;
    public void Compile()
    {
        string[] lines = textAsset.text.Split('\n'); // Split by lines

        AllText.currentDictionary = 0;
        AllText.languages = new List<Dictionary<string, string>>();
        string[] size = lines[0].Split(',');
        for(int i = 0; i < size.Length; i++)
        {
            AllText.languages.Add(new Dictionary<string, string>());
        }

        for (int i = 0; i < lines.Length; i++) // Start at 1 to skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            for(int j = 0; j < AllText.languages.Count; j++)
            {
                AllText.languages[j].Add(values[0], values[j + 1]);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextHelper))]
public class TextHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TextHelper me = (TextHelper)target;
        if (GUILayout.Button("Compile"))
        {
            me.Compile();
        }
        DrawDefaultInspector();
    }
}
#endif
