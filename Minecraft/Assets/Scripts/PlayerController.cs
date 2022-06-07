using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerInventory inventory;

    private int health = 100;
    private int maxHealth = 100;

    public int Health 
    { 
        get 
        { 
            return health; 
        } 
        set
        {
            health = value;

            if (health <= 0)
                OnDeath();
            else if (health > maxHealth)
                health = maxHealth;
        }
    }



    [SerializeField]
    private LayerMask hitLayer;
    private RaycastHit hit;
    private readonly float maxHitDistance = 5.0f;
    private Vector3 blockPosition = new Vector3(0, 0, 0);
    private Vector3 indicatorPosition = new Vector3(0, 0, 0);
    private bool success = false;

    public GameObject hitIndicator;

    private void Update ()
    {
        if (GameManager.singleton.gameState != eGameState.GAMEPLAY)
            return;

        if (success)
        {
            if (hit.transform.CompareTag("Environment"))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    blockPosition = hit.point - hit.normal / 2f;
                    Map.singleton.DestroyBlock(blockPosition);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Item selectedItem = inventory.GetSelectedItem();

                    if (selectedItem != null && selectedItem.itemData.ItemType == eItemType.BUILDING)
                    {
                        BuildableItemData buildableItemData = (BuildableItemData)selectedItem.itemData;

                        blockPosition = hit.point + hit.normal / 2f;
                        Map.singleton.PlaceBlock(blockPosition, buildableItemData.Block.id);

                        inventory.UseSelectedItem();
                    }
                }

                if (Input.GetMouseButtonDown(2))
                {
                    blockPosition = hit.point + hit.normal / 2f;
                    Map.singleton.PlaceWater(blockPosition);
                }
            }
        }
    }

    private void LateUpdate ()
    {
        DoRaycast();
        DoVoxelRaycast();
    }

    private void DoVoxelRaycast ()
    {
        VoxelRaycast.VoxelRaycastInfo info = VoxelRaycast.CastRay(transform.position, transform.position + transform.forward * 10);

        if (info != null)
        {
            indicatorPosition.x = Mathf.Floor(info.VoxelPosition.x) + 0.5f;
            indicatorPosition.y = Mathf.Floor(info.VoxelPosition.y) + 0.5f;
            indicatorPosition.z = Mathf.Floor(info.VoxelPosition.z) + 0.5f;

            hitIndicator.transform.position = indicatorPosition;
            hitIndicator.SetActive(true);
        }
        else
        {
            hitIndicator.SetActive(false);
        }

    }

    private void DoRaycast ()
    {
        success = Physics.Raycast(transform.position, transform.forward, out hit, maxHitDistance, hitLayer);

        if (success && hit.transform.CompareTag("Environment"))
        {
            blockPosition = hit.point - hit.normal / 2f;

            indicatorPosition.x = Mathf.Floor(blockPosition.x) + 0.5f;
            indicatorPosition.y = Mathf.Floor(blockPosition.y) + 0.5f;
            indicatorPosition.z = Mathf.Floor(blockPosition.z) + 0.5f;

            hitIndicator.transform.position = indicatorPosition;
            hitIndicator.SetActive(true);
        }
        else
        {
            hitIndicator.SetActive(false);
        }
    }

    private void OnDeath ()
    {
        Debug.Log("Player killed");
    }
}
