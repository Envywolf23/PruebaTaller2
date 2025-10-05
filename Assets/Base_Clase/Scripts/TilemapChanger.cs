using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapChanger : MonoBehaviour
{
    [SerializeField] private GameObject Level1;
    [SerializeField] private GameObject Level2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<ClassPlayerMovement>() != null)
        {
            ClassPlayerMovement player = collision.GetComponent<ClassPlayerMovement>();

            if (Level1.activeInHierarchy)
            {
                Level1.SetActive(false);
                Level2.SetActive(true);
            }
            else if(Level2.activeInHierarchy)
            {
                Level2.SetActive(false);
                Level1.SetActive(true);
            }
        }
    }

    // Código para que el jugador crezca si se queda dentro del collider

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.GetComponent<Movement>() != null)
    //    {
    //        collision.GetComponent<Transform>().localScale = new Vector3(collision.GetComponent<Transform>().localScale.x + 0.01f, collision.GetComponent<Transform>().localScale.y + 0.01f, collision.GetComponent<Transform>().localScale.z + 0.01f);
    //    }
    //}
}
