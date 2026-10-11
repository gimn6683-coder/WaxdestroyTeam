using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem; // 새 입력 시스템 네임스페이스 추가

public class InventorySystem : MonoBehaviour
{
    [Header("세로칸 (재료 수집용 4개)")]
    public List<Image> verticalSlots;

    [Header("가로칸 (완성된 왁뿌볼용 4개)")]
    public List<Image> horizontalSlots;

    private int currentMaterialCount = 0; // 현재 모인 재료 개수

    void Update()
    {
        // 키보드가 연결되어 있는지 확인 후 새 입력 시스템으로 체크
        if (Keyboard.current != null)
        {
            // 1. [스페이스바] 누르면 재료 획득
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                AcquireMaterial();
            }

            // 2. [F키] 누르면 첫 번째 가로칸에 있는 왁뿌볼이 공격 발사!
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                AttackWithWakpuBall();
            }
        }
    }

    // 재료를 획득했을 때 실행되는 함수
    public void AcquireMaterial()
    {
        if (currentMaterialCount >= verticalSlots.Count)
        {
            Debug.Log("세로칸이 가득 찼습니다! 자동 합성을 기다려주세요.");
            return;
        }

        // 세로칸에 재료 채우기 (노란색)
        verticalSlots[currentMaterialCount].color = Color.yellow;
        currentMaterialCount++;
        Debug.Log($"재료 획득! 현재 세로칸 개수: {currentMaterialCount}/4");

        // 세로칸이 다 찼는지 확인 후 자동 합성
        if (currentMaterialCount >= verticalSlots.Count)
        {
            AutoCraft();
        }
    }

    // 자동 합성 함수
    void AutoCraft()
    {
        Debug.Log("✨ 재료가 모두 모여 자동으로 합성됩니다!");

        // 세로칸 비우기
        for (int i = 0; i < verticalSlots.Count; i++)
        {
            verticalSlots[i].color = Color.white;
        }
        currentMaterialCount = 0;

        // 가로칸의 빈 곳을 찾아 완성된 왁뿌볼(초록색) 배치
        for (int i = 0; i < horizontalSlots.Count; i++)
        {
            if (horizontalSlots[i].color == Color.white) // 빈 칸 발견
            {
                horizontalSlots[i].color = Color.green; // 왁뿌볼 생성 완료
                Debug.Log($"✨ 왁뿌볼 합성 완료! {i + 1}번 가로칸에 배치되었습니다.");
                return;
            }
        }
        Debug.Log("가로칸이 모두 가득 차서 더 이상 왁뿌볼을 보관할 수 없습니다!");
    }

    // 첫 번째 칸의 왁뿌볼로 공격하고 뒤의 슬롯들을 앞으로 당기는 함수
    void AttackWithWakpuBall()
    {
        // 1번 가로칸(첫 번째 칸)에 왁뿌볼(초록색)이 있는지 확인
        if (horizontalSlots[0].color == Color.green)
        {
            Debug.Log("🚀 1번 칸의 왁뿌볼이 적을 향해 공격을 발사합니다!");

            // 뒤에 있는 슬롯의 왁뿌볼들을 한 칸씩 앞으로 당기기 (Shift)
            for (int i = 0; i < horizontalSlots.Count - 1; i++)
            {
                horizontalSlots[i].color = horizontalSlots[i + 1].color;
            }

            // 가장 마지막 슬롯은 비워주기 (하얀색으로 변경)
            horizontalSlots[horizontalSlots.Count - 1].color = Color.white;
        }
        else
        {
            Debug.Log("❌ 발사할 왁뿌볼이 없습니다! 첫 번째 칸이 비어있습니다.");
        }
    }
}