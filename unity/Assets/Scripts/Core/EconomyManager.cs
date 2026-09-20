using System;
using UnityEngine;

namespace GearDefenders
{
    public class EconomyManager : MonoBehaviour
    {
        public int Coins { get; private set; }
        public int KillsThisRound { get; private set; }
        public int RoundKillTarget { get; private set; }

        public event Action<int> CoinsChanged;
        public event Action<int, int> KillsChanged;

        public void ResetRun(int startingCoins)
        {
            Coins = startingCoins;
            KillsThisRound = 0;
            RoundKillTarget = 0;
            CoinsChanged?.Invoke(Coins);
            KillsChanged?.Invoke(KillsThisRound, RoundKillTarget);
        }

        public void PrepareRound(int killTarget)
        {
            KillsThisRound = 0;
            RoundKillTarget = Mathf.Max(0, killTarget);
            KillsChanged?.Invoke(KillsThisRound, RoundKillTarget);
        }

        public bool CanAfford(int cost) => Coins >= cost;

        public bool TrySpend(int cost)
        {
            if (cost < 0 || Coins < cost) return false;
            Coins -= cost;
            CoinsChanged?.Invoke(Coins);
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            CoinsChanged?.Invoke(Coins);
        }

        public void RegisterKill()
        {
            AddCoins(1);
            KillsThisRound++;
            KillsChanged?.Invoke(KillsThisRound, RoundKillTarget);
        }
    }
}
