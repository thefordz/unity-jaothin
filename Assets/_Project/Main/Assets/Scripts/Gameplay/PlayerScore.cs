using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    
    public TextMeshPro WinnerText;
    public TextMeshPro DrawText;

    public TextMeshPro m_foodScoreText;
    

    public void SetPlayer(int score)
    {
        // Set UI data base on the character data
        m_foodScoreText.text = string.Format("Score : "+score);
    }

    // Turn on the crown because I'm the best ship
    public void BestPlayer()
    {
        WinnerText.gameObject.SetActive(true);
    }
    

    public void Draw()
    {
        DrawText.gameObject.SetActive(true);
    }
    
    
}
