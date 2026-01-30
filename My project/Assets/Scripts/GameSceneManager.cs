using UnityEngine;
using UnityEngine.UI;
using TMPro; // テキスト表示用
using System.Collections.Generic; // リスト操作に必要
using System.Linq;               // シャッフルに便利
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using DG.Tweening;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public Transform handZone;

    public GameObject questionPanel;
    public TextMeshProUGUI questionText; // 追加：パネル内のテキスト

    [Header("Prefabs")]
    public GameObject piecePrefab;
    private StageInfo currentStage;
    private GamePiece selectedPiece;

    [Header("Board References")]
    public Transform boardZone;

    [Header("Game System")]
    public int maxLife = 3;
    private int currentLife;
    public List<GameObject> lifeIcons;

    [Header("Result UI")]
    public GameObject resultPanel;
    public CanvasGroup resultCanvasGroup;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultMessageText;
    public TextMeshProUGUI actionButtonText;
    public List<Image> resultStarIcons;
    public Sprite activeStarSprite;
    public Sprite emptyStarSprite;

    [Header("B-Plan UI")]
    public Transform questionContentParent; // Vertical Layout Groupを付けた親
    public GameObject textPartPrefab;       // テキスト用プレハブ
    public GameObject codePartPrefab;       // コード枠用プレハブ

    void Start()
    {
        currentLife = maxLife;
        Utility.UpdateLifeUI(currentLife, lifeIcons);
        LoadStageData();
        // SetupGameUI();
        // ShowQuestion();
    }

    // ランクを判定して保存する
    void SaveStageResult()
    {
        string rank = "";
        if (currentLife == 3) rank = "S";
        else if (currentLife == 2) rank = "A";
        else rank = "B";

        // 保存するデータ
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { $"Stage_{currentStage.id}_Rank", rank }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log($"{currentStage.id} のランク {rank} を保存しました"),
            error => Debug.LogError("セーブ失敗: " + error.GenerateErrorReport())
        );
    }

    public void ShowResultPanel(bool isWin)
    {
        resultPanel.SetActive(true);
        actionButtonText.text = "ステージ選択へ";

        // 一旦すべての星を「空」の状態にリセットし、サイズを0にする
        foreach (var star in resultStarIcons)
        {
            star.sprite = emptyStarSprite;
            star.transform.localScale = Vector3.zero;
        }

        // --- パターンA：下からスライドしてくる ---
        resultPanel.transform.localPosition = new Vector3(0, -1000, 0); // 初期位置
        resultPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // パネルが出きった後に、クリア時のみ星をアニメーションさせる
            if (isWin)
            {
                AnimateStars();
            }
        });

        resultCanvasGroup.alpha = 0;
        resultCanvasGroup.DOFade(1f, 0.5f);

        if (isWin)
        {
            AnimateStars();
            SaveStageResult();
            resultTitleText.text = "STAGE CLEAR!";
            resultTitleText.color = Color.yellow;
            resultMessageText.text = "素晴らしい！正解です。";

            AudioManager.instance.PlayResultSuccess(AudioManager.instance.seResultSuccessSource.clip);
        }
        else
        {
            resultTitleText.text = "GAME OVER";
            resultTitleText.color = Color.red;
            resultMessageText.text = "ライフがなくなってしまいました。";

            AudioManager.instance.PlayResultFailure(AudioManager.instance.seResultFailureSource.clip);
        }
    }

    // 星を1つずつ表示させるアニメーション
    void AnimateStars()
    {
        for (int i = 0; i < resultStarIcons.Count; i++)
        {
            if (i < currentLife)
            {
                int index = i; // ラムダ式用にインデックスを保持
                               // 0.2秒ずつずらして実行
                DOVirtual.DelayedCall(index * 0.3f, () =>
                {
                    resultStarIcons[index].sprite = activeStarSprite;
                    // 弾むようなアニメーション
                    resultStarIcons[index].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                    // 星が出る音を鳴らすのもおすすめ
                    // AudioManager.instance.PlaySE(starSE);
                });
            }
            else
            {
                // ライフがない分の星は、うっすら表示するだけにするなどの演出
                resultStarIcons[i].transform.DOScale(Vector3.one, 0.5f);
                resultStarIcons[i].color = new Color(1, 1, 1, 0.3f);
            }
        }
    }

    // パネルのボタンに紐付ける汎用メソッド
    public void OnClickResultButton()
    {
        SceneManager.LoadScene("HigherStages");
    }

    void GameOver()
    {
        Debug.Log("ゲームオーバー...");
        ShowResultPanel(false); // 結果パネルを「失敗」モードで表示
    }

    // パネルを表示するメソッド
    public void ShowQuestion()
    {
        if (currentStage.contents != null)
        {
            foreach (var part in currentStage.contents)
            {
                Debug.Log($"Part : {part}");
                Debug.Log($"Content Part - Type: {part.type}, Value: {part.value}");
                GameObject prefab = (part.type == "code") ? codePartPrefab : textPartPrefab;
                GameObject instance = Instantiate(prefab, questionContentParent);
                var tmp = instance.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = part.value;
            }
        }

        titleText.text = currentStage.stageName;
        questionPanel.SetActive(true);
        questionPanel.transform.localPosition = new Vector3(0, -1000, 0);
        questionPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack);
    }

    // パネルを閉じるメソッド（バツボタンに紐付ける）
    public void HideQuestion()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
            AudioManager.instance.PlayWindow(AudioManager.instance.seWindowSource.clip);
        }
    }

    public void SetSelectedPiece(GamePiece piece)
    {
        // 以前選択していたピースがあれば、ハイライトを解除する
        if (selectedPiece != null)
        {
            selectedPiece.SetHighlight(false);
        }

        // 新しく選択されたピースを保持し、ハイライトを付ける
        selectedPiece = piece;
        selectedPiece.SetHighlight(true);

        AudioManager.instance.PlayPiece(AudioManager.instance.sePieceSource.clip);
        Debug.Log($"ピース {piece.pieceId} が選択されました");
    }

    // --- 追加箇所 3: UIボタン（＋/－）から呼び出すメソッド ---
    public void OnClickIncrease()
    {
        if (selectedPiece != null)
        {
            selectedPiece.IncreaseIndent();
            AudioManager.instance.PlayIndent(AudioManager.instance.seIndentSource.clip);
        }
    }

    public void OnClickDecrease()
    {
        if (selectedPiece != null)
        {
            selectedPiece.DecreaseIndent();
            AudioManager.instance.PlayIndent(AudioManager.instance.seIndentSource.clip);
        }
    }

    public void OnClickExecute()
    {
        // 1. ボードに並んでいるピースの現在の状態を取得
        // 2. 正解データ（currentStage.correctPieces）と比較
        // 3. すべて一致していれば「クリア！」、違えば「失敗...」

        Debug.Log("判定を開始します...");
        CheckAnswer();
    }

    void CheckAnswer()
    {
        // --- デバッグ用チェック ---
        if (currentStage == null)
        {
            Debug.LogError("currentStage (JSON全体) が読み込めていません！");
            return;
        }
        if (currentStage.correctPieces == null)
        {
            Debug.LogError("JSONの中に 'correctPieces' の項目が見つかりません！");
            return;
        }
        // ------------------------

        // 1. Boardの子要素から、GamePieceスクリプトを持っているものだけをリストアップ
        List<GamePiece> placedPieces = new List<GamePiece>();
        for (int i = 0; i < boardZone.childCount; i++)
        {
            GamePiece p = boardZone.GetChild(i).GetComponent<GamePiece>();
            if (p != null)
            {
                placedPieces.Add(p);
            }
        }

        bool isAllCorrect = true;

        // 2. 正解の数と合っているか確認
        if (placedPieces.Count != currentStage.correctPieces.Length)
        {
            Debug.Log($"ピースの数が違います。現在: {placedPieces.Count}枚 / 正解: {currentStage.correctPieces.Length}枚");
            isAllCorrect = false;
        }

        for (int i = 0; i < placedPieces.Count; i++)
        {
            GamePiece piece = placedPieces[i];
            PieceData correctData = currentStage.correctPieces[i];

            // IDとインデントをチェック
            if (piece.pieceId == correctData.id && piece.currentIndent == correctData.indent)
            {
                Debug.Log($"{i + 1}行目: OK");
            }
            else
            {
                Debug.Log($"{i + 1}行目: 間違い！ (期待ID:{correctData.id}, 期待インデント:{correctData.indent})");
                isAllCorrect = false;
            }
        }

        if (isAllCorrect)
        {
            ShowResultPanel(true);
        }
        else
        {
            currentLife--;
            Utility.Miss(currentLife, lifeIcons, GameOver);
        }
    }

    // 個別JSONファイルを読み込む処理
    void LoadStageData()
    {
        int targetId = GameData.SelectedStageId;
        string key = $"Stage_{targetId}";

        var request = new GetTitleDataRequest
        {
            Keys = new List<string> { key }
        };

        PlayFabClientAPI.GetTitleData(request,
            result =>
            {
                if (!this) return;
                if (!gameObject.activeInHierarchy) return;

                if (result.Data != null && result.Data.ContainsKey(key))
                {
                    currentStage = JsonUtility.FromJson<StageInfo>(result.Data[key]);
                    Debug.Log($"PlayFabからステージ {targetId} を読み込みました");

                    // データが届いた後にUIをセットアップ
                    SetupGameUI();
                }
                else
                {
                    Debug.LogError("PlayFabにデータが存在しません: " + key);
                }
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // 読み込んだデータを画面に反映する処理
    void SetupGameUI()
    {
        if (currentStage == null) return;

        // --- A. 問題文エリアの動的生成 (B案) ---
        foreach (Transform child in questionContentParent)
        {
            Destroy(child.gameObject);
        }
        ShowQuestion();


        List<PieceData> shuffledPieces = new List<PieceData>(currentStage.handPieces);
        System.Random rng = new System.Random();
        shuffledPieces = shuffledPieces.OrderBy(p => rng.Next()).ToList();

        foreach (Transform child in handZone)
        {
            Destroy(child.gameObject);
        }

        foreach (var pData in shuffledPieces)
        {
            GameObject pObj = Instantiate(piecePrefab, handZone);
            GamePiece script = pObj.GetComponent<GamePiece>();
            if (script != null)
            {
                script.pieceId = pData.id;
                script.codeText.text = pData.code;
            }
        }
    }
}