# 아키텍처 원칙

## 문서 목적

이 프로젝트는 2026년 8월 10일 제출을 목표로 하는 짧은 Unity 프로토타입이다. 복잡한 계층 구조보다 기능을 찾고 수정하기 쉬운 구조를 우선한다.

## 기본 방향

- 코드는 `Cards`, `Deck`, `Hands`, `Combat`, `Rewards`, `UI` 기능별로 나눈다.
- 한 기능에 필요한 데이터, 규칙, Unity 연결 코드는 같은 기능 폴더에 둔다.
- 별도 계층, 인터페이스, 이벤트 버스는 실제로 필요할 때만 만든다.
- 족보, 덱 순환, 피해 계산 같은 규칙은 가능한 한 `MonoBehaviour`와 분리된 일반 C# 클래스로 작성한다.
- UI는 계산 결과를 표시하고 입력을 전달한다. UI에서 족보나 피해를 다시 계산하지 않는다.
- 제출 범위에 없는 저장, 네트워크, 범용 프레임워크는 미리 만들지 않는다.

## 현재 폴더 구조

```text
Assets/
  Images/
  Prefabs/
    UI/
  Scenes/
  Scripts/
    Hwatu/
      Cards/
        ScriptableObjects/
      Combat/
      Deck/
      Hands/
      Randomness/
      Rewards/
      UI/
```

사용하지 않는 빈 기능 폴더는 미리 만들지 않는다.

## 기능별 책임

### `Hwatu/Cards`

현재 구성:

- `CardData`: Inspector에서 작성하는 ScriptableObject 카드 원본
- `CardCatalogData`: 전체 카드 목록과 ID 조회
- `CardDefinition`: 덱과 족보 계산에 사용하는 일반 C# 정의
- `CardInstance`: 덱에서 같은 정의의 카드 여러 장을 구분하는 런타임 카드 한 장
- `CardType`: 피, 광, 띠, 열끗 중 하나

`CardData.ToDefinition()`이 Unity 데이터에서 규칙 데이터로 넘어가는 경계다.

### `Hwatu/Deck`

현재 구성:

- `PlayerDeck`: 한 런 동안 보유하는 전체 카드
- `PlayerDeck.UpgradeCard`: 같은 월의 Normal 카드를 Bright, Ribbon, Animal 카드 정의로 교체
- `BattleDeck`: 전투용 드로우 더미, 손패, 버림 더미
- `PlayerDeckInitializer`: 1~10월 Normal 카드로 시작 덱 생성
- `BattleDeckController`: Unity와 전투 덱 초기화 연결

`BattleDeck`은 생성 시 Fisher–Yates 방식으로 셔플한다. 손패를 목표 수량까지 뽑고, 드로우 더미가 비면 버림 더미를 다시 섞어 사용한다.

### `Hwatu/Hands`

현재 구성:

- `HandEvaluator`: 두 카드의 섯다 족보 판정
- `HandResult`: 족보 타입, 순위, 끗, 태그, 구성 카드
- `HandType`: 끗, 중간 족보, 땡, 광땡
- `HandTag`: 일반 족보와 특수 상성에서 사용할 결과 태그
- `HandComparer`: 두 `HandResult.Rank`의 단순 비교

`HandEvaluator`는 피해량이나 UI 문자열을 계산하지 않는다. UI 표시 이름은 `HandDisplayName`이 담당한다.

### `Hwatu/Combat`

현재 구성:

- `BattleController`: 전투 덱, 플레이어 손패 UI, Submit 상태와 등록된 적 1~2명을 연결
- `EnemyController`: 적 한 명의 시작 Money, 패턴과 현재 패턴 순서를 관리
- `CharacterState`: 플레이어와 적이 공통으로 사용하는 일반 C# Money 상태
- `HandDamageCalculator`: 기본 판돈과 족보에 따른 피해를 계산하는 일반 C# 규칙 객체
- `EnemyPatternData`: 에디터에서 작성하는 적의 턴별 패턴 원본

`BattleController`는 전투 시작 시 다음 흐름을 연결한다.

