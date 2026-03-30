// API 기본 URL
const API_BASE = window.location.origin;

// 게임 상태
let sessionId = null;
let currentState = null;
let isProcessing = false;
let currentDialogue = null;

// DOM 요소
const screens = {
  start: document.getElementById('start-screen'),
  game: document.getElementById('game-screen'),
  ending: document.getElementById('ending-screen')
};

// 시작 버튼
document.getElementById('btn-start').addEventListener('click', startGame);
document.getElementById('btn-restart').addEventListener('click', restartGame);
document.getElementById('btn-dialogue-next').addEventListener('click', advanceDialogue);
document.getElementById('btn-dialogue-skip').addEventListener('click', skipDialogue);

// 게임 시작
async function startGame() {
  try {
    showLoading();
    
    const response = await fetch(`${API_BASE}/api/start`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const data = await response.json();
    
    if (data.success) {
      sessionId = data.sessionId;
      currentState = data.state;
      updateUI();
      showScreen('game');
      addLogEntry('system', '게임을 시작합니다. 루아의 육성을 시작하세요!');
    } else {
      showError(data.error);
    }
  } catch (error) {
    showError('서버 연결 실패: ' + error.message);
  }
}

// 게임 다시 시작
function restartGame() {
  sessionId = null;
  currentState = null;
  document.getElementById('event-log').innerHTML = '';
  showScreen('start');
}

// 화면 전환
function showScreen(screenName) {
  Object.values(screens).forEach(screen => screen.classList.remove('active'));
  screens[screenName].classList.add('active');
}

// 상태 업데이트
function updateUI() {
  if (!currentState) return;
  
  // 기본 정보
  document.getElementById('age').textContent = currentState.age;
  document.getElementById('month').textContent = currentState.month;
  document.getElementById('turn').textContent = currentState.turn;
  document.getElementById('sweets').textContent = currentState.sweets;
  
  // 스탯
  const stats = ['HP', 'Charm', 'Int', 'Art', 'Morality', 'Stress'];
  const statNames = {
    'HP': 'hp',
    'Charm': 'charm',
    'Int': 'int',
    'Art': 'art',
    'Morality': 'morality',
    'Stress': 'stress'
  };
  
  stats.forEach(stat => {
    const value = currentState.stats[stat];
    const element = document.getElementById(`stat-${statNames[stat]}`);
    const bar = document.getElementById(`bar-${statNames[stat]}`);
    
    if (element) element.textContent = value;
    if (bar) bar.style.width = `${Math.min(100, (value / 999) * 100)}%`;
  });
  
  // NPC 호감도
  const npcs = ['Ino', 'Aileen', 'Kyle', 'Lian'];
  npcs.forEach(npc => {
    const value = currentState.favor[npc] || 0;
    const element = document.getElementById(`favor-${npc.toLowerCase()}`);
    const bar = document.getElementById(`bar-${npc.toLowerCase()}`);
    
    if (element) element.textContent = value;
    if (bar) {
      const max = npc === 'Lian' ? 180 : 200;
      bar.style.width = `${Math.min(100, (value / max) * 100)}%`;
    }
  });
  
  // 리안 상태
  const lianStatus = document.getElementById('lian-status');
  if (lianStatus) {
    if (currentState.age >= 13) {
      lianStatus.textContent = '';
    } else {
      lianStatus.textContent = `(${(13 - currentState.age) * 12 - currentState.month + 1}개월 후 등장)`;
    }
  }
  
  // 활동 목록 업데이트
  loadActions();
}

// 활동 목록 로드
async function loadActions() {
  try {
    const response = await fetch(`${API_BASE}/api/actions/${sessionId}`);
    const data = await response.json();
    
    if (data.success) {
      renderActions(data.actions);
    }
  } catch (error) {
    console.error('Failed to load actions:', error);
  }
}

// 활동 카드 렌더링
function renderActions(actions) {
  const container = document.getElementById('actions-list');
  container.innerHTML = '';
  
  const categories = {
    'Lesson': { name: '📚 수업', color: '#74b9ff' },
    'Job': { name: '💼 아르바이트', color: '#00b894' },
    'Rest': { name: '🏠 휴식', color: '#a29bfe' },
    'Out': { name: '🚶 외출', color: '#fd79a8' },
    'Special': { name: '✨ 특별', color: '#fdcb6e' }
  };
  
  actions.forEach(action => {
    const card = document.createElement('div');
    card.className = 'action-card';
    card.dataset.actionId = action.action_id;
    
    const category = categories[action.category] || { name: action.category, color: '#999' };
    
    // 효과 문자열 생성
    const effects = [];
    Object.entries(action.effects).forEach(([stat, value]) => {
      if (value !== 0) {
        const sign = value > 0 ? '+' : '';
        effects.push(`${stat} ${sign}${value}`);
      }
    });
    
    card.innerHTML = `
      <div class="action-category" style="color: ${category.color}">${category.name}</div>
      <div class="action-name">${action.name}</div>
      <div class="action-effects">${effects.join(', ') || '효과 없음'}</div>
      <div class="action-cost">
        ${action.cost_sweets > 0 ? `<span class="cost">💰-${action.cost_sweets}</span>` : ''}
        ${action.income_sweets > 0 ? `<span class="income">💰+${action.income_sweets}</span>` : ''}
      </div>
    `;
    
    card.addEventListener('click', () => selectAction(action.action_id));
    container.appendChild(card);
  });
}

// 활동 선택
async function selectAction(actionId) {
  if (isProcessing) return;
  
  // 선택 표시
  document.querySelectorAll('.action-card').forEach(card => {
    card.classList.remove('selected');
  });
  document.querySelector(`[data-action-id="${actionId}"]`).classList.add('selected');
  
  isProcessing = true;
  
  try {
    const response = await fetch(`${API_BASE}/api/turn/${sessionId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ actionId })
    });
    
    const data = await response.json();
    
    if (data.success) {
      currentState = data.result.state;
      
      // 로그 추가
      const action = data.result.action;
      addLogEntry('action', `${currentState.age}세 ${currentState.month}월: ${action.name} 선택`);
      
      // 스탯 변화 로그
      Object.entries(action.effects).forEach(([stat, value]) => {
        if (value !== 0) {
          const type = value > 0 ? 'stat-up' : 'stat-down';
          addLogEntry(type, `${stat} ${value > 0 ? '+' : ''}${value}`);
        }
      });
      
      // 이벤트 로그
      if (data.result.events.length > 0) {
        data.result.events.forEach(event => {
          addLogEntry('event', `📢 이벤트 발생: ${event.name}`);
        });
      }
      
      // 대화 표시
      if (data.result.dialogue) {
        showDialogue(data.result.dialogue);
      }
      
      // UI 업데이트
      updateUI();
      
      // 엔딩 체크
      if (data.result.ending) {
        showEnding(data.result.ending);
      }
    } else {
      showError(data.error);
    }
  } catch (error) {
    showError('턴 처리 실패: ' + error.message);
  } finally {
    isProcessing = false;
  }
}

// 로그 추가
function addLogEntry(type, message) {
  const log = document.getElementById('event-log');
  const entry = document.createElement('div');
  entry.className = `log-entry ${type}`;
  entry.textContent = message;
  log.appendChild(entry);
  log.scrollTop = log.scrollHeight;
}

// 엔딩 표시
function showEnding(ending) {
  const container = document.getElementById('ending-content');
  
  const typeNames = {
    'Normal': '일반 엔딩',
    'Hidden': '히든 엔딩',
    'Special': '특별 엔딩'
  };
  
  const stats = currentState.stats;
  const favor = currentState.favor;
  
  container.innerHTML = `
    <div class="ending-title">${ending.name}</div>
    <div class="ending-type">${typeNames[ending.ending_type] || ending.ending_type}</div>
    <div class="ending-description">${ending.description}</div>
    <div class="ending-stats">
      <h4>📊 최종 스탯</h4>
      <div class="stats-list">
        <div class="stat-final">❤️ 체력: ${stats.HP}</div>
        <div class="stat-final">✨ 매력: ${stats.Charm}</div>
        <div class="stat-final">📚 지능: ${stats.Int}</div>
        <div class="stat-final">🎨 기품: ${stats.Art}</div>
        <div class="stat-final">🌟 도덕: ${stats.Morality}</div>
        <div class="stat-final">😰 스트레스: ${stats.Stress}</div>
      </div>
      <div style="margin-top: 15px;">
        <h4>💕 최종 호감도</h4>
        <div class="stats-list">
          <div class="stat-final">☕ 이노: ${favor.Ino}</div>
          <div class="stat-final">🍪 아이린: ${favor.Aileen}</div>
          <div class="stat-final">🍵 카일: ${favor.Kyle}</div>
          <div class="stat-final">🌙 리안: ${favor.Lian}</div>
        </div>
      </div>
    </div>
  `;
  
  addLogEntry('ending', `🎉 엔딩 도달: ${ending.name}`);
  showScreen('ending');
}

// 로딩 표시
function showLoading() {
  const container = document.getElementById('actions-list');
  container.innerHTML = `
    <div class="loading">
      <div class="spinner"></div>
      <p>게임을 로딩 중...</p>
    </div>
  `;
}

// 에러 표시
function showError(message) {
  const container = document.getElementById('actions-list');
  container.innerHTML = `
    <div class="error-message">
      ❌ 오류: ${message}
    </div>
  `;
  console.error(message);
}

// 대화 표시
function showDialogue(dialogue) {
  currentDialogue = dialogue;
  const modal = document.getElementById('dialogue-modal');
  const npcName = document.getElementById('dialogue-npc-name');
  const content = document.getElementById('dialogue-content');
  const choices = document.getElementById('dialogue-choices');
  const nextBtn = document.getElementById('btn-dialogue-next');
  const skipBtn = document.getElementById('btn-dialogue-skip');
  
  // NPC 아이콘 설정
  const npcIcons = {
    'Ino': '☕',
    'Aileen': '🍪',
    'Kyle': '🍵',
    'Lian': '🌙'
  };
  const npcNames = {
    'Ino': '이노',
    'Aileen': '아이린',
    'Kyle': '카일',
    'Lian': '리안'
  };
  
  document.getElementById('dialogue-npc-icon').textContent = npcIcons[dialogue.npcId] || '💬';
  npcName.textContent = npcNames[dialogue.npcId] || dialogue.npcId;
  
  // 대사 내용 표시 (누적되지 않고 현재 대사만)
  content.innerHTML = dialogue.texts.map(text => 
    `<div class="dialogue-text">${text}</div>`
  ).join('');
  
  // 선택지 표시 (대사가 끝난 후)
  if (dialogue.choices && dialogue.choices.length > 0) {
    choices.innerHTML = dialogue.choices.map(choice => 
      `<button class="choice-button" data-choice-id="${choice.choiceId}">${choice.text}</button>`
    ).join('');
    
    choices.classList.remove('hidden');
    nextBtn.classList.add('hidden');
    skipBtn.classList.add('hidden'); // 선택지 있을 때는 건너뛰기 숨김
    
    // 선택지 버튼에 이벤트 리스너 추가
    choices.querySelectorAll('.choice-button').forEach(btn => {
      btn.addEventListener('click', () => selectChoice(parseInt(btn.dataset.choiceId)));
    });
  } else if (dialogue.hasNext) {
    // 다음 대사가 있음
    choices.classList.add('hidden');
    choices.innerHTML = '';
    nextBtn.classList.remove('hidden');
    skipBtn.classList.remove('hidden');
    nextBtn.textContent = '다음 ▶';
  } else {
    // 대사도 없고 선택지도 없음 (대화 종료)
    choices.classList.add('hidden');
    choices.innerHTML = '';
    nextBtn.classList.remove('hidden');
    skipBtn.classList.remove('hidden');
    nextBtn.textContent = '대화 종료';
  }
  
  modal.classList.remove('hidden');
}

// 대화 건너뛰기
async function skipDialogue() {
  if (!sessionId) return;
  
  try {
    const response = await fetch(`${API_BASE}/api/dialogue/${sessionId}/skip`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const data = await response.json();
    
    if (data.success) {
      hideDialogue();
      if (data.state) {
        currentState = data.state;
        updateUI();
      }
    }
  } catch (error) {
    console.error('대화 건너뛰기 실패:', error);
  }
}

// 대화 패널 숨기기
function hideDialogue() {
  const modal = document.getElementById('dialogue-modal');
  modal.classList.add('hidden');
  currentDialogue = null;
}

// 대화 진행
async function advanceDialogue() {
  if (!sessionId || !currentDialogue) return;
  
  try {
    const response = await fetch(`${API_BASE}/api/dialogue/${sessionId}/next`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const data = await response.json();
    
    if (data.success) {
      if (data.dialogue) {
        showDialogue(data.dialogue);
      } else {
        // 대화 종료
        hideDialogue();
      }
      
      if (data.state) {
        currentState = data.state;
        updateUI();
      }
    }
  } catch (error) {
    console.error('대화 진행 실패:', error);
  }
}

// 선택지 선택
async function selectChoice(choiceId) {
  if (!sessionId || !currentDialogue) return;
  
  // 선택 표시
  document.querySelectorAll('.choice-button').forEach(btn => {
    btn.classList.remove('selected');
  });
  document.querySelector(`[data-choice-id="${choiceId}"]`).classList.add('selected');
  
  try {
    const response = await fetch(`${API_BASE}/api/dialogue/${sessionId}/choice`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ choiceId })
    });
    
    const data = await response.json();
    
    if (data.success) {
      // 결과 표시
      const result = data.result;
      const content = document.getElementById('dialogue-content');
      
      let resultHtml = `<div class="dialogue-result">${result.text}</div>`;
      
      if (Object.keys(result.statChanges).length > 0 || Object.keys(result.favorChanges).length > 0) {
        resultHtml += '<div class="result-stats">';
        
        Object.entries(result.statChanges).forEach(([stat, value]) => {
          const sign = value > 0 ? '+' : '';
          const color = value > 0 ? '#27ae60' : '#e74c3c';
          resultHtml += `<span style="color: ${color}">${stat} ${sign}${value}</span>`;
        });
        
        Object.entries(result.favorChanges).forEach(([npc, value]) => {
          const sign = value > 0 ? '+' : '';
          const color = value > 0 ? '#e17055' : '#e74c3c';
          const npcNames = { 'Ino': '이노', 'Aileen': '아이린', 'Kyle': '카일', 'Lian': '리안' };
          resultHtml += `<span style="color: ${color}">${npcNames[npc] || npc} 호감도 ${sign}${value}</span>`;
        });
        
        resultHtml += '</div>';
      }
      
      content.innerHTML += resultHtml;
      
      // 2초 후 대화 패널 숨기기
      setTimeout(() => {
        hideDialogue();
      }, 2000);
      
      // 로그 추가
      addLogEntry('event', `💬 대화 완료: ${result.text}`);
      
      if (data.state) {
        currentState = data.state;
        updateUI();
      }
    }
  } catch (error) {
    console.error('선택지 선택 실패:', error);
  }
}

// 대화 건너뛰기
async function skipDialogue() {
  if (!sessionId) return;
  
  try {
    const response = await fetch(`${API_BASE}/api/dialogue/${sessionId}/skip`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const data = await response.json();
    
    if (data.success) {
      hideDialogue();
      if (data.state) {
        currentState = data.state;
        updateUI();
      }
    }
  } catch (error) {
    console.error('대화 건너뛰기 실패:', error);
  }
}

// 대화 패널 숨기기
function hideDialogue() {
  const modal = document.getElementById('dialogue-modal');
  modal.classList.add('hidden');
  currentDialogue = null;
}
