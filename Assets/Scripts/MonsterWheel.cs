using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterWheel : MonoBehaviour
{
    public enum WheelState
    {
        Spin,
        Stopping,
        Stoped
    }
    [Header("Preferences")]
    [SerializeField] private RectTransform cellHolder;
    [SerializeField] private RectTransform slotGroup;
    [SerializeField] private List<Image> slotImages;
    [Header("Prefabs")]
    [SerializeField] private GameObject cellItemPrefab;
    [Header("Params")]
    [Tooltip("Images of items are repeated in order")]
    [SerializeField] private List<Sprite> sprites;
    [Tooltip("Screen resolution")]
    [SerializeField] private Vector2 resolution;
    [Tooltip("Distance between items")]
    [SerializeField] private Vector2 cellSpacing;
    [Tooltip("Total row of item")]
    [SerializeField] private int rows;
    [Tooltip("Total col of item")]
    [SerializeField] private int cols;
    [Tooltip("Total number of round added before stopping")]
    [SerializeField] private int spinRoundMultiplier;
    [Tooltip("Min speed of wheel")]
    [SerializeField] private float wheelMinSpeed;
    [Tooltip("Item's effect duration")]
    [SerializeField] private float wheelMaxSpeed;
    [SerializeField] private float cellEffectTime;

    //List of items on scene
    private List<Item> items = new();
    //Index of current item
    private int spinIndex;
    //Index of target item
    private int slotIndex;
    private float step = float.MaxValue;
    private RectTransform selectSlotImage;
    //Wheel status
    private WheelState isSpin = WheelState.Stoped;

    void Start()
    {
        CalculationScreen();
        items[slotIndex].Select();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isSpin == WheelState.Stoped)
        {
            Spin();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isSpin == WheelState.Spin)
        {
            StopSpin();
        }
    }

    /// <summary>
    /// Calculate the size of the entire block and generate items according to the calculated position
    /// </summary>
    private void CalculationScreen()
    {
        float x, y;
        int spriteIndex = 0;
        Vector2 prefabSize = cellItemPrefab.GetComponent<RectTransform>().sizeDelta;
        Vector2 prefabSizeSpacing = prefabSize + cellSpacing;

        //Calculate spawn starting location
        float offsetX = - (cols * prefabSizeSpacing.x - prefabSizeSpacing.x) / 2f;
        float offsetY = - (rows * prefabSizeSpacing.y - prefabSizeSpacing.y) / 2f;

        //Generates the items of the top row
        for (int i = 0; i < cols; i++)
        {
            x = offsetX + i * (cellSpacing.x + prefabSize.x);
            y = offsetY + (rows - 1) * (cellSpacing.y + prefabSize.y);
            SpawnCell(sprites[spriteIndex],new Vector2(x,y));
            spriteIndex = ++spriteIndex % sprites.Count;
        }
        //Generates items of the right column
        for (int i = rows - 2; i > 0; i--)
        {
            x = offsetX  + (cols - 1) * (cellSpacing.x + prefabSize.x);
            y = offsetY  + i * (cellSpacing.y + prefabSize.y);
            SpawnCell(sprites[spriteIndex],new Vector2( x,y ));
            spriteIndex = ++spriteIndex % sprites.Count;
        }
        //Generates the items of the bottom row
        for (int i = cols - 1; i >= 0; i--)
        {
            x = offsetX + i * (cellSpacing.x + prefabSize.x);
            y = offsetY + 0f * (cellSpacing.y + prefabSize.y);
            SpawnCell(sprites[spriteIndex], new Vector2(x, y));
            spriteIndex = ++spriteIndex % sprites.Count;
        }
        //Generates items of the left column
        for (int i = 1; i < rows - 1; i++)
        {
            x = offsetX + 0f * (cellSpacing.x + prefabSize.x);
            y = offsetY + i * (cellSpacing.y + prefabSize.y);
            SpawnCell(sprites[spriteIndex], new Vector2( x,y ));
            spriteIndex = ++spriteIndex % sprites.Count;
        }
    }
    /// <summary>
    /// Generate item and set position on holder
    /// </summary>
    /// <param name="sprite">Image of the item</param>
    /// <param name="position">Position of the item</param>
    private void SpawnCell(Sprite sprite , Vector2 position)
    {
        RectTransform itemRect = Instantiate(cellItemPrefab, Vector2.zero, Quaternion.identity).GetComponent<RectTransform>();
        itemRect.SetParent(cellHolder);
        itemRect.anchoredPosition = position;

        Item item = itemRect.GetComponent<Item>();
        item.SetIcon(sprite, cellEffectTime);

        items.Add(item);
    }
    /// <summary>
    /// Random item index and calculates the number of steps to reach the goal
    /// </summary>
    private void Spin()
    {
        isSpin = WheelState.Spin;
        StartCoroutine(WaitStopSpin());
    }
    /// <summary>
    /// Random item index and calculates the number of steps to reach the goal
    /// </summary>
    private void StopSpin()
    {
        isSpin = WheelState.Stopping;
        slotIndex = Random.Range(0, items.Count);
        //number of steps from current index to random index
        int temp = spinIndex > slotIndex ? slotIndex - spinIndex : items.Count - spinIndex + slotIndex;
        //The number of wheel revolutions is added
        step += temp + items.Count * spinRoundMultiplier;
        Debug.LogWarning("Index random : "+ slotIndex);
    }
    /// <summary>
    /// Wait for the rotation to stop at the randomized item
    /// </summary>
    private IEnumerator WaitStopSpin()
    {
        float timer = 0;
        float speed = wheelMaxSpeed;

        while (timer < step)
        {
            if (isSpin == WheelState.Spin)
            {
                step = timer + 18f;
            }
            //The speed will gradually increase from min to max in the first round
            //The speed will gradually decrease from max to min in the last round
            //The remaining rounds always reach maximum speed
            if (timer <= items.Count && isSpin == WheelState.Spin)
            {
                speed = Mathf.Lerp(wheelMinSpeed, wheelMaxSpeed, timer / items.Count);
            }
            else if (step - timer < items.Count)
            {
                speed = Mathf.Lerp(wheelMinSpeed, wheelMaxSpeed, (step - timer) / items.Count);
            }
            else
            {
                speed = wheelMaxSpeed;
            }

            //Change the selected item when moving to a new step
            if (Mathf.CeilToInt(timer + Time.deltaTime * speed) > Mathf.CeilToInt(timer))
            {
                items[spinIndex].StartEffect();
                spinIndex++;
                if (spinIndex >= items.Count) spinIndex = 0;
            }
            //Move slot according to current step progression
            MoveSlot(Time.deltaTime * speed ,  Mathf.FloorToInt(step - timer) == 1);
            timer += Time.deltaTime * speed;
            yield return null;
        }
        //Select item when wheel stops
        items[slotIndex].Select();
        Debug.LogWarning("Wheel stop at index : " + spinIndex);
        while (selectSlotImage.anchoredPosition.y > -50f)
        {
            yield return null;
            float percent = Time.deltaTime * wheelMinSpeed;
            MoveSlot(percent, false);
        }
        while (true)
        {
            yield return null;
            float percent = Time.deltaTime * wheelMinSpeed * (selectSlotImage.anchoredPosition.y > 0 ? 1 : -1 );
            MoveSlot(percent, false);
            if(Mathf.Abs(selectSlotImage.anchoredPosition.y) < 1f)
            {
                break;
            }
        }
        isSpin = WheelState.Stoped;
    }

    /// <summary>
    /// The function moves slots
    /// </summary>
    /// <param name="percent">Number of steps between 2 frames</param>
    /// <param name="isLast">Check is the last step</param>
    private void MoveSlot(float percent , bool isLast)
    {
        foreach (var image in slotImages)
        {
            //Move based on number of steps
            float moveDistance = image.rectTransform.sizeDelta.y * percent;
            image.rectTransform.anchoredPosition = new Vector2(0f, image.rectTransform.anchoredPosition.y - moveDistance);
            ////Move the slot up when leaving the SlotFrame displayed on the scene
            if (image.rectTransform.anchoredPosition.y < -(slotGroup.sizeDelta.y + image.rectTransform.sizeDelta.y) / 2f)
            {
                float posY = image.rectTransform.anchoredPosition.y + image.rectTransform.sizeDelta.y * slotImages.Count;
                image.rectTransform.anchoredPosition = new Vector2(0f, posY);
                if (isLast)
                {
                    image.sprite = sprites[slotIndex % sprites.Count];
                    selectSlotImage = image.rectTransform;
                }
                else
                {
                    image.sprite = sprites[Random.Range(0, sprites.Count)];
                }
            }
        }
    }
}