1. 등록된 `EnemyController` 1~2명 검증과 초기화
2. 초기화된 `BattleDeck` 확인
3. 시작 손패 3장 드로우
4. `PlayerHandView`에 카드 전달
5. `DeckCountView` 갱신

카드 선택, 플레이어 족보 미리보기와 Submit 입력은 연결되어 있다. Submit 시 플레이어 패를 살아 있는 각 적의 패와 개별 비교하고, 적 등록 순서대로 연출한 뒤 승자의 패로 피해를 계산해 패자의 Money를 이전한다. 플레이어가 패배하면 남은 비교를 중단한다. 연출 종료 후 전투 중이면 전체 손패를 버리고 살아 있는 적의 패턴을 진행한 뒤 다음 손패를 뽑는다. 모든 적이 패배하면 입력을 잠그고 카드 보상 화면을 연다. 다음 전투 생성과 화면 전환은 아직 구현하지 않았다.

### `Hwatu/Randomness`

현재 구성:

- `IRandomSource`: 덱과 보상 규칙에 주입하는 최소 난수 인터페이스
- `SeededRandomSource`: `System.Random`을 사용하는 고정 시드 난수 공급자
- `RandomStreamId`: 현재 사용하는 `BattleDeck`, `CardReward` 난수 스트림 구분
- `RunRandomProvider`: 하나의 `RunSeed`에서 용도별 독립 난수 스트림 제공

같은 `RunSeed`는 같은 스트림별 결과를 재현한다. 한 스트림의 난수 소비량이 달라져도 다른 스트림의 결과에는 영향을 주지 않는다. 적 행동, 상점, 맵처럼 실제 난수 기능이 추가될 때만 새 스트림 ID를 정의한다.

### `Hwatu/Rewards`

현재 구성:

- `CardRewardGenerator`: 카드 카탈로그에서 중복 ID 없는 보상 후보를 추첨
- `CardRewardController`: 보상 후보 생성, UI 결과 수신과 `PlayerDeck.AddCard` 연결

현재 보상 생성기는 전체 카드 카탈로그에서 `Normal` 카드만 필터링하고 세 장을 제시한다. 선택한 카드의 확정 또는 보상 건너뛰기가 끝나면 보상 UI를 닫는다.

### `Hwatu/UI`

현재 구성:

- `CardView`: 카드 이미지, 월, 타입 표시
- `PlayerHandView`: 카탈로그 조회와 카드 프리팹 생성
- `FanCardLayout`: 손패 부채꼴 배치
- `DeckCountView`: 드로우 더미와 버림 더미 수 표시
- `PlayerActionView`: Submit과 Reroll 버튼 입력 전달
- `EnemyHandView`: 적 패턴의 카드 두 장과 족보 이름 표시
- `BattleResultView`: 등록된 적별 패 비교 결과 표시
- `CharacterBattleView`: 대치 스프라이트와 공격 준비·돌진·피격·복귀 연출
- `CharacterMoneyPileView`: 현재 전을 텍스트와 단위별 동전 Sprite 더미로 표시
- `CardRewardView`: 보상 카드 세 장의 생성, 선택 표시와 확인·건너뛰기 입력 전달
- `RewardButtonView`: `Button.interactable`을 기준으로 보상 화면 버튼의 호버 표시
- `CardTypeDisplayName`: 카드 타입의 한국어 표시
- `HandDisplayName`: 족보 결과의 한국어 표시

UI는 `CardInstance` ID를 `CardCatalogData`에서 조회해 Sprite를 연결한다. 족보와 덱 규칙은 UI에 다시 구현하지 않는다.

`CardRewardView`는 각 보상 카드의 `CardView`, `CardData`, 위치와 투명도 상태를 하나의 런타임 항목으로 관리한다. 보상 후보 수는 Controller의 별도 상수가 아니라 씬에 연결된 보상 슬롯 수를 단일 기준으로 사용한다.

## 현재 실행 흐름

