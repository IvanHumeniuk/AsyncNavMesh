using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameObjectToPrefabSwitcher : MonoBehaviour
{

    public List<GameobjectReplacementConfig> objectsToReplace = new List<GameobjectReplacementConfig>();
 
    [ContextMenu("Replace")]
    public void Replace()
    {
#if UNITY_EDITOR
        for (int i = 0; i < objectsToReplace.Count; i++)
        {
            List<GameObject> matchedGameObjects = GetAllLoadedSceneGameObjectsWithName(objectsToReplace[i].name);

            for (int j = 0; j < matchedGameObjects.Count; j++)
            {
                Transform instantiatedPrefab = PrefabUtility.InstantiatePrefab(objectsToReplace[i].prefab) as Transform;
                instantiatedPrefab.name = objectsToReplace[i].name;

                instantiatedPrefab.parent = matchedGameObjects[j].transform.parent;
                instantiatedPrefab.position = matchedGameObjects[j].transform.position;
                instantiatedPrefab.rotation = matchedGameObjects[j].transform.rotation;

                if (objectsToReplace[i].applyScale)
                    instantiatedPrefab.localScale = matchedGameObjects[j].transform.localScale;

                DestroyImmediate(matchedGameObjects[j]);
            }
        }
#endif
    }

    public List<GameObject> GetAllLoadedSceneGameObjectsWithName(string name)
    {
        GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();

        List<GameObject> matchedGameObjects = new List<GameObject>();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            string[] splittedName = gameObjects[i].name.Split(' ');

            if (splittedName[0] == name)
            {
                matchedGameObjects.Add(gameObjects[i]);
            }
        }
        return matchedGameObjects;
    }
}
