using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserInput : MonoBehaviour
{
    public GameObject slot1;
    private Solitaire solitaire;

    private void Start()
    {
        solitaire = FindObjectOfType<Solitaire>();
        slot1 = this.gameObject;
    }

    private void Update()
    {
        GetMouseClick();
    }

    private void GetMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            //Raycast to see where mouse has been clicked
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                //If click was on the deck
                if (hit.collider.CompareTag("Deck"))
                {
                    //Call deck fucntion
                    Deck();
                }
                //If click was on a card
                else if (hit.collider.CompareTag("Card"))
                {
                    //Call card function
                    Card(hit.collider.gameObject);
                }
                //If click was on top row
                else if (hit.collider.CompareTag("Top"))
                {
                    //Call top function
                    Top(hit.collider.gameObject);
                }
                //If click was on the bottom row
                else if (hit.collider.CompareTag("Bottom"))
                {
                    //Call bottom row
                    Bottom(hit.collider.gameObject);
                }
            }
        }
    }

    private void Deck()
    {
        //Calls function to deal the deck
        solitaire.DealFromDeck();
        //Clears any selected card
        slot1 = this.gameObject;

    }
    private void Card(GameObject selected)
    {
        //If card clicked on is facedown
        if (!selected.GetComponent<Selectable>().faceUp)
        {
            //If the card is not blocked by other cards
            if (!Blocked(selected))
            {
                //Flip the card
                selected.GetComponent<Selectable>().faceUp = true;
                slot1 = this.gameObject;
            }
        }
        //If card clicked on is in deck pile
        else if (selected.GetComponent<Selectable>().inDeckPile)
        {
            //If the card is not blocked by other cards
            if (!Blocked(selected))
            {
                //Select card
                slot1 = selected;   
            }
        }
        //If the card is face up
        else
        {
            //If there is no card selected
            if (slot1 == this.gameObject)
            {
                //Select the card
                slot1 = selected;
            }

            //If there is already a card selected and its not the same card
            else if (slot1 != selected)
            {
                //If the card is stackable on old card
                if (Stackable(selected))
                {
                    //Stack the card
                    Stack(selected);
                }
                //If the card is not stackable on old card
                else
                {
                    //Select new card
                    slot1 = selected;
                }
            }
        }
    }

    private void Top(GameObject selected)
    {
        //If clicked on card
        if (slot1.CompareTag("Card"))
        {
            //If the card is an ace and the empty slot is top, then stack
            if (slot1.GetComponent<Selectable>().value == 1)
            {
                Stack(selected);
            }
        }
    }

    private void Bottom(GameObject selected)
    {
        //If clicked on card
        if (slot1.CompareTag("Card"))
        {
            //If the card is a king and the empty slot is bottom, then stack
            if (slot1.GetComponent<Selectable>().value == 13)
            {
                Stack(selected);
            }
        }
    }

    private bool Stackable(GameObject selected)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        
        //Compare the cards to see if they are stackable
        if (!s2.inDeckPile)
        {
            //If in the top pile, must stack suited Ace to King
            if (s2.top) 
            {
                if (s1.suit == s2.suit || (s1.value == 1 && s2.suit == null))
                {
                    if (s1.value == s2.value + 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            //If in the bottom pile, must stack alternate colours King to Ace
            else
            {
                if (s1.value == s2.value - 1)
                {
                    bool card1Red = true;
                    bool card2Red = true;

                    if (s1.suit == "C" || s1.suit == "S")
                    {
                        card1Red = false;
                    }
                    if (s2.suit == "C" || s2.suit == "S")
                    {
                        card2Red = false;
                    }
                    if (card1Red == card2Red)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void Stack(GameObject selected)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        //Stack the cards with a negative y offset
        float yOffset = 0.3f;

        //If on top of king or empty, bottom stack the cards in place
        if (s2.top || (!s2.top && s1.value == 13))
        {
            yOffset = 0;
        }

        //Gives the stacked card an offset
        slot1.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y - yOffset, selected.transform.position.z - 0.01f);
        //Moves the children with the parent
        slot1.transform.parent = selected.transform;

        //Removes cards from top pile to prevent duplicate cards
        if (s1.inDeckPile)
        {
            solitaire.tripsOnDisplay.Remove(slot1.name);
        }
        //Alows movement of cards between top row
        else if (s1.top && s2.top && s1.value == 1)
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = 0;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = null;
        }
        //Keeps track of current value of the top decks when a card has been removed
        else if (s1.top)
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value - 1;
        }
        //Removes the card string from appropriate bottom list
        else
        {
            solitaire.bottoms[s1.row].Remove(slot1.name);
        }

        //Disables adding cards to trips pile
        s1.inDeckPile = false;
        s1.row = s2.row;

        //Moves a card to the top and assign it with appropriate value and suit
        if (s2.top)
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = s1.suit;
            s1.top = true;
        }
        else
        {
            s1.top = false;
        }

        //Reset hand
        slot1 = this.gameObject;
    }

    private bool Blocked(GameObject selected)
    {
        Selectable s2 = selected.GetComponent<Selectable>();
        if (s2.inDeckPile == true)
        {
            //If it is the last trip 
            if (s2.name == solitaire.tripsOnDisplay.Last())
            {
                //Unblock it
                return false;
            }
            else
            {
                print(s2.name + " is blocked by " + solitaire.tripsOnDisplay.Last());
                return true;
            }
        }
        else
        {
            //Check if its the bottom card
            if (s2.name == solitaire.bottoms[s2.row].Last())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
