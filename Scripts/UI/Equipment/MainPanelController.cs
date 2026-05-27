
using System;
using TMPro;
using UnityEngine.UI;
using Utils;

public class MainPanelController : PanelController
{
    [Header("Main Panel")]
    [SerializeField] private Button itembutton;

    [SerializeField] private Button equipmentButton;
    [SerializeField] private TMP_Text currencyAmountText;
    
    [Header("Panel")]
    [SerializeField] private ItemPanelController itemPanelController;
    [SerializeField] private EquipmentPanelController equipmentPanelController;
    [SerializeField] private CanvasGroup leftPartCanvasGroup;

    private GameObject _currentPanel;
    private Button _lastButton;
    
    /* ----------------------------------------------------------------------------------------- */

    private void Awake()
    {
        ReBindButtons(itembutton, OpenItemPanel);
        ReBindButtons(equipmentButton, OpenEquipmentPanel);
    }

    private void OnEnable()
    {
        FirstButton = itembutton;
        SetDefaultSelection();
        UpdateCurrencyDisplay();
    }

    private void OnDisable()
    {
        if (_currentPanel != null)
            _currentPanel.SetActive(false);
        leftPartCanvasGroup.interactable = true;
        _currentPanel = null;
    }

    /* ----------------------------------------------------------------------------------------- */
    
    private void OpenItemPanel()
    {
        OpenPanel(itemPanelController.gameObject, itembutton);
        itemPanelController.SetupPanel(PanelType.Item, null);
    }

    private void OpenEquipmentPanel()
    {
        OpenPanel(equipmentPanelController.gameObject, equipmentButton);
        var partyMember = PartyManager.Instance != null ? 
            PartyManager.Instance.PartyMembers : null;
        equipmentPanelController.SetupWithPartyMember(partyMember);
    }
    
    private void OpenPanel(GameObject panel, Button sourceButton)
    {
        _lastButton = sourceButton;
        panel.SetActive(true);
        _currentPanel = panel;
        leftPartCanvasGroup.interactable = false;
    }
    
    private void UpdateCurrencyDisplay()
    {
        if (currencyAmountText == null)
            return;
        
        var inventory = InventoryManager.Instance;
        int currentMonery = inventory.Currency;
        currencyAmountText.text = $"持有金额: {currentMonery.ToString()}";
    }

    public override bool HandleCancelInput()
    {
        if (_currentPanel == null || !_currentPanel.activeSelf)
            return false;

        if (equipmentPanelController != null
            && _currentPanel == equipmentPanelController.gameObject
            && equipmentPanelController.HandleCancelInput())
        {
            return true;
        }

        CloseCurrentPanel();
        
        return true;
    }

    private void CloseCurrentPanel()
    {
        FirstButton = _lastButton;
        _currentPanel.SetActive(false);
        _currentPanel = null;

        leftPartCanvasGroup.interactable = true;
        SetDefaultSelection();
    }
}
