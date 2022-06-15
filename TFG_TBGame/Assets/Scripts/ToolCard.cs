using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*enum ToolTypes{
    Tentativa,
    Observaciones, 
    Oportunidad,
    Llaves,
    Business_idea,
    Oferta,
    Afirmaciones,
    Ruta_financiera,
    Deliverable,
}*/

public class ToolCard : MonoBehaviour
{
    // Start is called before the first frame update

    public GameManager gm;

    public Color tempColor;

    public bool isBeingDiscarded;

    public string id;


    
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        tempColor = GetComponent<Renderer>().material.color;

    }

    // Update is called once per frame
    void Update()
    {
        
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
