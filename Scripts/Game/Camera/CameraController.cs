using System;
using Framework.Event;
using Utils;

public class CameraController : MonoBehaviour, 
    IEventReceiver<GameModeChangedEvent>,
    IEventReceiver<BattleEndedEvent>
{
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject battleCamera;
    [SerializeField] private GameObject battleResultCameraRoot;

    void OnEnable()
    {
        EventBus.Subscribe<GameModeChangedEvent>(this);
        EventBus.Subscribe<BattleEndedEvent>(this);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
        EventBus.Unsubscribe<BattleEndedEvent>(this);
    }

    #region 接口

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
    
    public void OnEvent(BattleEndedEvent e)
    {
        SetCameraView(CameraView.BattleResult);
       
        if (!e.IsWin)
        {
            // SetCameraView(CameraView.Explore);
            EventBus.Publish(new BattleLoseViewEvent());
            return;
        }
        
        EventBus.Publish(new BattleResultViewEnterEvent(e.ExpReward, e.MoneyReward, e.DropRewards));
    }
    #endregion

    private void SetCameraView(CameraView view)
    {
        followCamera.SetActive(view == CameraView.Explore);
        battleCamera.SetActive(view == CameraView.Battle);
        battleResultCameraRoot.SetActive(view == CameraView.BattleResult);
    }

    
}