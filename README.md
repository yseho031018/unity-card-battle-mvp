# Unity 카드 배틀 MVP

유희왕에서 영감을 받은 Unity 2D 1인용 카드 배틀 프로토타입입니다.

현재 프로젝트는 아주 작은 MVP에 집중합니다. 카드 데이터, 덱 드로우, 손패 UI, 카드 선택, 몬스터 소환, 마법 카드 사용 흐름, 함정 카드 세트 흐름까지만 구현되어 있습니다.

## Unity 버전

- Unity 6000.4.8f1
- uGUI
- TextMesh Pro
- Universal Render Pipeline 2D

## 현재 구현된 기능

- `ScriptableObject` 기반 카드 데이터
- `CardType` enum
  - Monster
  - Spell
  - Trap
- 덱 셔플 및 시작 손패 5장 드로우
- `CardView` 프리팹 기반 손패 UI 생성
- 카드 정보 표시
  - 이름
  - 타입
  - 코스트 또는 레벨
  - 공격력 / 수비력
  - 설명
- 카드 hover 및 선택 표시
- 몬스터 존 5칸
- 함정 존 5칸
- 턴 패널 및 드로우 버튼
- 손패가 많아질 때 자동 축소/정렬
- 마법 카드 사용 흐름
  - 실제 효과는 아직 없음
  - 로그만 출력
  - 사용 후 손패에서 제거
  - 내부 Graveyard 리스트에 추가
- 함정 카드 세트 흐름
  - 함정 존에 세트
  - 세트 후 손패에서 제거
- 기본 UI와 테스트 카드 생성을 위한 Editor Tool

## 아직 구현하지 않은 것

현재 MVP 범위에서 의도적으로 제외한 기능입니다.

- 전투 시스템
- 체인 시스템
- AI 상대
- 실제 카드 효과
- 애니메이션
- 묘지 UI
- 카드 일러스트 표시

## Editor Tool

Unity 상단 메뉴에서 사용할 수 있습니다.

```text
Tools > Create Default Card Prefab
Tools > Create Test Card Data
```

`Create Default Card Prefab`은 다음 항목을 생성하거나 갱신합니다.

- Canvas
- EventSystem
- CardView 프리팹
- 손패 영역
- 몬스터 존
- 함정 존
- 턴 패널
- 카드 액션 버튼
- 필요한 Manager 오브젝트들

`Create Test Card Data`는 현재 테스트용 카드 10장을 생성하거나 갱신합니다.

## 테스트 카드 목록

### 몬스터

- Iron Knight
  - ATK 1600
  - DEF 1200
  - LV 4
- Fire Dragon
  - ATK 2400
  - DEF 1800
  - LV 6
- Stone Golem
  - ATK 1000
  - DEF 2500
  - LV 5
- Wind Falcon
  - ATK 1400
  - DEF 1000
  - LV 3
- Shadow Assassin
  - ATK 1800
  - DEF 800
  - LV 4

### 마법

- Power Boost
  - 임시 설명: 몬스터 공격력 +500
- Healing Light
  - 임시 설명: 라이프 1000 회복
- Draw Scroll
  - 임시 설명: 카드 2장 드로우

### 함정

- Mirror Shield
  - 임시 설명: 공격 무효
- Pitfall Trap
  - 임시 설명: 공격 몬스터 파괴

## 실행 방법

1. Unity에서 프로젝트를 엽니다.
2. `Assets/Scenes/SampleScene.unity` 씬을 엽니다.
3. Unity 상단 메뉴에서 아래 메뉴를 실행합니다.

```text
Tools > Create Default Card Prefab
Tools > Create Test Card Data
```

4. 씬의 `DeckManager` 오브젝트를 선택합니다.
5. `Starting Deck`에 테스트 카드 에셋들을 넣습니다.
6. Play 버튼을 누릅니다.

## 현재 게임 흐름

1. 덱이 셔플됩니다.
2. 시작 손패 5장을 드로우합니다.
3. 몬스터 카드를 선택한 뒤 빈 몬스터 존을 클릭하면 소환됩니다.
4. 마법 카드를 선택한 뒤 `Use Card` 버튼을 누르면 사용됩니다.
5. 함정 카드를 선택한 뒤 `Set Trap` 버튼을 누르면 함정 존에 세트됩니다.
6. `Next Turn / Draw` 버튼을 누르면 턴이 증가하고 카드 1장을 드로우합니다.

## 다음 작업 후보

- `SampleScene`을 `BattleScene`으로 이름 변경
- `CardData`에 카드 일러스트 필드 추가
- 카드 hover 시 확대 프리뷰 추가
- 묘지 UI 추가
- 간단한 마법 효과 구현
- 함정 카드 발동 구조 추가
- 이후 간단한 전투 단계 구현

