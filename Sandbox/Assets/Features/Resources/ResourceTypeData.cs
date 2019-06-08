using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    /// <summary>
    /// Attach this component onto any GameObject that you want to get the GUID for.
    /// </summary>
    public class ResourceTypeData : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private string m_GUID;
        public string GUID { get { return m_GUID; } }

#if UNITY_EDITOR
        /// <summary>
        /// Called when we attach the component to a gameobject to assign the GUID (prefabs only)
        /// </summary>
        private void Reset()
        {
            var type = PrefabUtility.GetPrefabAssetType(gameObject);
            if(type == PrefabAssetType.Regular || type == PrefabAssetType.Variant)
            {
                Debug.Log("Setting m_GUID for asset!");
                m_GUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gameObject));
            }
        }
#endif
    }
}


