using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate(bool action){
        gameObject.SetActive(action);
    }
}
