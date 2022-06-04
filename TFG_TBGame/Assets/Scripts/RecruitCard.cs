using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RecruitCard : MonoBehaviour
{

    public bool[] toolPool;
    private bool isTaken;
    public int tools;

    public bool isBeingRecruited;

    public Color tempColor;

    //private Vector3 initialPosition;
     
    public GameManager gm;

   private void OnMouseDown() {
        if(isTaken == false&&!gm.recruitIsBeingRecruited){
            //initialPosition = transform.position;
            GetComponent<Renderer>().material.color = Color.red;
            gm.recruitIsBeingRecruited=true;
            gm.cardBeingRecruited=this;
            isBeingRecruited=true;
            gm.ActivateButtons(true);
            return;
        }
        if(isBeingRecruited){
            GetComponent<Renderer>().material.color = tempColor;
            //transform.position = initialPosition;
            isBeingRecruited = false;
            gm.cardBeingRecruited = null;
            gm.recruitIsBeingRecruited = false;
            gm.ActivateButtons(false);
            gm.UnhighlightTools();
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        tempColor = GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int PutRecruitedInZone()
    {
        int pos=-1;
        for (int i = 0; i < 3; i++)
        {
            if (gm.availableCardSlotsRecruit[i] == true)
            {
                transform.position = gm.yourRecruitSlots[i].position;
                gm.availableCardSlotsRecruit[i] = false;
                pos=i;
                break;
            }
        }
        GetComponent<Renderer>().material.color = tempColor;
        isTaken=true;
        return pos;
    }
}
