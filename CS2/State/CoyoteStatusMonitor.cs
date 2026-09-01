using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.CoyoteMonitor", "CoyoteMonitor", "1.0.0")]
    public class CoyoteStatusMonitor : BaseUnityPlugin
    {
        public static int CurrentStrength { get; private set; } = 0;

        private DGLabApiClient _apiClient;

        private void Awake()
        {
            _apiClient = new DGLabApiClient(Logger);
            Logger.LogInfo("[CoyoteStatusMonitor] 独立插件已加载");
        }

        private void Start()
        {
            Logger.LogInfo("[CoyoteStatusMonitor] Start 被调用");
            StartCoroutine(FetchLoop());
        }

        private IEnumerator FetchLoop()
        {
            var wait = new WaitForSeconds(0.5f);
            string url = "http://127.0.0.1:8920/api/v2/game/all";

            while (true)
            {
                yield return StartCoroutine(FetchStrength(url));
                yield return wait;
            }
        }

        private IEnumerator FetchStrength(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    string strengthStr = ExtractStrength(response);
                    if (int.TryParse(strengthStr, out int strength))
                    {
                        _apiClient.SetStrengthFromHub(strength);
                        CurrentStrength = strength;
                        // 可选日志：按需取消注释
                        // Logger.LogInfo($"[CoyoteStatusMonitor] 强度更新: {strength}");
                    }
                    else
                    {
                        Logger.LogError($"[CoyoteStatusMonitor] 解析强度失败: '{strengthStr}'");
                    }
                }
                else
                {
                    Logger.LogError($"[CoyoteStatusMonitor] 网络请求失败: {request.error}");
                }
            }
        }

        private string ExtractStrength(string json)
        {
            string key = "\"strength\":";
            int idx = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx == -1) return "0";

            idx += key.Length;
            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;

            int end = idx;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;

            return json.Substring(idx, end - idx);
        }
    }
}