using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageButton : MonoBehaviour
{
    public int stageNumber;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI rankText;

    // ボタンが押された時に実行する
    public void OnClickStage()
    {
        // 1. 自分の持っているIDを預かり所にセット
        GameData.SelectedStageId = stageNumber;

        Debug.Log($"ID:{stageNumber} を保存しました。ゲームシーンへ移動します。");

        // 2. ゲームシーンを読み込む（Scene名の綴りに注意！）
        SceneManager.LoadScene("GameScene");
    }
}
