# 프로젝트 개요

화투패로 덱을 구성하고, 손패 3장 중 2장을 내어 섯다 족보와 적의 공개 패를 활용해 싸우는 싱글 플레이 로그라이트 덱빌딩 게임 프로젝트다.

## 현재 단계

현재는 Unity 프로토타입 구현 준비 단계다. 첫 목표는 일반 전투 3회와 12월 비광 보스로 구성된 짧은 플레이어블을 만드는 것이다.

확정된 개발 환경:

- Unity `6000.3.21f1`
- Unity 2D / Universal Render Pipeline
- WebGL

플레이어의 카드 교체 기회는 전투당 1회로 확정했다.

기본 피해 공식은 구현 전에 고정하지 않고 세 후보를 시뮬레이션해 비교한다.

## 관련 문서

- [에이전트 작업 지침](../AGENTS.md)
- [게임플레이 명세](GAMEPLAY.md)
- [아키텍처 원칙](ARCHITECTURE.md)
- [데이터 스키마](DATA_SCHEMA.md)
- [아트 가이드](ART_GUIDE.md)
- [구현 계획](IMPLEMENTATION_PLAN.md)
- [피해 공식 결정 기록](decisions/0001-damage-formula.md)

## 다음 작업

`IMPLEMENTATION_PLAN.md`의 마일스톤 0에 따라 기능별 폴더를 준비하고 첫 전투 흐름을 구현한다.
