using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace unityEssentials.symbolicLinks.extensions.editor
{
    public static class ExtFindEditor
    {
        /// <summary>
        /// find the first asset of type T
        /// </summary>
        /// <typeparam name="T">Type to find</typeparam>
        /// <param name="directory">directory where to search the asset</param>
        /// <returns>first asset found with type T</returns>
        public static T GetAssetByGenericType<T>(string directory = "Assets/", string searchExtension = "*.asset") where T : Object
        {
            List<T> assets = GetAssetsByGenericType<T>(directory, searchExtension);
            if (assets.Count == 0)
            {
                return (null);
            }
            return (assets[0]);
        }


        /// <summary>
        /// find all asset of type T
        /// </summary>
        /// <typeparam name="T">Type to find</typeparam>
        /// <returns>all asset found with type T, return an list<Object>, you have to cast them to use them, see GetAssetsByRaceScriptableObjects for an exemple</returns>
        public static List<T> GetAssetsByGenericType<T>(string directory = "Assets/", string searchExtension = "*.asset") where T : Object
        {
            List<T> AllAssetFound = new List<T>();

            DirectoryInfo dir = new DirectoryInfo(directory);
            FileInfo[] infoAssets = dir.GetFiles(searchExtension, SearchOption.AllDirectories);

            for (int i = 0; i < infoAssets.Length; i++)
            {
                string relativePath = ExtPaths.ConvertAbsoluteToRelativePath(infoAssets[i].FullName);

                System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(relativePath);
                if (assetType == null)
                {
                    continue;
                }
                T asset = (T)AssetDatabase.LoadAssetAtPath(relativePath, typeof(T));
                if (asset)
                {
                    AllAssetFound.Add(asset);
                }
            }
            return (AllAssetFound);
        }

        /// <summary>
        /// get an asset type with the path given
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static T GetAssetByPath<T>(string relativePath) where T : Object
        {
            T asset = (T)AssetDatabase.LoadAssetAtPath(relativePath, typeof(T));
            return (asset);
        }
    }
}