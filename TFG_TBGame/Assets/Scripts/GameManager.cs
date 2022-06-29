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
    //Diccionario de cartas de personaje (id, carta de escena)
    public Dictionary<string, RecruitCard> recruitCardsList = new Dictionary<string, RecruitCard>();
    //Diccionario de cartas de tools (id, carta de escena)
    public Dictionary<string, ToolCard> toolsCardsList = new Dictionary<string, ToolCard>();
    //Posiciones de cartas de personajes y tools en la escena
    public Transform[] cardSlots;
    //Listas de cartas
    public List<ToolCard> toolDeck = new List<ToolCard>();
    public List<RecruitCard> recruitDeck = new List<RecruitCard>();
    //Posiciones en escena de las cartas de tools del cliente
    public Transform[] toolCardSlots;
    //Lista de tools para enviar al servidor
    private string[] toolsToCheck;
    //Las cartas de tools en mano
    private ToolCard[] yourToolHand = new ToolCard[8];
    //Qué personaje se está cogiendo para reclutar o aprender
    public RecruitCard recruitBeingUsed;
    //Devuelve true si un personaje está siendo usado para reclutar o aprender
    public bool isRecruitBeingUsed;
    //Lista de tools usada para comprovaciones de validez
    public List<string> matchingTools = new List<string>();
    //Mensaje en pantalla
    public Text message;
    public ButtonUI OkButton;
    public GamePhase phase;
    private GameClient client;
    private State state;
    private int myPlayerNumber;
    [SerializeField]
    private GameObject[] toolCard; //Lista de prefabs de tools, para instanciarlas en pantalla

    void Start()
    {
        toolsToCheck = new string[5];
        int i = 1;
        string id = "";
        foreach (RecruitCard r in recruitDeck)
        {
            id = "r" + i;
            r.id = id;
            recruitCardsList.Add(id, r);
            i++;
        }
        i = 1;
        foreach (ToolCard t in toolDeck)
        {
            id = "T" + i;
            t.id = id;
            toolsCardsList.Add(id, t);
            i++;
        }
        client = GameClient.Instance;

        if (!client.Connected)
        {
            SceneManager.LoadScene("ConnectingScene");
            return;
        }
        client.OnInitialState += InitialStateHandler;
        client.OnGamePhaseChange += GamePhaseChangeHandler;

        if (client.State != null)
        {
            InitialStateHandler(this, client.State);
        }

    }

    private void OnDestroy()
    {
        client.OnInitialState -= InitialStateHandler;
        client.OnGamePhaseChange -= GamePhaseChangeHandler;
    }
    //Robar cartas, limpiar la tienda y hacer refresh
    public void BeginRecruit()
    {

        phase = GamePhase.Recruit;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DrawToolCards();
        if (state.firstTurn)
        {
            string[][] recruitToDestroy = new string[4][];
            recruitToDestroy[0] = new string[] { "", "" };
            recruitToDestroy[1] = new string[] { "", "" };
            recruitToDestroy[2] = new string[] { "", "" };
            recruitToDestroy[3] = new string[] { "", "" };

            int indexShop = 6;
            for (int i = 0; i < recruitToDestroy.Length; i++)
            {
                if (state.cards[indexShop] != null && state.cards[indexShop] != "")
                {
                    recruitToDestroy[i][0] = state.cards[indexShop];
                    recruitToDestroy[i][1] = indexShop.ToString();
                    indexShop++;
                }
                else
                {
                    recruitToDestroy[i][0] = "";
                }
            }
            client.DestroyRecruits(recruitToDestroy);
            for (int j = 0; j < recruitToDestroy.Length; j++)
            {
                client.NullRecruit(indexShop.ToString());
            }
            client.RefreshShop();
        }

        message.text = "Recruit an entrepeneur";
    }

    public void WaitForOpponentToRecruit()
    {
        phase = GamePhase.Recruit;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        message.text = "Waiting for an opponent to recruit";
    }

    public void BeginLearn()
    {
        phase = GamePhase.Learn;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        message.text = "Learn tools from your entrepeneurs";
    }

    public void WaitForOpponentLearn()
    {
        phase = GamePhase.Learn;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        message.text = "Waiting for an opponent to learn";
    }

    public void ShowResult()
    {
        phase = GamePhase.Result;
        if (state.winningPlayer == myPlayerNumber)
        {
            message.text = "Winner!";
        }
        else
        {
            message.text = "Loser...";
        }
    }

    private void DrawToolCards()
    {
        int rand = 0;
        for (int i = 0; i < toolCardSlots.Length; i++)
        {
            if (yourToolHand[i] == null)
            {
                rand = Random.Range(1, 10);
                GameObject tc = Instantiate(toolCard[rand - 1], toolCardSlots[i].position, Quaternion.identity);
                yourToolHand[i] = tc.GetComponent<ToolCard>();
                tc.gameObject.SetActive(true);
            }
        }
    }

    private void Skip()
    {
        client.SendSkip();
    }

    public void OK()
    {
        if (isRecruitBeingUsed && phase == GamePhase.Recruit)
        {
            if (CountDiscarded() == recruitBeingUsed.tools)
            {
                DiscardTools();
                isRecruitBeingUsed = false;
                int oldPosition = recruitBeingUsed.position;
                if (myPlayerNumber == 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (state.cards[i] == null || state.cards[i] == "")
                        {
                            recruitBeingUsed.position = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 3; i < 6; i++)
                    {
                        if (state.cards[i] == null || state.cards[i] == "")
                        {
                            recruitBeingUsed.position = i;
                            break;
                        }
                    }
                }
                client.SendRecruited(recruitBeingUsed.id, oldPosition, recruitBeingUsed.position);
                recruitBeingUsed.UnhighlightRecruit();
                if (state.playersSkipped == 1)
                {
                    client.SendSkip();
                }
            }
        }
        //Comprovar que las cartas son correctas
        else if (isRecruitBeingUsed && phase == GamePhase.Learn)
        {
            matchingTools.Clear();
            int indexTools = 0;
            for (int i = 0; i < toolCardSlots.Length; i++)
            {
                if (yourToolHand[i] != null)
                {
                    if (yourToolHand[i].isBeingDiscarded)
                    {
                        string tagTool = yourToolHand[i].tag;
                        if (recruitBeingUsed.toolPool.Contains(tagTool))
                        {
                            if (matchingTools.Contains(tagTool))
                            {
                                return;
                            }
                            matchingTools.Add(tagTool);
                            toolsToCheck[indexTools] = tagTool;
                            indexTools++;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

            }
            //Destruir el personaje en tienda y enviar petición al server.
            isRecruitBeingUsed = false;
            string[][] recruitToDestroy = new string[4][];
            recruitToDestroy[0] = new string[] { "", "" };
            recruitToDestroy[1] = new string[] { "", "" };
            recruitToDestroy[2] = new string[] { "", "" };
            recruitToDestroy[3] = new string[] { "", "" };

            recruitToDestroy[0][0] = recruitBeingUsed.id;
            recruitToDestroy[0][1] = recruitBeingUsed.position.ToString();
            client.DestroyRecruits(recruitToDestroy);
            DiscardTools();
            client.SendLearn(recruitBeingUsed.id, recruitBeingUsed.position, toolsToCheck);
            client.NullRecruit(recruitBeingUsed.position.ToString());
            if (state.playersSkipped == 1)
            {
                client.SendSkip();
            }
        }
    }

    public void ActivateButtons(bool action)
    {
        OkButton.Activate(action);
    }

    public void UnhighlightTools()
    {
        for (int i = 0; i < toolCardSlots.Length; i++)
        {
            if (yourToolHand[i] != null)
            {
                yourToolHand[i].isBeingDiscarded = false;
                yourToolHand[i].GetComponent<Renderer>().material.color = yourToolHand[i].tempColor;
            }

        }
    }

    private int CountDiscarded()
    {
        int discardedNumForRecruit = 0;
        for (int i = 0; i < toolCardSlots.Length; i++)
        {
            if (yourToolHand[i] != null)
            {
                if (yourToolHand[i].isBeingDiscarded)
                {
                    discardedNumForRecruit++;
                }
            }

        }
        return discardedNumForRecruit;
    }

    private void DiscardTools()
    {
        for (int i = 0; i < toolCardSlots.Length; i++)
        {
            if (yourToolHand[i] != null)
            {
                if (yourToolHand[i].isBeingDiscarded)
                {
                    yourToolHand[i].gameObject.SetActive(false);
                    yourToolHand[i] = null;
                }
            }

        }
    }

    //networking
    private void InitialStateHandler(object sender, State initialState)
    {
        state = initialState;

        Player me = state.players[client.SessionId];

        myPlayerNumber = me != null ? me.seat : -1;

        //Se ejectua cuando el estado cambia
        state.OnChange += StateChangeHandler;

        //Se ejecuta cuando se modifica la tabla que ve en que posición va cada carta
        state.cards.OnChange += CardHandling;
        state.cards.OnAdd += CardHandling;

        //Se ejecuta cuando se modifica la tabla que ve qué cartas de personaje hay que borrar de la mesa
        state.recruitsToDestroy.OnChange += DestroyCardsHandling;
        state.recruitsToDestroy.OnAdd += DestroyCardsHandling;

        //Verifica el estado inicial
        GamePhaseChangeHandler(this, state.phase);
    }

    private void StateChangeHandler(object sender, Colyseus.Schema.OnChangeEventArgs args)
    {
        foreach (var change in args.Changes)
        {
            if ((change.Field == "playerTurn" || change.Field == "playersSkipped") && state.phase == "Recruit")
            {
                CheckTurnRecruit();
            }
            else if ((change.Field == "playerTurn" || change.Field == "playersSkipped") && state.phase == "Learn")
            {
                CheckTurnLearn();
            }
        }
    }

    private void CardHandling(object sender, Colyseus.Schema.KeyValueEventArgs<string, int> change)
    {
        // 0-2 recruits1
        // 3-5 recruits2
        // 6-9 shop
        // 10-18 toolboard 1
        // 19-27 toolboard 2
        for (int i = 0; i <= 9; i++)
        {
            string id = state.cards[i];
            if (id != null && id != "")
            {
                RecruitCard c = recruitCardsList[id];
                c.gameObject.SetActive(true);
                c.position = i;

                if (myPlayerNumber == 1)
                {
                    c.transform.position = cardSlots[i].position;
                }
                else
                {
                    if (i >= 0 && i <= 2)
                    {
                        c.transform.position = cardSlots[i + 3].position;
                    }
                    else if (i >= 3 && i <= 5)
                    {
                        c.transform.position = cardSlots[i - 3].position;
                    }
                    else
                    {
                        c.transform.position = cardSlots[i].position;
                    }
                }
            }
        }

        for (int i = 10; i <= 27; i++)
        {
            if (state.cards[i] != null && state.cards[i] != "")
            {
                string id = state.cards[i];
                int toolId = (int)char.GetNumericValue(id[1]);
                Vector3 posCard;
                if (myPlayerNumber == 1)
                {
                    posCard = cardSlots[i].position;
                }
                else
                {
                    if (i >= 10 && i <= 18)
                    {
                        posCard = cardSlots[i + 9].position;
                    }
                    else
                    {
                        posCard = cardSlots[i - 9].position;
                    }
                }
                GameObject tc = Instantiate(toolCard[toolId - 1], posCard, Quaternion.identity);
                tc.gameObject.SetActive(true);
            }
        }
    }

    private void DestroyCardsHandling(object sender, Colyseus.Schema.KeyValueEventArgs<string, int> change)
    {
        for (int idxDes = 0; idxDes < state.recruitsToDestroy.Count; idxDes++)
        {
            string id = state.recruitsToDestroy[idxDes];
            if (state.recruitsToDestroy[idxDes] != null && state.recruitsToDestroy[idxDes] != "")
            {
                GameObject go = GameObject.Find(id);
                Destroy(go);
            }
        }
    }

    private void CheckTurnRecruit()
    {
        if (state.playerTurn == myPlayerNumber)
        {
            BeginRecruit();
        }
        else
        {
            WaitForOpponentToRecruit();
        }
    }

    private void CheckTurnLearn()
    {
        if (state.playerTurn == myPlayerNumber)
        {
            BeginLearn();
        }
        else
        {
            WaitForOpponentLearn();
        }
    }

    private void Leave()
    {
        client.Leave();
        SceneManager.LoadScene("ConnectingScene");
    }

    private void GamePhaseChangeHandler(object sender, string phase)
    {
        switch (phase)
        {
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
