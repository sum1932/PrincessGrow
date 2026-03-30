"""
디저트 왕국 시뮬레이션 메인 실행 스크립트

사용법:
    python main.py [옵션]

옵션:
    --count N        시뮬레이션 횟수 (기본: 100)
    --batch N        배치 크기 (기본: 10)
    --strategy TYPE  전략 타입 (random/balanced/stress_aware, 기본: random)
    --output DIR     결과 저장 디렉토리 (기본: ./results)

예시:
    python main.py --count 100 --batch 10 --strategy random
"""
import sys
import os
import argparse

# 현재 디렉토리를 Python 경로에 추가
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from src.simulation.simulator import GameSimulator
from src.simulation.report_generator import ReportGenerator
from src.data.csv_parser import get_data_loader


def parse_arguments():
    """명령행 인자 파싱"""
    parser = argparse.ArgumentParser(
        description='디저트 왕국 육성 시뮬레이션',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
예시:
    python main.py                    # 기본 설정 (100회, random 전략)
    python main.py --count 200        # 200회 시뮬레이션
    python main.py --strategy balanced # 균형 잡힌 전략 사용
        """
    )
    
    parser.add_argument(
        '--count',
        type=int,
        default=100,
        help='총 시뮬레이션 횟수 (기본: 100)'
    )
    
    parser.add_argument(
        '--batch',
        type=int,
        default=10,
        help='배치 크기 (기본: 10)'
    )
    
    parser.add_argument(
        '--strategy',
        type=str,
        default='random',
        choices=['random', 'balanced', 'stress_aware'],
        help='시뮬레이션 전략 (기본: random)'
    )
    
    parser.add_argument(
        '--output',
        type=str,
        default=None,
        help='결과 저장 디렉토리 (기본: ./results)'
    )
    
    parser.add_argument(
        '--data-path',
        type=str,
        default=None,
        help='CSV 데이터 경로 (기본: ../01_GameDesign/Data)'
    )
    
    return parser.parse_args()


def print_header():
    """프로그램 헤더 출력"""
    print("=" * 70)
    print("디저트 왕국 육성 시뮬레이션")
    print("=" * 70)
    print()
    print("CSV 데이터 기반 게임 시뮬레이션 프로그램")
    print("Unity 의존성 없이 게임 로직 테스트")
    print()
    print("-" * 70)


def print_configuration(args):
    """설정 출력"""
    print()
    print("설정:")
    print(f"  - 시뮬레이션 횟수: {args.count}회")
    print(f"  - 배치 크기: {args.batch}회")
    print(f"  - 전략: {args.strategy}")
    print(f"  - 출력 디렉토리: {args.output or './results'}")
    if args.data_path:
        print(f"  - 데이터 경로: {args.data_path}")
    print()


def main():
    """메인 실행 함수"""
    args = parse_arguments()
    
    print_header()
    print_configuration(args)
    
    # 데이터 로드
    print("데이터 로드 중...")
    try:
        data_loader = get_data_loader(args.data_path)
        print(f"  [OK] 캐릭터: {len(data_loader.characters)}개")
        print(f"  [OK] 스탯: {len(data_loader.stats)}개")
        print(f"  [OK] 이벤트: {len(data_loader.events)}개")
        print(f"  [OK] 엔딩: {len(data_loader.endings)}개")
        print(f"  [OK] 퀘스트: {len(data_loader.quests)}개")
        print(f"  [OK] 활동: {len(data_loader.activities)}개")
    except Exception as e:
        print(f"\n[ERROR] 데이터 로드 실패: {e}")
        print("데이터 경로를 확인해주세요.")
        sys.exit(1)
    
    print()
    
    # 시뮬레이터 생성
    print("시뮬레이션 준비 중...")
    simulator = GameSimulator(data_loader)
    
    # 시뮬레이션 실행
    print(f"\n{args.count}회 시뮬레이션 시작...")
    print("=" * 70)
    
    try:
        report = simulator.run_batch_simulations(
            total_simulations=args.count,
            batch_size=args.batch,
            strategy_type=args.strategy
        )
    except KeyboardInterrupt:
        print("\n\n[WARNING] 시뮬레이션이 사용자에 의해 중단되었습니다.")
        sys.exit(0)
    except Exception as e:
        print(f"\n[ERROR] 시뮬레이션 실패: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
    
    print("\n" + "=" * 70)
    print("[OK] 시뮬레이션 완료!")
    
    # 결과 요약 출력
    print()
    print("결과 요약:")
    print(f"  - 실행 시간: {report.execution_time:.2f}초")
    print(f"  - 평균 총 스탯: {report.overall_statistics.get('avg_total_stats', 0):.1f}")
    print(f"  - 평균 스트레스: {report.overall_statistics.get('avg_stress', 0):.1f}")
    print(f"  - 엔딩 성공률: {report.overall_statistics.get('success_rate', 0):.1f}%")
    
    # 엔딩 분포 출력
    if report.ending_distribution:
        print()
        print("엔딩 분포:")
        for ending_name, count in sorted(
            report.ending_distribution.items(),
            key=lambda x: x[1],
            reverse=True
        ):
            percentage = (count / report.total_simulations * 100)
            print(f"  - {ending_name}: {count}회 ({percentage:.1f}%)")
    
    # 보고서 생성
    print()
    print("보고서 생성 중...")
    report_generator = ReportGenerator(args.output)
    
    try:
        # Markdown 보고서
        md_path = report_generator.generate_full_report(report)
        
        # CSV 요약
        csv_path = report_generator.generate_summary_csv(report)
        
        print()
        print("=" * 70)
        print("모든 작업이 완료되었습니다!")
        print()
        print("생성된 파일:")
        print(f"  - Markdown 보고서: {md_path}")
        print(f"  - CSV 요약: {csv_path}")
        
    except Exception as e:
        print(f"\n[ERROR] 보고서 생성 실패: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
    
    print()
    print("=" * 70)


if __name__ == '__main__':
    main()
