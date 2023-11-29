using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayUI : MonoBehaviour
{
   public GameObject[] pages;
   public int currentPage;
   public GameObject nextButton;
   public GameObject prevButton;

   public void Next()
   {
      if (currentPage < pages.Length - 1)
      {
         pages[currentPage].SetActive(false);
         currentPage++;
         pages[currentPage].SetActive(true);
         prevButton.SetActive(true);
         if(currentPage == pages.Length - 1)
         {
            nextButton.SetActive(false);
         }
      }
   }
   public void Prev()
   {
      if(currentPage > 0)
      {
         pages[currentPage].SetActive(false);
         currentPage--;
         pages[currentPage].SetActive(true);
         nextButton.SetActive(true);
         if(currentPage == 0)
         {
            prevButton.SetActive(false);
         }
      }
   }

    // Start is called before the first frame update
    void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {

   }
}
