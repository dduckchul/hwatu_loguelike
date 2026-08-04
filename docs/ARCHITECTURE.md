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
      UI/
```

추후 보상 기능을 구현할 때 `Assets/Scripts/Hwatu/Rewards`를 추가한다. 사용하지 않는 빈 기능 폴더는 미리 만들지 않는다.

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
- `IRandomSource`: 셔플에서 사용하는 최소 난수 인터페이스
- `SeededRandomSource`: 고정 시드 기반 난수 구현

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
- `EnemyPatternData`: 에디터에서 작성하는 적의 턴별 패턴 원본

`BattleController`는 전투 시작 시 다음 흐름을 연결한다.

1. 등록된 `EnemyController` 1~2명 검증과 초기화
2. 초기화된 `BattleDeck` 확인
3. 시작 손패 3장 드로우
4. `PlayerHandView`에 카드 전달
5. `DeckCountView` 갱신

카드 선택, 플레이어 족보 미리보기와 Submit 입력은 연결되어 있다. Submit 시 플레이어 패를 등록된 각 적의 패와 개별 비교해 표시하는 코드가 있으며, 씬 연결과 플레이 모드 검증은 남아 있다. 비교 결과를 보관하는 턴 상태, 적 행동 순서, 피해와 전투 승패는 아직 구현하지 않았다.

### `Hwatu/UI`

현재 구성:

- `CardView`: 카드 이미지, 월, 타입 표시
- `PlayerHandView`: 카탈로그 조회와 카드 프리팹 생성
- `FanCardLayout`: 손패 부채꼴 배치
- `DeckCountView`: 드로우 더미와 버림 더미 수 표시
- `PlayerActionView`: Submit과 Reroll 버튼 입력 전달
- `EnemyHandView`: 적 패턴의 카드 두 장과 족보 이름 표시
- `BattleResultView`: 등록된 적별 패 비교 결과 표시
- `CardTypeDisplayName`: 카드 타입의 한국어 표시
- `HandDisplayName`: 족보 결과의 한국어 표시

UI는 `CardInstance` ID를 `CardCatalogData`에서 조회해 Sprite를 연결한다. 족보와 덱 규칙은 UI에 다시 구현하지 않는다.

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
  → BattleResultView.ShowPlayerOutcomes
```

## 기능 사이의 의존 방향

```text
Combat → Deck
Combat → UI
Deck   → Cards
Hands  → Cards
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
- 적 캐릭터 원본 데이터와 보상 데이터는 해당 기능을 구현할 때 추가한다.

## 난수

- 셔플은 `IRandomSource`를 통해 난수를 받는다.
- 현재 구현은 `SeededRandomSource`와 `System.Random`을 사용한다.
- 같은 시드는 같은 셔플 결과를 재현할 수 있어야 한다.
- 전역 Unity 난수와 프레임 시간은 덱 규칙에서 사용하지 않는다.

## 수동 검증

Unity 에디터에서 다음을 직접 확인한다.

- 시작 덱이 1~10월 Normal 카드 한 장씩으로 생성되는지
- 같은 시드에서 같은 카드 순서가 나오는지
- 시작 손패가 3장 표시되는지
- 카드 이미지, 월, 타입이 올바르게 표시되는지
- 덱 수량과 버림 더미 수량이 맞는지
- 끗, 중간 족보, 땡, 13·18·38광땡 판정이 맞는지
- 드로우 더미가 비면 버림 더미가 다시 섞이는지

자동화된 EditMode·PlayMode 테스트는 이번 제출 범위에서 제외한다.

## 네임스페이스

기능 이름을 그대로 사용한다.

- `Hwatu.Cards`
- `Hwatu.Deck`
- `Hwatu.Hands`
- `Hwatu.Combat`
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
