# DicomViewer

macOS용 DICOM 의료 영상 뷰어 애플리케이션입니다.

## 주요 기능

- **DICOM 파일 열기**: 단일 파일 및 시리즈(CT/MRI) 지원
- **환자/검사 정보 표시**: 이름, ID, 생년월일, 검사일, 모달리티 등
- **Window/Level 조절**: 슬라이더 및 마우스 드래그로 밝기/대비 조절
- **줌/팬**: 마우스 휠 및 드래그로 이미지 확대/이동
- **시리즈 탐색**: 키보드 화살표 또는 마우스 휠로 슬라이스 이동
- **메타데이터 뷰어**: 모든 DICOM 태그 표시

## 기술 스택

- **.NET 9.0**
- **Avalonia UI 11.3** - 크로스플랫폼 UI 프레임워크
- **fo-dicom 5.1** - DICOM 파일 파싱
- **CommunityToolkit.Mvvm** - MVVM 패턴

## 실행 방법

```bash
# 프로젝트 복원
dotnet restore

# 빌드
dotnet build

# 실행
dotnet run
```

## 사용 방법

| 기능 | 조작 방법 |
|------|----------|
| Window/Level 조절 | 마우스 왼쪽 버튼 드래그 |
| 이미지 이동 (팬) | 마우스 가운데 버튼 드래그 |
| 줌 인/아웃 | Ctrl + 마우스 휠 |
| 슬라이스 탐색 | 마우스 휠 또는 ← → 화살표 키 |
| 뷰 리셋 | R 키 |

## 프로젝트 구조

```
DicomViewer/
├── Models/           # 데이터 모델 (Patient, Study, Series, Image)
├── Services/         # 비즈니스 로직 (DICOM 파일 처리, 이미지 렌더링)
├── ViewModels/       # MVVM 뷰모델
├── Views/            # Avalonia UI 뷰
└── Program.cs        # 진입점
```

## 스크린샷

![DicomViewer Screenshot](screenshot.png)

## 라이선스

MIT License
