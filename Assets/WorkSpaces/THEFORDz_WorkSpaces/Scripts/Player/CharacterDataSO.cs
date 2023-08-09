using UnityEngine;

[CreateAssetMenu(fileName = "Player Variant",menuName ="Character/Character Data",order = 2)]
public class CharacterDataSO : ScriptableObject
{
    [Header("Data")]
    public Sprite characterSprite;          // CharacterSprite for the character selection scene
    public Sprite iconSprite;               // Sprite use on the player UI on gameplay scene
    public Sprite iconDeathSprite;          // Sprite use on the player UI on gameplay scene for his death
    public string characterName;            // Character name
    public GameObject characterPrefab; //Player Character Prefabs
    public GameObject characterPrefabsScore;
    //public GameObject spaceshipScorePrefab; // Sprite for the ship on the endgame scene UI  
    public Color color;                     // The color that identifies this character, use for coloring sprites (laser)
    public Color darkColor;  

    [Header("Client Info")]
    public ulong clientId;
    public int playerId;
    public bool isSelected;
    
    [Header("Score")]
    public int foodScore;            // The enemies defeat by the player for the final score
    


    void OnEnable()
    {
        EmptyData();
    }

    public void EmptyData()
    {
        isSelected= false;
        clientId= 0;
        playerId= -1;
        foodScore = 0;
    }
}
