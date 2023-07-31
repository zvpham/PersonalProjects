using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using System;

public class Item : MonoBehaviour
{
    [field:SerializeField]
    public ItemSO inventoryItem { get; private set; }

    [field: SerializeField]
    public int quantity { get; set; } = 1;
    /*
    [SerializeField]
    private AudioSource audioSource;
    */
    [SerializeField]
    private float duration = 0.3f;

    private  GameManager gameManager;
    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = inventoryItem.itemImage;
        gameManager = GameManager.instance; 
        gameManager.itemLocations.Add(transform.position);
        gameManager.items.Add(this);

    }

    public void DestroyItem()
    {
        //GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickup());
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
