# CodingTest_Tetris

### 테트리스

* 이번 테트리스 과제에서 기본적인 테트리스 로직은 최대한 코드로만 구현할 수 있도록 했습니다.


* 조작
  * 이동 : ◀, 🔽, ▶ 방향키
  * 회전 : Z, X
  * 즉시 내림 : Space
  * 다시 시작 : R(홀드)
  * 홀드 : C
  * LShift : 아이템 사용


* 테트리스 룰
  * 7 Bag : 7번의 테트리미노가 나오는 동안 모두 다른 테트로미노가 나오도록 설정했습니다.
  * Hold : 현재의 테트로미노를 저장해 둘 수 있는 기능
  * Infinity : 더이상 내려갈 수 없는 상황에서 움직이면 바로 보드에 붙지 않는 기능
  * Ghost : 테트로미노가 떨어질 위치를 보여줍니다.
  * Next : 다음 나올 테트로미노를 알려줍니다.

* 아이템
  * 획득 방법 : 아이템은 테트로미노가 생성될 때 10%의 확률로 테트로미노의 4개의 블럭 중 하나에 생성, 홀드 또는 그 블럭을 제거할 경우 얻을 수 있습니다.
  * 종류 : 폭탄
  * 폭탄 : 가장 아래 줄을 삭제합니다.

* 오브젝트 풀링
  * Hold, Next 의 테트로미노는 좌표를 맞추기 위해 오브젝트 풀링 방식으로 생성한 뒤 활성/비활성화를 바꾸면서 보여주는 방법을 사용했습니다.

* 테트로미노 생성
  * 테트로미노 생성은 hold 혹은 보드에 붙을 때마다 새로 Instatiate하는 방식으로 구현했습니다.
