
using TMPro;

public class EquipmentStatCompareRow : MonoBehaviour
{
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text modifyValueText;
    
    [Header("Colors")]
    [SerializeField] private Color increaseColor;
    [SerializeField] private Color decreaseColor;

    // 设置当前值和预览值，并根据预览模式调整显示
    public void SetRaw(int currentValue, int previewValue, bool isInPreviewMode)
    {
        currentValueText.text = currentValue.ToString();
        
        // 预览模式下，根据预览值与当前值的比较调整颜色和显示
        if (!isInPreviewMode || previewValue == currentValue)
        {
            modifyValueText.text = "";
            return;
        }
        modifyValueText.text = "> " + previewValue.ToString();
        
        // set color
        if (previewValue > currentValue)
        {
            modifyValueText.color = increaseColor;
        }
        else
        {
            modifyValueText.color = decreaseColor;
        }
    }
}
 