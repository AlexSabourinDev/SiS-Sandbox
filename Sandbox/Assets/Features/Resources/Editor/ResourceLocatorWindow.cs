using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// A window that allows you to configure ResourceLocatorData with a simple click of a button.
    /// 
    /// Parses the asset directory for all resources and builds a map of Guid => { Resource Name, Resource GUID }
    /// </summary>
    public class ResourceLocatorWindow : EditorWindow
    {
        private ResourceLocatorData m_ResourceLocatorData = null;

        [MenuItem("Game/Windows/Resource Locator Window")]
        private static void Create()
        {
            GetWindow<ResourceLocatorWindow>().Show();
        }

        private void OnGUI()
        {
            m_ResourceLocatorData = EditorGUILayout.ObjectField("Resource Locator Data", m_ResourceLocatorData, typeof(ResourceLocatorData), false) as ResourceLocatorData;
            if(GUILayout.Button("Update"))
            {
                UpdateData();
            }
        }

        private void UpdateData()
        {
            if(!m_ResourceLocatorData)
            {
                return;
            }

            // Enumerate all directories of the assets folder for resource directories
            List<string> dirs = new List<string>();
            GetResourceDirectories(Application.dataPath, dirs);
            
            // Fetch all the resource names in resource directories.
            int trim = Application.dataPath.Length - "Assets".Length;
            List<string> resourceNames = new List<string>();
            for(int i = 0; i < dirs.Count; ++i)
            {
                GetResourcePrefabs(trim, dirs[i], resourceNames);
            }

            // Initialize the resource locator data.
            m_ResourceLocatorData.ResourcePaths = resourceNames;
            m_ResourceLocatorData.Guids = new List<ResourceLocatorData.GuidToIndex>();
            m_ResourceLocatorData.ResourceNames = new List<string>();
            var names = m_ResourceLocatorData.ResourceNames;
            var guids = m_ResourceLocatorData.Guids;
            for(int i = 0; i < resourceNames.Count; ++i)
            {
                names.Add(GetResourceName(resourceNames[i]));
                guids.Add(new ResourceLocatorData.GuidToIndex() { GUID = AssetDatabase.AssetPathToGUID(resourceNames[i]), Index = i });
            }
            guids.Sort((x,y) => { return x.GUID.CompareTo(y.GUID); });
            EditorUtility.SetDirty(m_ResourceLocatorData);
        }

        private void GetResourceDirectories(string path, List<string> directories)
        {
            int index = path.IndexOf("Resources");
            if(index > 0 && index >= Application.dataPath.Length)
            {
                return;
            }

            string[] dirs = Directory.GetDirectories(path);
            for(int i = 0; i < dirs.Length; ++i)
            {
                index = dirs[i].IndexOf("Resources");
                if(index > 0 && index >= Application.dataPath.Length)
                {
                    directories.Add(dirs[i]);
                }
                GetResourceDirectories(dirs[i], directories);
            }
        }

        private void GetResourcePrefabs(int relativePathTrim, string path, List<string> resourceNames)
        {
            string[] dirs = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);

            for(int i = 0; i < dirs.Length; ++i)
            {
                GetResourcePrefabs(relativePathTrim, dirs[i], resourceNames);
            }

            for(int i = 0; i < files.Length; ++i)
            {
                string filename = files[i];
                if(filename.Contains(".meta"))
                {
                    continue;
                }
                if(filename.Contains(".prefab"))
                {
                    resourceNames.Add(filename.Substring(relativePathTrim));
                }
            }
        }

        private string GetResourceName(string path)
        {
            int resourcesIndex = path.IndexOf("Resources") + "Resources".Length + 1;
            return path.Substring(resourcesIndex, path.Length - (resourcesIndex + ".prefab".Length));
        }
    }
}
