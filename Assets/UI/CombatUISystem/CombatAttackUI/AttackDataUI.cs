using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDataUI
{
    public string data;
    public int min;
    public int max;
    public attackState attackState; 
    public attackDataType attackDataType;
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
