const express = require('express');
const cors = require('cors');
const path = require('path');
const GameEngine = require('./game-engine');

const app = express();
const PORT = process.env.PORT || 3001;

app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

// 게임 세션 저장 (메모리)
const sessions = new Map();

// 새 게임 시작
app.post('/api/start', (req, res) => {
  const sessionId = Date.now().toString();
  const engine = new GameEngine();
  
  try {
    engine.initialize();
    sessions.set(sessionId, engine);
    
    res.json({
      success: true,
      sessionId: sessionId,
      state: engine.getState()
    });
  } catch (error) {
    console.error('Error starting game:', error);
    res.status(500).json({
      success: false,
      error: error.message,
      stack: error.stack
    });
  }
});

// 현재 상태 조회
app.get('/api/status/:sessionId', (req, res) => {
  const { sessionId } = req.params;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  res.json({
    success: true,
    state: engine.getState()
  });
});

// 가능한 활동 목록
app.get('/api/actions/:sessionId', (req, res) => {
  const { sessionId } = req.params;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  res.json({
    success: true,
    actions: engine.getAvailableActions()
  });
});

// 턴 진행 (활동 선택)
app.post('/api/turn/:sessionId', (req, res) => {
  const { sessionId } = req.params;
  const { actionId } = req.body;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  try {
    const result = engine.processTurn(actionId);
    
    // 게임 종료 체크
    if (result.ending) {
      sessions.delete(sessionId);
    }
    
    res.json({
      success: true,
      result: result
    });
  } catch (error) {
    res.status(400).json({
      success: false,
      error: error.message
    });
  }
});

// CSV 데이터 조회 (디버깅용)
app.get('/api/data/:type', (req, res) => {
  const { type } = req.params;
  const engine = new GameEngine();
  
  try {
    let data;
    switch (type) {
      case 'actions':
        data = engine.loadActions();
        break;
      case 'events':
        data = engine.loadEvents();
        break;
      case 'endings':
        data = engine.loadEndings();
        break;
      default:
        return res.status(400).json({
          success: false,
          error: 'Invalid data type'
        });
    }
    
    res.json({
      success: true,
      data: data
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message
    });
  }
});

// 루트 페이지
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// 대화 상태 조회
app.get('/api/dialogue/:sessionId', (req, res) => {
  const { sessionId } = req.params;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  res.json({
    success: true,
    dialogue: engine.getCurrentDialogueState(),
    history: engine.dialogueHistory.slice(-20)
  });
});

// 대화 진행 (다음 텍스트)
app.post('/api/dialogue/:sessionId/next', (req, res) => {
  const { sessionId } = req.params;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  try {
    const dialogue = engine.advanceDialogue();
    
    res.json({
      success: true,
      dialogue: dialogue,
      state: engine.getState()
    });
  } catch (error) {
    res.status(400).json({
      success: false,
      error: error.message
    });
  }
});

// 대화 선택지 선택
app.post('/api/dialogue/:sessionId/choice', (req, res) => {
  const { sessionId } = req.params;
  const { choiceId } = req.body;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  try {
    const result = engine.selectChoice(choiceId);
    
    res.json({
      success: true,
      result: result,
      state: engine.getState()
    });
  } catch (error) {
    res.status(400).json({
      success: false,
      error: error.message
    });
  }
});

// 대화 건너뛰기/종료
app.post('/api/dialogue/:sessionId/skip', (req, res) => {
  const { sessionId } = req.params;
  const engine = sessions.get(sessionId);
  
  if (!engine) {
    return res.status(404).json({
      success: false,
      error: 'Session not found'
    });
  }
  
  engine.endDialogue();
  
  res.json({
    success: true,
    state: engine.getState()
  });
});

app.listen(PORT, () => {
  console.log(`🎮 Dessert Kingdom Web Test Server`);
  console.log(`📡 Server running at http://localhost:${PORT}`);
  console.log(`📁 Working directory: ${__dirname}`);
});

// 세션 정리 (1시간 이상 사용되지 않은 세션 삭제)
setInterval(() => {
  const now = Date.now();
  const expireTime = 60 * 60 * 1000; // 1시간
  
  for (const [sessionId, engine] of sessions) {
    if (now - parseInt(sessionId) > expireTime) {
      sessions.delete(sessionId);
    }
  }
}, 10 * 60 * 1000); // 10분마다 체크
