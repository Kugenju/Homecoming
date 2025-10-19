using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


public class SoupBowlZone : DroppableZone
{
    [Header("预制体配置")]
    public List<SoupState> soupStates = new List<SoupState>();

    [Header("汤碗预制体")]
    public GameObject baseBowlPrefab;
    public Transform bowlSpawnPoint;

    [Header("运行时状态")]
    [SerializeField] private List<IngredientSO> currentIngredients = new List<IngredientSO>();
    private GameObject currentSoupBowl;

    [Header("拖拽交付配置")]
    public bool enableBowlDelivery = true;
    public float bowlReturnSpeed = 5f;

    [Header("显示配置")]
    public Vector3 targetScale = new Vector3(1f, 1f, 1f);

    [Header("音频反馈")]
    public AudioClip addToppingSound;
    public AudioClip bowlPickupSound;
    public AudioClip bowlReturnSound;
    private AudioSource audioSource;

    // 拖拽状态
    private bool isBowlBeingDragged = false;
    private Vector3 originalBowlPosition;
    private Coroutine returnCoroutine;

    protected override void Awake()
    {
        base.Awake();

        if (bowlSpawnPoint == null)
        {
            bowlSpawnPoint = transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        CreateBaseBowl();
    }

    private void Update() {
        if (currentSoupBowl == null) CreateBaseBowl();
    }

    private void CreateBaseBowl()
    {
        if (baseBowlPrefab == null)
        {
            Debug.LogError("基础汤碗预制体未指定！");
            return;
        }

        GameObject baseBowl = Instantiate(baseBowlPrefab, bowlSpawnPoint.position, bowlSpawnPoint.rotation, transform);
        baseBowl.transform.localScale = targetScale;
        baseBowl.name = "BaseSoupBowl";
        SetupBowlForDragging(baseBowl, false);
        currentSoupBowl = baseBowl;
    }

    private void SetupBowlForDragging(GameObject bowlObject, bool makeDraggable = true)
    {
        if (bowlObject == null || !enableBowlDelivery) return;

        EnsureProperCollider(bowlObject);

        ClickableItem clickableItem = bowlObject.GetComponent<ClickableItem>();
        if (clickableItem == null)
        {
            clickableItem = bowlObject.AddComponent<ClickableItem>();
        }
        
        if(makeDraggable == true)
        {
            clickableItem.isDraggable = makeDraggable;
            clickableItem.isUsable = makeDraggable;
            clickableItem.showHighlightOnHover = makeDraggable;
        }
        else
        {
            clickableItem.isDraggable = false;
        }
        Debug.Log($"clickableItem.isDraggable: {clickableItem.isDraggable}");

        if (makeDraggable)
        {
            clickableItem.OnDragStart.AddListener(OnBowlDragStart);
            clickableItem.OnDragEnd.AddListener(OnBowlDragEnd);
            clickableItem.OnClicked.AddListener(OnBowlClicked);
        }
    }

    private void EnsureProperCollider(GameObject bowlObject)
    {
        Collider2D[] oldColliders = bowlObject.GetComponents<Collider2D>();
        foreach (var collider in oldColliders)
        {
            DestroyImmediate(collider);
        }

        if (baseBowlPrefab != null)
        {
            Collider2D prefabCollider = baseBowlPrefab.GetComponent<Collider2D>();
            if (prefabCollider != null)
            {
                if (prefabCollider is BoxCollider2D boxCollider)
                {
                    BoxCollider2D newCollider = bowlObject.AddComponent<BoxCollider2D>();
                    newCollider.offset = boxCollider.offset;
                    newCollider.size = boxCollider.size;
                    newCollider.isTrigger = true;
                }
                else if (prefabCollider is PolygonCollider2D polyCollider)
                {
                    PolygonCollider2D newCollider = bowlObject.AddComponent<PolygonCollider2D>();
                    newCollider.points = polyCollider.points;
                    newCollider.isTrigger = true;
                }
            }
            else
            {
                BoxCollider2D defaultCollider = bowlObject.AddComponent<BoxCollider2D>();
                defaultCollider.isTrigger = true;
            }
        }
    }

    private void OnBowlDragStart()
    {
        if (!enableBowlDelivery || !IsReadyToServe() || isBowlBeingDragged) return;

        isBowlBeingDragged = true;
        originalBowlPosition = currentSoupBowl.transform.position;
        //CreateDragProxy();

        if (bowlPickupSound != null)
            audioSource.PlayOneShot(bowlPickupSound);
        
        Debug.Log("开始拖拽汤碗！");
    }

    private void CreateDragProxy()
    {
        ProxyDragTag proxyTag = currentSoupBowl.GetComponent<ProxyDragTag>();
        if (proxyTag == null)
        {
            proxyTag = currentSoupBowl.AddComponent<ProxyDragTag>();
        }

        GameObject proxyPrefab = CreateProxyPrefab();
        proxyTag.proxyPrefab = proxyPrefab;
        proxyTag.hideOriginalDuringDrag = true;
        proxyTag.successDropBehavior = ProxyDragTag.DropBehavior.DestroyProxy;
        proxyTag.cancelDropBehavior = ProxyDragTag.DropBehavior.ReturnToOriginal;

        currentSoupBowl.SetActive(false);
    }

    private GameObject CreateProxyPrefab()
    {
        GameObject proxy = new GameObject("BowlProxy");
        SpriteRenderer renderer = proxy.AddComponent<SpriteRenderer>();
        renderer.sprite = currentSoupBowl.GetComponent<SpriteRenderer>().sprite;
        renderer.color = new Color(1, 1, 1, 0.8f);

        Collider2D originalCollider = currentSoupBowl.GetComponent<Collider2D>();
        if (originalCollider is BoxCollider2D boxCollider)
        {
            BoxCollider2D newCollider = proxy.AddComponent<BoxCollider2D>();
            newCollider.size = boxCollider.size;
            newCollider.isTrigger = true;
        }

        return proxy;
    }

    private void OnBowlDragEnd()
    {
        if (!isBowlBeingDragged) return;

        isBowlBeingDragged = false;
        bool deliverySuccess = CheckDeliveryToCustomer();

        if (deliverySuccess)
        {
            HandleSuccessfulDelivery();
        }
        else
        {
            HandleFailedDelivery();
        }
    }

    private IEnumerator ReturnBowlToOriginalPosition()
    {
        GameObject bowlToReturn = currentSoupBowl;
        if (bowlToReturn == null) yield break;

        Vector3 startPosition = bowlToReturn.transform.position;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * bowlReturnSpeed;
            bowlToReturn.transform.position = Vector3.Lerp(startPosition, originalBowlPosition, journey);
            yield return null;
        }

        bowlToReturn.transform.position = originalBowlPosition;
        returnCoroutine = null;
    }

