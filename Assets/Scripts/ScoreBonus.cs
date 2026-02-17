using System;
using UnityEngine;

public class ScoreBonus : MonoBehaviour
{
   [SerializeField]
   private int _score;

   private float _rotationSpeed;

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
