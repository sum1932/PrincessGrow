"""
시뮬레이션 결과 보고서 생성기 - MD 파일 출력
"""
import os
from typing import List, Dict, Any, Optional
from datetime import datetime

import sys
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from simulation.simulator import FullSimulationReport, SimulationBatchResult, SingleSimulationResult


class ReportGenerator:
    """시뮬레이션 결과 보고서 생성기"""
    
    def __init__(self, output_dir: Optional[str] = None):
        if output_dir is None:
            # 기본 결과 디렉토리
            self.output_dir = os.path.join(
                os.path.dirname(os.path.dirname(os.path.dirname(__file__))),
                'results'
            )
        else:
            self.output_dir = output_dir
            
        # 결과 디렉토리 생성
        os.makedirs(self.output_dir, exist_ok=True)
    
    def generate_full_report(self, report: FullSimulationReport, filename: Optional[str] = None):
        """전체 보고서 생성"""
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"simulation_report_{timestamp}.md"
        
        filepath = os.path.join(self.output_dir, filename)
        
        content = self._generate_markdown(report)
        
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print(f"\n[OK] 보고서 저장 완료: {filepath}")
        return filepath
    
    def _generate_markdown(self, report: FullSimulationReport) -> str:
        """Markdown 형식 보고서 생성"""
        lines = []
        
        # 헤더
        lines.append("# 🎮 디저트 왕국 시뮬레이션 결과 보고서")
        lines.append("")
        lines.append(f"**생성 일시:** {report.timestamp}")
        lines.append(f"**데이터 소스:** `{report.data_source}`")
        lines.append(f"**총 실행 시간:** {report.execution_time:.2f}초")
        lines.append("")
        
        # 개요
        lines.append("---")
        lines.append("")
        lines.append("## 📊 실행 개요")
        lines.append("")
        lines.append(f"| 항목 | 값 |")
        lines.append(f"|------|-----|")
        lines.append(f"| 총 시뮬레이션 횟수 | {report.total_simulations}회 |")
        lines.append(f"| 배치 크기 | {report.batch_size}회 |")
        lines.append(f"| 배치 수 | {len(report.batches)}개 |")
        lines.append("")
        
        # 전체 통계
        lines.append("---")
        lines.append("")
        lines.append("## 📈 전체 통계")
        lines.append("")
        
        stats = report.overall_statistics
        lines.append(f"| 통계 항목 | 값 |")
        lines.append(f"|----------|-----|")
        lines.append(f"| 성공적인 엔딩 | {stats.get('successful_endings', 0)}개 |")
        lines.append(f"| 성공률 | {stats.get('success_rate', 0):.1f}% |")
        lines.append(f"| 평균 총 스탯 | {stats.get('avg_total_stats', 0):.1f} |")
        lines.append(f"| 평균 스트레스 | {stats.get('avg_stress', 0):.1f} |")
        lines.append(f"| 평균 소지금 | {stats.get('avg_money', 0):.0f} 스위트 |")
        lines.append(f"| 평균 진행 턴 | {stats.get('avg_turns', 0):.1f}턴 |")
        lines.append(f"| 최고 총 스탯 | {stats.get('max_total_stats', 0)} |")
        lines.append(f"| 최저 총 스탯 | {stats.get('min_total_stats', 0)} |")
        lines.append("")
        
        # 엔딩 분포
        lines.append("---")
        lines.append("")
        lines.append("## 🏆 엔딩 분포")
        lines.append("")
        
        if report.ending_distribution:
            lines.append("| 엔딩 | 횟수 | 비율 |")
            lines.append("|------|------|------|")
            
            total = sum(report.ending_distribution.values())
            # 비율 높은 순으로 정렬
            sorted_endings = sorted(
                report.ending_distribution.items(),
                key=lambda x: x[1],
                reverse=True
            )
            
            for ending_name, count in sorted_endings:
                percentage = (count / total * 100) if total > 0 else 0
                lines.append(f"| {ending_name} | {count} | {percentage:.1f}% |")
        else:
            lines.append("*엔딩 데이터 없음*")
        
        lines.append("")
        
        # 배치별 상세 결과
        lines.append("---")
        lines.append("")
        lines.append("## 📋 배치별 상세 결과")
        lines.append("")
        lines.append("각 배치는 10회의 시뮬레이션 결과를 포함합니다.")
        lines.append("")
        
        for batch in report.batches:
            lines.append(self._generate_batch_section(batch))
        
        # 요약
        lines.append("---")
        lines.append("")
        lines.append("## 📝 요약")
        lines.append("")
        lines.append("### 주요 발견 사항")
        lines.append("")
        
        # 가장 많이 달성된 엔딩
        if report.ending_distribution:
            top_ending = max(report.ending_distribution.items(), key=lambda x: x[1])
            lines.append(f"- **가장 많이 달성된 엔딩:** {top_ending[0]} ({top_ending[1]}회)")
        
        # 평균 성능
        lines.append(f"- **평균 총 스탯:** {stats.get('avg_total_stats', 0):.1f}점")
        lines.append(f"- **평균 스트레스:** {stats.get('avg_stress', 0):.1f}점")
        lines.append(f"- **엔딩 성공률:** {stats.get('success_rate', 0):.1f}%")
        lines.append("")
        
        # 데이터 출처
        lines.append("---")
        lines.append("")
        lines.append("## 📂 데이터 출처")
        lines.append("")
        lines.append(f"- **캐릭터 데이터:** `Characters.csv`")
        lines.append(f"- **스탯 데이터:** `Character_Stats.csv`")
        lines.append(f"- **이벤트 데이터:** `Events.csv`")
        lines.append(f"- **퀘스트 데이터:** `Quests.csv`")
        lines.append(f"- **엔딩 조건:** `Ending_Conditions.csv`")
        lines.append("")
        lines.append("---")
        lines.append("")
        lines.append("*이 보고서는 자동 생성되었습니다.*")
        lines.append("")
        
        return "\n".join(lines)
    
    def _generate_batch_section(self, batch: SimulationBatchResult) -> str:
        """배치 섹션 생성"""
        lines = []
        
        lines.append(f"### 배치 {batch.batch_number} (시뮬레이션 {batch.start_sim} ~ {batch.end_sim})")
        lines.append("")
        
        # 배치 통계
        stats = batch.statistics
        lines.append("#### 배치 통계")
        lines.append("")
        lines.append(f"| 항목 | 값 |")
        lines.append(f"|------|-----|")
        lines.append(f"| 시뮬레이션 수 | {stats.get('count', 0)} |")
        lines.append(f"| 성공 수 | {stats.get('success_count', 0)} |")
        lines.append(f"| 성공률 | {stats.get('success_rate', 0):.1f}% |")
        lines.append(f"| 평균 총 스탯 | {stats.get('avg_total_stats', 0):.1f} |")
        lines.append(f"| 평균 스트레스 | {stats.get('avg_stress', 0):.1f} |")
        lines.append(f"| 평균 소지금 | {stats.get('avg_money', 0):.0f} |")
        lines.append("")
        
        # 엔딩 분포
        ending_dist = stats.get('ending_types', {})
        if ending_dist:
            lines.append("#### 엔딩 달성 현황")
            lines.append("")
            lines.append("| 엔딩 | 횟수 |")
            lines.append("|------|------|")
            for ending_name, count in ending_dist.items():
                lines.append(f"| {ending_name} | {count} |")
            lines.append("")
        
        # 개별 시뮬레이션 결과
        lines.append("#### 개별 결과")
        lines.append("")
        lines.append("| # | 엔딩 | 총 스탯 | 스트레스 | 소지금 | 이벤트 수 |")
        lines.append("|---|------|---------|----------|--------|----------|")
        
        for result in batch.results:
            total_stats = sum(result.final_stats.values())
            event_count = len(result.triggered_events)
            lines.append(
                f"| {result.simulation_number} | {result.ending_name} | "
                f"{total_stats} | {result.final_stress} | "
                f"{result.final_money} | {event_count} |"
            )
        
        lines.append("")
        
        # 상세 정보 (첫 3개만)
        lines.append("#### 상세 정보 (대표 샘플)")
        lines.append("")
        
        for i, result in enumerate(batch.results[:3]):
            lines.append(f"**시뮬레이션 #{result.simulation_number}**")
            lines.append("")
            lines.append(f"- **엔딩:** {result.ending_name}")
            lines.append(f"- **최종 나이:** {result.final_age}세")
            lines.append(f"- **진행 턴:** {result.total_turns}턴")
            lines.append("")
            lines.append("**최종 스탯:**")
            lines.append("")
            lines.append("| 스탯 | 값 |")
            lines.append("|------|-----|")
            for stat, value in result.final_stats.items():
                lines.append(f"| {stat} | {value} |")
            lines.append("")
            
            lines.append("**NPC 호감도:**")
            lines.append("")
            lines.append("| NPC | 호감도 |")
            lines.append("|-----|--------|")
            for npc_id, favor in result.npc_favors.items():
                lines.append(f"| {npc_id} | {favor} |")
            lines.append("")
            
            if result.triggered_events:
                lines.append(f"**발생 이벤트 ({len(result.triggered_events)}개):**")
                lines.append("")
                for event in result.triggered_events[:10]:  # 최대 10개만 표시
                    lines.append(f"- {event}")
                if len(result.triggered_events) > 10:
                    lines.append(f"- ... 외 {len(result.triggered_events) - 10}개")
                lines.append("")
        
        lines.append("")
        lines.append("---")
        lines.append("")
        
        return "\n".join(lines)
    
    def generate_summary_csv(self, report: FullSimulationReport, filename: Optional[str] = None):
        """요약 CSV 생성"""
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"simulation_summary_{timestamp}.csv"
        
        filepath = os.path.join(self.output_dir, filename)
        
        import csv
        
        with open(filepath, 'w', newline='', encoding='utf-8-sig') as f:
            writer = csv.writer(f)
            
            # 헤더
            writer.writerow([
                'Simulation', 'Batch', 'Ending', 'TotalStats', 'Stress', 
                'Money', 'Age', 'Turns', 'HP', 'CHARM', 'INT', 'ART', 'MORALITY',
                'Ino_Favor', 'Aileen_Favor', 'Kyle_Favor', 'Lian_Favor',
                'Events_Count', 'Success'
            ])
            
            # 데이터
            for batch in report.batches:
                for result in batch.results:
                    total_stats = sum(result.final_stats.values())
                    writer.writerow([
                        result.simulation_number,
                        batch.batch_number,
                        result.ending_name,
                        total_stats,
                        result.final_stress,
                        result.final_money,
                        result.final_age,
                        result.total_turns,
                        result.final_stats.get('HP', 0),
                        result.final_stats.get('CHARM', 0),
                        result.final_stats.get('INT', 0),
                        result.final_stats.get('ART', 0),
                        result.final_stats.get('MORALITY', 0),
                        result.npc_favors.get('Char_Ino', 0),
                        result.npc_favors.get('Char_Aileen', 0),
                        result.npc_favors.get('Char_Kyle', 0),
                        result.npc_favors.get('Char_Lian', 0),
                        len(result.triggered_events),
                        'Yes' if result.is_success else 'No'
                    ])
        
        print(f"[OK] CSV 요약 저장 완료: {filepath}")
        return filepath
