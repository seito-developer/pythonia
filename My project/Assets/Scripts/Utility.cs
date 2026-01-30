using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public static class Utility
{
    public static void UpdateLifeUI(int currentLife, List<GameObject> lifeIcons)

    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            // i番目のアイコンを表示するかどうか判定
            // 例：ライフが2なら、0番目と1番目は表示(true)、2番目は非表示(false)
            if (i < currentLife)
            {
                lifeIcons[i].SetActive(true);
            }
            else
            {
                // 非表示にする際、DOTweenで少し演出を入れると豪華になります
                if (lifeIcons[i].activeSelf)
                {
                    // 小さくなって消える演出（任意）
                    lifeIcons[i].transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                    {
                        lifeIcons[i].SetActive(false);
                    });
                }
            }
        }
    }

    public static void Miss(int currentLife, List<GameObject> lifeIcons, System.Action GameOver)
    {
        UpdateLifeUI(currentLife, lifeIcons);
        Debug.Log($"ミス！残りライフ: {currentLife}");

        AudioManager.instance?.PlayMiss(AudioManager.instance.seMissSource?.clip);

        if (currentLife <= 0)
        {
            GameOver();
        }
    }
}