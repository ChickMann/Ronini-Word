using System;
using UnityEngine;

public class LayerConfig :MonoBehaviour
{
   public GameObject[] layer;
   public GameObject layerController;
   public Rigidbody2D body {get;private set;} 
   public Transform startPos;
   public Transform endPos;
   private GameObject currentLayer;

   private int index;

   private void Start()
   {
      index = 0;
      body = layerController.GetComponent<Rigidbody2D>();
   }

   public void RepeatLayer()
   {
       currentLayer = layer[index];
         if (currentLayer.transform.position.x < endPos.transform.position.x)
         {
            currentLayer.transform.position = startPos.transform.position;
            if(index >= layer.Length -1) index = 0 ;
            else index++;
         }
   }
}
