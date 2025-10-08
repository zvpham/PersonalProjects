using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using System;

public class Item : MonoBehaviour
{
    [field:SerializeField]
    public ItemSO inventoryItem { get; set; }

    [field: SerializeField]
    public int quantity { get; set; } = 1;
    /*
    [SerializeField]
    private AudioSource audioSource;
    */
    [SerializeField]
    private float duration = 0.3f;

    public GameManager gameManager;
    // Start is called before the first frame update
    private void Start()
    {
        if(gameManager.itemgrid.GetGridObject(gameObject.transform.position) == null)
        {
            gameManager.itemgrid.SetGridObject(gameObject.transform.position, new List<Item>() {this});
            GetComponent<SpriteRenderer>().sprite = inventoryItem.itemImage;
        }
        else
        {
            Debug.Log(gameManager.itemgrid.GetGridObject(gameObject.transform.position) + ", is this here");
            List<Item> tempItemList = gameManager.itemgrid.GetGridObject(gameObject.transform.position);
            tempItemList[tempItemList.Count - 1].gameObject.GetComponent<SpriteRenderer>().sprite = null;
            GetComponent<SpriteRenderer>().sprite = inventoryItem.itemImage;
            tempItemList.Add(this);
            gameManager.itemgrid.SetGridObject(gameObject.transform.position, tempItemList);
        }
        gameManager.items.Add(this);
    }

    public void DestroyItem(int itemIndexInList)
    {
        List<Item> tempItemList = gameManager.itemgrid.GetGridObject(gameObject.transform.position);
        if(itemIndexInList == tempItemList.Count - 1) 
        {
            for(int i = 0; i < tempItemList.Count; i++)
            {
                if (tempItemList[i].quantity <= 0)
                {
                    if(i != tempItemList.Count - 1)
                    {
                        gameManager.items.Remove(tempItemList[i]);
                        Destroy(tempItemList[i].gameObject);
                    }
                    tempItemList.RemoveAt(i);
                    i--;
                }
            }
            if(tempItemList.Count == 0)
            {
                gameManager.items.Remove(this);
                StartCoroutine(AnimateItemPickup());
                gameManager.itemgrid.SetGridObject(gameObject.transform.position, null);
            }
        }
    }

    public void Death()
    {
        List<Item> tempItemList = gameManager.itemgrid.GetGridObject(gameObject.transform.position);
        for(int i = 0; i < tempItemList.Count; i++)
        {
            if (tempItemList[i] == this)
            {
                tempItemList.RemoveAt(i);
                if(tempItemList.Count == 0)
                {
                    tempItemList = null;
                }
                break;
            }
        }
        gameManager.itemgrid.SetGridObject(gameObject.transform.position, tempItemList);
        gameManager.items.Remove(this);
        Destroy(gameObject);
    }

    private IEnumerator AnimateItemPickup()
    {
       // audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
