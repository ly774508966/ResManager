using UnityEngine;
using System;
using System.Collections.Generic;

namespace WLGame
{
    public class ResourceHelper
    {
        /*****************************************
         * 函数说明: 加载系统资源
         * 返 回 值: GameObject
         * 参数说明: prefabName @ Resources目录下的预制体名称
         * 注意事项: 
         *****************************************/
        public static GameObject LoadResource(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return null;
            }

            UnityEngine.Object res = Resources.Load(prefabName);
            if (res == null)
            {
                return null;
            }

            GameObject go = GameObject.Instantiate(res) as GameObject;
            return go;
        }

        /*****************************************
         * 函数说明: 加载系统资源
         * 返 回 值: GameObject
         * 参数说明: prefabName @ Resources目录下的预制体名称
         * 注意事项: 
         *****************************************/
        public static T LoadResource<T>(string prefabName) where T : MonoBehaviour
        {
            UnityEngine.Object res = Resources.Load(prefabName);
            if (res == null)
            {
                return null;
            }
            GameObject go = GameObject.Instantiate(res) as GameObject;
            if (go == null)
            {
                return null;
            }
            T script = go.GetComponent<T>();
            if (script == null)
            {
                script = go.AddComponent<T>();
            }
            return script;
        }
    }
}
