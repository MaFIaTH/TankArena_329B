using System;
using UnityEngine;


public enum Map
{
   Default
}


public enum GameMode
{
   Default
}

public enum GameQueue
{
   Solo,
   Team
}

[Serializable]
public class UserData
{
   public string username;
   public string userAuthId;
   public int teamIndex = -1;
   public int userColorIndex;
   public GameInfo userGamePreferences = new();
}

[Serializable]

public class GameInfo
{
   public Map map;
   public GameMode gameMode;
   public GameQueue gameQueue;

   public string ToMultiplayQueue()
   {
      return gameQueue switch
      {
         GameQueue.Solo => "solo-queue",
         GameQueue.Team => "team-queue",
         _ => "solo-queue"
      };
   }
}