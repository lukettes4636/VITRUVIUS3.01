using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image imagen; 
    public bool eventoDeActivacion = false;

    void Start()
    {
        
        if (imagen != null)
            imagen.enabled = false;
    }

    void Update()
    {
        
        
        if (imagen != null)
            imagen.enabled = eventoDeActivacion;
    }

    
    public void ToggleImagen()
    {
        if (imagen != null)
            imagen.enabled = !imagen.enabled;
    }

    
    public void MostrarImagen()
    {
        if (imagen != null)
            imagen.enabled = true;
    }

    
    public void OcultarImagen()
    {
        if (imagen != null)
            imagen.enabled = false;
    }
}


