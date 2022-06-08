using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Instance.InitializeClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
