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

현재 선택 카드는 `PlayerHandView`가 입력 중 임시로 관리한다. Submit 시 `BattleController`가 플레이어와 각 적의 비교 결과를 해당 턴 동안 보관하고, 타격 연출 완료 이벤트에 맞춰 `HandDamageCalculator`의 피해를 `CharacterState` 사이의 Money 이전으로 적용한다. 연출이 모두 끝나면 전투 종료 여부를 확인하고 다음 턴의 버림, 적 패턴 진행과 드로우를 연결한다.

## 현재 카드 보상 상태

- `CardRewardGenerator`: `CardCatalogData.Cards`에서 `Normal` 카드만 모아 중복 ID 없는 후보 세 장을 추첨한다.
- `CardRewardController`: 후보 생성을 요청하고 선택 확정 시 새 `CardInstance`를 `PlayerDeck`에 추가한다.
- `CardRewardView`: 후보별 표시용 `CardInstance`와 `CardPrefab`을 생성하고 선택·확인·건너뛰기 입력을 전달한다.
- `RunRandomProvider`: `CardReward` 전용 난수 스트림을 제공해 덱 셔플 난수와 보상 추첨 난수를 분리한다.

보상 후보와 현재 선택은 런타임 상태이며 `CardData`나 `CardCatalogData` ScriptableObject를 변경하지 않는다. 현재는 별도 보상 카드 풀 ScriptableObject 없이 전체 카탈로그를 원본 후보군으로 사용한다.

보상 확정 흐름은 다음과 같다.

```text
전투 승리
  → Normal 후보 세 장 추첨
  → Reward1~3에 CardPrefab 표시
  → 카드 한 장 선택
  → 확인: PlayerDeck.AddCard
  → 건너뛰기: PlayerDeck 변경 없음
```

## 현재 적 패턴 데이터

- `EnemyPatternData`: 에디터에서 작성하는 ScriptableObject 적 패턴 목록
- `EnemyTurnPattern`: 한 턴에 제출할 `CardData` 두 장
- `EnemyController`: 적 한 명의 `CharacterState`, 패턴과 현재 패턴 인덱스를 연결
- `EnemyHandView`: 현재 패턴의 카드 이미지 두 장과 족보 이름을 표시
- 턴 인덱스가 패턴 수를 넘으면 첫 패턴부터 다시 순환한다.

현재 적 데이터는 턴별 카드 패턴까지만 정의한다. 적 캐릭터의 실제 이미지, 콘셉트와 공통 원본 데이터는 아직 구현하지 않았다.

## 아직 구현하지 않은 데이터

다음 ScriptableObject는 필요해질 때 현재 코드에 맞춰 정의한다. 지금은 필드 구조를 확정하지 않는다.

- 적 원본 데이터
- 보상 레벨별 전투 보상 카드 풀 데이터
- 성장 요소용 피해 보너스 밸런스 데이터
- 카드별 강화 대상 목록과 강화 비용 데이터

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
