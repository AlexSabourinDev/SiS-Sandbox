using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Use the ResourceLocatorWindow to configure the data before using.
    /// 
    /// note: The 'Guids' are assumed sorted for faster searches using binary search.
    /// </summary>
    [CreateAssetMenu(fileName = "NewResourceLocatorData")]
    public class ResourceLocatorData : ScriptableObject
    {
        [System.Serializable]
        public struct GuidToIndex
        {
            public string GUID;
            public int Index;
        }

        public struct GuidCompare : IComparer<GuidToIndex>
        {
            public int Compare(GuidToIndex x, GuidToIndex y)
            {
                return x.GUID.CompareTo(y.GUID);
            }
        }

        public List<string> ResourcePaths = new List<string>();
        public List<string> ResourceNames = new List<string>();
        public List<GuidToIndex> Guids = new List<GuidToIndex>();

        public string GetResourcePath(string guid)
        {
            int index = GetResourceIndex(guid);
            return index >= 0 ? ResourcePaths[index] : string.Empty;
        }

        public string GetResourceName(string guid)
        {
            int index = GetResourceIndex(guid);
            return index >= 0 ? ResourceNames[index] : string.Empty;
        }

        public int GetResourceIndex(string guid)
        {
            int index = Guids.BinarySearch(new GuidToIndex() { GUID = guid }, new GuidCompare());
            if (index >= 0)
            {
                return Guids[index].Index;
            }
            return -1;
        }
    }
}