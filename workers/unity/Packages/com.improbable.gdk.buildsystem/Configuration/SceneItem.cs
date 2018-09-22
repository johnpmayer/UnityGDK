using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Improbable.Gdk.BuildSystem.Configuration
{
    internal class SceneItem
    {
        public bool Included;
        public readonly SceneAsset SceneAsset;
        
        private readonly bool exists;

        public SceneItem(SceneAsset sceneAsset, bool included, SceneAsset[] inAssetDatabase)
        {
            SceneAsset = sceneAsset;
            Included = included;
            exists = inAssetDatabase.Contains(sceneAsset);
        }

        public static SceneItem Drawer(Rect position, SceneItem item)
        {
            var oldColor = GUI.color;
            if (!item.exists)
            {
                GUI.color = Color.red;
            }
            
            var positionWidth = position.width;
            var labelWidth = GUI.skin.toggle.CalcSize(GUIContent.none).x + 5;

            position.width = labelWidth;
            item.Included = EditorGUI.Toggle(position, item.Included);

            position.x += labelWidth;
            position.width = positionWidth - labelWidth;

            EditorGUI.ObjectField(position, item.SceneAsset, typeof(SceneAsset), false);

            GUI.color = oldColor;

            return item;
        }
    }
}
