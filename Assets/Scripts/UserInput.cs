using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserInput : MonoBehaviour
{
    public GameObject inHand;
    private Game game;

    private void Start()
    {
        game = FindObjectOfType<Game>();
        inHand = this.gameObject;
    }

    private void Update()
    {
        GetMouseClick();
    }

    private void GetMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                if (hit.collider.CompareTag("Deck"))
                {
                    Deck();
                }
                else if (hit.collider.CompareTag("Card"))
                {
                    Card(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Top"))
                {
                    Top();
                }
                else if (hit.collider.CompareTag("Bottom"))
                {
                    Bottom();
                }
            }
        }
    }

    private void Deck()
    {
        print("Clicked on deck");
        game.DealFromDeck();
    }

    private void Card(GameObject selected)
    {
        print("Clicked on card");

        //If card clicked on is facedown
        if (!selected.GetComponent<Selectable>().faceUp)
        {
            //If card is not blocked
            if (!Blocked(selected))
            {
                //Flip it over
                selected.GetComponent<Selectable>().faceUp = true;
                inHand = this.gameObject;
            }
        }
        //If card clicked on in deck pile
        else if (selected.GetComponent<Selectable>().inDeckPile)
        {
            //If its not blocked
            if (!Blocked(selected))
            {
                inHand = selected;
            }
        }

            if (inHand == this.gameObject)
        {
            inHand = selected;
        }

        else if (inHand != selected)
        {
            //If new card is stackable on old card
            if (Stackable(selected))
            {
                Stack(selected);
            }
            //Else
            else
            {
                //Select new card
                inHand = selected;
            }
        }
    }
    private void Top()
    {
        print("Clicked on top");

    }
    private void Bottom()
    {
        print("Clicked on bottom");

    }

    private bool Stackable(GameObject selected)
    {
        Selectable slot1 = inHand.GetComponent<Selectable>();
        Selectable slot2 = selected.GetComponent<Selectable>();

        if (!slot2.inDeckPile)
        {
            //If in top pile, must tack suited Ace to King
            if (slot2.top)
            {
                if (slot1.suit == slot2.suit || (slot1.value == 1 && slot2.suit == null))
                {
                    if (slot1.value == slot2.value + 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            //If in bottom pile, must stack alternate colors King to Ace
            else
            {
                if (slot1.value == slot2.value - 1)
                {
                    bool card1Red = true;
                    bool card2Red = true;

                    if (slot1.suit == "C" || slot1.suit == "S")
                    {
                        card1Red = false;
                    }

                    if (slot2.suit == "C" || slot2.suit == "S")
                    {
                        card2Red = false;
                    }

                    if (card1Red == card2Red)
                    {
                        print("Unstackable");
                        return false;
                    }
                    else
                    {
                        print("Stackable");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void Stack(GameObject selected)
    {
        //If on top of king or empty bottom, stack cards in place
        //Else stack cards with a negative y offset
        
        Selectable slot1 = inHand.GetComponent<Selectable>();
        Selectable slot2 = selected.GetComponent<Selectable>();
        float yOffset = 0.3f;

        if (slot2.top || (!slot2.top && slot1.value == 13))
        {
            yOffset = 0;
        }

        slot1.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y - yOffset, selected.transform.position.z - 0.01f);
        slot1.transform.parent = selected.transform;

        if (slot1.inDeckPile) //Removes cards from top pile to prevent duplicate cards
        {
            game.tripsOnDisplay.Remove(slot1.name);
        }
        else if (slot1.top && slot2.top && slot1.value == 1) //Allows movement of cards between top spots
        {
            game.topPos[slot1.row].GetComponent<Selectable>().value = 0;
            game.topPos[slot1.row].GetComponent<Selectable>().suit = null;
        }
        else if (slot1.top) //Keeps track of current value of top decks as a card has been removed
        {
            game.topPos[slot1.row].GetComponent<Selectable>().value = slot1.value - 1;
        }
        else //Removes card string from bottom list 
        {
            game.bottoms[slot1.row].Remove(slot1.name);
        }

        slot1.inDeckPile = false; //Cannot add cards to trips pile
        slot1.row = slot2.row;

        if (slot2.top)  //Moves a card to top and assigns the tops value and suit
        {
            game.topPos[slot1.row].GetComponent<Selectable>().value = slot1.value;
            game.topPos[slot1.row].GetComponent<Selectable>().suit = slot1.suit;
            slot1.top = true;
        }
        else
        {
            slot1.top = false;
        }

        //After completing move, clear cards in hand
        inHand = this.gameObject;
    }

    private bool Blocked(GameObject selected)
    {
        Selectable slot2 = selected.GetComponent<Selectable>();
        if (slot2.inDeckPile == true)
        {
            //If its last trip its not blocked
            if (slot2.name == game.tripsOnDisplay.Last())
            {
                return false;
            }
            else
            {
                print(slot2.name + " is blocked by " + game.tripsOnDisplay.Last());
                return true;
            }
        }
        else
        {
            //Check if its bottom card
            if (slot2.name == game.bottoms[slot2.row].Last()) 
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
