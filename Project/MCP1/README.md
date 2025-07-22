# WebnoriMemory MCP Server

웹노리메모리(WebnoriMemory)는 Model Context Protocol (MCP)를 기반으로 구현된 로컬 노트 저장 및 검색 시스템입니다. RavenDB를 백엔드로 사용하여 풀텍스트 검색, 공간 검색, 벡터 유사도 검색 기능을 제공합니다.

## MCP (Model Context Protocol) 개념

MCP는 AI 모델과 외부 도구/서비스 간의 표준화된 통신 프로토콜입니다. 주요 특징:

- **표준화된 인터페이스**: AI 모델이 다양한 도구를 일관된 방식으로 사용
- **도구 발견 및 설명**: 도구의 기능을 자동으로 탐색하고 이해
- **타입 안전성**: 강타입 파라미터와 반환값으로 안정적인 통신
- **확장성**: 새로운 도구를 쉽게 추가하고 통합

## RavenDB 특징

RavenDB는 .NET 생태계에 최적화된 NoSQL 문서 데이터베이스입니다:

- **문서 지향**: JSON 형태의 유연한 데이터 모델
- **ACID 트랜잭션**: 데이터 일관성 보장
- **자동 인덱싱**: 쿼리 성능 최적화
- **풀텍스트 검색**: Lucene 기반의 강력한 검색 엔진
- **공간 검색**: 지리적 위치 기반 쿼리
- **벡터 검색**: AI/ML 임베딩을 위한 네이티브 지원
- **분산 아키텍처**: 고가용성과 확장성

## 프로젝트 구조

```
Project/MCP1/
├── WebnoriMemory/
│   ├── Models/
│   │   └── WebnoriNote.cs          # 노트 문서 모델 및 인덱스
│   ├── Repositories/
│   │   └── WebnoriRepository.cs    # RavenDB 데이터 접근 계층
│   ├── Services/
│   │   ├── RavenDbService.cs       # RavenDB 연결 관리
│   │   ├── VectorService.cs        # 벡터 임베딩 (더미 구현)
│   │   └── GeocodingService.cs     # 지오코딩 서비스
│   ├── Tools/
│   │   └── WebnoriMemoryTools.cs   # MCP 도구 구현
│   ├── Program.cs                  # 애플리케이션 진입점
│   └── mcp.json                    # MCP 서버 설정
└── docker-compose.yml              # RavenDB Docker 설정
```

## 주요 기능

### 1. 노트 저장 (SaveMemory)
- 제목과 내용으로 노트 저장
- 내용에서 자동으로 위치 정보 추출 및 위경도 변환
- 제목의 벡터 임베딩 생성 (더미 구현)

### 2. 풀텍스트 검색 (SearchMemory)
- 제목과 내용에서 텍스트 검색
- RavenDB의 Lucene 기반 검색 엔진 활용

### 3. 위치 기반 검색 (SearchNearLocation)
- "창원에 가까운 노트" 같은 자연어 쿼리 지원
- 지정된 위치 반경 내의 노트 검색
- 거리 계산 및 정렬

### 4. 유사 제목 검색 (SearchSimilarTitles)
- 벡터 임베딩을 통한 의미적 유사도 검색
- 코사인 유사도 기반 순위 결정

### 5. 전체 노트 목록 (ListAllMemories)
- 저장된 모든 노트 조회
- 생성 시간 기준 정렬

## 설치 및 실행

### 전제 조건

1. .NET 9.0 SDK
2. Docker 및 Docker Compose
3. MCP 호환 클라이언트 (예: Claude Desktop)

### .NET 설치 (Ubuntu/Debian)

```bash
sudo apt install -y wget apt-transport-https software-properties-common
# Microsoft GPG 키 가져오기
wget https://packages.microsoft.com/keys/microsoft.asc -O- | sudo gpg --dearmor -o /usr/share/keyrings/microsoft.gpg
# Microsoft 패키지 리포지터리 추가 (Debian 12 기준)
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" | sudo tee /etc/apt/sources.list.d/microsoft-dotnet.list
sudo apt update
sudo apt install -y dotnet-sdk-9.0
```

### 실행 방법

1. RavenDB 시작:
```bash
cd Project/MCP1
docker-compose up -d
```

2. 프로젝트 빌드:
```bash
cd WebnoriMemory
dotnet build
```

3. MCP 서버 모드로 실행:
```bash
dotnet run -- --serverMode
```

## 기술적 특징

### 벡터 임베딩 (더미 구현)
- SHA256 해시 기반의 일관된 벡터 생성
- 128차원 정규화 벡터
- 코사인 유사도 계산

### 지오코딩
- 한국 주요 도시 데이터베이스 내장
- 자연어에서 위치 정보 자동 추출
- Haversine 공식을 이용한 거리 계산

### RavenDB 고급 기능 활용
- 커스텀 인덱스로 검색 성능 최적화
- 공간 인덱싱으로 위치 기반 쿼리
- 논블로킹 쿼리로 일관된 결과 보장

## MCP 클라이언트 설정

Claude Desktop 등의 MCP 클라이언트에서 사용하려면 설정에 다음을 추가:

```json
{
  "mcpServers": {
    "webnori-memory": {
      "command": "dotnet",
      "args": ["run", "--", "--serverMode"],
      "cwd": "/path/to/Project/MCP1/WebnoriMemory"
    }
  }
}
```

## 라이선스

이 프로젝트는 학습 및 테스트 목적으로 제작되었습니다.