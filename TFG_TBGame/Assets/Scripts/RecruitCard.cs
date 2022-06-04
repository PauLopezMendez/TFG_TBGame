using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RecruitCard : MonoBehaviour
{

    public bool[] toolPool;
    private bool isTaken;
    public int tools;

    public GameManager gm;

   private void OnMouseDown() {
        if(isTaken == false){
            for(int i=0;i<3;i++){
                if(gm.availableCardSlotsRecruit[i]==true){
                    transform.position = gm.yourRecruitSlots[i].position;
                    gm.availableCardSlotsRecruit[i]=false;
                    return;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
