using UnityEngine;
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
    public TextMeshProUGUI lifeText;

    [Header("Result UI")]
    public GameObject resultPanel;
    public CanvasGroup resultCanvasGroup;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultMessageText;
    public TextMeshProUGUI actionButtonText;

    void Start()
    {
        currentLife = maxLife;
        UpdateLifeUI();
        LoadStageData();
        SetupGameUI();
        ShowQuestion();
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

        // --- パターンA：下からスライドしてくる ---
        resultPanel.transform.localPosition = new Vector3(0, -1000, 0); // 初期位置
        resultPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack);

        // --- パターンB：透明度をフェードインさせる ---
        resultCanvasGroup.alpha = 0;
        resultCanvasGroup.DOFade(1f, 0.5f);

        if (isWin)
        {
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

    // パネルのボタンに紐付ける汎用メソッド
    public void OnClickResultButton()
    {
        SceneManager.LoadScene("HigherStages");
    }

    void UpdateLifeUI()
    {
        if (lifeText != null)
        {
            // 例: 「ライフ: 3」や「★★★」と表示
            lifeText.text = "ライフ: " + new string('★', currentLife);
        }
    }
    // CheckAnswerの中で不正解だった時に呼ぶ
    void Miss()
    {
        currentLife--;
        UpdateLifeUI();
        Debug.Log($"ミス！残りライフ: {currentLife}");

        AudioManager.instance.PlayMiss(AudioManager.instance.seMissSource.clip);

        if (currentLife <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("ゲームオーバー...");
        ShowResultPanel(false); // 結果パネルを「失敗」モードで表示
    }

    // パネルを表示するメソッド
    public void ShowQuestion()
    {
        if (currentStage != null && questionPanel != null)
        {
            questionText.text = currentStage.question;
            questionPanel.SetActive(true);
            AudioManager.instance.PlayWindow(AudioManager.instance.seWindowSource.clip);
        }
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
            Miss();
        }
    }

    // 個別JSONファイルを読み込む処理
    void LoadStageData()
    {
        int targetId = GameData.SelectedStageId;

        // Resources/Stages/Stage_X を読み込む
        string filePath = $"Stages/Stage_{targetId}";
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            // 個別ファイルは StageInfo 構造そのものなので直接デコード
            currentStage = JsonUtility.FromJson<StageInfo>(jsonFile.text);
            Debug.Log($"ステージ {targetId} のデータを読み込みました。");
        }
        else
        {
            Debug.LogError($"エラー: {filePath} が見つかりません。Resources/Stages フォルダを確認してください。");
        }
    }

    // 読み込んだデータを画面に反映する処理
    void SetupGameUI()
    {
        if (currentStage == null)
        {
            Debug.LogError("currentStageがNullです！JSON読み込みに失敗しています。");
            return;
        }

        // 問題文とタイトルのセット
        titleText.text = currentStage.stageName;
        questionText.text = currentStage.question;

        // 1. 手札のデータを一時的なリストにコピーする
        List<PieceData> shuffledPieces = new List<PieceData>(currentStage.handPieces);

        // 2. リストをランダムにシャッフルする（System.LinqとSystem.Randomを使用）
        System.Random rng = new System.Random();
        shuffledPieces = shuffledPieces.OrderBy(p => rng.Next()).ToList();

        // 3. 既存のピースを削除（念のため）
        foreach (Transform child in handZone)
        {
            Destroy(child.gameObject);
        }

        // 4. シャッフルされた順番でピースを生成し、HandZoneを親にする
        foreach (var pData in shuffledPieces)
        {
            // 第二引数に handZone を指定することで、生成時に直接 HandZone の中に入ります
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