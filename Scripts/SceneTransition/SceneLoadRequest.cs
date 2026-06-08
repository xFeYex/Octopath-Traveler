
using UnityEngine.AddressableAssets;
using Utils;

public struct SceneLoadRequest
{
    public readonly AssetReference Scene;
    public readonly FadeStyle  FadeStyle;
    public readonly GameMode ModeAfterLoad;
    public readonly string SpawnPointId;
    
    public readonly float FadeOutDurationOverride;
    public readonly float FadeInDurationOverride;

    public SceneLoadRequest(AssetReference scene, FadeStyle fadeStyle, GameMode modeAfterLoad, string spawnPointId = null,
        float fadeOutDurationOverride = -1, float fadeInDurationOverride = -1)
    {
        Scene = scene;
        FadeStyle = fadeStyle;
        ModeAfterLoad = modeAfterLoad;
        SpawnPointId = spawnPointId;
        FadeOutDurationOverride =  fadeOutDurationOverride;
        FadeInDurationOverride = fadeInDurationOverride;
    }
}