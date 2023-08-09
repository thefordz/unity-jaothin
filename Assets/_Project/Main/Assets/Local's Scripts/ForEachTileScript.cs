using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ForEachTileScript : NetworkBehaviour
{
    //private NetworkVariable<SpriteRenderer> gridSprite;
    private SpriteRenderer gridSprite;
    private PlayerTurnOnline PlayerMoveScript;
    private GameplayManager gameController;

    [Header("Sprites")] 
    public Sprite map1_NormalTile1;
    public Sprite map1_NormalTile2;
    public Sprite map1_Hole;
    public Sprite map1_Wall;
    public Sprite map1_Food;
    

    private void Start()
    {
        gameController = GameObject.Find("GameManager").GetComponent<GameplayManager>();
        //Randomly Put Normal Tile Sprites
        if (this.gameObject.CompareTag("Normal Tile"))
        {
            int i;
            i = Random.Range(1, 2);

            switch (i)
            {
                case 1:
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_NormalTile1;
                    break;
                case 2:
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_NormalTile2;
                    break;
            }
        }
        else if (this.gameObject.CompareTag("Hole"))
        {
            GetComponentInChildren<SpriteRenderer>().sprite = map1_Hole;
        }

        //Randomly spawn Hole, Wall, and Food in map
        if (this.gameObject.CompareTag("Normal Tile"))
        {
            if (this.gameObject != gameController.mapTopLeftGrid && this.gameObject != gameController.mapTopRightGrid &&
                this.gameObject != gameController.mapBottomLeftGrid &&
                this.gameObject != gameController.mapBottomRightGrid)
            {
                int i;
                i = Random.Range(1, 100);

                if (i >= 1 && i <= 5) //Hole
                {
                    gameObject.tag = "Hole";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Hole;
                }
                else if (i >= 6 && i <= 8) //Wall
                {
                    gameObject.tag = "Wall";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Wall;
                }
                else if (i >= 9 && i <= 11) //Food
                {
                    gameObject.tag = "Food";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Food;
                }
            }
        }
    }

    /*[ServerRpc(RequireOwnership = false)]
    public void spawnMapServerRpc()
    {
        SpawnMapClientRpc();
    }

    [ClientRpc]
    public void SpawnMapClientRpc()
    {
        gameController = GameObject.Find("GameManager").GetComponent<GameplayManager>();
        //Randomly Put Normal Tile Sprites
        if (this.gameObject.CompareTag("Normal Tile"))
        {
            int i;
            i = Random.Range(1, 2);

            switch (i)
            {
                case 1:
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_NormalTile1;
                    break;
                case 2:
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_NormalTile2;
                    break;
            }
        }
        else if (this.gameObject.CompareTag("Hole"))
        {
            GetComponentInChildren<SpriteRenderer>().sprite = map1_Hole;
        }

        //Randomly spawn Hole, Wall, and Food in map
        if (this.gameObject.CompareTag("Normal Tile"))
        {
            if (this.gameObject != gameController.mapTopLeftGrid && this.gameObject != gameController.mapTopRightGrid &&
                this.gameObject != gameController.mapBottomLeftGrid &&
                this.gameObject != gameController.mapBottomRightGrid)
            {
                int i;
                i = Random.Range(1, 100);

                if (i >= 1 && i <= 5) //Hole
                {
                    gameObject.tag = "Hole";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Hole;
                }
                else if (i >= 6 && i <= 8) //Wall
                {
                    gameObject.tag = "Wall";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Wall;
                }
                else if (i >= 9 && i <= 11) //Food
                {
                    gameObject.tag = "Food";
                    GetComponentInChildren<SpriteRenderer>().sprite = map1_Food;
                }
            }
        }
    }*/
}