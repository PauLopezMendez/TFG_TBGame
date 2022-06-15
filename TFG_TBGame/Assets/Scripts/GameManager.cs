using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Colyseus;

public enum GamePhase
{
    Recruit,
    Learn,
    Result,
}

public class GameManager : MonoBehaviour
{
    public Dictionary<string, RecruitCard> recruitCardsList = new Dictionary<string, RecruitCard>();
    public Dictionary<string, ToolCard> toolsCardsList = new Dictionary<string, ToolCard>();
    public Transform[] cardSlots;
    public List<ToolCard> toolDeck = new List<ToolCard>();
    public List<RecruitCard> recruitDeck = new List<RecruitCard>();
    public Transform[] toolCardSlots;
    public bool[] availableCardSlotsHand;
    private string[] toolsToCheck;
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

    private GameClient client;
    private State state;
    private int myPlayerNumber;
    [SerializeField]
    private GameObject[] toolCard;

    // Start is called before the first frame update
    void Start()
    {
        toolsToCheck = new string[5];
        int i = 1;
        string id = "";
        foreach(RecruitCard r in recruitDeck){
            id = "r"+i;
            r.id = id;
            recruitCardsList.Add(id, r);
            i++;
        }
        i = 1;
        foreach(ToolCard t in toolDeck){
            id = "T"+i;
            t.id = id;
            toolsCardsList.Add(id, t);            
            i++;
        }
        client = GameClient.Instance;
        client.InitialState();

        if(!client.Connected){
            SceneManager.LoadScene("ConnectingScene");
            return;
        }
        client.OnInitialState += InitialStateHandler;
        client.OnGamePhaseChange += GamePhaseChangeHandler;

        if(client.State != null){
            InitialStateHandler(this, client.State);
        }

    }

