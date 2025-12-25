using UnityEngine;
using TMPro; // テキスト表示用

public class GameSceneManager : MonoBehaviour
{
    public TextMeshProUGUI stageTitleText;

    void Start()
    {
        // 預かり所からIDを取り出す
        int id = GameData.SelectedStageId;

        if (id == 0)
        {
            stageTitleText.text = "ステージが選択されていません";
        }
        else
        {
            // ここでIDに応じた処理を行う（まずは確認用に表示）
            stageTitleText.text = "ステージ " + id + " 開始！";
            Debug.Log($"ステージ {id} のデータを読み込みます...");
        }
    }
}