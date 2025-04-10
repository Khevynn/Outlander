using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData ItemData { get; private set; }
    public int Quantity { get; private set; }
    
    [Header("Slot UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemIcon;
    
    [Header("Drag & Drop Control")]
    private Canvas _mainCanvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private GameObject _initialParent;

    private void Start()
    {
        _mainCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        
        _initialParent = transform.parent.gameObject;
    }

    public void SetItem(ItemData itemData, int quantity)
    {
        ItemData = itemData;
        Quantity = quantity;
        
        nameText.text = itemData.Name;
        itemIcon.sprite = itemData.Icon;
        quantityText.text = Quantity.ToString();
        
        itemIcon.preserveAspect = true;
    }
    private void UpdateQuantityText()
    {
        quantityText.text = Quantity.ToString();
    }

    public void AddItem(int amount, out int outRemainingQuantity)
    {
        if(Quantity + amount > ItemData.MaxStack)
        {
            outRemainingQuantity = Quantity + amount - ItemData.MaxStack;
            Quantity = ItemData.MaxStack;
        }
        else
        {
            Quantity += amount;
            outRemainingQuantity = 0;
        }
        UpdateQuantityText();
    }

    public void UIShowOrHideItemInformation()
    {
        InventoryManager.Instance.ShowOrHideItemInfoPopup(ItemData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryManager.Instance.ActivateDropArea();
        transform.SetParent(_mainCanvas.transform);
        _canvasGroup.alpha = .6f;
        _canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _mainCanvas.scaleFactor;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject && eventData.pointerCurrentRaycast.gameObject.CompareTag("DropArea"))
        {
            InventoryManager.Instance.DropItems(this);
        }
        else
        {
            transform.SetParent(_initialParent.transform);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }
        InventoryManager.Instance.DeactivateDropArea();
    }
}
