# Excel Converter Module

Excel 파일을 Unity ScriptableObject로 변환하는 범용 모듈입니다.
ExcelDataReader를 사용하여 리플렉션 기반으로 동작합니다.

## 📁 폴더 구조

```
Assets/
├── Editor/
│   └── ExcelConverter/              # 모듈 폴더
│       ├── Attributes/              # Attribute 정의
│       │   ├── ExcelIdAttribute.cs
│       │   ├── ExcelIgnoreAttribute.cs
│       │   └── ExcelColumnAttribute.cs
│       ├── Core/                    # 핵심 로직
│       │   ├── ExcelDataConverter.cs
│       │   ├── ExcelSheetReader.cs
│       │   ├── TypeConverter.cs
│       │   ├── ExcelFileInfo.cs
│       │   └── FieldMapping.cs
│       └── Editor/                  # Editor 툴
│           └── ExcelConverterWindow.cs
├── Scripts/Data/                    # 샘플 데이터 클래스
└── Prefabs/                         # GameData.asset 생성 위치
```

## 📦 필수 패키지 설치

ExcelDataReader 패키지를 설치해야 합니다:

### 방법 1: NuGet for Unity 사용 (권장)
1. Unity Asset Store 또는 GitHub에서 NuGet for Unity 설치
2. Unity 메뉴: `NuGet` → `Manage NuGet Packages`
3. 검색: `ExcelDataReader`
4. `ExcelDataReader` 및 `ExcelDataReader.DataSet` 설치

### 방법 2: DLL 직접 다운로드
1. https://www.nuget.org/packages/ExcelDataReader/ 접속
2. "Download package" 클릭
3. `.nupkg` 파일을 `.zip`로 변경 후 압축 해제
4. `lib/netstandard2.0/ExcelDataReader.dll`을 `Assets/Plugins/ExcelDataReader/`에 복사

## 🚀 사용 방법

### 1. 데이터 클래스 생성

```csharp
using System;
using ExcelConverter.Attributes;

[Serializable]
public class MyData
{
    [ExcelId]  // 반드시 하나의 필드에 적용 (PK 역할)
    public string ID;
    
    public string Name;        // 자동 매핑: Excel 헤더 "Name"
    public int Value;          // 자동 매핑: Excel 헤더 "Value"
    
    [ExcelColumn("Description Text")]  // 컬럼명이 다를 때
    public string Description;
    
    [ExcelIgnore]  // 변환에서 제외
    public float RuntimeValue;
}
```

### 2. GameData ScriptableObject 생성

```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game Data/GameData")]
public class GameData : ScriptableObject
{
    // Converter가 자동으로 List<T> 필드를 찾아 업데이트합니다
    public List<MyData> myDataList = new List<MyData>();
}
```

### 3. Excel 파일 준비

| ID | Name | Value | Description Text |
|----|------|-------|------------------|
| 001 | 아이템1 | 100 | 첫 번째 아이템 |
| 002 | 아이템2 | 200 | 두 번째 아이템 |

### 4. 변환 실행

1. Unity 메뉴: `Tools` → `Excel Converter`
2. 경로 설정:
   - Excel 폴더: `Assets/ExcelFiles/`
   - GameData 경로: `Assets/Prefabs/GameData.asset`
3. 변환 설정 추가:
   - 데이터 타입: `MyData`
   - Excel 파일: `MyData.xlsx`
   - 시트명: `Sheet1`
4. `전체 변환` 버튼 클릭

## 📋 Attribute 설명

- `[ExcelId]`: ID(PK) 필드 지정 (필수)
- `[ExcelIgnore]`: 변환에서 제외
- `[ExcelColumn("name")]`: 명시적 매핑

## 🔧 지원 타입

- `string`, `int`, `float`, `double`, `long`
- `bool` (true/false, 1/0, yes/no, y/n)
- `Enum` (문자열 기반, 대소문자 무시)

## 📝 Excel 규칙

1. 첫 번째 행: 헤더 (필드명과 동일하거나 `[ExcelColumn]`로 매핑)
2. 두 번째 행부터: 데이터
3. 빈 셀: 기본값 (0, false, "", null)

## ⚠️ 주의사항

1. `[ExcelId]`가 없는 클래스는 변환되지 않습니다
2. Excel 파일이 열려있으면 읽기가 실패할 수 있습니다
3. Git에 Excel 파일을 포함하려면 `.gitignore`에서 제외하세요
