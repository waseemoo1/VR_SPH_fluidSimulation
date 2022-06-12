using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{

    TabAreaGroup tabs;
    
    GameObject activePage;
    bool existObj = false;
    int activeObj; 

    // Start is called before the first frame update
    void Start()
    {
        tabs = FindObjectOfType<TabAreaGroup>();
    }

    #region World Settings
    public void Set_camDistance(float value) {
    }
    public void Set_precision(float value)
    {
 
    }

    public void Set_maxDist(float value)
    {

    }

    public void Set_maxSteps(float value)
    {

    }

    public void Set_smoothness(float value)
    {

    }

    public void Set_plane() {

    }
    #endregion



}

