using UnityEngine;

/// <summary>
/// This class is used to control the Virtual Avatar, by enabling / disabling the avatar and playing animations.
/// </summary>
public class VirtualAvatar : MonoBehaviour
{
    #region Singleton
    public static VirtualAvatar instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    /// <summary>
    /// Reference to the animator of the Virtual Avatar to control it’s animations.
    /// </summary>
    [SerializeField] private Animator animator;

    /// <summary>
    /// Enables the virtualAvatarGO GameObject.
    /// </summary>
    public void EnableAvatar()
    {
        // Play Fade in animation?
        //animator.SetTrigger("FadeIn");

        animator.gameObject.SetActive(true);
    }

    /// <summary>
    /// Diables the virtualAvatarGO GameObject.
    /// </summary>
    public void DisableAvatar()
    {
        // Play Fade out animation?
        //animator.SetTrigger("FadeOut");

        animator.gameObject.SetActive(false);
    }

    /// <summary>
    /// Plays the talking animation of the Virtual Avatar’s animator.
    /// </summary>
    public void PlayTalkingAnimation()
    {
        animator.SetTrigger("Talking");
    }
}
