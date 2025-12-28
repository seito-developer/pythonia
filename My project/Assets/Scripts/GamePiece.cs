using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class GamePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public int pieceId;        // JSONのIDと一致させる
    public int currentIndent = 0; // 現在のインデント数
    public TextMeshProUGUI codeText;

    private Transform originalParent; // 元の親（手札かボードか）
    private CanvasGroup canvasGroup;
    private float indentWidth = 40f; // 1インデントあたりのズレ幅

    private GameSceneManager manager;
    private Image frameImage;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // コンポーネント取得を追加
        frameImage = GetComponent<Image>();

        // シーン内のマネージャーを探しておく（FindFirstObjectByTypeはUnity2021.3以降の推奨）
        manager = Object.FindFirstObjectByType<GameSceneManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.SetSelectedPiece(this);
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (frameImage != null)
        {
            // 選択中は黄色、そうでなければ白（通常時）
            frameImage.color = isSelected ? Color.yellow : Color.white;
        }
    }

    // 1. ドラッグ開始
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("ドラッグ開始！"); // 👈 これを追加
        originalParent = transform.parent;

        // ドラッグ中は他のUIを突き抜けてマウス位置を最前面にするため、一時的に親をCanvas（最上位）に変える
        transform.SetParent(transform.root);

        // マウスの裏側に隠れないように、レイキャスト（当たり判定）を一時オフにする
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // 少し透けさせる
    }

    // 2. ドラッグ中（マウスについてくる）
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // 3. ドラッグ終了
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // ドロップした先に「Board」があるか判定
        GameObject overObj = eventData.pointerEnter;

        // もしボードの上、またはボード内の他のピースの上にドロップしたら
        if (overObj != null)
        {
            // 1. ボード（またはボード内のピース）の上にドロップした場合
            if (overObj.name == "Board" || overObj.transform.parent.name == "Board")
            {
                Transform boardTrans = (overObj.name == "Board") ? overObj.transform : overObj.transform.parent;
                transform.SetParent(boardTrans);

                // 並び順の計算
                int newIndex = 0;
                for (int i = 0; i < boardTrans.childCount; i++)
                {
                    if (transform.position.y > boardTrans.GetChild(i).position.y)
                    {
                        newIndex = i;
                        break;
                    }
                    newIndex = i;
                }
                transform.SetSiblingIndex(newIndex);
            }
            // 2. 追加：手札エリア（HandZone）の上にドロップした場合
            else if (IsUnderHandZone(overObj.transform))
            {
                // インデントをリセットして手札に戻す
                currentIndent = 0;
                UpdateVisual();

                // manager を通じて handZone を取得するか、保存しておいた originalParent（初回はHandZoneのはず）を使う
                // ここでは確実に現在の handZone に戻すため、manager から参照します
                transform.SetParent(manager.handZone);
            }
            else
            {
                // ボードでも手札でもない場所に捨てたら、直前の親に戻す
                transform.SetParent(originalParent);
            }
        }
        else
        {
            transform.SetParent(originalParent);
        }
    }

    // 親を遡って HandZone かどうかを判定するヘルパー関数
    private bool IsUnderHandZone(Transform target)
    {
        while (target != null)
        {
            if (target.name == "HandZone") return true;
            target = target.parent;
        }
        return false;
    }

    // インデントを増やす
    public void IncreaseIndent()
    {
        if (currentIndent < 5) // 最大インデント制限（任意）
        {
            currentIndent++;
            UpdateVisual();
        }
    }

    // インデントを減らす
    public void DecreaseIndent()
    {
        if (currentIndent > 0)
        {
            currentIndent--;
            UpdateVisual();
        }
    }

    // 見た目を更新（テキストを右にズラす）
    void UpdateVisual()
    {
        // テキストのRectTransformを操作して右にスライドさせる
        Vector3 pos = codeText.rectTransform.localPosition;
        pos.x = currentIndent * indentWidth;
        codeText.rectTransform.localPosition = pos;
    }
}