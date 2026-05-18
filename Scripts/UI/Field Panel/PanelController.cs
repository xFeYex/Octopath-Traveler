using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

public class PanelController : MonoBehaviour
{
    [Header("Action")]
    public ActionBase CurrentAction;

    [Header("Focus Navigation")] // 聚焦导航
    public Button FirstButton;
    
    /* --------------------------------------------------- */

    public virtual void SetupPanel(ActionBase actionBase)
    {
        CurrentAction = actionBase;
    }
    
    // 只取消当前一级的菜单，而不是全部取消
    public virtual void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // 完全关闭菜单
    protected void OnCancel()
    {
        GameModeManager.Instance.RequestChangeMode(GameMode.Explore); // 切换回自由移动状态
        ClosePanel();
    }
    
    protected void SetDefaultSelection()
    {
        FirstButton.Select(); // 选中按钮
        EventSystem.current.SetSelectedGameObject(FirstButton.gameObject); // 全局强制选择
    }
}