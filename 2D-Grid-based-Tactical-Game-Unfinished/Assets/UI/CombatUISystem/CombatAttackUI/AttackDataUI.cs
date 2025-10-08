using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDataUI
{
    //Display Data
    public string data;
    public int min;
    public int max;
    public attackState attackState; 
    public attackDataType attackDataType;

    // AI Data
    public int attackValue;
}

[SerializeField]
public enum attackState
{
    Benificial,  // good
    Benediction, // bad
    Benign       // neutral
}

[SerializeField]
public enum attackDataType
{
    Main,
    Modifier
}
