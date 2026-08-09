# 아키텍처

## 원칙

- 코드는 `Cards`, `Deck`, `Hands`, `Combat`, `Randomness`, `Rewards`, `UI` 기능별로 나눈다.
- 족보, 피해와 덱 순환은 Unity 비의존 일반 C# 코드로 유지한다.
- `MonoBehaviour`는 입력, 표시와 생명주기를 연결하고 규칙을 중복 계산하지 않는다.
- `ScriptableObject`는 콘텐츠 작성과 로딩 경계이며 런타임 상태를 저장하지 않는다.
- 난수는 `IRandomSource`로 주입하고 용도별 스트림을 분리한다.
- 제출 범위 밖의 저장, 네트워크와 범용 프레임워크는 만들지 않는다.

## 폴더와 책임

| 기능 | 주요 책임 |
|---|---|
| `Cards` | `CardData`, 카탈로그, Unity 비의존 정의와 카드 인스턴스 |
| `Deck` | 런 덱, 전투 덱, 드로우·버림·재순환·카드 교체 |
| `Hands` | 족보 판정, 비교 결과와 태그 |
| `Combat` | 전투 흐름, 전 이전, 적 패턴과 스테이지 조우 |
| `Randomness` | 런 시드와 독립 난수 스트림 |
| `Rewards` | 귀시장 전환, 카드 후보·가격·구매 |
| `UI` | 카드·전·족보 표시, 입력 전달과 전투/상점 연출 |

별도 asmdef, 계층형 Domain 구조, 이벤트 버스와 DI 컨테이너는 현재 사용하지 않는다.

## 핵심 객체

### 카드·덱·족보

- `CardData.ToDefinition()`이 Unity 카드 자산에서 규칙 데이터로 넘어가는 경계다.
- `CardInstance`는 같은 정의의 카드 여러 장을 런타임에서 구분한다.
- `PlayerDeck`은 런 전체 카드, `BattleDeck`은 한 전투의 드로우·손패·버림 상태를 가진다.
- `BattleDeck.TryExchangeCard()`가 선택 카드 폐기, 재순환과 새 카드 1장 드로우를 처리한다.
- `HandEvaluator`와 `HandComparer`는 피해나 UI 문자열을 계산하지 않는다.
- `HandDamageCalculator`와 `BattleStakeCalculator`가 피해와 회차별 판돈을 계산한다.

### 전투

- `BattleController`: 전투 상태, 손패 입력, 족보 비교, 연출 결과와 턴 진행을 연결한다.
- `PlayerController`, `EnemyController`: 각 캐릭터의 `CharacterState`와 View를 연결한다.
- `EnemyPatternData`: 적이 순환해서 공개할 카드 두 장 묶음을 작성한다.
- `BattleSequenceView`: 공격 준비, 돌진, 피격, 복귀와 결과 표시 순서를 담당한다.

`BattleController`는 현재 살아 있는 적 각각과 플레이어 패를 비교한다. 적 등록 순서대로 연출하고, 플레이어가 패배하면 나머지 비교를 중단한다.

### 스테이지 조우

스테이지별 적 로딩은 구현되어 있다.

- `StageEncounterData`: 스테이지 ID와 적 프리팹 1~2개를 순서대로 참조한다.
- `EnemyEncounterController`: 조우 순서, 적 인스턴스 생성·제거와 `CurrentEnemies`를 소유한다.
- `EnemyBase.prefab`: 적의 공통 계층과 컴포넌트를 가진 기준 프리팹이다.
- `Rorni.prefab`, `Goni.prefab`: 현재 몬스터 프리팹이다.
- 씬의 스폰 포인트 배열은 조우 데이터의 적 배열 순서와 대응한다.

`BattleController`는 적 프리팹을 선택하지 않고 `CurrentEnemies`만 사용한다. `StoreController`도 적 선택 규칙을 알지 않으며 현재 적의 Fade 대상만 수집한다.

### 귀시장

