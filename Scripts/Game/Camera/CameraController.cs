using Framework.Event;
using Utils;

public class CameraController : MonoBehaviour, IEventReceiver<GameModeChangedEvent>
{
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject battleCamera;

    void OnEnable()
    {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }
    
    public void OnEvent(GameModeChangedEvent e)
    {
        switch (e.newMode)
        {
            case GameMode.Explore:
                SetCameraView(CameraView.Explore);
                break;
            case GameMode.Battle:
                SetCameraView(CameraView.Battle);
                break;
        }
    }

    private void SetCameraView(CameraView view)
    {
        bool isFollow = false;
        bool isBattle = false;

        switch (view)
        {
            case CameraView.Explore:
                isFollow = true;
                break;
            case CameraView.Battle:
                isBattle = true;
                break;
        }
        
        followCamera.SetActive(isFollow);
        battleCamera.SetActive(isBattle);
    }
}