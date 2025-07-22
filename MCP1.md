
나만의 로컬 MCP 서버를 만들고자 합니다. Project/MCP1 하위폴더에 생성해주세요

# MCP 규칙
- 지금까지 내용을 프롬프트와 함께 요약하고 웹노리메모리에 기록해하면~ LLM이 요약한 내용을 웹노리메모리에 기록합니다.
- 웹노리메모리에서 검색만 요청하면, LLM이 웹노리메모리에서 검색한 내용을 요약하여 응답합니다. FullTextSearch를 이용합니다.
- 내용중 지역정보가 있다고하면 위경도로 변환하여 추가 메타정보로 저장합니다.
- 웹노리메모리에서 "창원에 가까운 노트" 검색요청하면, 위경도 정보를 이용하여 검색합니다.
- 프롬프트는 Title로 별도 저장합니다. 이 제목은 추가로 벡터값으로 임베디드하여 저장합니다. 추후 외부 AI-API이용예정으로, 더미모드로 구현합니다.
- 웹노리메모리에서 개발잘하는방법 "유사검색" 요청하면, 벡터값 임베디드된 제목을 이용하여 유사검색합니다.
- 한글프롬프트로 기본 작동합니다.


# 생성및 테스트지침
- dotnet build 로 먼저 빌드가 되는지 확인합니다.
- dotnet run -- --serverMode 수행해서 런타임으로 실행되는지 확인한후 종료합니다.
- 빌드및 실행이 완료되면 README.md 파일을 작성하여 프로젝트 개요와 사용법을 설명합니다. MCP컨셉과 RavenDB의 특징도 함께 기술합니다.

# 인프라 생성및 규칙
- MCP 서버는 .net을 이용합니다.
- RavenDB를 이용하여 웹노리메모리를 저장하며 도커로 구동합니다.
- RavenDB는 9000 포트로 구동하며, DB 이름은 "WebnoriMemory"로 생성합니다.


# 참고규칙
- MCP 구현방법포함 RavenDB와 이것을 활용하는 방법은, Sample 하위 디렉토리 프로젝트 내용을 학습후 참고합니다. *.cs 파일을 모두 확인합니다.
- 의존 모듈버전은 필요한경우 여기서 사용한버전과 동일한 버전을 사용합니다.


# 닷넷설치가이드
- dotnet이 없는경우 다음을 참고해서 설치

```
sudo apt install -y wget apt-transport-https software-properties-common
# Microsoft GPG 키 가져오기
wget https://packages.microsoft.com/keys/microsoft.asc -O- | sudo gpg --dearmor -o /usr/share/keyrings/microsoft.gpg
# Microsoft 패키지 리포지터리 추가 (Debian 12 기준)
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" | sudo tee /etc/apt/sources.list.d/microsoft-dotnet.list
sudo apt update
sudo apt install -y dotnet-sdk-9.0
```

# RavenDB 도커가이드

RavenDB를 구동은 다음명령을 이용~ 퍼시던트나 네트워크설정없이 로컬 StandAlone 모드로 구동해 활용할거임

```
version: '3.8'

services:
  ravendb:
    image: ravendb/ravendb:latest
    container_name: webnori-ravendb
    ports:
      - "9000:8080"
      - "38888:38888"
    environment:
      - RAVEN_Setup_Mode=None
      - RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork      
      - RAVEN_License_Eula_Accepted=true    
```