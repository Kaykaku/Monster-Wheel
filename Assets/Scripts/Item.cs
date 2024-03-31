using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [Tooltip("Item color when spin select")]
    [SerializeField] private Color blurColor;
    [Tooltip("Item color when disable")]
    [SerializeField] private Color originColor;
    [Tooltip("Item color when selected")]
    [SerializeField] private Color selectColor;

    private Coroutine effectCoroutine;
    private float effectTime;

    /// <summary>
    /// Set the item's image and effect duration
    /// </summary>
    /// <param name="sprite">Image of the item</param>
    /// <param name="time">Effect duration of the item</param>
    public void SetIcon(Sprite sprite , float time)
    {
        iconImage.sprite = sprite;
        iconImage.color = blurColor;
        effectTime = time;
    }
    /// <summary>
    /// Highlight the selected item
    /// </summary>
    public void Select()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        iconImage.color = selectColor;
    }
    /// <summary>
    /// Starts the item's blur effect
    /// </summary>
    public void StartEffect()
    {
        if( effectCoroutine != null ) 
        {
            StopCoroutine(effectCoroutine);
        }
        effectCoroutine = StartCoroutine(WaitEffect());
    }
    /// <summary>
    /// Transition from originColor => blurColor after each frame
    /// </summary>
    private IEnumerator WaitEffect()
    {
        iconImage.color = originColor;
        float timer = effectTime;

        while (timer > 0) {
            yield return null;
            timer -= Time.deltaTime;

            float r = Mathf.Lerp(blurColor.r, originColor.r, timer/effectTime);
            float b = Mathf.Lerp(blurColor.b, originColor.b, timer/effectTime);
            float g = Mathf.Lerp(blurColor.g, originColor.g, timer/effectTime);
            iconImage.color = new Color(r, b, g, 1f);
        }
    }
}
