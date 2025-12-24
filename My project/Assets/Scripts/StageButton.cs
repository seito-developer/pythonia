using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    public int stageNumber;
    public Text stageText;

    // ボタンが押された時に実行する
    public void OnClickStage()
    {
        Debug.Log($"ステージ {stageNumber} が選ばれました！");
        // ここで、選ばれたステージ番号を保持してゲーム画面へ遷移
    }
}