    private void SwitchToState(int index)
    {
        if (index < 0 || index >= soupStates.Count) return;

        var state = soupStates[index];

        if (currentSoupBowl != null)
        {
            Destroy(currentSoupBowl);
        }

        GameObject newBowl = Instantiate(state.visualPrefab, bowlSpawnPoint.position, bowlSpawnPoint.rotation, transform);
        newBowl.name = "StateSoupBowl_" + state.visualPrefab.name;
        newBowl.transform.localScale = targetScale;
        SetupBowlForDragging(newBowl, true);
        currentSoupBowl = newBowl;
    }

    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);

        var ingredientComp = item.GetComponent<IngredientComponent>();
        if (ingredientComp == null) return;

        IngredientSO ing = ingredientComp.ingredientData;

        if (ing.ingredientName == "粉丝汤底")
        {
            ResetBowl();
            AddIngredient(ing);
            return;
        }

        AddIngredient(ing);
        UpdateVisualState();

        if (addToppingSound != null)
            audioSource.PlayOneShot(addToppingSound);
    }

    private void AddIngredient(IngredientSO ingredient)
    {
        if (!currentIngredients.Contains(ingredient))
            currentIngredients.Add(ingredient);
    }

    private void UpdateVisualState()
    {
        currentIngredients.Sort((a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < soupStates.Count; i++)
        {
            var state = soupStates[i];
            if (IsMatch(currentIngredients, state.requiredIngredients))
            {
                SwitchToState(i);
                return;
            }
        }
    }

    private bool IsMatch(List<IngredientSO> current, List<IngredientSO> required)
    {
        if (current.Count != required.Count) return false;
        return required.All(current.Contains);
    }

    public void ResetBowl()
    {
        currentIngredients.Clear();

        if (currentSoupBowl != null)
        {
            Destroy(currentSoupBowl);
        }

        CreateBaseBowl();
    }

    public List<IngredientSO> GetCurrentIngredients() => new List<IngredientSO>(currentIngredients);
    public bool IsReadyToServe() => currentIngredients.Count > 1;

    private bool CheckDeliveryToCustomer()
    {
        if (DragAndDropHandler.Instance != null)
        {
            DroppableZone dropZone = DragAndDropHandler.Instance.FindValidDropZone();
            return dropZone != null && dropZone is Customer;
        }
        return false;
    }

    private void HandleSuccessfulDelivery()
    {
        Debug.Log("汤碗成功交付给客户！");
        ResetBowl();
    }

    private void HandleFailedDelivery()
    {
        if (currentSoupBowl != null)
        {
            currentSoupBowl.SetActive(true);
            returnCoroutine = StartCoroutine(ReturnBowlToOriginalPosition());
        }

        if (bowlReturnSound != null)
            audioSource.PlayOneShot(bowlReturnSound);
    }
}