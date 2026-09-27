using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [Serializable]
    public class PlayerSaveData
    {
        public float CurrentHP = -1f;
        public float CurrentResource = -1f;

        public float PositionX = 0f;
        public float PositionY = 0f;
        public string CurrentScene = "";
        public string LastCheckpointID = "Default";

        public int Coins = 0;
        public List<string> UnlockedAbilities = new List<string>();
        public List<string> VisitedRoomIDs = new List<string>();

        public ItemSaveData Inventory = new ItemSaveData();

        public Vector2 GetPosition() => new Vector2(PositionX, PositionY);
        public void SetPosition(Vector2 pos) { PositionX = pos.x; PositionY = pos.y; }
    }
}
