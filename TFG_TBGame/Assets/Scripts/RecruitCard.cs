using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RecruitCard : MonoBehaviour
{

    public List<ToolCard> toolPoolCards = new List<ToolCard>();
    public List<string> toolPool = new List<string>();
    private bool isTaken;
    public int tools;

    public bool isBeingUsed;

    public Color tempColor;

    public int position;

    public string id;

    
     
    public GameManager gm;

   private void OnMouseDown() {
        if(isTaken == false&&!gm.recruitBeingUsed){
            useRecruit();
            return;
        }
        if(isBeingUsed){
            GetComponent<Renderer>().material.color = tempColor;
            isBeingUsed = false;
            gm.recruitBeingUsed = null;
            gm.isRecruitBeingUsed = false;
            gm.ActivateButtons(false);
            gm.UnhighlightTools();
            return;
        }
        if(isTaken&&gm.phase==GamePhase.Learn){
            useRecruit();
            return;
        }
    }

    public void UnhighlightRecruit(){
        GetComponent<Renderer>().material.color = tempColor;
        isBeingUsed = false;
        gm.recruitBeingUsed = null;
        gm.isRecruitBeingUsed = false;
        gm.ActivateButtons(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        tempColor = GetComponent<Renderer>().material.color;
        foreach(ToolCard t in toolPoolCards){
            toolPool.Add(t.tag);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void useRecruit(){
        GetComponent<Renderer>().material.color = Color.red;
        gm.isRecruitBeingUsed=true;
        gm.recruitBeingUsed=this;
        isBeingUsed=true;
        gm.ActivateButtons(true);
    }
}
