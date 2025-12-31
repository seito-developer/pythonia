using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

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

        AudioManager.instance.PlayMenu(AudioManager.instance.seMenuSource.clip);
        // 1. ボタンを少し大きくしてから元のサイズに戻す（パンチ演出）
        // 引数：(弾ませる方向と強さ, 持続時間, 弾む回数, 弾み具合)
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 10, 1);
        // 2. 少し遅らせてシーン遷移（アニメーションを見せるため）
        DOVirtual.DelayedCall(0.3f, () =>
        {
            // 実際の遷移処理
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        });
    }
}
