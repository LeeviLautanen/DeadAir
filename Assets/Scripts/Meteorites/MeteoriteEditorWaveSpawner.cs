#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeteoriteWaveManager))]
public class MeteoriteEditorWaveSpawner : Editor
{
    private int spawnAmount = 50;
    private int spawnDuration = 5;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeteoriteWaveManager manager = (MeteoriteWaveManager)target;

        EditorGUILayout.Space();
        spawnAmount = EditorGUILayout.IntField("Meteorite count", spawnAmount);
        spawnDuration = EditorGUILayout.IntField("Spawn duration", spawnDuration);

        if (GUILayout.Button("Spawn attack"))
        {
            MeteoriteWaveData waveData = CreateInstance<MeteoriteWaveData>();
            waveData.Amount = spawnAmount;
            waveData.DurationSeconds = spawnDuration;

            if (Application.isPlaying)
                manager.HandleWaveSpawn(waveData);
            else
                Debug.Log("Enter Play Mode first");
        }
    }
}

#endif
