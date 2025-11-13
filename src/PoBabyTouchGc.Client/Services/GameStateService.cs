using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoBabyTouchGc.Client.Services
{
    public class GameStateService
    {
        public int CurrentScore { get; private set; }
        public bool IsGameActive { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action? OnChange;

        public GameStateService()
        {
            CurrentScore = 0;
            IsGameActive = false;
            IsGameOver = false;
        }

        public void StartGame()
        {
            IsGameActive = true;
            IsGameOver = false;
            CurrentScore = 0;
            NotifyStateChanged();
        }

        public void EndGame()
        {
            IsGameActive = false;
            IsGameOver = true;
            NotifyStateChanged();
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            NotifyStateChanged();
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
