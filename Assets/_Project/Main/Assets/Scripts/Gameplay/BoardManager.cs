using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : NetworkBehaviour
{
    #region Scripts
    private GameplayManager _gameplayManager;
    #endregion

    #region Sprites
    [Header("Sprites")]
    public Sprite map1_NormalTile1;
    public Sprite map1_NormalTile2;
    public Sprite map1_Hole;
    public Sprite map1_Wall;
    public Sprite map1_Food;
    #endregion
    
    public GameObject[] childObjects;
    public SpriteRenderer[] gridSprites;

    private NetworkVariable<int> _randomValue = new NetworkVariable<int>();

    public List<int> randomList = new List<int>();

    //private NetworkList<int> randomValueList = new NetworkList<int>();

    public override void OnNetworkSpawn()
    {
        SpawneMap();
    }


    
    void SpawneMap()
    {
        _gameplayManager = GameObject.Find("GameManager").GetComponent<GameplayManager>();

        int childCount = transform.childCount;
        childObjects = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            childObjects[i] = transform.GetChild(i).gameObject;
        }
        
        gridSprites = new SpriteRenderer[childCount];
        for (int i = 0; i < childCount; i++)
        {
            if (childObjects[i].tag != "Untagged")
            { 
                gridSprites[i] = childObjects[i].GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        for (int i = 0; i < childCount; i++)
        {
            //Normal Tile
            if (childObjects[i].tag == "Normal Tile")
            {
                int randomNormalTileSprite = Random.Range(1, 2);
                switch (randomNormalTileSprite)
                {
                    case 1:
                        gridSprites[i].sprite = map1_NormalTile1;
                        break;
                    case 2:
                        gridSprites[i].sprite = map1_NormalTile2;
                        break;
                }
            }

            //if not corners and hole
            if (childObjects[i] != _gameplayManager.mapTopRightGrid &&
                childObjects[i] != _gameplayManager.mapTopLeftGrid &&
                childObjects[i] != _gameplayManager.mapBottomRightGrid &&
                childObjects[i] != _gameplayManager.mapBottomLeftGrid && childObjects[i].tag != "Hole")
            {
                //Hole
                //int randomValue = Random.Range(1, 101);
                if (IsServer)
                {
                    int random = Random.Range(1, 101);
                    //randomList.Add(_randomValue.Value);
                    //randomValueList.Add(random);
                    SpawnObjectClientRpc(i, random);

                }
            }
        }
    }

    

    [ClientRpc]
    public void SpawnObjectClientRpc(int index, int random)
    {
        randomList.Add(random);
        Debug.Log("============================" + _randomValue.Value);
        if (random <= 5)
        {
            
            childObjects[index].tag = "Hole";
            gridSprites[index].sprite = map1_Hole;
        }

        else if (random >= 6 && random <= 8)
        {
            childObjects[index].tag = "Wall";
            gridSprites[index].sprite = map1_Wall;
        }
                
        else if (random >= 9 && random <= 11)
        {
            childObjects[index].tag = "Food";
            gridSprites[index].sprite = map1_Food;
        } 
    }
}