// Main.cs
using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.28.0")]
    public class Main : BaseUnityPlugin
    {
        private ConfigManager _configManager; //配置加载
        private GameComponentManager _gameComponentManager; //游戏组件管理
        private PlayerStatusController _playerStatusController; //核心逻辑
        private DGLabApiClient _apiClient; //API客户端
        private UIManager _uiManager;
        private bool _originalCursorVisibleState;
        private CursorLockMode _originalCursorLockState;

        public void Awake()
        {
            _apiClient = new DGLabApiClient(Logger);
            _configManager = new ConfigManager(this.Config);
            _gameComponentManager = new GameComponentManager(Logger);
            _playerStatusController = new PlayerStatusController(_configManager, _apiClient, Logger);
            _uiManager = new UIManager(_configManager);

            Logger.LogInfo("DGLAB 插件已加载，按 F10 打开设置菜单。");
        }

        public void Start()
        {
            _gameComponentManager.CacheGameComponents();
        }

        public void Update()
        {
            if (Input.GetKeyDown(_configManager.ToggleUiKey.Value))
            {
                _uiManager.IsVisible = !_uiManager.IsVisible;

                if (_uiManager.IsVisible)
                {
                    _originalCursorVisibleState = Cursor.visible;
                    _originalCursorLockState = Cursor.lockState;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = _originalCursorVisibleState;
                    Cursor.lockState = _originalCursorLockState;
                }
            }

            if (!_gameComponentManager.AreComponentsReady())
            {
                _gameComponentManager.CacheGameComponents();
                if (!_gameComponentManager.AreComponentsReady())
                {
                    return;
                }
            }

            _playerStatusController.ProcessPlayerStatusUpdate(_gameComponentManager);
        }

        public void OnGUI()
        {
            _uiManager.OnGUI();
        }
    }
}