```text
PlayerDeckInitializer.Awake
  → Normal 카드 10장 검증
  → PlayerDeck 생성
  → BattleDeckController.Initialize
  → BattleDeck 생성 및 시드 셔플

BattleController.Start
  → BattleDeck.DrawToHand
  → PlayerHandView.SetCards
  → CardCatalogData.GetById
  → CardView.Bind
  → DeckCountView.Refresh

PlayerActionView.SubmitClicked
  → PlayerHandView.SelectedCards
  → HandEvaluator.Evaluate
  → 각 EnemyController.GetCurrentCards
  → HandComparer.Compare
  → BattleSequenceView.Play
  → 공격자 후퇴와 준비
  → 전진 공격과 피격 연출
  → HandDamageCalculator.Calculate
  → CharacterState.TransferMoneyTo
  → CharacterMoneyPileView 갱신
  → BattleDeck.DiscardHand
  → EnemyController.AdvancePattern
  → BattleDeck.DrawToHand

BattleSequenceView.SequenceCompleted
  → BattleController가 전투 승리 확인
  → CardRewardController.ShowRewards
  → CardRewardGenerator가 후보 3장 추첨
  → CardRewardView가 Reward1~3에 CardPrefab 생성
  → 확인 시 PlayerDeck.AddCard 또는 건너뛰기
```

## 기능 사이의 의존 방향

```text
Combat → Deck
Combat → Rewards
Combat → UI
Deck   → Cards
Deck   → Randomness
Hands  → Cards
Rewards → Cards
Rewards → Deck
Rewards → Randomness
Rewards → UI
UI     → Cards
UI     → Deck
UI     → Hands
```

- `Cards`는 다른 게임 기능에 의존하지 않는다.
- `Deck`과 `Hands`는 Unity UI를 알지 않는다.
- `Combat`은 규칙과 UI 흐름을 연결하지만 족보 계산을 직접 구현하지 않는다.
- 순환 참조가 실제로 생길 때만 책임을 다시 나눈다.

## ScriptableObject 경계

- `CardData`와 `CardCatalogData`는 작성용 원본 데이터다.
- ScriptableObject는 플레이 중 수정하지 않는다.
- `CardDefinition`과 `CardInstance`는 Unity 객체에 의존하지 않는다.
- UI는 카탈로그로 원본 데이터를 다시 조회해 이미지를 표시한다.
- 적의 턴별 카드 패턴은 `EnemyPatternData` ScriptableObject로 작성한다.
- 현재 보상 후보는 별도 보상 풀 ScriptableObject 없이 `CardCatalogData`를 사용한다.
- 보상 레벨별 풀이 실제 구현될 때 작성용 보상 데이터의 필요 여부를 다시 결정한다.

## 난수

- 덱 셔플과 보상 추첨은 `IRandomSource`를 통해 난수를 받는다.
- `RunRandomProvider`는 하나의 `RunSeed`에서 `BattleDeck`, `CardReward` 스트림을 분리한다.
- 현재 구현은 `SeededRandomSource`와 `System.Random`을 사용한다.
- 같은 `RunSeed`에서 같은 스트림별 결과를 재현할 수 있어야 한다.
- 전역 Unity 난수와 프레임 시간은 덱 규칙에서 사용하지 않는다.

## 네임스페이스

기능 이름을 그대로 사용한다.

- `Hwatu.Cards`
- `Hwatu.Deck`
- `Hwatu.Hands`
- `Hwatu.Combat`
- `Hwatu.Randomness`
- `Hwatu.Rewards`
- `Hwatu.UI`

이번 프로토타입을 위해 별도 asmdef를 만들지 않는다.

## 프로토타입에서 만들지 않는 구조

- Domain/Application/Infrastructure/Presentation 계층
- 기능마다 Repository, Service, UseCase 인터페이스 생성
- 범용 이벤트 버스와 의존성 주입 컨테이너
- 저장 데이터 마이그레이션
- 플러그인형 피해 공식 프레임워크
- Unity Test Framework 기반 자동화 테스트
- 제출 범위 밖 기능을 위한 확장 포인트

실제 중복이나 변경 요구가 생겼을 때만 구조를 확장한다.
