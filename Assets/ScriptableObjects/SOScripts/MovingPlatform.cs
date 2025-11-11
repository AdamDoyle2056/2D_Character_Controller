using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlatform", menuName = "MovingPlatform")] 
public class MovingPlatform : ScriptableObject
{
    public float speed;
    public float invertTime;
}
