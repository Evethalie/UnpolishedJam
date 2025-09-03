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
      animator.enabled = true;
      animator.SetBool("Open", true);
   }

   public IEnumerator Close()
   {
      animator.SetBool("Open", false);
      yield return new WaitForSeconds(0.3f);
      animator.enabled = false;
   }
}
