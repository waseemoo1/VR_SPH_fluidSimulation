using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{

    public Text ParticleCount;
    public Text Damping;
    int particla_count=35444;
    float H=1;
    float restDensity=15;
    int siffness=1000;
    float viscosity=1;
    float radius=1;
    float mass=0.1f;
    int row_size=90;
    float DAMPING_COEFFICIENT=-0.05f;
    float particle_Drag=0.025f;



    #region World Settings
    public void Set_particleCount(float value) {
        particla_count=(int)value;
        ParticleCount.text=particla_count.ToString();
    }
    public void Set_H(String value)
    {
        H=float.Parse(value);
    }

    public void Set_rest_density(String value)
    {
        restDensity=float.Parse(value);
    }

    public void Set_siffness(String value)
    {
        siffness=int.Parse(value);
    }

    public void Set_Viscosity(String value)
    {
        viscosity=float.Parse(value);
    }
    public void Set_radius(String value)
    {
        radius=float.Parse(value);
    }
    public void Set_damping(float value)
    {
        DAMPING_COEFFICIENT=(float)Math.Round(value, 2);
        Damping.text=DAMPING_COEFFICIENT.ToString();
    }
    public void Set_mass(String value)
    {
        mass=float.Parse(value);
    }
    public void Set_row_size(String value)
    {
        row_size=int.Parse(value);
    }
    public void Set_particle_drag(String value)
    {
        particle_Drag=float.Parse(value);
    }
    public void button_clicked() {
        PlayerPrefs.SetInt("particla_count",particla_count);
        PlayerPrefs.SetFloat("H",H);
        PlayerPrefs.SetFloat("restDensity",restDensity);
        PlayerPrefs.SetFloat("siffness",siffness);
        PlayerPrefs.SetFloat("viscosity",viscosity);
        PlayerPrefs.SetFloat("radius",radius);
        PlayerPrefs.SetFloat("mass",mass);
        PlayerPrefs.SetInt("row_size",row_size);
        PlayerPrefs.SetFloat("DAMPING_COEFFICIENT",DAMPING_COEFFICIENT);
        PlayerPrefs.SetFloat("particle_Drag",particle_Drag);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    #endregion



}

