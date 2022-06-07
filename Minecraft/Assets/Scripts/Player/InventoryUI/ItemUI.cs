using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IInitializePotentialDragHandler
{
    private RectTransform rectTransform;
    private Image image;
    private TextMeshProUGUI stacksCountText;


    private Vector2 positionBeforeDrag;
    public Transform parentBeforeDrag;


    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        stacksCountText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateItem (Item item)
    {
        image.sprite = item.itemData.Icon;
        stacksCountText.text = item.quantity > 1 ? item.quantity.ToString() : "";
    }

    public void OnPointerDown (PointerEventData eventData)
    {
        positionBeforeDrag = rectTransform.anchoredPosition;
        parentBeforeDrag = transform.parent;
    }

    public void OnInitializePotentialDrag (PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnBeginDrag (PointerEventData eventData)
    {
        transform.SetParent(parentBeforeDrag.parent.parent);
        image.raycastTarget = false;
    }

    public void OnDrag (PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag (PointerEventData eventData)
    {
        GameObject destinationObject = eventData.pointerCurrentRaycast.gameObject;

        if (destinationObject == null)
        {
            Debug.Log("Item dropped outside UI");
            Destroy(gameObject);
        }
        else
        {
            SlotUI destinationSlot = destinationObject.GetComponent<SlotUI>();
            ItemUI destinationItem = destinationObject.GetComponent<ItemUI>();

            if (destinationSlot != null) // if this item was dropped on slot
            {
                transform.SetParent(destinationSlot.transform);
                rectTransform.anchoredPosition = positionBeforeDrag;
                image.raycastTarget = true;
            }
            else if (destinationItem != null) // If this item was dropped on another item
            {
                transform.SetParent(parentBeforeDrag);
                rectTransform.anchoredPosition = positionBeforeDrag;
            }
            else // If this item was dropped on UI element but not on slot or another item
            {
                transform.SetParent(parentBeforeDrag);
                rectTransform.anchoredPosition = positionBeforeDrag;
            }

            image.raycastTarget = true;
        }
    }


}
