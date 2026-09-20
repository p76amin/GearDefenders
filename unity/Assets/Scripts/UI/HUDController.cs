using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearDefenders
{
    public class HUDController : MonoBehaviour
    {
        TextMeshProUGUI _roundLabel;
        TextMeshProUGUI _levelLabel;
        TextMeshProUGUI _timerLabel;
        TextMeshProUGUI _speedLabel;
        TextMeshProUGUI _coinsLabel;
        TextMeshProUGUI _killsLabel;
        TextMeshProUGUI _hpLabel;
        UnityEngine.UI.Image _hpFill;
        UnityEngine.UI.Image _timerFill;
        RectTransform _previewRoot;
        Sprite _chipSprite;

        public void Bind(
            TextMeshProUGUI roundLabel,
            TextMeshProUGUI levelLabel,
            TextMeshProUGUI timerLabel,
            TextMeshProUGUI speedLabel,
            TextMeshProUGUI coinsLabel,
            TextMeshProUGUI killsLabel,
            TextMeshProUGUI hpLabel,
            UnityEngine.UI.Image hpFill,
            UnityEngine.UI.Image timerFill,
            RectTransform previewRoot,
            Sprite chipSprite)
        {
            _roundLabel = roundLabel;
            _levelLabel = levelLabel;
            _timerLabel = timerLabel;
            _speedLabel = speedLabel;
            _coinsLabel = coinsLabel;
            _killsLabel = killsLabel;
            _hpLabel = hpLabel;
            _hpFill = hpFill;
            _timerFill = timerFill;
            _previewRoot = previewRoot;
            _chipSprite = chipSprite;
        }

        public void SetRound(int current, int total, int level)
        {
            _roundLabel.text = $"Wave {current}/{total}";
            _levelLabel.text = $"Level {level}";
        }

        public void SetTimer(float remaining, float duration)
        {
            remaining = Mathf.Max(0f, remaining);
            _timerLabel.text = Mathf.CeilToInt(remaining).ToString();
            _timerFill.fillAmount = duration <= 0f ? 0f : remaining / duration;
        }

        public void SetSpeed(int multiplier)
        {
            _speedLabel.text = "x" + multiplier;
        }

        public void SetCoins(int coins)
        {
            _coinsLabel.text = "Coins " + coins;
        }

        public void SetKills(int current, int target)
        {
            _killsLabel.text = $"Kills {current}/{target}";
        }

        public void SetBaseHp(float current, float max)
        {
            _hpLabel.text = Mathf.CeilToInt(current).ToString();
            _hpFill.fillAmount = max <= 0f ? 0f : current / max;
        }

        public void SetEnemyPreview(RoundDefinition round)
        {
            for (int i = _previewRoot.childCount - 1; i >= 0; i--)
                Destroy(_previewRoot.GetChild(i).gameObject);

            if (round?.spawns == null) return;
            var grouped = new Dictionary<string, (EnemyDefinition def, int count)>();
            for (int i = 0; i < round.spawns.Length; i++)
            {
                var s = round.spawns[i];
                if (s?.enemy == null) continue;
                if (!grouped.TryGetValue(s.enemy.id, out var entry))
                    entry = (s.enemy, 0);
                entry.count += s.count;
                grouped[s.enemy.id] = entry;
            }

            foreach (var pair in grouped)
            {
                var chip = UiFactory.Rect("Chip", _previewRoot);
                chip.sizeDelta = new Vector2(72, 36);
                var img = chip.gameObject.AddComponent<UnityEngine.UI.Image>();
                img.sprite = _chipSprite;
                img.color = pair.Value.def.tint;
                img.raycastTarget = false;
                UiFactory.Label("Count", chip, "x" + pair.Value.count, 16, Color.white);
            }
        }
    }
}