    private void OnDestroy(){
        client.OnInitialState -= InitialStateHandler;
        client.OnGamePhaseChange -= GamePhaseChangeHandler;
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    public void BeginRecruit(){

        phase = GamePhase.Recruit;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DrawToolCards();
                            print(state.firstTurn);

        if(state.firstTurn){
            string[][] recruitToDestroy = new string[4][];
            recruitToDestroy[0] = new string[] {"", ""};
            recruitToDestroy[1] = new string[] {"", ""};
            recruitToDestroy[2] = new string[] {"", ""};
            recruitToDestroy[3] = new string[] {"", ""};

            int indexShop = 6;
            for(int i = 0; i<recruitToDestroy.Length; i++){
                if(state.cards[indexShop]!=null&&state.cards[indexShop]!=""){
                    recruitToDestroy[i][0] = state.cards[indexShop];
                    recruitToDestroy[i][1] = indexShop.ToString();
                    indexShop++;
                } 
                else{
                    recruitToDestroy[i][0] = "";
                }
            }
            client.DestroyRecruits(recruitToDestroy);
            for(int j = 0; j<recruitToDestroy.Length; j++){
                client.NullRecruit(indexShop.ToString());
            }
            client.RefreshShop();
            state.firstTurn = false;
        }

        message.text = "Recruit an entrepeneur";
    }

    public void WaitForOpponentToRecruit(){
        phase = GamePhase.Recruit;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        print("-----"+state.firstTurn);
        message.text = "Waiting for an opponent to recruit";
    }

    public void BeginLearn(){
        phase = GamePhase.Learn;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        message.text = "Learn tools from your entrepeneurs";
    }

    public void WaitForOpponentLearn(){
        phase = GamePhase.Learn;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        message.text = "Waiting for an opponent to learn";
    }

    public void ShowResult(){
        phase = GamePhase.Result;
        message.text = "Game Over";
    }

      private void DrawToolCards(){
        int rand = 0;
        for(int i = 0; i<toolCardSlots.Length;i++){
            rand = Random.Range(1, 10);
            GameObject tc = Instantiate(toolCard[rand-1], toolCardSlots[i].position, Quaternion.identity);
            yourToolHand[i]=tc.GetComponent<ToolCard>();
            tc.gameObject.SetActive(true);
        }
    }

    public void OK(){
        if (isRecruitBeingUsed && phase == GamePhase.Recruit){
            if (CountDiscarded() == recruitBeingUsed.tools){
                DiscardTools();
                isRecruitBeingUsed=false;
                int oldPosition = recruitBeingUsed.position;
                print("I have recruited the card "+recruitBeingUsed);
                if(myPlayerNumber==1){
                    for(int i =0; i<3;i++){
                        if(state.cards[i]==null||state.cards[i]==""){
                            recruitBeingUsed.position = i;
                            break;
                        }
                    }
                }
                else{
                    for(int i =3; i<6;i++){
                        if(state.cards[i]==null||state.cards[i]==""){
                            recruitBeingUsed.position = i;
                            break;
                        }
                    }
                }
                print(recruitBeingUsed.id+" "+oldPosition+" "+recruitBeingUsed.position);
                client.SendRecruited(recruitBeingUsed.id, oldPosition, recruitBeingUsed.position);  
                recruitBeingUsed.UnhighlightRecruit();
                if(state.playersSkipped==1){
                    client.SendSkip();
                }
            }
        }
        else if (isRecruitBeingUsed && phase == GamePhase.Learn){
            matchingTools.Clear();
            int indexTools = 0;
            for (int i = 0; i < toolCardSlots.Length; i++){
                if (yourToolHand[i].isBeingDiscarded){
                    string tagTool = yourToolHand[i].tag;
                    if (recruitBeingUsed.toolPool.Contains(tagTool)){
                        if (matchingTools.Contains(tagTool)){
                            return;
                        }
                        matchingTools.Add(tagTool);
                        toolsToCheck[indexTools] = tagTool;
                        indexTools++;
                    }
                    else{
                        return;
                    }
                }
            }
            isRecruitBeingUsed=false;
            string [][] recruitToDestroy = new string[4][];
            recruitToDestroy[0] = new string[] {"", ""};
            recruitToDestroy[1] = new string[] {"", ""};
            recruitToDestroy[2] = new string[] {"", ""};
            recruitToDestroy[3] = new string[] {"", ""};

            recruitToDestroy[0][0] = recruitBeingUsed.id;
            recruitToDestroy[0][1] = recruitBeingUsed.position.ToString();
            print("-------I am destroying position"+recruitBeingUsed.position+"----------");
            client.DestroyRecruits(recruitToDestroy);
            DiscardTools();
            client.SendLearn(recruitBeingUsed.id, recruitBeingUsed.position, toolsToCheck);
            client.NullRecruit(recruitBeingUsed.position.ToString());
            if(state.playersSkipped==1){
                client.SendSkip();
            }
        }
    }

    public void Skip(){
        client.SendSkip();
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

    //networking

    private void InitialStateHandler (object sender, State initialState){
        state = initialState;

        Player me = state.players[client.SessionId];

        myPlayerNumber = me != null ? me.seat : -1;

        state.OnChange += StateChangeHandler;
        state.cards.OnChange += CardHandling;
        state.cards.OnAdd += CardHandling;
        state.recruitsToDestroy.OnChange += DestroyCardsHandling;
        state.recruitsToDestroy.OnAdd += DestroyCardsHandling;
        GamePhaseChangeHandler(this, state.phase);
    }

    private void StateChangeHandler(object sender, Colyseus.Schema.OnChangeEventArgs args){
        foreach (var change in args.Changes){
            if (change.Field == "playerTurn" && state.phase == "Recruit"){
                CheckTurnRecruit();
            }
            else if (change.Field == "playerTurn" && state.phase == "Learn"){
                CheckTurnLearn();
            }
        }
    }
  
    private void CardHandling(object sender, Colyseus.Schema.KeyValueEventArgs<string, int> change) {
        // 0-2 recruits1
        // 3-5 recruits2
        // 6-9 shop
        // 10-18 toolboard 1
        // 19-27 toolboard 2
        for(int i = 0; i<=9; i++){
            //print("The index of card handling is "+i);
            string id = state.cards[i];
            if(id!=null&&id!=""){
                //print("The card id is " + id + ".... and in the RecruitCard list there is "+recruitCardsList[id]);
                RecruitCard c = recruitCardsList[id];
                c.gameObject.SetActive(true);
                c.position = i;

                if(myPlayerNumber==1){
                    c.transform.position = cardSlots[i].position;
                }
                else{
                    if(i>=0&&i<=2){
                        c.transform.position = cardSlots[i+3].position;
                    }
                    else if(i>=3&&i<=5){
                        c.transform.position = cardSlots[i-3].position;
                    }
                    else{
                        c.transform.position = cardSlots[i].position;
                    } 
                }
            }
        }
    
        for(int i = 10; i<=27; i++){
            if(state.cards[i]!=null&&state.cards[i]!=""){
                string id = state.cards[i];
                int toolId = (int)char.GetNumericValue(id[1]);
                Vector3 posCard;
                if(myPlayerNumber==1){
                   posCard = cardSlots[i].position;
                }
                else{
                    if(i>=10&&i<=18){
                        posCard = cardSlots[i+9].position;
                    }
                    else{
                        posCard = cardSlots[i-9].position;
                    }
                }
                GameObject tc = Instantiate(toolCard[toolId-1], posCard, Quaternion.identity);
                tc.gameObject.SetActive(true);
            }
        }
    }

    private void DestroyCardsHandling(object sender, Colyseus.Schema.KeyValueEventArgs<string, int> change) {
        for(int idxDes = 0; idxDes<state.recruitsToDestroy.Count; idxDes++){
            string id = state.recruitsToDestroy[idxDes];
            if(state.recruitsToDestroy[idxDes]!=null&&state.recruitsToDestroy[idxDes]!=""){
                GameObject go = GameObject.Find(id);
                print("The card "+id+" has been destroyed.");
                Destroy(go);
                //client.NullRecruit(id);
                //print("The card "+id+" now is a null");
            }
        }

    }

    private void CheckTurnRecruit(){
        if(state.playerTurn == myPlayerNumber){
            BeginRecruit();
        }
        else{
            WaitForOpponentToRecruit();
        }
    }

    private void CheckTurnLearn(){
        if(state.playerTurn == myPlayerNumber){
            BeginLearn();
        }
        else{
            WaitForOpponentLearn();
        }
    }

    private void Leave (){
        client.Leave();
        SceneManager.LoadScene("ConnectingScene");
    }

    private void GamePhaseChangeHandler(object sender, string phase){
        switch (phase){
            case "waiting":
                break;
            case "Recruit":
                CheckTurnRecruit();
                break;
            case "Learn":
                CheckTurnLearn();
                break;
            case "Result":
                ShowResult();
                break;
        }
    }
}
