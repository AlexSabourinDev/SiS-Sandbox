using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    /// <summary>
    /// This resource type attribute lets you easily convert unity 'Resources' into a string
    /// when serialized via the Inspector window. 
    /// 
    /// You will still need to parse the string to convert it to a valid resource string though
    /// if you are to use it with Resources.Load
    /// 
    /// Example Usage:
    /// 
    /// public class Foo
    /// {
    ///     [SerializedField]
    ///     [ResourceType]
    ///     private string m_Resource = string.Empty;
    /// }
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ResourceTypeAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ResourceTypeAttribute))]
    public class ResourceTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }


            string resourceName = AssetDatabase.GUIDToAssetPath(property.stringValue);
            GameObject prefab = AssetDatabase.LoadAssetAtPath(resourceName, typeof(GameObject)) as GameObject;
            if (prefab)
            {
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefab);
                if(prefabType != PrefabAssetType.Regular && prefabType != PrefabAssetType.Variant)
                {
                    Debug.LogError($"GameObject is not a prefab. {prefab.name}:{prefab.GetInstanceID()}");
                    prefab = null;
                }
                else if(!resourceName.ToLower().Contains("/resources/"))
                {
                    Debug.LogError($"GameObject is not a prefab. {prefab.name}:{prefab.GetInstanceID()}");
                    prefab = null;
                }
            }

            GameObject result = EditorGUI.ObjectField(position, label, prefab, typeof(GameObject), false) as GameObject;
            if(result)
            {
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(result);
                if(prefabType != PrefabAssetType.Regular && prefabType != PrefabAssetType.Variant)
                {
                    Debug.LogError($"GameObject is not a prefab. {result.name}:{result.GetInstanceID()}");
                    property.stringValue = string.Empty;
                }
                else
                {
                    resourceName = AssetDatabase.GetAssetPath(result);
                    if(!resourceName.ToLower().Contains("/resources/"))
                    {
                        Debug.LogError($"GameObject is not a resource. {result.name}:{result.GetInstanceID()}");
                        property.stringValue = string.Empty;
                    }
                    else
                    {
                        property.stringValue = AssetDatabase.AssetPathToGUID(resourceName);
                    }
                }
            }
            else
            {
                property.stringValue = string.Empty;
            }
        }
    }
#endif
}
