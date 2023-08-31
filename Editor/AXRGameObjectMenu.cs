using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace onAirXR.Server.Editor {
    public static class AXRGameObjectMenu {
        private const int PriorityStart = 10;

        [MenuItem("GameObject/onAirXR/Volume", false, PriorityStart + 1)]
        static void GameObjectVolume(MenuCommand menuCommand) {
            instantiateFromPrefab("Prefabs/AXRVolume.prefab", "AXRVolume", menuCommand);
        }

        private static void instantiateFromPrefab(string prefabPath, string name, MenuCommand menuCommand) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(makeAssetPath(prefabPath));
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = name;

            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, "create " + prefab.name);

            Selection.activeObject = instance;
        }

        private static string makeAssetPath(string assetPath) => Path.Combine("Packages/com.onairxr.server", assetPath);
    }
}
