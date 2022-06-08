using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Colyseus;

public enum GamePhase
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
    private List<RecruitCard> recruitsShop = new List<RecruitCard>();
    private List<RecruitCard> discardedRecruits = new List<RecruitCard>();
    public Transform[] yourRecruitSlots;
    public bool[] availableCardSlotsRecruit;
    private ToolCard[] yourToolHand = new ToolCard[8];
    private RecruitCard[] yourRecruitHand = new RecruitCard[3];
    public RecruitCard recruitBeingUsed;
    public bool isRecruitBeingUsed;
    public List<string> matchingTools = new List<string>();

    private HashSet<string> toolboardToolsGot = new HashSet<string>();

    public Transform[] yourToolboardSlots;

    public Text message;
    public ButtonUI OkButton;

    public GamePhase phase;

    // Start is called before the first frame update
    void Start()
    {
        BeginRecruit();
        Invoke("BeginLearn",5);
        Invoke("BeginRecruit",10);
        Invoke("BeginLearn",15);

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
        message.text = "Learn tools from your entrepeneurs";
    }

    public void WaitForOpponentLearn(){
        phase = GamePhase.Learn;
    }

    public void Upkeep(){
        phase = GamePhase.Upkeep;
    }

    public void ShowResult(){
        phase = GamePhase.Result;
        message.text = "Game Over";
    }

      private void DrawToolCards(){
        if(toolDeck.Count >1){
            for(int i = 0; i<toolCardSlots.Length;i++){
                if(availableCardSlotsHand[i]==true){
                    ToolCard card = toolDeck[Random.Range(0,toolDeck.Count)];
                    card.gameObject.SetActive(true);
                    card.transform.position = toolCardSlots[i].position;
                    yourToolHand[i]=card;
                    availableCardSlotsHand[i] = false;
                    toolDeck.Remove(card);
                }
            }
        }
    }

    private void PutRecruitCards(){
        foreach(RecruitCard rc in recruitsShop){
            rc.gameObject.SetActive(false);
            discardedRecruits.Add(rc);
        }
        recruitsShop.Clear();
        for(int i = 0; i<4; i++){
            if(recruitDeck.Count<=0){
                recruitDeck=discardedRecruits;
            }
            RecruitCard card = recruitDeck[Random.Range(0,recruitDeck.Count)];
            recruitsShop.Add(card);
            card.gameObject.SetActive(true);
            card.transform.position = shopSlots[i].position;
            recruitDeck.Remove(card);
        }
    }

    public void OK(){
        if (isRecruitBeingUsed && phase == GamePhase.Recruit){
            if (CountDiscarded() == recruitBeingUsed.tools){
                DiscardTools();
                yourRecruitHand[recruitBeingUsed.PutRecruitedInZone()] = recruitBeingUsed;
                recruitBeingUsed = null;
            }
        }
        else if (isRecruitBeingUsed && phase == GamePhase.Learn){
            matchingTools.Clear();
            for (int i = 0; i < toolCardSlots.Length; i++){
                if (yourToolHand[i].isBeingDiscarded){
                    string tagTool = yourToolHand[i].tag;
                    if (recruitBeingUsed.toolPool.Contains(tagTool)){
                        if (matchingTools.Contains(tagTool)){
                            return;
                        }
                        matchingTools.Add(tagTool);
                    }
                    else{
                        return;
                    }
                }
            }
            recruitBeingUsed.gameObject.SetActive(false);
            for (int i = 0; i < toolCardSlots.Length; i++){
                ToolCard tool = yourToolHand[i];
                if (tool.isBeingDiscarded){
                    for(int j=0; j<yourToolboardSlots.Length;j++){
                        if(yourToolboardSlots[j].tag==tool.tag){
                            tool.transform.position = yourToolboardSlots[j].transform.position;
                            toolboardToolsGot.Add(tool.tag);
                            if(toolboardToolsGot.Count==9){
                                ShowResult();
                            }
                        }
                    }
                }
            }
        }
    }

    public void ActivateButtons(bool action){
        OkButton.Activate(action);
    }

    public void UnhighlightTools(){
        for(int i=0;i<toolCardSlots.Length;i++){
            yourToolHand[i].isBeingDiscarded=false;
            yourToolHand[i].GetComponent<Renderer>().material.color = yourToolHand[i].tempColor;
        }
    }

    private int CountDiscarded(){
        int discardedNumForRecruit = 0;
        for(int i=0;i<toolCardSlots.Length;i++){
            if(yourToolHand[i].isBeingDiscarded){
                discardedNumForRecruit++;
            }
        }
        return discardedNumForRecruit;
    }

    private void DiscardTools()
    {
        for (int i = 0; i < toolCardSlots.Length; i++)
        {
            if (yourToolHand[i].isBeingDiscarded)
            {
                yourToolHand[i].gameObject.SetActive(false);
            }
        }
    }
}
