using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public class Block : MonoBehaviour
{
    public Transform Min { get; protected set; }
    public Transform ShadowMin { get; protected set; }
    public Transform shadow;
    public Transform shadowObj;
    void Awake()
    {
        Min = GetBottomBlock(transform);
        if(shadow != null)
        {
            shadowObj = Instantiate(shadow);
            ShadowMin = GetBottomBlock(shadowObj.transform);
        }
    }
    public virtual void Rotate(int dir)
    {
        transform.Rotate(0f, 0f, 90f * dir);
        shadowObj.Rotate(0f, 0f, 90f * dir);
        Min = GetBottomBlock(transform);
        ShadowMin = GetBottomBlock(shadowObj);
    }
     
     
    public void ShadowBlockMove()
    {
        if(shadow == null)
        {
            return;
        } 

        Vector3 pos = shadowObj.position;
        pos.x = transform.position.x;
        pos.y -= 0.7f;
        shadowObj.position = pos;
    }
    
    public Transform GetBottomBlock(Transform obj)
    {
        float minY = float.MaxValue;
        Transform minChild = null;
        for (int i = 0; i < obj.childCount; i++)
        {
            Transform child = obj.GetChild(i);
            if (minY > child.position.y)
            {
                minY = child.position.y;
                minChild = child;
            }
        }
        
        return minChild;
    }
}
