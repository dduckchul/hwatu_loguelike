# ScriptableObject 데이터 구성

## 문서 목적

현재 구현된 카드 데이터와 런타임 카드 구조를 기록한다. 에디터에서 편집할 원본 콘텐츠는 `ScriptableObject`로 만들고, 덱과 전투 중 바뀌는 값은 일반 C# 객체가 가진다.

JSON, 저장 스키마, 범용 데이터 로더는 이번 제출 범위에 포함하지 않는다.

## 현재 데이터 위치

```text
Assets/
  Scripts/
    Hwatu/
      Cards/
        CardCatalogData.cs
        CardData.cs
        CardDefinition.cs
        CardInstance.cs
        CardType.cs
        ScriptableObjects/
          CardCatalogData.asset
          1_Bright_1.asset
          1_Normal_1.asset
          ...
```

카드 ScriptableObject 자산은 현재 `Assets/Scripts/Hwatu/Cards/ScriptableObjects`에 둔다.

## 카드 타입

카드 한 장은 하나의 `CardType`만 가진다.

```csharp
public enum CardType
{
    Normal, // 피
    Bright, // 광
    Ribbon, // 띠
    Animal  // 열끗
}
```

광땡 판정은 두 카드의 `CardType`이 모두 `Bright`인지 확인한다. 카드 종류는 중복 bool이나 범용 태그 목록으로 저장하지 않는다.

## 카드 원본 데이터

### `CardData`

Inspector에서 작성하는 카드 한 종류의 원본 데이터다.

```csharp
[CreateAssetMenu(fileName = "CardData", menuName = "Hwatu/Card")]
public sealed class CardData : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField, Range(1, 10)] private int month;
    [SerializeField] private CardType cardType;
    [SerializeField] private Sprite artwork;

    public string CardId => cardId;
    public int Month => month;
    public CardType CardType => cardType;
    public Sprite Artwork => artwork;

    public CardDefinition ToDefinition()
    {
        return new CardDefinition(cardId, month, cardType);
    }
}
```

필드 역할:

- `cardId`: 카탈로그와 UI 조회에 사용하는 고유 ID
- `month`: 섯다 족보 계산에 사용하는 1~10월 값
- `cardType`: 피, 광, 띠, 열끗 중 하나
- `artwork`: 카드 UI에 표시할 Sprite

`OnValidate`에서는 `cardId` 앞뒤 공백만 정리한다. ID 중복은 카탈로그를 조회할 때 검사한다.

## 카드 카탈로그

### `CardCatalogData`

현재 사용하는 모든 `CardData`를 한곳에 등록하고 ID로 다시 찾는 ScriptableObject다.

```csharp
[CreateAssetMenu(fileName = "CardCatalogData", menuName = "Hwatu/Card Catalog")]
public sealed class CardCatalogData : ScriptableObject
{
    [SerializeField] private List<CardData> cards;

    public IReadOnlyList<CardData> Cards => cards;
    public CardData GetById(string cardId);
}
```

카탈로그는 조회 테이블을 만들 때 다음 잘못된 데이터를 거부한다.

- null 카드
- 빈 카드 ID
- 중복 카드 ID
- 존재하지 않는 ID 조회

`PlayerHandView`는 런타임 카드의 ID로 카탈로그를 조회해 카드 이미지와 표시 정보를 가져온다.

## 런타임 카드

### `CardDefinition`

덱과 족보 계산에서 사용하는 Unity 비의존 카드 정의다.

```csharp
public sealed class CardDefinition
{
    public string Id { get; }
    public int Month { get; }
    public CardType CardType { get; }
}
```

생성 시 빈 ID, 1~10월 밖의 값, 정의되지 않은 `CardType`을 거부한다.

### `CardInstance`

플레이어 덱에 들어가는 카드 한 장의 런타임 상태다.

```csharp
public sealed class CardInstance
{
    public CardDefinition Definition { get; }
}
```

- 같은 `CardDefinition`을 가진 카드가 덱에 여러 장 들어갈 수 있다.
- 강화 레벨이나 변형 상태는 저장하지 않는다.
- 강화할 때는 `PlayerDeck.UpgradeCard`로 해당 카드를 별도 강화 카드 정의로 교체한다.
- 원본이 `Normal`이고 강화 카드가 같은 월의 `Bright`, `Ribbon`, `Animal` 중 하나일 때만 교체할 수 있다.
- 특수 타입 카드는 다시 강화할 수 없으며 카드의 월은 변경할 수 없다.
- 교체된 카드의 이전 정의와 강화 이력은 보존하지 않는다.

## 현재 카드 자산

- 1월부터 10월까지 월별 4장, 총 40개의 카드 자산이 생성되어 있다.
- 각 월에는 `Normal` 카드 2장과 화투 구성에 맞는 특수 타입 카드 2장이 있다.
- 특수 타입은 월에 따라 `Bright`, `Ribbon`, `Animal` 중에서 정해진다.
- `CardCatalogData.asset`에서 전체 카드 자산을 관리한다.
- 시작 덱은 1~10월의 `Normal` 카드 각 한 장, 총 10장만 사용한다.

## 현재 덱 상태

ScriptableObject가 아닌 일반 C# 객체가 다음 상태를 관리한다.

- `PlayerDeck`: 런 동안 보유하는 전체 카드
- `BattleDeck`: 전투 중 드로우 더미, 손패, 버림 더미
- `CardInstance`: 같은 카드 정의가 여러 장일 때 각 카드를 구분하는 런타임 객체
- `CharacterState`: 플레이어와 적의 현재 `Money`. 화면과 기획에서는 전으로 표시

## 현재 카드 보상과 상점 후보 상태

