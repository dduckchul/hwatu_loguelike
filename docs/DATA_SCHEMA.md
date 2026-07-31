# 데이터 스키마 초안

## 원칙

- 아래 이름은 Unity 코드와 콘텐츠가 공유하는 논리 스키마다.
- 작성용 데이터는 ScriptableObject를 우선 검토하고, 저장과 외부 시뮬레이션에는 JSON 호환 DTO를 사용한다.
- 모든 ID는 저장과 참조에 사용되므로 출시 후 임의로 바꾸지 않는다.
- 표시 이름과 설명은 ID와 분리해 현지화 가능하게 만든다.

## 공통 형식

```yaml
schema_version: 1
```

ID 권장 규칙:

```text
card.m01.base
enemy.m02.warbler
hand.ali
counter.ddang_killer
reward.common_cards
```

## 카드 정의

```yaml
id: card.m03.bright
month: 3
variant: bright
tags:
  - bright
base_damage_modifier: 0
upgrade:
  max_level: 1
  damage_modifier_per_level: 2
presentation:
  name_key: card.m03.bright.name
  art_id: hwatu.m03.bright
```

제약:

- `month`: 첫 플레이어블에서는 1~10
- `variant`: 같은 월 패를 구분하는 안정적인 문자열
- `tags`: 등록된 태그만 사용
- 강화 단계는 카드 정의가 아니라 카드 인스턴스에 저장

## 카드 인스턴스

```yaml
instance_id: run-card-0001
definition_id: card.m03.bright
upgrade_level: 0
permanent_modifiers: []
```

## 족보 정의

```yaml
id: hand.ali
rank: 30
matcher:
  months:
    - 1
    - 2
  required_tags: []
damage_tier: middle
presentation:
  name_key: hand.ali.name
```

`rank`는 족보 비교용이며 피해량이 아니다.

## 특수 상성 정의

```yaml
id: counter.ddang_killer
attacker_hand_ids:
  - hand.ddang_killer
target_hand_tags:
  - ddang
resolution:
  negate_target_damage: true
  bonus_damage: 0
priority: 100
```

## 피해 전략 설정

```yaml
id: damage.trailing_digit
kind: trailing_digit
hand_bonuses:
  middle: 5
  ddang: 10
  bright_ddang: 20
minimum_damage: 0
```

후보 전략마다 같은 전투 표본을 실행할 수 있도록 설정과 결과를 분리한다.

## 적 정의

```yaml
id: enemy.m02.warbler
max_hp: 40
intent_pattern_id: intent.m02.basic
passive_ids: []
reward_table_id: reward.common_cards
presentation:
  name_key: enemy.m02.warbler.name
  art_id: monster.m02.warbler
```

## 적 행동 패턴

```yaml
id: intent.m02.basic
mode: cycle
steps:
  - cards:
      - card.m02.base_a
      - card.m07.base
    effect_ids: []
  - cards:
      - card.m01.base
      - card.m02.base_b
    effect_ids: []
```

첫 구현에서는 고정 순환 패턴으로 시작하고, 이후 가중치와 조건 분기를 추가한다.

## 보상 테이블

```yaml
id: reward.common_cards
choices: 3
allow_skip: true
entries:
  - card_pool_id: pool.common
    weight: 100
```

## 런 상태

```yaml
schema_version: 1
run_seed: 123456
current_hp: 60
max_hp: 60
money: 0
stage_index: 0
deck:
  - instance_id: run-card-0001
    definition_id: card.m01.base
    upgrade_level: 0
    permanent_modifiers: []
```

## 초기 태그 목록

카드 태그:

- `bright`
- `ribbon`
- `animal`
- `junk`

족보 결과 태그:

- `middle_hand`
- `ddang`
- `bright_ddang`

카드 태그와 판정 결과 태그를 서로 다른 형식으로 관리한다. 실제 게임 로직에 사용되지 않는 미술 분류 태그는 별도 네임스페이스로 분리한다.
