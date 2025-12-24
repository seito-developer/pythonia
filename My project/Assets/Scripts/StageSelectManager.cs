using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    public GameObject stageButtonPrefab; // ボタンの設計図
    public Transform container;          // ボタンを並べる場所

    void Start()
    {
        // 例えば、12ステージ分作成する場合
        for (int i = 1; i <= 5; i++)
        {
            // 1. ボタンを生成してContainerの子要素にする
            GameObject btnObj = Instantiate(stageButtonPrefab, container);

            // 2. ボタンについているStageButtonスクリプトを取得
            StageButton script = btnObj.GetComponent<StageButton>();

            if (script != null)
            {
                // 3. ステージ番号をセット（これでボタンごとに個性がつく）
                script.stageNumber = i;

                // 4. テキストも書き換える
                if (script.stageText != null)
                {
                    script.stageText.text = "ステージ " + i;
                }
            }
        }
    }
}
