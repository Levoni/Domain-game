using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
   public int resources = 10;
   public int soldiers = 50;
   public int citizens = 1000;
   public int refugees = 0;
   public int currentLocation = -1;
   public int playerHealth = 3;
   public int remainingMoves = 9;
   public int requiredAmount = 100;
   public int level = 0;
   public float marketMultiplier = 1;
   public int influence = 0;
   public bool hasBuildMarketThisRound = false;
   public GameObject[] Board = new GameObject[9];
   public GameObject player;
   public Power currentPower = Power.none;
   public GameState state = GameState.Menu;
   public TMP_Dropdown dropdown;
   public GameObject button;
   public GameObject endUI;
   public GameObject UploadText;
   public GameObject QuickInfo;
   public GameObject Menu;

   const int SMALL_BONUS = 5;
   const int MEDIUM_BONUS = 10;
   const int LARGE_BONUS = 20;

   //string baseURL = "http://localhost:3752";
   string baseURL = "http://levonpersonalplayarea.com/iisnodetest";

   Dictionary<int, List<int>> connectedLocations = new Dictionary<int, List<int>>()
   {
      { -1, new List<int>() { 0, 1, 2, 3, 5, 6, 7, 8 } },
      { 0, new List<int>() { 1, 3 } },
      { 1, new List<int>() { 0, 2, 4 } },
      { 2, new List<int>() { 1, 5 } },
      { 3, new List<int>() { 0, 4, 6 } },
      { 4, new List<int>() { 1, 3, 5, 7 } },
      { 5, new List<int>() { 2, 4, 8 } },
      { 6, new List<int>() { 3, 7 } },
      { 7, new List<int>() { 4, 6, 8 } },
      { 8, new List<int>() { 5, 7 } },
   };


   // Start is called before the first frame update
   void Start()
   {
      endUI.SetActive(false);
      state = GameState.Play;
      BeginLevel();
   }

   // Update is called once per frame
   void Update()
   {

   }

   public void BeginLevel()
   {
      if (soldiers >= requiredAmount &&
         resources >= requiredAmount)
      {
         EndGame(true);
      }
      else
      {
         level++;
         var ids = GenerateIdList();
         if (level > 5)
         {
            citizens -= 100;
            refugees += 20;
         }
         if (citizens <= 0)
         {
            EndGame(false);
         }
         if (!hasBuildMarketThisRound)
         {
            marketMultiplier = 1;
         }
         hasBuildMarketThisRound = false;
         for (int i = 0; i < 9; i++)
         {
            int index = Mathf.FloorToInt(UnityEngine.Random.Range((int)0, (int)ids.Count));
            var card = Board[i].GetComponent<Card>();
            var sprite = GameObject.Find("SpriteManager").GetComponent<SpriteManager>().GetSprite(ids[index].Item2);
            card.SetCard(ids[index].Item2, ids[index].Item1 + 1, i, sprite);
            SetAllSelectableCardsButCurrentLocation(4);
            ids.RemoveAt(index);
         }
         remainingMoves = 9;
         currentLocation = -1;
         currentPower = Power.none;
         player.transform.position = new Vector3(7, -3.5f, -2);
         dropdown.options.Clear();
         ResetPowerCards();
         UpdateText();
      }
   }
   public bool HandleCardClick(GameObject obj, int id, bool firstTime, int location)
   {
      if(remainingMoves <= 0 || state != GameState.Play)
      {
         return false;
      }
      var isConnected = VerifyConnection(currentLocation, location);
      if(!isConnected)
      {
         return false;
      }
      var card = obj.GetComponent<Card>();
      if (currentPower == Power.scout)
      {
         card.ShowHint();
         var powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
         powerCard.FlipCard();
         remainingMoves -= 1;
         currentPower = Power.none;
         UpdateText();
         return false;
      }
      if(firstTime)
      {
         CalculateInfluence(location);
      }
      ApplyCardEffect(id, firstTime);
      var cardTransform = obj.transform;
      var cardRenderer = obj.GetComponent<SpriteRenderer>();
      //Move Player
      player.transform.position = new Vector3(cardTransform.position.x + cardRenderer.sprite.bounds.size.x / 40,
         cardTransform.position.y + cardRenderer.sprite.bounds.size.y / 40, -2);
      currentLocation = location;
      remainingMoves -= 1;

      //Handle undercover power
      if(currentPower == Power.undercover)
      {
         var powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
         powerCard.FlipCard();
      }

      //UpdateUI
      UpdateText();
      UpdateBuildMenu(card);
      UpdateSelectableCards(currentLocation);
      return true;
   }
   public void HandlePowerClick(Power power)
   {
      if(state != GameState.Play)
      {
         return;
      }
      //Deselct power if already selected
      if(currentPower != Power.none && currentPower == power)
      {
         SetPowerCardShading(Power.none);
         if(currentPower == Power.undercover)
         {
            UpdateSelectableCards(currentLocation);
         }
         currentPower = Power.none;
         return;
      }
      //Set power
      SetPowerCardShading(power);
      currentPower = power;

      //Handle power effects
      //undercover
      if(power == Power.undercover)
      {
         SetAllSelectableCardsButCurrentLocation(currentLocation);
      } else
      {
         UpdateSelectableCards(currentLocation);
      }

      //Wisper
      if(power == Power.wispers && currentLocation != -1)
      {
         currentPower = Power.none;
         remainingMoves -= 1;
         //TODO: Do wispers thing
         var locationList = connectedLocations[currentLocation];
         for(int i = 0; i < locationList.Count; i++)
         {
            var gameObject = Board[locationList[i]];
            var card = gameObject.GetComponent<Card>();
            card.ShowHint();
         }
         var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
         powerCard.FlipCard();
         UpdateText();
      }
   }
   public void HandleBuildClick()
   {
      if(state != GameState.Play)
      {
         return;
      }
      if (remainingMoves > 0)
      {
         var card = Board[currentLocation].GetComponent<Card>();
         if (card.building != Building.None) { return; }
         this.gameObject.GetComponent<AudioSource>().PlayOneShot(GameObject.Find("SoundManager").GetComponent<SoundManager>().GetAudioClip(Audio.hammer));
         var multiplier = card.id == 5 ? 2 : 1;
         var item = dropdown.options[dropdown.value];
         var text = item.text;
         if (text == "Market")
         {
            marketMultiplier += .3f * multiplier;
            resources += (int)(SMALL_BONUS * multiplier * marketMultiplier);
            hasBuildMarketThisRound = true;
            card.SetBuilding(Building.Market);
         }
         else if (text == "Blacksmith")
         {
            resources += MEDIUM_BONUS * multiplier;
            card.SetBuilding(Building.Blacksmith);
         }
         else if (text == "Recruitment center")
         {
            soldiers += SMALL_BONUS * multiplier;
            card.SetBuilding(Building.RecruitmentCenter);
         }
         else if (text == "Barracks")
         {
            soldiers += MEDIUM_BONUS * multiplier;
            card.SetBuilding(Building.Barracks);
         }
         remainingMoves--;
         UpdateText();
         dropdown.ClearOptions();
         dropdown.gameObject.SetActive(false);
         button.SetActive(false);
      }
   }

   public void HandleUniquePowerClick(UniquePower power)
   {
      if(state != GameState.Play)
      {
         return;
      }
      //rally
      if (power == UniquePower.rally)
      {
         if (influence >= 5)
         {
            soldiers += SMALL_BONUS;
            currentPower = Power.none;
            influence -= 5;
            UpdateText();
         }
      }
   }
   private bool VerifyConnection(int startLocation, int location)
   {
      if(currentPower == Power.undercover
         || currentPower == Power.scout)
      {
         return true;
      }

      var index = connectedLocations[startLocation].FindIndex((x) => { return x == location; });
      return index != -1;


      //if(startLocation == -1)
      //{
      //   return location == 0 || location == 1 || location == 2;
      //} else if(startLocation == 0)
      //{
      //   return location == 1 || location == 3;
      //} else if(startLocation == 1)
      //{
      //   return location == 0 || location == 2 || location == 4;
      //} else if (startLocation == 2)
      //{
      //   return location == 1 || location == 5;
      //} else if(startLocation == 3)
      //{
      //   return location == 0 || location == 4 || location == 6;
      //} else if(startLocation == 4)
      //{
      //   return location == 1 || location == 3 || location == 5 || location == 7;
      //} else if(startLocation == 5)
      //{
      //   return location == 2 || location == 4 || location == 8;
      //} else if(startLocation == 6)
      //{
      //   return location == 3 || location == 7;
      //} else if(startLocation == 7)
      //{
      //   return location == 4 || location == 6 || location == 8;
      //} else if(startLocation == 8)
      //{
      //   return location == 5 || location == 7;
      //}
      //return false;
   }
   private void ApplyCardEffect(int id, bool firstTime)
   {
      if (firstTime)
      {
         if (id == 1)
         {
            resources += SMALL_BONUS;
         }
         else if (id == 2)
         {
            remainingMoves += 1;
         }
         else if (id == 3)
         {
            resources += MEDIUM_BONUS;
         }
         else if (id == 9)
         {
            soldiers += refugees;
            refugees = 0;
         }
         else if (id == 4)
         {
            soldiers += SMALL_BONUS;
         }
         else if (id == 10)
         {

         }
         else if (id == 5)
         {

         }
         else if (id == 6)
         {
            soldiers -= SMALL_BONUS;
            if (resources < 0 || soldiers < 0)
            {
               EndGame(false);
            }
         }
         else if (id == 7)
         {
            soldiers -= MEDIUM_BONUS;
            if (resources < 0 || soldiers < 0)
            {
               EndGame(false);
            }
         }
         else if (id == 8)
         {
            resources -= MEDIUM_BONUS;
            soldiers -= MEDIUM_BONUS;
            remainingMoves = 0;
            if(resources < 0 || soldiers < 0)
            {
               EndGame(false);
            }
         }
         else if (id == 11)
         {
            playerHealth -= 1;
            remainingMoves = 0;
            if(playerHealth <= 0)
            {
               EndGame(false);
            }
            // Check for zero health loss condition
         }
      } 
      else
      {
         if (id == 2)
         {
            remainingMoves += 1;
         }
      }
   }
   private void CalculateInfluence(int location)
   {
      var row = location / 3;
      var column = location % 3;
      var completeRowOrColumn = 1;
      for(int i = row * 3;i < (row + 1) * 3; i++)
      {
         var card = Board[i].GetComponent<Card>();
         if(location != i && !card.isFlipped)
         {
            completeRowOrColumn -= 1;
            break;
         }
      }
      completeRowOrColumn += 1;
      for(int i = column; i <= 8; i+=3) {
         var card = Board[i].GetComponent<Card>();
         if (location != i && !card.isFlipped)
         {
            completeRowOrColumn -= 1;
            break;
         }
      }
      influence += completeRowOrColumn;
   }
   private void UpdateBuildMenu(Card card)
   {
      dropdown.ClearOptions();
      dropdown.gameObject.SetActive(false);
      button.SetActive(false);
      if(card.building != Building.None)
      {
         return;
      }
      if ( card.id == 5 )
      {
         dropdown.gameObject.SetActive(true);
         button.SetActive(true);
         dropdown.AddOptions(new List<string>() { "Market","Blacksmith","Recruitment center" });
      } else if ( card.id == 10)
      {
         dropdown.gameObject.SetActive(true);
         button.SetActive(true);
         dropdown.AddOptions(new List<string>() { "Market", "Blacksmith", "Recruitment center" });
      } else if ( card.id == 4)
      {
         dropdown.gameObject.SetActive(true);
         button.SetActive(true);
         dropdown.AddOptions(new List<string>() { "Barracks" });
      }
   }
   private void UpdateSelectableCards(int location)
   {
      for(int i = 0; i < Board.Length; i++)
      {
         var card = Board[i].GetComponent<Card>();
         card.setShading(connectedLocations[location].FindIndex((x) => x == i) != -1);
      }
   }
   private void SetAllSelectableCardsButCurrentLocation(int location)
   {
      for (int i = 0; i < Board.Length; i++)
      {
         var card = Board[i].GetComponent<Card>();
         card.setShading(i!=location);
      }
   }
   private void UpdateText()
   {
      GameObject.Find("Move Text").GetComponent<TextMeshProUGUI>().text = remainingMoves.ToString();
      GameObject.Find("Resource Text").GetComponent<TextMeshProUGUI>().text = resources.ToString();
      GameObject.Find("Troop Text").GetComponent<TextMeshProUGUI>().text = soldiers.ToString();
      GameObject.Find("Citizen Text").GetComponent<TextMeshProUGUI>().text = citizens.ToString();
      GameObject.Find("Wound Text").GetComponent<TextMeshProUGUI>().text = playerHealth.ToString();
      GameObject.Find("Refugee Text").GetComponent<TextMeshProUGUI>().text = refugees.ToString();
      GameObject.Find("Influence Text").GetComponent<TextMeshProUGUI>().text = influence.ToString();
   }
   private void ResetPowerCards()
   {
      var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
      powerCard.ResetCard();
      powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
      powerCard.ResetCard();
      powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
      powerCard.ResetCard();
   }
   private void SetPowerCardShading(Power power)
   {
      if(power == Power.none)
      {
         var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
         powerCard.SetShading(false);
      } else if(power == Power.scout)
      {
         var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
         powerCard.SetShading(true);
      } else if (power == Power.undercover)
      {
         var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
         powerCard.SetShading(true);
         powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
         powerCard.SetShading(false);
      } else if (power == Power.wispers)
      {
         var powerCard = GameObject.Find("Wispers").GetComponent<PowerCard>();
         powerCard.SetShading(true);
         powerCard = GameObject.Find("Undercover").GetComponent<PowerCard>();
         powerCard.SetShading(false);
         powerCard = GameObject.Find("Scout").GetComponent<PowerCard>();
         powerCard.SetShading(false);
      }
   }
   private List<(int,int)> GenerateIdList()
   {
      List<(int,int)> cards = new List<(int,int)>();
      cards.Add(Constents.CardOneArray[0]);
      cards.Add(Constents.CardTwoArray[0]);
      cards.Add(Constents.CardThreeArray[0]);
      if (level <= 5) {
         cards.Add(Constents.CardFourArray[0]);
      } else {
         cards.Add(Constents.CardFourArray[Mathf.FloorToInt(UnityEngine.Random.Range(0, 2))]);
      }
      cards.Add(Constents.CardFiveArray[Mathf.FloorToInt(UnityEngine.Random.Range(0,2))]);
      cards.Add(Constents.CardSixArray[0]);
      cards.Add(Constents.CardSevenArray[0]);
      cards.Add(Constents.CardEightArray[0]);
      cards.Add(Constents.CardNineArray[Mathf.FloorToInt(UnityEngine.Random.Range(0, 2))]);
      return cards;
   }
   private void EndGame(bool win)
   {
      if (state != GameState.End) {
         var equalResourceScore = Mathf.Min(Mathf.Max(soldiers, 0), Mathf.Max(resources, 0));
         var differentResourceScore = soldiers - equalResourceScore + resources - equalResourceScore;
         var totalResourceScore = equalResourceScore * 3 + differentResourceScore;
         var score = win ? citizens + totalResourceScore : totalResourceScore;
         endUI.SetActive(true);
         state = GameState.End;
         GameObject.Find("EndGame").GetComponent<TextMeshProUGUI>().text = win ? "Win!" : "Game Over";
         GameObject.Find("EndScore").GetComponent<TextMeshProUGUI>().text = "Score: " + score;
         SetLocalHighScore(score);
         SetHighScore(score);
      }
   }
   private async void SetHighScore(int score)
   {
      var username = PlayerPrefs.GetString("username", "");
      var password = PlayerPrefs.GetString("password","");
      if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
      {
         var helper = new HTTPRequestHelper();
         UploadText.GetComponent<TextMeshProUGUI>().text = "Uploading Score...";
         var success = await helper.SendSetHighScoreRequet(score);
         if(success)
         {
            UploadText.GetComponent<TextMeshProUGUI>().text = "Uploaded Score";
         } else
         {
            UploadText.GetComponent<TextMeshProUGUI>().text = "Failed to Upload Score";
         } 
      }
   }
   private void SetLocalHighScore(int score)
   {
      //all time scores
      var highScoresString = PlayerPrefs.GetString("localHighScore");
      var highScores = JsonUtility.FromJson<Leaderboard>(highScoresString);
      UpdateHighScoreList(highScores.highScores, score, highScores.maxCount);
      var objectString = JsonUtility.ToJson(highScores);
      PlayerPrefs.SetString("localHighScore", objectString);
      //daily scores
      var lastUpdate = PlayerPrefs.GetFloat("lastLocalUpdate"); 
      var updateDate = DateTime.FromBinary((long)lastUpdate);
      var currentDate = DateTime.Now;
      var dailyHighScoresString = PlayerPrefs.GetString("localDailyHighScore");
      var dailyHighScores = JsonUtility.FromJson<Leaderboard>(dailyHighScoresString);
      if (currentDate.Day != updateDate.Day || currentDate.Month != updateDate.Month
         || currentDate.Year != updateDate.Year)
      {
         dailyHighScores.highScores = new List<HighScore>();
      }
      UpdateHighScoreList(dailyHighScores.highScores, score, dailyHighScores.maxCount);
      var dailyObjectString = JsonUtility.ToJson(dailyHighScores);
      PlayerPrefs.SetString("localDailyHighScore", dailyObjectString);
      //update last updated
      PlayerPrefs.SetFloat("lastLocalUpdate", DateTime.Now.ToBinary());

   }

   private void UpdateHighScoreList(List<HighScore> scores, int score, int maxListSize)
   {
      HighScore holdingScore = new HighScore()
      {
         game = "Domain",
         score = score,
         user_name = "local",
         display_name = "local"
      };
      bool isHighscore = false;
      for(int i = 0; i < maxListSize; i++)
      {
         if(scores.Count <= i)
         {
            scores.Add(holdingScore);
            break;
         }
         if(isHighscore)
         {
            var tempScore = holdingScore;
            holdingScore = scores[i];
            scores[i] = tempScore;
         } 
         else
         {
            if(holdingScore.score > scores[i].score)
            {
               var tempScore = holdingScore;
               holdingScore = scores[i];
               scores[i] = tempScore;
               isHighscore = true;
            }
         }
      }
      if(scores.Count > maxListSize)
      {
         for(int i = scores.Count - 1; i >= maxListSize; i--)
         {
            scores.RemoveAt(i);
         }
      }
   }

   #region UIMethods
   public void display(bool isVisible, string ui)
   {
      if(ui == "QuickInfo")
      {
         state = isVisible ? GameState.Menu : GameState.Play;
         QuickInfo.SetActive(isVisible);
      } else if(ui == "Menu")
      {
         state = isVisible ? GameState.Menu : GameState.Play;
         Menu.SetActive(isVisible);
      }
   }
   #endregion
}

public enum Power
{
   none,
   undercover,
   scout,
   wispers
}

public enum UniquePower
{
   rally
}

public enum Building
{
   Market,
   Blacksmith,
   RecruitmentCenter,
   Barracks,
   None,
}

public enum GameState
{
   Menu,
   Play,
   End
}