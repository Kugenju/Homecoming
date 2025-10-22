using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;


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

        // 实例化汤碗
        GameObject baseBowl = Instantiate(baseBowlPrefab, bowlSpawnPoint.position, bowlSpawnPoint.rotation, transform);
        baseBowl.transform.localScale = targetScale;
        baseBowl.name = "BaseSoupBowl";
        SetupBowlForDragging(baseBowl, false);
        currentSoupBowl = baseBowl;

        // 启动淡入效果协程
        StartCoroutine(FadeInAllChildren(currentSoupBowl, 2f));
    }

    /// <summary>
    /// 递归获取所有子物体的渲染组件，并同步执行淡入效果
    /// </summary>
    private IEnumerator FadeInAllChildren(GameObject target, float duration)
    {
        // 收集所有需要淡入的组件（包括自身和所有子物体）
        List<Graphic> uiGraphics = new List<Graphic>(target.GetComponentsInChildren<Graphic>(true)); // true表示包含 inactive 组件
        List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>(target.GetComponentsInChildren<SpriteRenderer>(true));
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>(target.GetComponentsInChildren<MeshRenderer>(true));
        List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>(target.GetComponentsInChildren<SkinnedMeshRenderer>(true));

        // 保存所有组件的原始状态（用于淡入结束后恢复）
        Dictionary<Graphic, Color> originalGraphicColors = new Dictionary<Graphic, Color>();
        Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
        Dictionary<MeshRenderer, (Color color, Material originalMat)> originalMeshData = new Dictionary<MeshRenderer, (Color, Material)>();
        Dictionary<SkinnedMeshRenderer, (Color color, Material originalMat)> originalSkinnedMeshData = new Dictionary<SkinnedMeshRenderer, (Color, Material)>();

        // 初始化UI组件（设置初始透明度为0）
        foreach (var graphic in uiGraphics)
        {
            originalGraphicColors[graphic] = graphic.color;
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
        }

        // 初始化Sprite组件
        foreach (var sprite in spriteRenderers)
        {
            originalSpriteColors[sprite] = sprite.color;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
        }

        // 初始化MeshRenderer（创建材质实例避免影响其他物体）
        foreach (var mesh in meshRenderers)
        {
            if (mesh.material == null) continue;
            Material instance = new Material(mesh.material);
            originalMeshData[mesh] = (instance.color, mesh.material); // 保存原始颜色和材质
            mesh.material = instance; // 替换为实例材质
            instance.color = new Color(instance.color.r, instance.color.g, instance.color.b, 0); // 初始透明
        }

        // 初始化SkinnedMeshRenderer（骨骼动画网格）
        foreach (var skinnedMesh in skinnedMeshRenderers)
        {
            if (skinnedMesh.material == null) continue;
            Material instance = new Material(skinnedMesh.material);
            originalSkinnedMeshData[skinnedMesh] = (instance.color, skinnedMesh.material);
            skinnedMesh.material = instance;
            instance.color = new Color(instance.color.r, instance.color.g, instance.color.b, 0);
        }

        // 检查是否有可淡入的组件
        if (uiGraphics.Count == 0 && spriteRenderers.Count == 0 && meshRenderers.Count == 0 && skinnedMeshRenderers.Count == 0)
        {
            Debug.LogWarning($"物体 {target.name} 及其子物体中未找到任何可淡入的渲染组件", target);
            yield break;
        }

        // 同步淡入过程
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsedTime / duration); // 统一的插值进度

            // 更新UI组件透明度
            foreach (var graphic in uiGraphics)
            {
                if (graphic == null) continue; // 防止组件被销毁
                Color original = originalGraphicColors[graphic];
                graphic.color = new Color(original.r, original.g, original.b, alpha);
            }

            // 更新Sprite透明度
            foreach (var sprite in spriteRenderers)
            {
                if (sprite == null) continue;
                Color original = originalSpriteColors[sprite];
                sprite.color = new Color(original.r, original.g, original.b, alpha);
            }

            // 更新MeshRenderer透明度
            foreach (var mesh in meshRenderers)
            {
                if (mesh == null || mesh.material == null) continue;
                Color original = originalMeshData[mesh].color;
                mesh.material.color = new Color(original.r, original.g, original.b, alpha);
            }

            // 更新SkinnedMeshRenderer透明度
            foreach (var skinnedMesh in skinnedMeshRenderers)
            {
                if (skinnedMesh == null || skinnedMesh.material == null) continue;
                Color original = originalSkinnedMeshData[skinnedMesh].color;
                skinnedMesh.material.color = new Color(original.r, original.g, original.b, alpha);
            }

            yield return null; // 等待下一帧
        }

        // 淡入结束：恢复原始状态（确保完全不透明）
        foreach (var graphic in uiGraphics)
        {
            if (graphic != null) graphic.color = originalGraphicColors[graphic];
        }
        foreach (var sprite in spriteRenderers)
        {
            if (sprite != null) sprite.color = originalSpriteColors[sprite];
        }
        foreach (var mesh in meshRenderers)
        {
            if (mesh != null && originalMeshData.ContainsKey(mesh))
            {
                mesh.material.color = originalMeshData[mesh].color;
                // 可选：如果不需要保留实例材质，可以恢复原始材质（避免内存占用）
                // mesh.material = originalMeshData[mesh].originalMat;
            }
        }
        foreach (var skinnedMesh in skinnedMeshRenderers)
        {
            if (skinnedMesh != null && originalSkinnedMeshData.ContainsKey(skinnedMesh))
            {
                skinnedMesh.material.color = originalSkinnedMeshData[skinnedMesh].color;
                // 可选：恢复原始材质
                // skinnedMesh.material = originalSkinnedMeshData[skinnedMesh].originalMat;
            }
        }
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
            // clickableItem.OnClicked.AddListener(OnBowlClicked);
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