using Framework.Event;
using UnityEngine.SceneManagement;
using Utils;

public readonly struct SceneLoadCompleteEvent : IEvent
{
    public readonly Scene LoadedScene;
    public readonly GameMode ModeAfterLoad;

    public SceneLoadCompleteEvent(Scene Scene, GameMode mode)
    {
        LoadedScene = Scene;
        ModeAfterLoad = mode;
    }
}