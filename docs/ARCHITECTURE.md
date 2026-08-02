# 아키텍처 원칙

## 문서 목적

이 프로젝트는 2026년 8월 10일 제출을 목표로 하는 짧은 Unity 프로토타입이다. 복잡한 계층 구조보다 기능을 찾고 수정하기 쉬운 구조를 우선한다.

## 기본 방향

- 코드는 `Cards`, `Deck`, `Hands`, `Combat`, `Rewards`, `UI`처럼 기능별로 나눈다.
- 한 기능에 필요한 모델, 로직, Unity 연결 코드는 가능한 한 같은 기능 폴더에 둔다.
- 별도 계층, 인터페이스, 이벤트 버스는 실제로 두 곳 이상에서 필요할 때만 만든다.
- 족보 판정과 피해 계산처럼 독립적으로 확인할 규칙은 `MonoBehaviour`와 분리된 일반 C# 클래스로 작성한다.
- UI는 계산 결과를 표시하며 족보나 피해 공식을 다시 구현하지 않는다.
- 제출 범위에 없는 저장, 네트워크, 범용 프레임워크는 미리 만들지 않는다.

## 폴더 구조

기존 Unity 폴더를 유지하면서 아래 구조로 정리한다.

```text
Assets/
  Images/
  Prefabs/
  Scenes/
  Scripts/
    Cards/
    Deck/
    Hands/
    Combat/
    Rewards/
    UI/
    Common/
  Data/
    Cards/
    Enemies/
    Rewards/
```

### `Scripts/Cards`

- 카드의 월, 종류, 태그 등 기본 정보
- 런 중 카드의 강화 상태
- 카드 데이터와 런타임 카드 사이의 변환

예시:

- `CardDefinition.cs`
- `CardInstance.cs`
- `CardType.cs`

### `Scripts/Deck`

- 드로우 더미, 손패, 버림 더미 관리
- 셔플, 드로우, 제출, 재순환
- 시작 덱 생성

예시:

- `Deck.cs`
- `DeckFactory.cs`
- `IRandomSource.cs`

현재 빈 `Assets/Scripts/Deck.cs`는 덱 기능 구현을 시작할 때 `Assets/Scripts/Deck/Deck.cs`로 이동해 사용한다.

### `Scripts/Hands`

- 두 장의 카드로 섯다 족보 판정
- 족보 이름, 등급, 구성 결과 반환
- 특수 족보와 상성 규칙

예시:

- `HandEvaluator.cs`
- `HandResult.cs`
- `HandType.cs`

족보 판정기는 피해량이나 UI 문자열을 직접 결정하지 않는다.

### `Scripts/Combat`

- 플레이어와 적 체력
- 적의 공개 패와 행동
- 카드 제출, 패 비교, 승자 피해, 무승부와 전투 종료 판정
- 한 전투의 진행 순서

예시:

- `CombatController.cs`
- `CombatState.cs`
- `DamageCalculator.cs`
- `EnemyDefinition.cs`

`CombatController`가 Unity 생명주기와 입력 연결을 맡을 수 있지만, 족보와 피해 계산은 일반 C# 클래스에 위임한다.

### `Scripts/Rewards`

- 전투 후 카드 후보 생성
- 카드 선택과 건너뛰기
- 선택한 카드를 현재 덱에 추가

예시:

- `RewardService.cs`
- `RewardResult.cs`

### `Scripts/UI`

- 카드 선택 표시
- 적 패, 체력, 족보, 피해량 표시
- 버튼 입력을 전투와 보상 기능에 전달
- 최소한의 전투 연출

예시:

- `CardView.cs`
- `CombatView.cs`
- `RewardView.cs`

UI 클래스는 게임 규칙을 계산하지 않는다. 화면 갱신을 위해 필요한 경우 직접 메서드 호출이나 단순 C# 이벤트를 사용한다.

### `Scripts/Common`

둘 이상의 기능에서 실제로 공유하는 작은 코드만 둔다.

- 난수 공급자
- 공통 결과 타입
- 범용 확장 메서드

어느 기능에 둘지 애매하다는 이유만으로 `Common`에 넣지 않는다.

## 기능 사이의 연결

첫 프로토타입은 다음 정도의 직접 참조를 허용한다.

```text
UI → Combat → Deck
            → Hands

UI → Rewards → Deck

Cards ← Deck / Hands / Combat / Rewards
```

- `Combat`은 `Deck`과 `Hands`를 사용해 한 턴을 처리한다.
- `Rewards`는 선택된 카드를 `Deck`에 추가한다.
- `UI`는 `Combat`과 `Rewards`의 공개 메서드를 호출하고 결과를 표시한다.
- 순환 참조가 생기면 책임을 다시 나누되, 이를 예방한다는 이유로 처음부터 여러 추상 계층을 만들지 않는다.

## Unity 데이터 사용

- 카드와 적의 작성용 데이터는 `ScriptableObject`를 사용할 수 있다.
- ScriptableObject는 변하지 않는 원형 데이터만 보관한다.
- 체력, 현재 손패, 강화 단계 등 플레이 중 바뀌는 값은 일반 C# 객체에 둔다.
- 카드와 적은 표시 이름 대신 안정적인 ID로 참조한다.
- 첫 프로토타입에서는 JSON 변환이나 범용 데이터 로더를 만들지 않아도 된다.

## 난수

- 덱 셔플과 보상 생성에는 시드를 지정할 수 있는 난수 공급자를 사용한다.
- 재현이 필요한 디버깅에서는 고정 시드를 사용할 수 있게 한다.
- 난수 공급자 외에는 프레임 시간이나 전역 난수 상태에 의존하지 않는다.
- 난수 구현은 하나만 두고, 필요하기 전까지 별도 전략 계층을 늘리지 않는다.

## 수동 검증

자동화된 EditMode·PlayMode 테스트는 이번 제출 범위에서 제외한다. Unity 에디터에서 다음 흐름을 직접 확인한다.

- 기본 족보와 족보 우선순위
- 광 태그가 필요한 광땡 판정
- 덱 드로우와 버림 더미 재순환
- 제출한 패 2장과 제출하지 않은 패 1장의 턴 종료 후 버림 처리
- 고정 시드 셔플 재현
- 피해 계산 예시
- 패 비교에 따른 승자 피해와 무승부 처리
- 동시 전멸 예외의 게임 오버 처리
- 보상 카드가 덱에 추가되는 흐름

## 네임스페이스

- 이번 프로토타입을 위해 별도 asmdef를 만들지 않는다.
- 네임스페이스는 `Hwatu.Cards`, `Hwatu.Deck`, `Hwatu.Hands`, `Hwatu.Combat`, `Hwatu.Rewards`, `Hwatu.UI`처럼 기능 이름을 사용한다.

## 프로토타입에서 만들지 않는 구조

- Domain/Application/Infrastructure/Presentation 계층 분리
- 기능마다 Repository, Service, UseCase 인터페이스 생성
- 범용 이벤트 버스 또는 메시지 브로커
- 의존성 주입 컨테이너
- 저장 데이터 마이그레이션 체계
- 플러그인형 피해 공식 프레임워크
- Unity Test Framework 기반 자동화 테스트
- 제출 범위 밖 기능을 위한 확장 포인트

필요한 기능을 가장 단순한 형태로 완성하고, 실제 중복이나 변경 요구가 생겼을 때만 구조를 확장한다.
