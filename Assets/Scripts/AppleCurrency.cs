using System;
using UnityEngine;

public class AppleCurrency : MonoBehaviour
{
   [SerializeField]
   private int _score;

   private float _rotationSpeed = 5f;

   public int Score => _score;

   private void Update()
   {
      Rotate();
   }

   private void Rotate()
   {
      transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
   }
   
   
}