- 후보, 방문 중 구매 횟수와 후보별 구매 여부는 런타임 상태이며 `CardData`나 `CardCatalogData`를 변경하지 않는다.
- 후보 원본은 현재 `CardCatalogData`의 `Normal` 카드이며, 별도 보상 풀 ScriptableObject는 사용하지 않는다.
- 후보 수는 `CardStoreView`에 연결된 `CardStoreSlotView` 배열 길이를 기준으로 하고 현재 씬은 세 슬롯을 사용한다.
- 구매한 후보는 새 `CardInstance`가 되어 `PlayerDeck`에 추가된다.

기존 일회성 카드 보상 기능은 상점 상단의 카드 구매 목록으로 병합했다. `CardStoreController`가 후보 생성, 가격 확인, 전 차감과 덱 추가를 연결하고 `StoreController`는 귀시장 전체 화면 전환만 담당한다. 상점 방문마다 구매 횟수를 런타임 상태로 관리하며 구매 비용은 순서대로 `0전`, `20전`, `40전`이다. 후보 카드를 클릭하면 즉시 구매하며 `CardData`에는 런타임 가격이나 구매 상태를 저장하지 않는다.

강화와 영구 제거는 같은 방문에서 각각 한 번 사용할 수 있다는 게임 규칙만 확정됐다. 방문별 사용 여부를 소유할 Controller와 런타임 필드는 아직 구현하지 않았으며, 실제 구현 전에는 데이터 스키마로 간주하지 않는다.

## 현재 전 표시 데이터

`CharacterState.Money`가 전투 규칙의 실제 값이며 `CharacterMoneyPileView`는 이 값을 표시만 한다. 표시 설정은 캐릭터별 컴포넌트에 다음 형태로 직렬화한다.

- 동전 단위별 Sprite
- 단위 값
- 표시 색상
- 화면에 표시할 최대 동전 개수
- 동전 사이의 가로 간격과 크기
- 겹침 순서를 위한 기본 Sorting Order

표시할 동전은 큰 단위부터 현재 전을 분해해 만들고, 최대 표시 개수를 넘으면 나머지를 생략한다. 정확한 현재 전은 항상 텍스트로 함께 표시한다. 이 동전 목록은 UI 표현 상태이며 `CharacterState`나 실제 Money 계산을 변경하지 않는다.

## 적과 스테이지 조우 데이터

### 현재 적 패턴 데이터

- `EnemyPatternData`: 에디터에서 작성하는 적의 순환 패턴 목록
- `EnemyTurnPattern`: 한 턴에 제출할 `CardData` 두 장

현재 구현된 적 데이터는 턴별 카드 패턴까지이며 씬의 `EnemySample`에 직접 연결되어 있다.

### `StageEncounterData`

**확정, 미구현**

한 스테이지에서 생성할 적 프리팹 구성을 작성하는 ScriptableObject다.

```csharp
[CreateAssetMenu(fileName = "StageEncounterData", menuName = "Hwatu/Combat/Stage Encounter")]
public sealed class StageEncounterData : ScriptableObject
{
    [SerializeField] private string stageId;
    [SerializeField] private EnemyController[] enemyPrefabs;
}
```

- `stageId`: 스테이지 구성을 식별하는 안정적인 고유 ID
- `enemyPrefabs`: 등장 순서대로 등록하는 적 프리팹 1~2개
- 배열 인덱스는 씬의 `EnemySpawnPoint1`, `EnemySpawnPoint2`에 대응한다.
- 프리팹은 프로젝트 에셋 직접 참조를 사용하며 문자열 경로나 Resources 로딩을 사용하지 않는다.
- 현재 Money, 패턴 인덱스와 피격 상태 같은 런타임 값은 저장하지 않는다.

### 적 프리팹

**확정, 미구현**

`EnemyBase.prefab`에 공통 `EnemyController`, `EnemyHandView`, `CharacterBattleView`, `CharacterMoneyPileView`와 이펙트 계층을 둔다. 몬스터별 프리팹은 기준 프리팹의 Variant로 작성하고 다음 콘텐츠 참조와 표시 설정만 변경한다.

- `EnemyPatternData`
- 시작 전
- 대치·승부·공격 Sprite
- 공격·피격 연출 설정
- 크기와 위치 보정

프리팹 인스턴스의 `CharacterState`와 현재 패턴 인덱스는 매 전투 초기화되는 런타임 상태다. 스테이지 데이터나 프리팹 원본을 플레이 중 수정하지 않는다.

## 아직 구현하지 않은 데이터

다음 ScriptableObject는 필요해질 때 현재 코드에 맞춰 정의한다. 지금은 필드 구조를 확정하지 않는다.

- 보상 레벨별 전투 보상 카드 풀 데이터
- 성장 요소용 피해 보너스 밸런스 데이터
- 카드별 강화 대상 목록과 강화 비용 데이터
- 카드 영구 제거 비용과 최소 덱 수 데이터

별도 맵 씬, 맵 노드와 경로 데이터는 첫 번째 플레이어블에 사용하지 않는다. 런 진행 순서는 같은 전투 씬에서 관리하는 고정 단계로 둔다.

## 현재 만들지 않는 데이터 구조

- 저장 및 불러오기 스키마
- JSON DTO와 변환기
- 현지화 키 시스템
- 카드의 범용 태그 목록
- 족보별 ScriptableObject
- 특수 상성 ScriptableObject
- 효과와 패시브의 범용 ID 시스템
- 데이터 마이그레이션과 전체 자동 검증 도구

족보는 현재 `HandEvaluator`, `HandType`, `HandTag`의 일반 C# 코드로 판정한다.
