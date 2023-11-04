using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum EType { Ammo, Coin, Grenade, Heart, Weapon }
    public EType type;
    public int value;
    public float rotateSpeed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
