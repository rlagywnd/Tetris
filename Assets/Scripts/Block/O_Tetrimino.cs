using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Tetrimino : Block
{
    // Start is called before the first frame update
    void Start()
    {
        Min = GetBottomBlock(transform);
    }
     
    public override void Rotate(int dir)
    {
        return;
    }
}
