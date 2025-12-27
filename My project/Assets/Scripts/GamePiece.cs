using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class GamePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int pieceId;        // JSONのIDと一致させる
    public int currentIndent = 0; // 現在のインデント数
    public TextMeshProUGUI codeText;

    private Transform originalParent; // 元の親（手札かボードか）
    private CanvasGroup canvasGroup;
    private float indentWidth = 40f; // 1インデントあたりのズレ幅

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
        if (overObj != null && (overObj.name == "Board" || overObj.transform.parent.name == "Board"))
        {
            Transform boardTrans = (overObj.name == "Board") ? overObj.transform : overObj.transform.parent;

            // ボードに子要素として入れる
            transform.SetParent(boardTrans);

            // 【重要】ドロップした位置に基づいて、適切な順序（SiblingIndex）に差し込む
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
        else
        {
            // ボード以外で離したら元の場所（手札など）に戻す
            transform.SetParent(originalParent);
        }
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