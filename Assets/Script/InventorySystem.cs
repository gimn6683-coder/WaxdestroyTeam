using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public enum WakpuType { None, A, B, C, D }

public class InventorySystem : MonoBehaviour
{
    [Header("세로칸 (재료 수집용 4개)")]
    public List<Image> verticalSlots;

    [Header("가로칸 (완성된 왁뿌볼용 4개)")]
    public List<Image> horizontalSlots;

    [Header("색 지정 (Inspector에서 수정)")]
    [Tooltip("빈 칸일 때 색")]
    public Color emptyColor = Color.white;

    [Tooltip("재료 색: 0=A, 1=B, 2=C, 3=D")]
    public Color[] materialColors = { Color.red, Color.green, Color.blue, Color.magenta };

    [Tooltip("완성된 왁뿌볼 색: 0=A, 1=B, 2=C, 3=D")]
    public Color[] wakpuColors = { Color.red, Color.green, Color.blue, Color.magenta };

    // 상태는 색이 아니라 enum으로 관리
    private WakpuType[] verticalTypes;    // 세로칸에 모인 재료 종류
    private WakpuType[] horizontalTypes;  // 가로칸의 왁뿌볼 종류
    private int currentMaterialCount = 0;

    void Start()
    {
        verticalTypes = new WakpuType[verticalSlots.Count];
        horizontalTypes = new WakpuType[horizontalSlots.Count];
        RefreshUI();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // 1~4키: 재료 A~D 획득
        if (Keyboard.current.digit1Key.wasPressedThisFrame) AcquireMaterial(WakpuType.A);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) AcquireMaterial(WakpuType.B);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) AcquireMaterial(WakpuType.C);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) AcquireMaterial(WakpuType.D);

        // F키: 첫 번째 칸 왁뿌볼 발사
        if (Keyboard.current.fKey.wasPressedThisFrame) AttackWithWakpuBall();
    }

    public void AcquireMaterial(WakpuType type)
    {
        if (currentMaterialCount >= verticalSlots.Count)
        {
            Debug.Log("세로칸이 가득 찼습니다!");
            return;
        }

        verticalTypes[currentMaterialCount] = type;
        currentMaterialCount++;
        RefreshUI();
        Debug.Log($"재료 {type} 획득! ({currentMaterialCount}/{verticalSlots.Count})");

        if (currentMaterialCount >= verticalSlots.Count)
            AutoCraft();
    }

    void AutoCraft()
    {
        WakpuType result = DecideCraftResult();

        // 세로칸 비우기
        for (int i = 0; i < verticalTypes.Length; i++)
            verticalTypes[i] = WakpuType.None;
        currentMaterialCount = 0;

        // 가로칸의 첫 빈 칸에 배치
        for (int i = 0; i < horizontalTypes.Length; i++)
        {
            if (horizontalTypes[i] == WakpuType.None)
            {
                horizontalTypes[i] = result;
                Debug.Log($"{result} 왁뿌볼 합성 완료! {i + 1}번 가로칸에 배치되었습니다.");
                RefreshUI();
                return;
            }
        }

        Debug.Log("가로칸이 가득 차서 왁뿌볼을 보관할 수 없습니다!");
        RefreshUI();
    }

    // 합성 규칙: 가장 많이 모인 재료 종류 (동점이면 먼저 모은 재료 쪽)
    WakpuType DecideCraftResult()
    {
        WakpuType best = verticalTypes[0];
        int bestCount = 0;

        for (int i = 0; i < verticalTypes.Length; i++)
        {
            int count = 0;
            for (int j = 0; j < verticalTypes.Length; j++)
                if (verticalTypes[j] == verticalTypes[i]) count++;

            if (count > bestCount) // 엄격하게 큰 경우만 교체 -> 동점이면 먼저 모은 쪽 유지
            {
                bestCount = count;
                best = verticalTypes[i];
            }
        }
        return best;
    }

    void AttackWithWakpuBall()
    {
        if (horizontalTypes[0] == WakpuType.None)
        {
            Debug.Log("발사할 왁뿌볼이 없습니다!");
            return;
        }

        WakpuType fired = horizontalTypes[0];
        Debug.Log($"{fired} 왁뿌볼 발사!"); // 종류별 공격은 여기서 switch (fired)로 분기

        // 한 칸씩 앞으로 당기기
        for (int i = 0; i < horizontalTypes.Length - 1; i++)
            horizontalTypes[i] = horizontalTypes[i + 1];
        horizontalTypes[horizontalTypes.Length - 1] = WakpuType.None;

        RefreshUI();
    }

    // 상태(enum)를 읽어서 색만 칠해 주는 함수
    void RefreshUI()
    {
        for (int i = 0; i < verticalSlots.Count; i++)
            verticalSlots[i].color = GetColor(verticalTypes[i], materialColors);

        for (int i = 0; i < horizontalSlots.Count; i++)
            horizontalSlots[i].color = GetColor(horizontalTypes[i], wakpuColors);
    }

    Color GetColor(WakpuType type, Color[] palette)
    {
        int index = (int)type - 1; // None(0)은 제외, A=0, B=1 ...
        if (index < 0 || index >= palette.Length) return emptyColor;
        return palette[index];
    }
}