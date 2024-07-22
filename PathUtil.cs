using System.IO;
using UnityEngine;

namespace Chichiche
{
    public static class PathUtil
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/" + nameof( OpenSaveDataDirectory ))]
        public static void OpenSaveDataDirectory()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }
#endif

        public static string GetSaveDataFilePath(string key)
        {
            return Path.Combine(Application.persistentDataPath, GetFileName(key));
        }

        public static string GetEmbeddedFilePath(string key)
        {
            return Path.Combine(Application.streamingAssetsPath, GetFileName(key));
        }

        static string GetFileName(string key)
        {
            return $"{key.Md5().Replace('/', '_')}.dat";
        }
    }
}