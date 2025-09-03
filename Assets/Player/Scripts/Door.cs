using System;
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
   Animator animator;
   
   public void Start()
   {
      animator = GetComponent<Animator>();
      animator.enabled = false;
   }

   public void Open()
   {
     gameObject.SetActive(false);
   }

   public IEnumerator Close()
   {
      gameObject.SetActive(true);
      yield return null;
   }
}
