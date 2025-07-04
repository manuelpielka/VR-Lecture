using UnityEngine;

/// <summary>
/// This class is used to control the Virtual Avatar, by enabling / disabling the avatar and playing animations.
/// </summary>
public class VirtualAvatar : MonoBehaviour
{
    /// <summary>
    /// Reference to the animator of the Virtual Avatar to control it’s animations.
    /// </summary>
    [SerializeField] private Animator animator;

    /// <summary>
    /// Reference to the GameObject of the Virtual Avatar to enable and disable it.
    /// </summary>
    private GameObject virtualAvatarGO;

    /// <summary>
    /// The start method called by unity.
    /// </summary>
    private void Start()
    {
        virtualAvatarGO = animator.gameObject;
    }

    /// <summary>
    /// Enables the virtualAvatarGO GameObject.
    /// </summary>
    public void EnableAvatar()
    {
        // Play Fade in animation?
        //animator.SetTrigger("FadeIn");

        virtualAvatarGO.SetActive(true);
    }

    /// <summary>
    /// Diables the virtualAvatarGO GameObject.
    /// </summary>
    public void DisableAvatar()
    {
        // Play Fade out animation?
        //animator.SetTrigger("FadeOut");

        virtualAvatarGO.SetActive(false);
    }

    /// <summary>
    /// Plays the talking animation of the Virtual Avatar’s animator.
    /// </summary>
    public void PlayTalkingAnimation()
    {
        animator.SetTrigger("Talking");
    }
}
