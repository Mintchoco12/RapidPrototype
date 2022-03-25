using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public string suit;
    public int value;
    public int row;

    public bool top = false;
    public bool faceUp = false;
    public bool inDeckPile = false;

    private string valueString;
    private string[] cards = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private void Start()
    {
        if (CompareTag("Card"))
        {
            suit = transform.name[0].ToString();

            for (int i = 1; i < transform.name.Length; i++)
            {
                char c = transform.name[i];
                valueString = valueString + c.ToString();
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (valueString == cards[i])
                {
                    value += i + 1;
                }
            }
        }
    }
}
