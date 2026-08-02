# ScriptableObject 데이터 구성

## 문서 목적

8월 10일 제출용 프로토타입에 필요한 데이터만 정의한다. 카드와 적처럼 에디터에서 조정할 콘텐츠는 `ScriptableObject`로 만들고, 전투 중 바뀌는 값은 일반 C# 객체에 둔다.

JSON, 저장 스키마, 범용 데이터 로더는 현재 범위에 포함하지 않는다.

## 기본 원칙

- 카드, 적, 보상 풀은 ScriptableObject로 작성한다.
- 체력, 손패, 덱 순서, 강화 단계는 런타임 객체에 저장한다.
- ScriptableObject 원본은 플레이 중 수정하지 않는다.
- 프로토타입에서 사용하지 않는 필드는 미리 추가하지 않는다.
- Inspector에서 직접 알아볼 수 있는 이름과 참조를 사용한다.
- 데이터 검증은 `OnValidate`의 간단한 범위 검사만 사용한다.

## 폴더

```text
Assets/
  Data/
    Cards/
    Enemies/
    Rewards/
  Scripts/
    Cards/
    Combat/
    Rewards/
```

## 카드 데이터

### `CardData`

화투패 한 종류의 변하지 않는 원본 데이터다.

```csharp
[CreateAssetMenu(menuName = "Hwatu/Card")]
public sealed class CardData : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField, Range(1, 10)] private int month;
    [SerializeField] private bool isBright;
    [SerializeField] private Sprite artwork;

    public CardDefinition ToDefinition()
        => new(cardId, month, isBright);
}
```

8월 7일까지 필요한 필드:

- `cardId`: 카드 구분용 문자열
- `month`: 1~10월
- `isBright`: 광땡 판정에 사용
- `artwork`: 카드 화면 표시

띠, 열끗, 피 분류는 해당 기능을 실제로 구현할 때 추가한다.

### 런타임 카드

족보와 덱 로직이 `ScriptableObject`에 직접 의존하지 않도록 필요한 값만 일반 C# 타입으로 변환한다. 덱에 같은 카드가 여러 장 들어가거나 카드별 강화가 필요하므로 원본 정의와 카드 한 장의 상태도 분리한다.

```csharp
public sealed class CardDefinition
{
    public string Id { get; }
    public int Month { get; }
    public bool IsBright { get; }

    public CardDefinition(string id, int month, bool isBright)
    {
        Id = id;
        Month = month;
        IsBright = isBright;
    }
}

public sealed class CardInstance
{
    public CardDefinition Definition { get; }
    public int UpgradeLevel { get; private set; }
}
```

- `Definition`: `CardData`에서 복사한 최소 규칙 데이터
- `upgradeLevel`: 현재 카드 한 장의 강화 단계

강화를 구현하기 전에는 `upgradeLevel`을 항상 0으로 두어도 된다.

## 적 데이터

### `EnemyData`

적의 표시 정보, 체력, 공개할 패 순서를 가진다.

```csharp
[CreateAssetMenu(menuName = "Hwatu/Enemy")]
public sealed class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyId;
    [SerializeField] private string displayName;
    [SerializeField, Min(1)] private int maxHp;
    [SerializeField] private Sprite artwork;
    [SerializeField] private List<EnemyTurnData> turnPattern;
}

[Serializable]
public sealed class EnemyTurnData
{
    [SerializeField] private CardData firstCard;
    [SerializeField] private CardData secondCard;
}
```

8월 7일까지 적 AI는 `turnPattern`을 처음부터 끝까지 반복하는 방식으로 충분하다.

현재 넣지 않는 필드:

- 패시브 목록
- 행동 가중치
- 체력 조건 분기
- 보상 테이블 ID
- 특수 효과 ID

## 보상 데이터

### `RewardPoolData`

전투 후 제시할 수 있는 카드 목록만 가진다.

```csharp
[CreateAssetMenu(menuName = "Hwatu/Reward Pool")]
public sealed class RewardPoolData : ScriptableObject
{
    [SerializeField] private List<CardData> cards;
}
```

보상 로직은 목록에서 중복되지 않는 카드 3장을 뽑고, 플레이어가 한 장을 선택하거나 건너뛰게 한다.

현재 넣지 않는 필드:

- 희귀도
- 가중치
- 조건부 보상
- 별도 카드 풀 참조
- 스킵 보너스

## 피해 밸런스 데이터

피해 수치를 Inspector에서 자주 조정해야 한다면 `DamageBalanceData` 하나만 추가한다.

```csharp
[CreateAssetMenu(menuName = "Hwatu/Damage Balance")]
public sealed class DamageBalanceData : ScriptableObject
{
    [SerializeField] private int middleHandBonus;
    [SerializeField] private int ddangBonus;
    [SerializeField] private int brightDdangBonus;
}
```

피해 공식 자체는 `DamageCalculator`에 두고, ScriptableObject에는 조정할 숫자만 둔다. 피해 비교 실험 전까지 이 자산은 선택 사항이다.

## ScriptableObject로 만들지 않는 것

다음 값은 플레이 중 계속 바뀌므로 일반 C# 객체가 소유한다.

- 플레이어와 적의 현재 체력
- 드로우 더미, 손패, 버림 더미
- 현재 선택한 카드
- 전투 턴 번호
- 카드별 강화 단계
- 현재 보상 후보와 선택 결과
- 전투 승패 상태

씬 전환 중 상태 보존이 필요해지기 전에는 별도 저장용 ScriptableObject를 만들지 않는다.

## 8월 7일까지 필요한 자산

- 1~10월 시작 덱용 `CardData` 10개
- 같은 월 중복과 광땡을 확인할 추가 `CardData`
- 일반 적용 `EnemyData` 1개
- 카드 보상용 `RewardPoolData` 1개
- 카드와 적의 플레이스홀더 Sprite

일반 적 추가, 12월 비광 보스, 카드 강화 데이터는 시간이 남으면 만든다.

## 현재 만들지 않는 데이터 구조

- `schema_version`
- JSON DTO와 변환기
- 저장 및 불러오기 데이터
- 현지화 키와 별도 표시 데이터
- 범용 태그 시스템
- 족보별 ScriptableObject
- 특수 상성 ScriptableObject
- 적 행동 하나마다 별도 ScriptableObject
- 효과와 패시브의 범용 ID 시스템
- 데이터 마이그레이션과 전체 자동 검증 도구

족보는 수가 적고 규칙이 고정되어 있으므로 우선 `HandEvaluator`와 enum으로 구현한다. 실제 콘텐츠 추가 과정에서 코드 수정이 반복될 때만 데이터화한다.