- `StoreController`: 배경, 전투 UI, 적, 플레이어 위치와 `StoreView`의 전환을 담당한다.
- `CardStoreController`: 카드 후보 생성, 방문별 구매 횟수, 가격 확인, 전 차감과 덱 추가를 담당한다.
- `CardRewardGenerator`: 카탈로그에서 중복 ID 없는 `Normal` 후보를 뽑는다.
- `StoreCardPriceCalculator`: 구매 횟수를 `0전`, `20전`, `40전` 가격으로 변환한다.
- `CardStoreView`: `CardData` 미리보기를 표시하고 후보 클릭을 전달한다. 소유 카드 인스턴스는 구매 성공 시 Controller가 생성한다.
- `CardStoreSlotView`: 가격과 매진 표현을 담당한다.

`StoreView`는 강화·제거 요청 이벤트를 제공하지만 해당 Controller와 대상 선택 패널은 아직 없다.

### UI

- `PlayerHandView`, `CardView`, `FanCardLayout`: 손패 생성·선택·배치
- `PlayerActionView`: 제출과 카드 교체 입력
- `EnemyHandView`, `BattleResultView`: 적 패와 비교 결과
- `CharacterBattleView`, `CharacterMoneyPileView`: 전투 동작과 전 표시
- `UpperUIView`: 현재 전, 전투 회차, 판돈과 귀시장 제목
- `BackgroundTransitionView`, `StoreView`: 전투/귀시장 화면 상태
- `HandRankPreviewHover`: 족보 통이미지 미리보기
- `HoverButtonView`: 일반 버튼 호버 표현

UI는 규칙 객체가 만든 결과를 표시하며 족보나 피해를 다시 계산하지 않는다.

## 실행 흐름

### 런과 첫 전투

```text
PlayerDeckInitializer.Awake
  → 시작 카드 10장 검증 및 PlayerDeck 생성
  → BattleDeck 생성과 시드 셔플
BattleController.Start
  → 플레이어 초기화
  → EnemyEncounterController.LoadInitialEncounter
  → 적 초기화와 상단 UI 갱신
  → 손패 3장 드로우
```

`RunRandomProvider.BeginRun(seed)`은 스트림 초기화 API로 존재하지만 새 런 UI에는 아직 연결되지 않았다.

### 한 턴

```text
카드 선택 또는 1장 교체
  → 카드 2장 제출
  → HandEvaluator / HandComparer
  → BattleSequenceView
  → HandDamageCalculator / CharacterState.TransferMoneyTo
  → 전투 지속 시 손패 버림, 적 패턴 진행, 재드로우
```

### 전투에서 다음 전투로

```text
적 전원 패배
  → StoreController.EnterStore
  → 배경 전환, 전투 UI·적 FadeOut, 플레이어 중앙 이동
  → StoreView와 카드 상점 표시
  → 넘어가기
  → EnemyEncounterController.LoadNextEncounter
  → 현재 PlayerDeck으로 BattleDeck 재생성
  → 새 적 초기화 및 FadeIn
  → FadeIn 완료 후 손패 3장 드로우
```

현재 씬에는 1·2스테이지 조우만 등록되어 있다. 마지막 조우 뒤의 런 완료 분기는 아직 없다.

## 의존 방향

```text
Deck/Hands → Cards
Deck/Rewards → Randomness
Combat → Deck, Hands, Rewards, UI
Rewards → Cards, Deck, Combat, Randomness, UI
UI → Cards, Deck, Hands
```

`Cards`와 핵심 규칙 객체는 UI에 의존하지 않는다. 새 시스템은 기존 공개 경로를 우회해 같은 상태를 따로 소유하지 않는다.

## 데이터와 난수 경계

- `CardData`, `CardCatalogData`, `EnemyPatternData`, `StageEncounterData`와 프리팹은 작성용 원본이다.
- 카드 소유, Money, 덱 더미, 패턴 인덱스와 구매 여부는 런타임 객체가 소유한다.
- `RunRandomProvider`는 `BattleDeck`, `CardReward` 스트림을 하나의 런 시드에서 독립적으로 만든다.
- Unity 전역 난수와 프레임 시간은 덱과 보상 추첨 규칙에 사용하지 않는다.
