using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Status : ScriptableObject
{
    public int statusDuration;
    public string statusName;
    public Sprite statusImage;
    public bool ApplyEveryTurn;
    /*  
    public static void make()
    {
        Instantiate(statusPrefab);
       // Instance = statusPrefab.GetComponent<Sprinting>();
    }
    */
    /*
    public void Create(Unit target)
    {
        GameObject temp = Instantiate(statusPrefab);
        Status status = temp.GetComponent<Status>();
        target.activeStatuses.Add(status);
    }
    */
    // Start is called before the first frame update
    abstract public void ApplyEffect(Unit target);


    // Update is called once per frame
    abstract public void RemoveEffect(Unit target);

}
