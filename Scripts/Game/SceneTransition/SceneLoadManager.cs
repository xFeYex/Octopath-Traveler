
using System;
using Unity.Cinemachine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Utils;


public class SceneLoadManager: Singleton<SceneLoadManager>
{
    private bool isLoading;
    public bool IsLoding => isLoading;
    
    [SerializeField] private AssetReference menuScene;
    [SerializeField] private bool loadMenuStartup = false;
    public AssetReference MenuScene => menuScene;

    public AssetReference activeScene;
    [SerializeField] private AssetReference _startupGamePlayScene;
    public AssetReference StartupGamePlayScene => _startupGamePlayScene;

    [Header("Transition Timing")] 
    [SerializeField, Range(0.01f, 2f)] private float postLoadBlackScreenDuration = 0.35f;
    private AsyncOperationHandle<SceneInstance>? currentSceneHandle; // 检测当前场景加载进度

    /* -------------------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        var firstScene = loadMenuStartup ? menuScene : activeScene;
        var loadHandle = Addressables.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        currentSceneHandle = loadHandle;

        loadHandle.Completed += (handle) =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
                return;

            SceneManager.SetActiveScene(handle.Result.Scene);
            if (loadMenuStartup)
                GameModeManager.Instance.RequestChangeMode(GameMode.InteractionMenu);
        };
    }

    /* -------------------------------------------------------------------------------------- */
    
    public void RequestLoad(SceneLoadRequest request)
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(LoadFlow(request));
    }

    private IEnumerator LoadFlow(SceneLoadRequest request)
    {
        try
        {
            // 1.先切到过渡模式，锁住原场景里的普通输入和交互。
            GameModeManager.Instance.RequestChangeMode(GameMode.InteractionMenu);
            
            // 2.根据这次请求配置Fade样式和单次时长覆盖。
            FadeController.Instance.SetStyle(request.FadeStyle);
            FadeController.Instance.SetNextFadeOutDuration(
                request.FadeOutDurationOverride,
                request.FadeInDurationOverride);
            
            // 3.先等整屏淡出完成，再真正开始卸载/加载。
            bool fadeOutComplete = false;
            FadeController.Instance.FadeOut(() => fadeOutComplete = true);
            yield return new WaitUntil(() => fadeOutComplete);
            
            // 4.如果旧场景还在，先把它卸掉
            if (currentSceneHandle.HasValue && currentSceneHandle.Value.IsValid())
            {
                yield return Addressables.UnloadSceneAsync(currentSceneHandle.Value, true);
                currentSceneHandle = null;
            }
            
            // 5.再加载目标场景
            var loadHandle = Addressables.LoadSceneAsync(request.Scene, LoadSceneMode.Additive, true);
            yield return loadHandle;
            
            bool loadSucceeded = loadHandle.Status == AsyncOperationStatus.Succeeded;
            if (loadSucceeded)
            {
                currentSceneHandle = loadHandle;
                activeScene = request.Scene;
                
                SceneManager.SetActiveScene(loadHandle.Result.Scene);
                // 6. 加载成功后，先做落点定位，再广播“场景加载完成”事件。
                EventBus.Publish(new SceneLoadCompleteEvent(loadHandle.Result.Scene, request.ModeAfterLoad));
            }
            else
            {
                Debug.LogError($"Scene {request.Scene} failed to load");
            }
            
            // 7.如果这次是回探索场景，就先在黑场里恢复人物和队伍，再开始淡入。
            bool restoreExploreModeBeforeFadeIn = loadSucceeded && request.ModeAfterLoad == GameMode.Explore;
            if (restoreExploreModeBeforeFadeIn)
            {
                GameModeManager.Instance.RequestChangeMode(GameMode.Explore);

                yield return null;
                
                // 硬控摄像机位置归零
                ApplySpawnPointAfterLoad(request, loadHandle.Result.Scene);
            }
            // 8.新场景加载好后，再留一小段黑场缓冲
            yield return new WaitForSecondsRealtime(postLoadBlackScreenDuration);
            
            // 9.黑场准备做完后，再把画面淡入回来
            bool fadeInComplete = false;
            FadeController.Instance.FadeIn(() => fadeInComplete = true);
            yield return new WaitUntil(() => fadeInComplete);
            
            // 10.如果目标模式不是Explore，就保持“淡入后再切模式”的节奏
            if (loadSucceeded && !restoreExploreModeBeforeFadeIn)
            {
                GameModeManager.Instance.RequestChangeMode(request.ModeAfterLoad);
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    #region 传送

    private SceneSpawnPoint FindSpawnPoint(Scene scene, string targetSpawnId)
    {
        SceneSpawnPoint fallback = null;
        SceneSpawnPoint first = null;

        var rootsObj = scene.GetRootGameObjects();
        foreach (var root in rootsObj)
        {
            var spawnPoint = root.GetComponent<SceneSpawnPoint>();
            if (spawnPoint == null) continue;

            if (spawnPoint.SpawnId == targetSpawnId)
                return spawnPoint;
            
            if (first == null) 
                first = spawnPoint;

            if (spawnPoint.IsDefaultFallback && fallback == null)
            {
                fallback = spawnPoint;
            }
        }
        
        return fallback ?? first;
    }

    /// <summary>
    /// 场景加载完成后，根据请求的SpawnPointId定位玩家与队伍。
    /// </summary>
    private void ApplySpawnPointAfterLoad(SceneLoadRequest request, Scene loadedScene)
    {
        var spawnPoint = FindSpawnPoint(loadedScene, request.SpawnPointId);

        PlayerController player = FindObjectOfType<PlayerController>();
        // 1.获取玩家对象并计算位移（用于相机切镜）
        var delta = spawnPoint.transform.position - player.transform.position;
        // 2.直接交给PartyManager处理玩家和队伍落位。
        PartyManager.Instance.TeleportPartyTo(spawnPoint.transform.position, spawnPoint.transform.rotation);
    }

    /// <summary>
    /// 强制主相机与虚拟相机瞬间切镜，彻底消除Cinemachine阻尼滑行感
    /// </summary>
    private void SnapMainCamera(Vector3 delta)
    {
        Camera mainCamera = Camera.main;
        
        // 1.先把主相机的位置一起挪过去，避免画面第一帧拉扯。
        mainCamera.transform.position += delta;
        
        // 2．遍历场景的虚拟相机，告诉 Cinema chine 目标发生了瞬移。
        foreach (CinemachineCamera vcan in FindObjectsOfType<CinemachineCamera>())
        {
            // 先把上一帧状态标成无效，下一帧就不会再从旧位置慢慢过渡。
            vcan.PreviousStateIsValid = false;
            
            // 再把这次位移量告诉虚拟相机，让跟随目标立刻对齐。
            vcan.OnTargetObjectWarped(vcan.Follow, delta);
        }
        // 3。最后重启一下Brain，把可能残留的镜头混合状态清掉。
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        brain.enabled = false;
        brain.enabled = true;
    }

    #endregion
}