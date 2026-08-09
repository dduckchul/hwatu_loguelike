# Hwatu LogueLike

화투패로 덱을 구성하고 손패 3장 중 2장을 내어, 공개된 적의 패와 섯다 족보를 겨루는 싱글 플레이 로그라이트 덱빌딩 웹게임입니다.

## 현재 플레이 흐름

```text
전투 1 → 귀시장 → 전투 2 → 귀시장 → 전투 3 → 런 완료 → 타이틀
```

- 1~10월 Normal 카드 한 장씩으로 시작하는 10장 덱
- 손패 3장 중 2장 제출, 전투당 한 번 1장 교체
- 적의 패 2장 공개, 족보 비교, 피해와 전(錢) 이전
- 전투 회차마다 `5전`씩 증가하는 기본 판돈
- 전투 승리 후 같은 씬에서 귀시장으로 전환
- 카드 후보 3장 즉시 구매: `0전 → 20전 → 40전`
- 귀시장 종료 시 현재 런 덱으로 전투 덱을 다시 만들고 다음 적 로드
- 전투 화면 Fade-In 완료 후 새 손패 3장 드로우
- 방문당 한 번, `15전` 카드 강화와 `20전` 카드 제거
- 마지막 승리 시 `To Be Continued..`, 패배 시 `사망` 표시 후 타이틀 복귀
- 상점 결제 후 최소 `1전` 유지

현재 `StageEncounterData`와 적 프리팹 방식으로 1~3스테이지가 연결되어 있으며, 3스테이지에는 적 두 명이 등장합니다.

## 남은 제출 작업

- 적 능력치·패턴 밸런싱
- 타이틀 화면에서 새 런 초기화 연결
- 상점 버튼과 실제 결제의 정확한 비용 경계 통일
- 전투·상점 연출과 조작감 보완
- WebGL 빌드 및 브라우저 전체 흐름 검증

12월 비광 보스, 유물, 아이템, 분기형 맵과 저장 기능은 현재 제출 범위에서 제외한 백로그입니다.

## 개발 환경

- Unity `6000.3.21f1`
- Unity 2D / Universal Render Pipeline
- 대상 플랫폼: WebGL
- 제출 목표일: 2026년 8월 10일

## 문서

- [프로젝트와 세계관](docs/PROJECT_OVERVIEW.md)
- [게임 규칙](docs/GAMEPLAY.md)
- [코드 구조](docs/ARCHITECTURE.md)
- [데이터 구조](docs/DATA_SCHEMA.md)
- [구현 계획](docs/IMPLEMENTATION_PLAN.md)
- [상점 밸런스 검증](docs/SHOP_BALANCE_SIMULATION.md)
- [아트 가이드](docs/ART_GUIDE.md)
- [에이전트 작업 지침](AGENTS.md)

## 협업 환경 설정

- Windows: `git config --global core.autocrlf true`
- macOS: `git config --global core.autocrlf input`
