
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

public class SkillButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text spCostText;
    [SerializeField] private GameObject cursorTrans;
    
    /* ------------------------------------------------------------------------------ */

    public void SetupSkillButton(SkillDataSO skillData)
    {
        skillName.text = skillData.skillName;
        spCostText.text = "SP" +  skillData.spCost.ToString();

        bool showIcon = skillData.elementType != ElementType.None;
        skillIcon.sprite = skillData.icon;
        skillIcon.gameObject.SetActive(showIcon);
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        cursorTrans.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        cursorTrans.SetActive(false);
    }
}