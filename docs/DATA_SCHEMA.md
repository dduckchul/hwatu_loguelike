# 데이터 구성

에디터에서 작성하는 원본은 `ScriptableObject`와 프리팹으로 관리하고, 플레이 중 바뀌는 값은 일반 C# 객체 또는 Controller가 소유한다. JSON, 저장 스키마와 범용 데이터 로더는 제출 범위에 포함하지 않는다.

## 카드 원본

### `CardData`

| 필드 | 역할 | 제약 |
|---|---|---|
| `cardId` | 카탈로그·UI 조회용 고유 ID | 비어 있지 않아야 함 |
| `month` | 족보 계산용 월 | 1~10 |
| `cardType` | `Normal`, `Bright`, `Ribbon`, `Animal` | 정의된 값만 허용 |
| `artwork` | 카드 UI Sprite | 표시용 원본 |

`CardData.ToDefinition()`이 ScriptableObject를 Unity 비의존 `CardDefinition`으로 변환한다.

### `CardCatalogData`

전체 `CardData` 목록과 ID 조회를 제공한다. 조회 테이블 생성 시 null 카드, 빈 ID와 중복 ID를 거부하고 존재하지 않는 ID 조회도 실패시킨다.

현재 `Assets/Scripts/Hwatu/Cards/ScriptableObjects`에 1~10월별 4장, 총 40개 카드와 카탈로그 자산이 있다. 각 월은 `Normal` 2장과 월에 맞는 특수 타입 2장으로 구성되며 시작 덱은 월별 `Normal` 한 장만 사용한다.

## 런타임 카드와 상태

| 객체 | 소유 상태 |
|---|---|
| `CardDefinition` | ID, 월, 타입 |
| `CardInstance` | 덱 안의 카드 한 장과 현재 정의 |
| `PlayerDeck` | 런 동안 보유하는 전체 카드 |
| `BattleDeck` | 드로우 더미, 손패, 버림 더미 |
| `CharacterState` | 플레이어 또는 적의 현재 `Money` |

같은 정의의 카드를 여러 장 가질 수 있지만 각 장은 별도 `CardInstance`다. 강화는 원본 `Normal` 카드를 같은 월의 `Ribbon` 또는 `Animal` 정의를 가진 새 인스턴스로 교체하며 카드 수와 위치를 유지한다. 두 장을 제출했을 때의 타입 조합 태그는 카드 데이터에 저장하지 않고 `HandEvaluator`가 판정한다.

## 상점 런타임 상태

- 후보 원본: 카탈로그의 `Normal` 카드
- 후보 수: `CardStoreView`의 슬롯 배열 길이, 현재 3개
- 방문 중 구매 횟수: `CardStoreController`
- 후보별 구매·표시 상태: `CardStoreView`
- 소유 카드 생성: 구매 성공 시 `CardData.ToDefinition()`으로 새 `CardInstance` 생성
- 가격: 구매 성공 횟수에 따라 `0전`, `20전`, `40전`
- 강화 상태: `CardUpgradeController`가 방문별 1회 여부와 `15전` 비용을 관리
- 제거 상태: `CardRemovalController`가 방문별 1회 여부와 `20전` 비용을 관리

가격과 구매·강화·제거 여부는 `CardData`에 저장하지 않는다. 강화 후보는 같은 월의 `Ribbon`, `Animal` 카드 데이터를 카탈로그에서 조회하고, 제거 대상은 현재 `PlayerDeck`의 `CardInstance`다.

## 적 패턴

### `EnemyPatternData`

- `EnemyTurnPattern`: 한 턴에 공개하고 제출할 `CardData` 두 장
- `EnemyPatternData`: 한 개 이상의 턴 패턴을 순환 제공

패턴은 null 카드와 정확히 두 장이 아닌 턴을 거부한다. 현재 `Stage01_Pattern.asset`, `Stage02_Pattern.asset`이 있다.

## 스테이지 조우

### `StageEncounterData`

| 필드 | 역할 | 제약 |
|---|---|---|
| `stageId` | 조우 식별용 ID | 비어 있지 않아야 함 |
| `enemyPrefabs` | 등장 순서의 `EnemyController` 프리팹 | 1~2개, null 불가 |

프리팹은 직접 참조하며 문자열 경로나 `Resources` 로딩을 사용하지 않는다. 배열 순서는 씬의 스폰 포인트 순서와 대응한다.

현재 자산:

- `Stage01_Enemies.asset` → `Rorni.prefab`
- `Stage02_Enemies.asset` → `Goni.prefab`
- `EnemyBase.prefab` → 적 공통 구조

조우 자산과 프리팹에는 현재 전, 패턴 인덱스, 피격 상태를 기록하지 않는다. `EnemyEncounterController`와 생성된 `EnemyController`가 이 런타임 상태를 가진다.

## 표시 데이터

`CharacterMoneyPileView`는 동전 단위별 Sprite, 값, 색상, 최대 개수, 간격과 정렬 순서를 직렬화한다. 이는 `CharacterState.Money`를 시각화할 뿐 실제 전을 변경하지 않는다.

카드 상점은 `CardData` 미리보기를 사용한다. 플레이어 손패는 `CardInstance.Definition.Id`로 카탈로그를 조회해 대응하는 이미지를 표시한다.

## 아직 정의하지 않는 데이터

- 3스테이지 조우와 패턴 콘텐츠
- 보상 레벨별 카드 풀
- 카드별로 현재 월·타입 규칙과 다른 강화 대상을 지정하는 데이터
- 카드 제거 최소 덱 크기
- 성장 효과와 피해 보너스
- 저장·불러오기와 현지화
- 범용 카드 태그, 효과·패시브 ID 시스템
- 12월 비광 보스 데이터

족보는 ScriptableObject가 아니라 `HandEvaluator`, `HandType`, `HandTag`의 일반 C# 코드로 판정한다.
