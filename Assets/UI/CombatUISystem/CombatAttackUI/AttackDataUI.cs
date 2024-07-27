using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDataUI
{
    public string data;
    public attackState attackState;
}

[SerializeField]
public enum attackState
{
    Benificial,  // good
    Benediction, // bad
    Benign       // neutral
}
