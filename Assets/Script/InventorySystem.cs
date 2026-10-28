using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[System.Serializable]
public class MaterialDef
{
    public string name = "재료";
    public Color color = Color.white;
    [Tooltip("비워두면 색만 사용")] public Sprite icon;
    [Tooltip("맵에 뜨는 아이템 프리팹 (Collider를 Is Trigger로)")] public GameObject worldPrefab;
    [Tooltip("스폰 비율 (왁스 1 : 점토 2 이면 1과 2)"), Min(0)] public int spawnWeight = 1;
}

[System.Serializable]
public class ProductDef
{
    public string name = "왁뿌볼";
    public Color color = Color.green;
    [Tooltip("비워두면 색만 사용")] public Sprite icon;

    [Tooltip("발사할 총알 프리팹 (Bullet 스크립트가 붙은 것)")] public GameObject bulletPrefab;
    [Tooltip("이 왁뿌볼의 데미지")] public int damage = 1;
}

[System.Serializable]
public class Ingredient
{
    [Tooltip("Materials 리스트의 번호 (0부터)")] public int materialIndex;
    [Min(1)] public int count = 1;
}

[System.Serializable]
public class Recipe
{
    public string label = "레시피";
    public List<Ingredient> ingredients = new List<Ingredient>();
    [Tooltip("Products 리스트의 번호 (0부터)")] public int productIndex;
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("세로칸 (재료 수집용)")]
    public List<Image> verticalSlots;

    [Header("가로칸 (완성된 왁뿌볼용)")]
    public List<Image> horizontalSlots;

    [Header("빈 칸 색")]
    public Color emptyColor = Color.white;

    [Header("발사 설정")]
    [Tooltip("총알이 나가는 위치와 방향 (비우면 Player 태그 오브젝트 사용)")]
    public Transform firePoint;
    [Tooltip("발사 위치를 앞쪽으로 얼마나 띄울지")]
    public float muzzleOffset = 0.5f;

    [Header("재료 목록 (+ 버튼으로 점토 종류 추가)")]
    public List<MaterialDef> materials = new List<MaterialDef>
    {
        new MaterialDef { name = "왁스", color = Color.yellow, spawnWeight = 1 },
        new MaterialDef { name = "점토", color = new Color(0.6f, 0.4f, 0.2f), spawnWeight = 2 },
    };

    [Header("합성 결과 목록 (3개)")]
    public List<ProductDef> products = new List<ProductDef>
    {
        new ProductDef { name = "왁뿌볼 1", color = Color.green },
        new ProductDef { name = "왁뿌볼 2", color = Color.cyan },
        new ProductDef { name = "왁뿌볼 3", color = Color.magenta },
    };

    [Header("레시피 (재료가 모이면 자동 합성)")]
    public List<Recipe> recipes = new List<Recipe>
    {
        new Recipe
        {
            label = "왁스1 + 점토2",
            ingredients = new List<Ingredient>
            {
                new Ingredient { materialIndex = 0, count = 1 },
                new Ingredient { materialIndex = 1, count = 2 },
            },
            productIndex = 0
        }
    };

    // -1 = 빈 칸, 그 외 = materials / products 리스트의 번호
    private int[] verticalItems;
    private int[] horizontalItems;
    private Sprite[] verticalEmptySprites;
    private Sprite[] horizontalEmptySprites;

    void Awake() { Instance = this; }

    void Start()
    {
        verticalItems = NewEmptyArray(verticalSlots.Count);
        horizontalItems = NewEmptyArray(horizontalSlots.Count);

        // 슬롯 원래 스프라이트를 저장해서 빈 칸일 때 복원
        verticalEmptySprites = new Sprite[verticalSlots.Count];
        for (int i = 0; i < verticalSlots.Count; i++) verticalEmptySprites[i] = verticalSlots[i].sprite;
        horizontalEmptySprites = new Sprite[horizontalSlots.Count];
        for (int i = 0; i < horizontalSlots.Count; i++) horizontalEmptySprites[i] = horizontalSlots[i].sprite;

        if (firePoint == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) firePoint = player.transform;
        }

        RefreshUI();
    }

    int[] NewEmptyArray(int size)
    {
        int[] arr = new int[size];
        for (int i = 0; i < size; i++) arr[i] = -1;
        return arr;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            AttackWithWakpuBall();
    }

    // 아이템 획득. 성공하면 true (칸이 가득 차면 false -> 아이템은 맵에 남음)
    public bool AcquireMaterial(int materialIndex)
    {
        if (materialIndex < 0 || materialIndex >= materials.Count) return false;

        List<int> pool = GetPool();     // 현재 세로칸 재료를 복사
        pool.Add(materialIndex);        // 새 재료를 맨 뒤(가장 최근)에 추가

        // 합성 가능한 만큼 합성 (가로칸 빈 자리 수까지만)
        List<int> made = new List<int>();
        CraftAll(pool, made, EmptyCount(horizontalItems));

        // 합성 후에도 세로칸 칸 수를 넘으면 못 먹음 (예: 점토 4개 + 점토)
        if (pool.Count > verticalItems.Length) return false;

        CommitPool(pool, made);

        ScreenMessage.Show($"{materials[materialIndex].name} 획득!");

        RefreshUI();
        return true;
    }

    void TryAutoCraft()
    {
        List<int> pool = GetPool();
        List<int> made = new List<int>();
        CraftAll(pool, made, EmptyCount(horizontalItems));
        CommitPool(pool, made);
        RefreshUI();
    }

    List<int> GetPool()
    {
        List<int> pool = new List<int>();
        foreach (int item in verticalItems)
            if (item >= 0) pool.Add(item);
        return pool;
    }

    // pool에서 합성 가능한 만큼 합성해 made에 결과를 쌓음
    void CraftAll(List<int> pool, List<int> made, int maxProducts)
    {
        while (made.Count < maxProducts)
        {
            Recipe best = null;
            List<int> bestIndices = null;
            int bestScore = -1;

            foreach (Recipe r in recipes)
            {
                List<int> indices = FindConsumeIndices(pool, r);
                if (indices == null) continue;

                // 점수 = 소모할 재료 중 가장 오래된 것의 위치. 클수록 최근에 먹은 재료끼리 합성
                int score = int.MaxValue;
                foreach (int idx in indices) if (idx < score) score = idx;

                if (score > bestScore) // 동점이면 먼저 적힌 레시피 유지
                {
                    bestScore = score;
                    best = r;
                    bestIndices = indices;
                }
            }

            if (best == null) break; // 더 합성할 게 없음

            // 뒤에서부터 지워야 앞쪽 위치가 밀리지 않음
            bestIndices.Sort();
            for (int i = bestIndices.Count - 1; i >= 0; i--)
                pool.RemoveAt(bestIndices[i]);

            made.Add(best.productIndex);
        }
    }

    // 레시피에 필요한 재료 위치를 최근 것부터 골라 반환 (부족하면 null)
    List<int> FindConsumeIndices(List<int> pool, Recipe r)
    {
        if (r.ingredients.Count == 0) return null;
        if (r.productIndex < 0 || r.productIndex >= products.Count) return null;

        List<int> used = new List<int>();
        foreach (Ingredient ing in r.ingredients)
        {
            int found = 0;
            for (int i = pool.Count - 1; i >= 0 && found < ing.count; i--) // 뒤(최근)부터 검색
            {
                if (pool[i] == ing.materialIndex && !used.Contains(i))
                {
                    used.Add(i);
                    found++;
                }
            }
            if (found < ing.count) return null;
        }
        return used;
    }

    // 계산이 끝난 pool과 결과를 실제 칸에 반영
    void CommitPool(List<int> pool, List<int> made)
    {
        for (int i = 0; i < verticalItems.Length; i++)
            verticalItems[i] = i < pool.Count ? pool[i] : -1;

        foreach (int productIndex in made)
        {
            int slot = FirstEmpty(horizontalItems);
            if (slot < 0) break;
            horizontalItems[slot] = productIndex;
        }
    }

    // 빈 칸 개수
    int EmptyCount(int[] arr)
    {
        int count = 0;
        foreach (int item in arr) if (item < 0) count++;
        return count;
    }

    void AttackWithWakpuBall()
    {
        if (horizontalItems[0] < 0)
        {
            ScreenMessage.Show("발사할 왁뿌볼이 없습니다!");
            return;
        }

        ProductDef product = products[horizontalItems[0]];

        // 총알을 못 쏘는 상태면 왁뿌볼을 소모하지 않고 중단
        if (!FireBullet(product)) return;

        // 한 칸씩 앞으로 당기기
        for (int i = 0; i < horizontalItems.Length - 1; i++)
            horizontalItems[i] = horizontalItems[i + 1];
        horizontalItems[horizontalItems.Length - 1] = -1;

        TryAutoCraft(); // 가로칸이 비었으니 보류된 합성 재시도
    }

    // 총알 생성 후 발사. 성공하면 true
    bool FireBullet(ProductDef product)
    {

        if (product.bulletPrefab == null)
        {
            Debug.LogWarning($"{product.name}의 Bullet Prefab이 비어 있습니다.");
            return false;
        }
        if (firePoint == null)
        {
            Debug.LogWarning("Fire Point가 없습니다. Player 태그나 Fire Point를 설정하세요.");
            return false;
        }

        Vector3 pos = firePoint.position + firePoint.forward * muzzleOffset;
        GameObject go = Instantiate(product.bulletPrefab, pos, firePoint.rotation);

        Bullet bullet = go.GetComponent<Bullet>();
        if (bullet != null) bullet.Launch(firePoint.forward, product.damage);

        return true;
    }

    int FirstEmpty(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++) if (arr[i] < 0) return i;
        return -1;
    }

    // 상태를 읽어서 화면만 갱신
    void RefreshUI()
    {
        for (int i = 0; i < verticalSlots.Count; i++)
        {
            int idx = verticalItems[i];
            if (idx < 0) ApplyEmpty(verticalSlots[i], verticalEmptySprites[i]);
            else Apply(verticalSlots[i], materials[idx].icon, materials[idx].color);
        }

        for (int i = 0; i < horizontalSlots.Count; i++)
        {
            int idx = horizontalItems[i];
            if (idx < 0) ApplyEmpty(horizontalSlots[i], horizontalEmptySprites[i]);
            else Apply(horizontalSlots[i], products[idx].icon, products[idx].color);
        }
    }

    void Apply(Image img, Sprite icon, Color color)
    {
        if (icon != null) { img.sprite = icon; img.color = Color.white; }
        else img.color = color;
    }

    void ApplyEmpty(Image img, Sprite originalSprite)
    {
        img.sprite = originalSprite;
        img.color = emptyColor;
    }
}