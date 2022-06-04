using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GamePhase
{
    Recruit,
    Learn,
    Upkeep,
    Result,
}

public class GameManager : MonoBehaviour
{

    public List<ToolCard> toolDeck = new List<ToolCard>();
    public List<RecruitCard> recruitDeck = new List<RecruitCard>();
    public Transform[] toolCardSlots;
    public bool[] availableCardSlotsHand;
    public Transform[] shopSlots;
    public Transform[] yourRecruitSlots;
    public bool[] availableCardSlotsRecruit;


    public Text message;

    private GamePhase phase;


    // Start is called before the first frame update
    void Start()
    {
        BeginRecruit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  

    public void BeginRecruit(){
        phase = GamePhase.Recruit;
        PutRecruitCards();
        DrawToolCards();
        
        message.text = "Recruit an entrepeneur";
    }

    public void WaitForOpponentToRecruit(){
        phase = GamePhase.Recruit;
        message.text = "Waiting for an opponent to recruit";
    }

    public void BeginLearn(){
        phase = GamePhase.Learn;
    }

    public void WaitForOpponentLearn(){
        phase = GamePhase.Learn;
    }

    public void Upkeep(){
        phase = GamePhase.Upkeep;
    }

    public void ShowResult(){
        phase = GamePhase.Result;
    }

      private void DrawToolCards(){
        if(toolDeck.Count >1){
            for(int i = 0; i<toolCardSlots.Length;i++){
                if(availableCardSlotsHand[i]==true){
                    ToolCard card = toolDeck[Random.Range(0,toolDeck.Count)];
                    card.gameObject.SetActive(true);
                    card.transform.position = toolCardSlots[i].position;
                    availableCardSlotsHand[i] = false;
                    toolDeck.Remove(card);
                }
            }
        }
    }

    private void PutRecruitCards(){
        if(recruitDeck.Count >1){
            for(int i = 0; i<4; i++){
                RecruitCard card = recruitDeck[Random.Range(0,recruitDeck.Count)];
                card.gameObject.SetActive(true);
                card.transform.position = shopSlots[i].position;
                print(card.transform.position);
                                print(card.isActiveAndEnabled);

                recruitDeck.Remove(card);
            }
        }
    }
}
