using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterSelection : MonoBehaviour
{
    public GameObject[] characters;
    public int indexCharacter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNonPlayableChar()
    {
        foreach (var character in characters)
        {
            character.SetActive(false);
        }
        
    }

    public void SetPlayableChar(int index)
    {
        SetNonPlayableChar();
        characters[index].SetActive(true);
       
    }
}
