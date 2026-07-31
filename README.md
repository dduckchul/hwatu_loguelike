# Hwatu LogueLike

화투패로 덱을 구성하고, 손패 3장 중 2장을 내어 섯다 족보와 적의 공개 패를 활용해 싸우는 싱글 플레이 로그라이트 덱빌딩 게임입니다.

## 게임 개요

- **장르:** 섯다 기반 로그라이트 덱빌딩
- **전투:** 매 턴 3장의 손패에서 2장을 선택하고, 공개된 적의 패와 동시에 승부합니다.
- **성장:** 전투 보상으로 같은 월의 패를 추가해 땡과 새로운 족보를 만들거나, 카드를 강화해 덱을 성장시킵니다.
- **콘셉트:** 화투에 등장하는 동식물을 일반 적으로, 12월 비광을 첫 보스로 재해석합니다.
- **아트 방향:** 저채도 청회색과 먹 번짐을 사용하는 동양 수묵화풍 배경을 지향합니다.

첫 번째 플레이어블은 `일반 전투 3회 → 휴식/카드 강화 → 12월 비광 보스`로 구성된 짧은 런을 목표로 합니다. 피해 공식과 일부 세부 규칙은 프로토타입과 시뮬레이션을 통해 확정할 예정입니다.

## 개발 환경

- Unity `6000.3.21f1`
- Unity 2D / Universal Render Pipeline
- 1차 대상 플랫폼: WebGL
- 테스트: Unity Test Framework

## 프로젝트 문서

- [게임플레이 명세](docs/GAMEPLAY.md)
- [아키텍처 원칙](docs/ARCHITECTURE.md)
- [데이터 스키마](docs/DATA_SCHEMA.md)
- [아트 가이드](docs/ART_GUIDE.md)
- [구현 계획](docs/IMPLEMENTATION_PLAN.md)
- [에이전트 작업 지침](AGENTS.md)

## 프로젝트 초기 세팅

### 1. 유니티 설치

* Unity `6000.3.21f1` 버전
* Unity 2D 사용
* 웹 빌드용 WebGL 모듈 설치

### 2. 깃 설정 - 윈도우 & 맥 협업용

### 깃의 개행문자 옵션 맞추기 (터미널 or CMD 실행)

* [깃 개행문자 설정 참고자료](https://dsaint31.tistory.com/209)

#### 윈도 쓰는사람 설정

> git config --global core.autocrlf true

#### 맥 쓰는 사람 설정

> git config --global core.autocrlf input
