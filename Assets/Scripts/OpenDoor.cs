using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    private Animator anim;
    public float autoCloseDely = 10f;

    // 👇 新增：开门音效（你拖音频进来）
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    void Start()
    {
        anim = GetComponent<Animator>();

        // 👇 自动添加音频播放器（不用手动加）
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false; // 绝对不循环
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            anim.SetTrigger("open");
            PlayDoorOpenSound(); // 👈 播放开门音效
            StartCoroutine(AutoCloseDoor());
        }
    }

    // 👇 新增：播放开门音效（只播一次）
    void PlayDoorOpenSound()
    {
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
            Debug.Log("🔊 播放开门音效");
        }
    }

    IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(autoCloseDely);
        anim.SetTrigger("close");
    }
}