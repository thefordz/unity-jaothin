using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    /*public Image artImage;*/
    private void Start()
    {
        ShowCard();
    }

    private void Update()
    {
        ShowCard();
    }

    void ShowCard()
    {
        nameText.text = card.cardName;
        descriptionText.text = card.cardDescription;
    }
}
