# 주문서 처리 예제 프로그램
## 본 프로그램을 실행하기 위하여 아래의 세가지 프로그램이 구동되어야 한다.

### pos.order.dummy
  주문 데이터 시뮬레이션 더미 프로젝트. 윈도우 콘솔 어플리케이션 이며 메인 윈도우 어플리케이션 실행시 주문 요청을 시작한다.
  
    1. 주문 테이블은 총 10개 기준으로 하며 주문 주기는 1 ~ 20 초로 랜덤 하게 요청된다.
    2. 주문순서는 1 ~ 10 까지 순차적으로 요청되며 Order Model 에 주문 상태 및 테이블 번호, 주문번호 등의 속성을 가진다. 주문번호가 10번테이블을 초과할경우 다시 1번 테이블부터 주문 요청을 처리한다.
    3. 주문 데이터는 NamedPipe 를 통하여 메인 윈도우 어플리케이션 으로 전송한다.
       
### pos.wpf.winapp
  메인 윈도우 어플리케이션. 주문 더미로부터 주문 요청을 받아 UI 에  내용을 출력하며 상태 정보 업데이트를 서비스 프로세스 측으로 요청하여 처리한다.
  본 프로젝트는 .Net 8.0 WPF 어플리케이션으로 동작하며 MVVM 패턴및 DI 방식을 사용하여 구현되었다. MVVM 패턴은 CommunityToolkit.Mvvm 패키키지를 활용, DI 방식은 CommunityToolkit.Mvvm.DependencyInjection 을 이용하여 앱 시작시 ServiceCollection 에 MainWindowsViewModel 을 Singleton 으로 레지스트리 하고 ObserverbleObject 를 상속받아 구현하였다. 주문상태는 "접수","처리중","완료" 표시되며 테이블의 상태 변경시 ChangeOrderStatusCommand 를 통하여 MainWindowViewModel 에서 처리 하였다.
  
    1. 주문 테이블은 총 10개의 테이블로 세팅되었으며 dummy 단 NamedPipe 로 요청된 주문데이터의 테이블 번호에 해당하는 UI 를 각 테이블별로 1개씩 생성한다.
    2. 주문 테이블 생성시 요청 상태를 표시하며 각 테이블별 주문 상태 정보 변경처리를 worker 프로세스 로 NamedPipe 를 통하여 전송한다.
    3. 주문 데이터 수신된 모든 요청에 대하여 worker 프로세서로 전달한다.
       
### pos.order.worker
  서비스 프로세스. 주문 상태 업데이트 및 요청 로그를 Mongo DB 에 적재한다.
  
    1. 해당 프로젝트는 윈도우 서비스로 동작하다록 Windows.Hosting 을 상속받아 구현되었다.
    2. 본 서비스의 기능 주문 접수 상태 등록/업데이트 및 주문 이력 조회 기능을 수행하도록 구현되었다.
    3. 서비스 실행시 MongoDB Context 객체를 Singleton 으로 등록하고 해당 인스턴스를 사용하며 DB 연결정보는 workersettings.json 설정파일에 작성되었다.
    4. winapp 을 통하여 전달된 주문 데이터를 MongoDB - OrderDatabase - Orders 컬렉션에 적재 및 상태 정보를 업데이트 한다.
    5. winapp 을 통하여 전달된 로그 데이터를 MongoDB - OrderDatabse - Logs 컬렉션에 적재한다.
 
