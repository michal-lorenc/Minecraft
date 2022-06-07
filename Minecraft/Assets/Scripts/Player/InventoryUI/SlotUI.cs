using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IDropHandler
{
    public GameObject itemPrefab;
    public Image selectionOutline;

    [HideInInspector]
    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrop (PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

     //   eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
    }

    public ItemUI GetSlotItem ()
    {
        Component itemFound = transform.GetComponentInChildren(typeof(ItemUI));

        if (itemFound != null)
            return itemFound as ItemUI;
        else
        {
            ItemUI itemCreated = Instantiate(itemPrefab, this.transform, false).GetComponent<ItemUI>();
            return itemCreated;
        }
    }

    public void TryRemoveSlotItem ()
    {
        Component itemFound = transform.GetComponentInChildren(typeof(ItemUI));

        if (itemFound != null)
            Destroy(itemFound.gameObject);
    }
}
