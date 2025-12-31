using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // どこからでもアクセスできるようにするための変数
    public static AudioManager instance;

    public AudioSource bgmSource; // BGM用スピーカー
    public AudioSource seMenuSource;  // SE用スピーカー
    public AudioSource seTitleSource;  // タイトル用スピーカー

    void Awake()
    {
        // シングルトンの設定：すでに存在してたら自分を消す、なければ自分を保持
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 👈 これが重要！シーンが変わっても消えなくなる
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // BGMを再生する関数
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // SEを再生する関数
    public void PlayMenu(AudioClip clip)
    {
        seMenuSource.PlayOneShot(clip);
    }
    public void PlayTitle(AudioClip clip)
    {
        seTitleSource.PlayOneShot(clip);
    }


    // game-clear.mp3 
    // game-over.mp3 
    // indent.mp3 
    // menu.mp3 
    // miss.mp3 
    // piece.mp3 
    // title.mp3 
    // window-open.mp3
}