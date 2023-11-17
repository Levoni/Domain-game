using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
   public Sprite[] sprites = new Sprite[11];
   public Sprite[] buildingSprites = new Sprite[4];

   public Sprite GetSprite(int id)
   {
      return sprites[id];
   }
   public Sprite GetBuildingSprite(Building building)
   {
      return buildingSprites[(int)building];
   }
}
