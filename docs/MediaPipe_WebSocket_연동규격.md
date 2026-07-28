# MediaPipe WebSocket 연동 규격

## 목적
- MediaPipe 기반 외부 인식 시스템이 Unity MVP에 모션 결과를 전달할 수 있도록 공통 메시지 형식을 정의한다.

## 권장 엔드포인트
- 기본 URL 예시: `ws://127.0.0.1:8765`

## 권장 메시지 형식
```json
{
  "type": "motion_result",
  "motion": "raise_both_hands",
  "confidence": 0.92,
  "timestamp": 1722052800.25,
  "status": "MediaPipe tracking OK"
}
```

## 필드 설명
- `type`
  - 현재는 `motion_result`를 사용
- `motion`
  - Unity 내부 동작 타입으로 변환할 외부 동작 이름
- `confidence`
  - 0.0 ~ 1.0 범위 권장
- `timestamp`
  - 초 단위 시간값 권장
- `status`
  - 선택값
  - 추적 상태, 연결 상태, 오류 메시지 등을 UI에 표시하는 용도

## 현재 매핑 규칙
- `raise_both_hands`
- `raisebothhands`
- `hands_up`
  - Unity: `RaiseBothHands`

- `reach_left`
- `left_reach`
- `move_left`
  - Unity: `ReachLeft`

- `reach_right`
- `right_reach`
- `move_right`
  - Unity: `ReachRight`

## 권장 처리 흐름
1. MediaPipe에서 포즈 또는 제스처 인식
2. 외부 애플리케이션에서 동작 판정
3. Unity로 `motion_result` 이벤트 전송
4. Unity에서 `MotionActionType`으로 매핑
5. Calibration 또는 Gameplay에서 동일 로직으로 소비

## 주의사항
- MVP 단계에서는 Unity 내부에서 원시 랜드마크를 직접 처리하지 않는다.
- 초기에는 판정 완료 이벤트만 전달하는 것이 안정적이다.
- 연속 프레임 전송보다 의미 있는 동작 이벤트 전송을 우선한다.
