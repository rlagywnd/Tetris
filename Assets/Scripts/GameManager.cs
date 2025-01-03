using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine; 

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Transform> Dic { get; private set; }
        = new Dictionary<Vector2, Transform>();
    
    private bool isGameOver = false;
    private bool downArrow = false;
    public float moveOffset = 0.7f;
    public int blockPoolCount = 5;
    public float blockDownTime = 0.5f;
    public Vector3[] wallKickOffsets = new Vector3[]
    {
        new Vector3(0,0,0),
        new Vector3(0.7f, 0, 0),   // 오른쪽으로 한 칸
        new Vector3(-0.7f, 0, 0),  // 왼쪽으로 한 칸
        new Vector3(0, -0.7f, 0),  // 아래로 한 칸
        new Vector3(0, 0.7f, 0),  // 위로 한 칸
    };

    public Block[] blocks;
    public GameObject[] lines;
    public Transform startPos;
    public List<Block> blockPool = new List<Block>();
    private Block currentBlock;
    private WaitForSeconds waitForBlockDown;
    public Transform blockContainer;
    public GameObject gameOverUI;
     
    private void Awake()
    {
        waitForBlockDown = new WaitForSeconds(blockDownTime);
    }
    void Start()
    {
        if (blockPool.Count == 0)
        {
            for (int i = 0; i < blockPoolCount; i++)
            {
                AddBlockInPool();
            }
        }

        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].transform.childCount; j++)
            {
                Transform line = lines[i].transform;
                Transform cell = line.GetChild(j);

                Vector2 pos = GetKey(cell);
                Dic.Add(pos, null);
            }
        } 
    }
    private void Update()
    {
        if (!isGameOver)
        {
            MoveController();
            BlockDown();
            GenerateBlock(); 
        }

    }

    void DownBlock()
    {
        while (!(CheckGround(currentBlock.Min) || !IsBlockBelow(currentBlock.transform)))
        {
            MoveBlock(Vector2.down);
        }
        CheckManager();
    }

    void ShadowController()
    {
        if(currentBlock == null)
        {
            return;
        }
        Vector2 pos = currentBlock.transform.position; 

        currentBlock.shadowObj.position = pos; 
        while (!(CheckGround(currentBlock.ShadowMin) || !IsBlockBelow(currentBlock.shadowObj)))
        {  
            currentBlock.ShadowBlockMove();
        }

    }

    float timer = 0f;
    void BlockDown()
    {
        
        if (currentBlock == null || downArrow)
        {
            return;
        } 
        timer += Time.deltaTime;
        if(timer >= blockDownTime)
        {
            if (CheckGround(currentBlock.Min) || !IsBlockBelow(currentBlock.transform))
            {
                CheckManager();
            }
            MoveBlock(Vector2.down);
            timer = 0;
        } 
    }
    public void PosRegistration()
    {
        currentBlock.shadowObj.gameObject.SetActive(false);
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            Transform child = currentBlock.transform.GetChild(i);
            Vector2 pos = child.position;
            pos = GetKey(pos);
            if (pos.y >= 3.84f || pos.y >= 4.54f)
            {
                GameOver();
            }
            Dic[pos] = child;
        }
    }
    void GameOver()
    {
        isGameOver = true;
        gameOverUI.SetActive(true);
        Time.timeScale = 0;
    }
    public void CheckManager()
    {
        //3.84 , 4.54
        PosRegistration();
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            if (CheckCurrentLine(i))
            {
                int filledLine = GetLine(currentBlock.transform.GetChild(i).position.y);
                DestroyBlock(filledLine);
                DropBlocks(filledLine);
            }
        }
        currentBlock = null;
         
    }
    
    void Rotate()
    {
        if(currentBlock == null)
        {
            return;
        }

        currentBlock.Rotate(1);
        if (!ApplyWallKick())
        { 
            currentBlock.Rotate(-1);
        }
        ShadowController();
    } 
    bool IsValidPosition()
    {
        if (IsOutOfBottom() || !CheckCellX() || !IsBlockAtPosition())
        {
            return false;
        }
        return true;
    }
    bool ApplyWallKick()
    { 
        //회전했을때 벽을 넘어가거나 이미 고정 돼 있는 블록 영역에 침범하면 위치를 이동시킴
        for (int i = 0;i < wallKickOffsets.Length;i++)
        {
            for(int j = 1;j < 5; j++)
            {
                Vector3 offset = wallKickOffsets[i] * j;
                Vector3 pos = currentBlock.transform.position;
                pos += offset;
                currentBlock.transform.position = pos;
               
                if (IsValidPosition())
                {
                    return true;
                }
                pos -= offset;
                currentBlock.transform.position = pos; 
            }
        }
        return false; // 벽 킥 실패
    }

    bool CheckGround(Transform obj)
    {
        // 
        Vector2 pos = GetKey(obj.position);
        float min = 4.54f - (0.7f * (lines.Length - 1)); 
        return pos.y <= min;
    }
    bool IsOutOfBottom()
    {
        float min = 4.54f - (0.7f * (lines.Length - 1));
        return currentBlock.Min.position.y < min;
    }
    bool CheckCellX()
    {
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            Vector2 pos = currentBlock.transform.GetChild(i).position;

            pos = GetKey(pos);
            if (pos.x < -3.12f || pos.x > 3.18f)
            {
                return false;
            }
        }
        return true;
    }
    bool CheckCellX(int dir)
    {
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            Vector2 currentBlockPos = currentBlock.transform.GetChild(i).position;
            currentBlockPos.x -= moveOffset * dir;

            Vector2 pos = GetKey(currentBlockPos);
            if (pos.x < -3.12f || pos.x > 3.18f || Dic[pos] != null)
            {
                return false;
            }
        }
        return true;
    }
    bool CurrentPosBlock()
    {
        //현재 위치에 블록이 있는지 확인해주는 함수
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            Vector2 pos = currentBlock.transform.GetChild(i).position; 

            pos = GetKey(pos);
            if (Dic[pos] != null)
            {
                return true;
            }
        }
        return false;
    }

    bool IsBlockAtPosition()
    {
        //현재 블록 위치에 또다른 블록이 있는지 확인
        //float min = 4.54f - (0.7f * (lines.Length - 1));
        for (int i = 0; i < currentBlock.transform.childCount; i++)
        {
            Vector2 pos = currentBlock.transform.GetChild(i).position; 

            pos = GetKey(pos);
            if (Dic[pos] != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// true: 아무것도 없음
    /// false: 있음
    /// </summary>
    /// <returns></returns>
    bool IsBlockBelow(Transform obj)
    {
        //현재 떨어지고 있는 블록 아래에 또다른 블록이 있는지 확인
        for (int i = 0; i < obj.childCount; i++)
        {
            Vector2 pos = obj.GetChild(i).position;
            pos.y -= moveOffset;

            pos = GetKey(pos);
            if (Dic[pos] != null)
            {
                return false;
            }
        }
        return true;
    }

    void MoveBlock(Vector2 dir)
    {
        if(currentBlock == null)
        {
            return;
        }
        Vector2 pos = currentBlock.transform.position;
        pos += dir * moveOffset;
        currentBlock.transform.position = pos;
    }
    Block GetBlockInPool()
    {
        //블록을 Pool에서 하나 가져옴
        Block block = blockPool.First();
        blockPool.RemoveAt(0);

        return block;
    }
    void AddBlockInPool()
    {
        //블록을 하나 추가
        int rdm = UnityEngine.Random.Range(0, blocks.Length);
        blockPool.Add(blocks[rdm]);
    }

    private void GenerateBlock()
    {
        //블록 생성
        if (currentBlock != null)
        {
            return;
        }
        currentBlock = Instantiate(GetBlockInPool());
        currentBlock.transform.position = startPos.position;
        AddBlockInPool();
        ShadowController();
    } 
    public bool CheckCurrentLine(int childNum)
    {
        //현재 블록의 자식 오브젝트가 속해 있는 라인이 채워졌는지 확인
        Transform[] blocks = Dic.Values.ToArray();
        Vector2 pos = currentBlock.transform.GetChild(childNum).position;
        pos = GetKey(pos);
        int line = GetLine(pos.y);
        for(int i = line * 10; i < (line * 10) + 10; i++)
        {
            if(blocks[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    int GetLine(float y)
    {
        int line = (int)((y + 4.65f) / 0.7f);

        return line;
    }

    public void DestroyBlock(int line)
    {
        //한줄이 채워지면 라인 삭제 
        Transform[] blocks = Dic.Values.ToArray();
        for (int i = line * 10; i < line * 10 + 10; i++)
        {
            Vector2 block = GetKey(blocks[i]);
            Destroy(blocks[i].gameObject);
            Dic[block] = null;
        }
    }
    public void DropBlocks(int line)
    {
        //한줄이 채워지면 블록들을 지운 후 나머지 블록들을 한칸 내려줌
        Transform[] blocks = Dic.Values.ToArray();  
        for (int i = line * 10; i < blocks.Length - 2; i++)
        {
            if (blocks[i] != null)
            {
                Transform block = blocks[i];
                Dic[GetKey(block)] = null;
                Vector2 pos = block.position;
                pos.y -= moveOffset;
                Dic[GetKey(pos)] = block;
                block.position = pos;
            }
        }
    }
    
    void MoveController()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow))
            Rotate();

        if (Input.GetKeyDown(KeyCode.LeftArrow) && CheckCellX(1))
        {
            MoveBlock(Vector2.left);
            ShadowController();
        }
            

        else if (Input.GetKeyDown(KeyCode.RightArrow) && CheckCellX(-1))
        { 
            MoveBlock(Vector2.right);
            ShadowController();
        }

        else if (Input.GetKeyDown(KeyCode.Space))
            DownBlock();

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (CheckGround(currentBlock.Min) || !IsBlockBelow(currentBlock.transform))
            {
                return;
            }
            downArrow = true;
            MoveBlock(Vector2.down);
        }
        else
        {
            downArrow = false;
        }
    }

    Vector2 GetKey(Transform block)
    {
        return GetKey(block.position);
    }
    Vector2 GetKey(Vector2 blockPos)
    {
        float x = (float)Math.Round(blockPos.x, 2);
        float y = (float)Math.Round(blockPos.y, 2);
        Vector2 pos = new Vector2(x, y);
        return pos;
    }
}
