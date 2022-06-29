using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolCard : MonoBehaviour
{
    public GameManager gm;

    public Color tempColor;

    public bool isBeingDiscarded;

    public string id;

    
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        tempColor = GetComponent<Renderer>().material.color;
    }

    private void OnMouseDown() {
        if(gm.isRecruitBeingUsed&&!isBeingDiscarded){
            GetComponent<Renderer>().material.color = Color.red;
            isBeingDiscarded=true;
            return;
        }
        if(isBeingDiscarded){
            isBeingDiscarded=false;
            GetComponent<Renderer>().material.color = tempColor;
            return;
        }
    }
}
