
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Utils;


public class SceneLoadManager: Singleton<SceneLoadManager>
{
    private bool isLoading;
    
    [SerializeField] private AssetReference menuScene;
    //[SerializeField] private bool loadMenuStartup = false;

    public AssetReference activeScene;

    [Header("Transition Timing")] 
    [SerializeField, Range(0.01f, 2f)] private float postLoadBlackScreenDuration = 0.35f;
    private AsyncOperationHandle<SceneInstance>? currentSceneHandle; // 检测当前场景加载进度

    /* -------------------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        
        var loadHandle = Addressables.LoadSceneAsync(activeScene, LoadSceneMode.Additive);
        currentSceneHandle = loadHandle;

        loadHandle.Completed += (handle) =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
                return;

            SceneManager.SetActiveScene(handle.Result.Scene);
            EventBus.Publish(new SceneLoadCompleteEvent(handle.Result.Scene, GameModeManager.Instance.CurrentGameMode));   
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
                // 加载成功后，先做落点定位，再广播“场景加载完成”事件。
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
                
                // todo: 硬控摄像机位置归零
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
}