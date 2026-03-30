const fs = require('fs');
const path = require('path');
const csv = require('csv-parse/sync');

class GameEngine {
  constructor() {
    this.config = null;
    this.actions = [];
    this.events = [];
    this.endings = [];
    this.npcs = {};
    this.actionsNpcEffects = {};
    this.eventsEffects = {};
    this.initialStats = {};
    this.dialogues = {};
    
    // 게임 상태
    this.state = null;
    this.occurredEvents = new Set();
    this.flags = new Set();
    this.dialogueHistory = [];
    this.currentDialogue = null;
    this.completedDialogues = new Set();
  }

  // 설정 로드
  loadConfig() {
    const configPath = path.join(__dirname, 'Data', 'Config.json');
    this.config = JSON.parse(fs.readFileSync(configPath, 'utf-8'));
  }

  // CSV 파일 로드
  loadCSV(filename) {
    const filePath = path.join(__dirname, 'Data', filename);
    const content = fs.readFileSync(filePath, 'utf-8');
    return csv.parse(content, {
      columns: true,
      skip_empty_lines: true,
      comment: '#',
      relax_column_count: true
    });
  }

  // 메인 CSV 로드
  loadMainCSV(filename) {
    const filePath = path.join(__dirname, this.config.paths[filename]);
    const content = fs.readFileSync(filePath, 'utf-8');
    return csv.parse(content, {
      columns: true,
      skip_empty_lines: true,
      comment: '#'
    });
  }

  // Actions 로드
  loadActions() {
    const rows = this.loadMainCSV('actions_csv');
    this.actions = rows.map(row => ({
      action_id: row.Action_ID,
      name: row.Name_KO,
      category: row.Category,
      subcategory: row.Subcategory,
      effects: {
        HP: parseInt(row.Effect_HP) || 0,
        Charm: parseInt(row.Effect_Charm) || 0,
        Int: parseInt(row.Effect_Int) || 0,
        Art: parseInt(row.Effect_Art) || 0,
        Morality: parseInt(row.Effect_Morality) || 0,
        Stress: parseInt(row.Effect_Stress) || 0
      },
      cost_sweets: parseInt(row.Cost_Sweets) || 0,
      income_sweets: parseInt(row.Income_Sweets) || 0,
      required_stat: row.Required_Stat,
      required_value: parseInt(row.Required_Value) || 0,
      required_age: parseInt(row.Required_Age) || 0,
      description: row.Description,
      is_repeatable: row.Is_Repeatable === 'TRUE'
    }));
  }

  // Events 로드
  loadEvents() {
    const rows = this.loadMainCSV('events_csv');
    this.events = rows.map(row => ({
      event_id: row.Event_ID,
      name: row.Name_KO,
      event_type: row.Type,
      trigger_type: row.Trigger_Type,
      trigger_value: row.Trigger_Value,
      required_prev: row.Required_Previous_Event,
      age_min: parseInt(row.Age_Min) || 0,
      age_max: parseInt(row.Age_Max) || 99,
      month: parseInt(row.Month) || 0,
      day: parseInt(row.Day) || 0,
      stat_condition: row.Stat_Condition,
      npc_condition: row.NPC_Condition,
      priority: parseInt(row.Priority) || 0,
      is_repeatable: row.Is_Repeatable === 'TRUE',
      is_hidden: row.Is_Hidden === 'TRUE',
      description: row.Description
    }));
  }

  // Endings 로드
  loadEndings() {
    const rows = this.loadMainCSV('endings_csv');
    this.endings = rows.map(row => {
      const reqStats = {};
      const reqFavor = {};
      
      ['HP', 'Charm', 'Int', 'Art', 'Morality', 'Stress'].forEach(stat => {
        const val = row[`Req_${stat}`];
        if (val && val.trim() !== '') {
          reqStats[stat] = val.trim();
        }
      });
      
      ['Ino', 'Aileen', 'Kyle', 'Lian'].forEach(npc => {
        const val = row[`Req_Favor_${npc}`];
        if (val && val.trim() !== '') {
          reqFavor[npc] = val.trim();
        }
      });
      
      return {
        ending_id: row.Ending_ID,
        name: row.Name_KO,
        name_en: row.Name_EN,
        ending_type: row.Type,
        priority: parseInt(row.Priority) || 0,
        req_stats: reqStats,
        req_favor: reqFavor,
        req_events: row.Req_Events,
        req_choices: row.Req_Choices,
        req_flags: row.Req_Flags,
        description: row.Description
      };
    });
  }

  // NPC 데이터 로드
  loadNPCs() {
    const rows = this.loadMainCSV('npc_favor_csv');
    rows.forEach(row => {
      const npcId = row.NPC_ID;
      if (!this.npcs[npcId]) {
        this.npcs[npcId] = [];
      }
      this.npcs[npcId].push({
        npc_id: npcId,
        name: row.NPC_Name,
        favor_level: parseInt(row.Favor_Level),
        min_favor: parseInt(row.Min_Favor),
        max_favor: parseInt(row.Max_Favor)
      });
    });
  }

  // 확장: Actions NPC Effects 로드
  loadActionsNpcEffects() {
    const rows = this.loadCSV('Actions_NPC_Effects.csv');
    rows.forEach(row => {
      this.actionsNpcEffects[row.Action_ID] = {
        Ino: parseInt(row.NPC_Ino) || 0,
        Aileen: parseInt(row.NPC_Aileen) || 0,
        Kyle: parseInt(row.NPC_Kyle) || 0,
        Lian: parseInt(row.NPC_Lian) || 0
      };
    });
  }

  // 확장: Events Effects 로드
  loadEventsEffects() {
    const rows = this.loadCSV('Events_Effects.csv');
    rows.forEach(row => {
      this.eventsEffects[row.Event_ID] = {
        stats: {
          HP: parseInt(row.Effect_HP) || 0,
          Charm: parseInt(row.Effect_Charm) || 0,
          Int: parseInt(row.Effect_Int) || 0,
          Art: parseInt(row.Effect_Art) || 0,
          Morality: parseInt(row.Effect_Morality) || 0,
          Stress: parseInt(row.Effect_Stress) || 0
        },
        sweets: parseInt(row.Effect_Sweets) || 0,
        npc: {
          Ino: parseInt(row.NPC_Ino) || 0,
          Aileen: parseInt(row.NPC_Aileen) || 0,
          Kyle: parseInt(row.NPC_Kyle) || 0,
          Lian: parseInt(row.NPC_Lian) || 0
        },
        flags_added: row.Flags_Added ? row.Flags_Added.split(',').filter(f => f) : []
      };
    });
  }

  // 확장: Initial Stats 로드
  loadInitialStats() {
    const rows = this.loadCSV('Initial_Stats.csv');
    rows.forEach(row => {
      if (row.Stat.startsWith('NPC_')) {
        const npcName = row.Stat.replace('NPC_', '');
        this.initialStats[`favor_${npcName}`] = parseInt(row.Value);
      } else {
        this.initialStats[row.Stat] = parseInt(row.Value);
      }
    });
  }

  // 대화 데이터 로드
  loadDialogues() {
    try {
      const rows = this.loadCSV('Dialogues_KO.csv');
      console.log(`Loaded ${rows.length} dialogue rows`);
      
      rows.forEach(row => {
        if (!row.Dialogue_ID || row.Dialogue_ID.trim() === '') return;
        
        const dialogueId = row.Dialogue_ID.trim();
        const textType = row.Text_Type?.trim() || 'Dialogue';
        const sequence = parseInt(row.Sequence) || 1;
        const choiceId = row.Choice_ID ? parseInt(row.Choice_ID) : null;
        const textContent = row.Text_Content?.trim() || '';
        const nextDialogueId = row.Next_Dialogue_ID?.trim() || '';
        const statChanges = row.Stat_Changes?.trim() || '';
        const favorChanges = row.Favor_Changes?.trim() || '';
        
        // Dialogue_ID별로 그룹화
        if (!this.dialogues[dialogueId]) {
          this.dialogues[dialogueId] = {
            id: dialogueId,
            dialogues: [],
            choices: [],
            results: {}
          };
        }
        
        if (textType === 'Dialogue') {
          this.dialogues[dialogueId].dialogues.push({
            sequence: sequence,
            text: textContent
          });
        } else if (textType === 'Choice' && choiceId) {
          this.dialogues[dialogueId].choices.push({
            choiceId: choiceId,
            sequence: sequence,
            text: textContent,
            nextId: nextDialogueId,
            statChanges: statChanges,
            favorChanges: favorChanges
          });
        } else if (textType === 'Result' && choiceId) {
          // 결과는 Next_Dialogue_ID가 Result ID
          if (!this.dialogues[dialogueId].results[choiceId]) {
            this.dialogues[dialogueId].results[choiceId] = {
              text: textContent,
              statChanges: statChanges,
              favorChanges: favorChanges
            };
          }
        }
      });
      
      console.log(`Parsed ${Object.keys(this.dialogues).length} unique dialogues`);
    } catch (error) {
      console.error('Error loading dialogues:', error);
      this.dialogues = {};
    }
  }

  // NPC ID 추출 (Dialogue_ID에서)
  extractNpcId(dialogueId) {
    const match = dialogueId.match(/^D_(\w+)_/);
    return match ? match[1] : null;
  }

  // 대화 레벨 추출
  extractDialogueLevel(dialogueId) {
    const match = dialogueId.match(/_L(\d+)_/);
    return match ? parseInt(match[1]) : 1;
  }

  // 대화 이벤트 번호 추출 (D_Ino_L1_001 → 1)
  extractEventNumber(dialogueId) {
    const match = dialogueId.match(/_L\d+_(\d+)$/);
    return match ? parseInt(match[1]) : 999;
  }

  // 대화 트리거 체크
  checkDialogueTrigger() {
    // 3개월마다 체크
    const interval = this.config.dialogue?.trigger_interval_months || 3;
    const turn = this.state.turn;
    const month = this.state.month;
    
    console.log(`Checking dialogue trigger: turn=${turn}, month=${month}, interval=${interval}`);
    
    // 현재 대화가 진행 중이면 새로운 대화 트리거 안함
    if (this.currentDialogue) {
      console.log('Current dialogue exists, skipping trigger');
      return null;
    }
    
    // 각 NPC별로 가능한 대화 찾기
    const availableDialogues = [];
    
    console.log(`Total dialogues loaded: ${Object.keys(this.dialogues).length}`);
    
    for (const [dialogueId, dialogueData] of Object.entries(this.dialogues)) {
      // 이미 완료한 대화는 스킵
      if (this.completedDialogues.has(dialogueId)) continue;
      
      const npcId = this.extractNpcId(dialogueId);
      const level = this.extractDialogueLevel(dialogueId);
      
      if (!npcId) continue;
      
      // NPC별 호감도 레벨 체크
      const favor = this.state.favor[npcId] || 0;
      let requiredMin = 0, requiredMax = 20;
      
      switch(level) {
        case 1: requiredMin = 0; requiredMax = 20; break;
        case 2: requiredMin = 21; requiredMax = 40; break;
        case 3: requiredMin = 41; requiredMax = 60; break;
        case 4: requiredMin = 61; requiredMax = 80; break;
        case 5: requiredMin = 81; requiredMax = 200; break;
      }
      
      // 리안은 13세 이상부터 등장
      if (npcId === 'Lian' && this.state.age < 13) continue;
      
      // 호감도 조건 체크
      if (favor >= requiredMin && favor <= requiredMax) {
        availableDialogues.push({
          dialogueId: dialogueId,
          npcId: npcId,
          level: level,
          eventNum: this.extractEventNumber(dialogueId),
          favor: favor,
          data: dialogueData
        });
      }
    }
    
    console.log(`Available dialogues: ${availableDialogues.length}`);
    if (availableDialogues.length > 0) {
      console.log('Available:', availableDialogues.map(d => `${d.npcId}_L${d.level}_${String(d.eventNum).padStart(3, '0')}`).join(', '));
    }
    
    // 랜덤하게 하나 선택 (간격 체크)
    if (availableDialogues.length > 0 && (turn === 1 || month % interval === 1)) {
      // 우선순위: 낮은 레벨 먼저, 같은 레벨에서는 번호 순서대로
      availableDialogues.sort((a, b) => {
        if (a.level !== b.level) {
          return a.level - b.level; // 낮은 레벨 우선
        }
        return a.eventNum - b.eventNum; // 같은 레벨에서는 번호 순서대로
      });
      
      // 첫 번째(가장 낮은 레벨, 가장 작은 번호) 선택
      const selected = availableDialogues[0];
      
      console.log(`Selected dialogue: ${selected.dialogueId} (Level ${selected.level}, Event ${selected.eventNum})`);
      return selected;
    }
    
    return null;
  }

  // 대화 시작
  startDialogue(dialogueTrigger) {
    if (!dialogueTrigger) return null;
    
    const { dialogueId, npcId, data } = dialogueTrigger;
    
    this.currentDialogue = {
      dialogueId: dialogueId,
      npcId: npcId,
      currentSequence: 1,
      isComplete: false,
      data: data
    };
    
    return this.getCurrentDialogueState();
  }

  // 현재 대화 상태 반환
  getCurrentDialogueState() {
    if (!this.currentDialogue) return null;
    
    const { data, currentSequence } = this.currentDialogue;
    
    // 현재 순서의 대화 텍스트 찾기
    const currentTexts = data.dialogues.filter(d => d.sequence === currentSequence);
    
    // 다음 대화가 있는지 확인 (대사가 더 있는지)
    const nextTexts = data.dialogues.filter(d => d.sequence === currentSequence + 1);
    const hasNextDialogue = nextTexts.length > 0;
    
    // 선택지는 모든 대사가 끝난 후에만 표시
    let choices = [];
    if (!hasNextDialogue && data.choices.length > 0) {
      choices = data.choices;
    }
    
    return {
      dialogueId: this.currentDialogue.dialogueId,
      npcId: this.currentDialogue.npcId,
      texts: currentTexts.map(d => d.text),
      choices: choices,
      isComplete: this.currentDialogue.isComplete,
      hasNext: hasNextDialogue // 다음 대사가 있는지 여부
    };
  }

  // 대화 진행 (다음으로)
  advanceDialogue() {
    if (!this.currentDialogue) return null;
    
    const { data, currentSequence } = this.currentDialogue;
    
    // 다음 대화가 있는지 확인
    const nextTexts = data.dialogues.filter(d => d.sequence === currentSequence + 1);
    
    if (nextTexts.length > 0) {
      this.currentDialogue.currentSequence++;
      return this.getCurrentDialogueState();
    } else {
      // 대사가 끝났는데 선택지가 있으면 선택지 표시
      const choices = data.choices;
      if (choices.length > 0) {
        return this.getCurrentDialogueState();
      } else {
        // 선택지도 없으면 대화 종료
        this.endDialogue();
        return null;
      }
    }
  }

  // 선택지 선택
  selectChoice(choiceId) {
    if (!this.currentDialogue) return null;
    
    const { dialogueId, data } = this.currentDialogue;
    const choice = data.choices.find(c => c.choiceId === choiceId);
    
    if (!choice) {
      throw new Error(`Choice not found: ${choiceId} in ${dialogueId}`);
    }
    
    // 결과 적용
    const result = this.applyDialogueResult(dialogueId, choiceId);
    
    // 대화 기록에 추가
    this.dialogueHistory.push({
      dialogueId: dialogueId,
      npcId: this.currentDialogue.npcId,
      choiceId: choiceId,
      choiceText: choice.text,
      result: result,
      turn: this.state.turn,
      age: this.state.age,
      month: this.state.month
    });
    
    // 완료된 대화로 표시
    this.completedDialogues.add(dialogueId);
    
    // 대화 종료
    this.currentDialogue = null;
    
    return result;
  }

  // 대화 결과 적용
  applyDialogueResult(dialogueId, choiceId) {
    const dialogueData = this.dialogues[dialogueId];
    if (!dialogueData) return null;
    
    const choice = dialogueData.choices.find(c => c.choiceId === choiceId);
    if (!choice) return null;
    
    const result = {
      text: '',
      statChanges: {},
      favorChanges: {},
      flagsAdded: []
    };
    
    // 결과 텍스트 찾기
    const resultData = dialogueData.results[choiceId];
    if (resultData) {
      result.text = resultData.text;
    }
    
    // 스탯 변화 적용
    if (choice.statChanges) {
      const statMatch = choice.statChanges.match(/([\uac00-\ud7af]+)\s*([+-]\d+)/g);
      if (statMatch) {
        statMatch.forEach(match => {
          const [, statName, valueStr] = match.match(/([\uac00-\ud7af]+)\s*([+-]\d+)/);
          const value = parseInt(valueStr);
          
          // 스탯명 매핑
          const statMap = {
            '체력': 'HP',
            '매력': 'Charm',
            '지능': 'Int',
            '기품': 'Art',
            '도덕': 'Morality',
            '도덕성': 'Morality',
            '스트레스': 'Stress',
            '유머': 'Int',
            '전체스탯': 'ALL'
          };
          
          const statKey = statMap[statName];
          if (statKey) {
            if (statKey === 'ALL') {
              Object.keys(this.state.stats).forEach(key => {
                this.state.stats[key] = Math.max(0, Math.min(999, this.state.stats[key] + value));
                result.statChanges[key] = value;
              });
            } else if (this.state.stats[statKey] !== undefined) {
              this.state.stats[statKey] = Math.max(0, Math.min(999, this.state.stats[statKey] + value));
              result.statChanges[statKey] = value;
            }
          }
        });
      }
    }
    
    // 호감도 변화 적용
    if (choice.favorChanges) {
      const favorMatch = choice.favorChanges.match(/(\w+)\s*([+-]\d+)/g);
      if (favorMatch) {
        favorMatch.forEach(match => {
          const [, npcName, valueStr] = match.match(/(\w+)\s*([+-]\d+)/);
          const value = parseInt(valueStr);
          
          // NPC 이름 매핑
          const npcMap = {
            '이노': 'Ino',
            '아이린': 'Aileen',
            '카일': 'Kyle',
            '리안': 'Lian'
          };
          
          const npcKey = npcMap[npcName] || npcName;
          if (this.state.favor[npcKey] !== undefined) {
            const maxFavor = npcKey === 'Lian' ? 180 : 200;
            this.state.favor[npcKey] = Math.min(maxFavor, this.state.favor[npcKey] + value);
            result.favorChanges[npcKey] = value;
          }
        });
      }
    }
    
    // 특수 플래그 체크
    if (choice.nextId && choice.nextId.includes('엔딩')) {
      result.flagsAdded.push(choice.nextId);
      this.flags.add(choice.nextId);
    }
    
    return result;
  }

  // 대화 종료
  endDialogue() {
    if (!this.currentDialogue) return;
    
    // 완료된 대화로 표시하지만 결과 없이 종료
    this.completedDialogues.add(this.currentDialogue.dialogueId);
    this.currentDialogue = null;
  }

  // 초기화
  initialize() {
    this.loadConfig();
    this.loadActions();
    this.loadEvents();
    this.loadEndings();
    this.loadNPCs();
    this.loadActionsNpcEffects();
    this.loadEventsEffects();
    this.loadInitialStats();
    this.loadDialogues();
    this.resetState();
  }

  // 상태 초기화
  resetState() {
    this.state = {
      age: this.initialStats.Age || 6,
      month: this.initialStats.Month || 1,
      turn: this.initialStats.Turn || 1,
      stats: {
        HP: this.initialStats.HP || 50,
        Charm: this.initialStats.Charm || 50,
        Int: this.initialStats.Int || 50,
        Art: this.initialStats.Art || 50,
        Morality: this.initialStats.Morality || 50,
        Stress: this.initialStats.Stress || 0
      },
      sweets: this.initialStats.Sweets || 500,
      favor: {
        Ino: this.initialStats.favor_Ino || 30,
        Aileen: this.initialStats.favor_Aileen || 20,
        Kyle: this.initialStats.favor_Kyle || 20,
        Lian: this.initialStats.favor_Lian || 0
      }
    };
    this.occurredEvents.clear();
    this.flags.clear();
    this.dialogueHistory = [];
    this.currentDialogue = null;
    this.completedDialogues.clear();
  }

  // 현재 상태 반환
  getState() {
    return {
      ...this.state,
      flags: Array.from(this.flags),
      occurredEvents: Array.from(this.occurredEvents),
      currentDialogue: this.currentDialogue,
      dialogueHistory: this.dialogueHistory.slice(-50), // 최근 50개만
      completedDialogues: Array.from(this.completedDialogues)
    };
  }

  // 가능한 활동 반환
  getAvailableActions() {
    return this.actions.filter(action => {
      // 나이 체크
      if (action.required_age > 0 && this.state.age < action.required_age) {
        return false;
      }
      // 스탯 체크
      if (action.required_stat !== 'None' && this.state.stats[action.required_stat] < action.required_value) {
        return false;
      }
      // 비용 체크
      if (this.state.sweets < action.cost_sweets) {
        return false;
      }
      return true;
    });
  }

  // 활동 적용
  applyAction(actionId) {
    const action = this.actions.find(a => a.action_id === actionId);
    if (!action) {
      throw new Error(`Action not found: ${actionId}`);
    }

    // 스탯 효과 적용
    for (const [stat, value] of Object.entries(action.effects)) {
      if (this.state.stats[stat] !== undefined) {
        this.state.stats[stat] = Math.max(0, Math.min(999, this.state.stats[stat] + value));
      }
    }

    // 돈 계산
    this.state.sweets -= action.cost_sweets;
    this.state.sweets += action.income_sweets;
    this.state.sweets = Math.max(0, this.state.sweets);

    // NPC 호감도 변화 (확장 CSV 사용)
    const npcEffects = this.actionsNpcEffects[actionId];
    if (npcEffects) {
      for (const [npc, value] of Object.entries(npcEffects)) {
        if (this.state.favor[npc] !== undefined) {
          // 리안은 13세 이상부터
          if (npc === 'Lian' && this.state.age < 13) {
            continue;
          }
          const maxFavor = npc === 'Lian' ? 180 : 200;
          this.state.favor[npc] = Math.min(maxFavor, this.state.favor[npc] + value);
        }
      }
    }

    return action;
  }

  // 이벤트 조건 체크
  checkEventConditions(event) {
    // 이미 발생했고 반복 불가
    if (this.occurredEvents.has(event.event_id) && !event.is_repeatable) {
      return false;
    }

    // 나이 체크
    if (event.age_min > 0 && this.state.age < event.age_min) return false;
    if (event.age_max > 0 && this.state.age > event.age_max) return false;

    // 월 체크
    if (event.month > 0 && this.state.month !== event.month) return false;

    // 날짜 체크 (생일 등)
    if (event.trigger_type === 'Date' && event.trigger_value) {
      const match = event.trigger_value.match(/Date=(\d{2})(\d{2})/);
      if (match) {
        const month = parseInt(match[1]);
        const day = parseInt(match[2]);
        if (this.state.month !== month || event.day !== day) {
          return false;
        }
      }
    }

    // 스탯 조건 체크
    if (event.stat_condition) {
      if (event.stat_condition.includes('Stress>=')) {
        const val = parseInt(event.stat_condition.split('>=')[1]);
        if (this.state.stats.Stress < val) return false;
      } else if (event.stat_condition.includes('INT<')) {
        const val = parseInt(event.stat_condition.split('<')[1]);
        if (this.state.stats.Int >= val) return false;
      }
    }

    // NPC 조건 체크
    if (event.npc_condition) {
      const npcMatch = event.npc_condition.match(/Favor_(\w+)>=(\d+)/);
      if (npcMatch) {
        const npc = npcMatch[1];
        const val = parseInt(npcMatch[2]);
        if (!this.state.favor[npc] || this.state.favor[npc] < val) {
          return false;
        }
      }
    }

    return true;
  }

  // 발생 가능한 이벤트 찾기
  getTriggeredEvents() {
    const triggered = this.events.filter(event => this.checkEventConditions(event));
    triggered.sort((a, b) => b.priority - a.priority);
    return triggered;
  }

  // 이벤트 효과 적용 (확장 CSV 사용)
  applyEvent(event) {
    const effects = this.eventsEffects[event.event_id];
    
    if (effects) {
      // 스탯 효과
      for (const [stat, value] of Object.entries(effects.stats)) {
        if (this.state.stats[stat] !== undefined) {
          this.state.stats[stat] = Math.max(0, Math.min(999, this.state.stats[stat] + value));
        }
      }

      // 스위츠
      if (effects.sweets) {
        this.state.sweets += effects.sweets;
      }

      // NPC 호감도
      for (const [npc, value] of Object.entries(effects.npc)) {
        if (this.state.favor[npc] !== undefined && value !== 0) {
          const maxFavor = npc === 'Lian' ? 180 : 200;
          this.state.favor[npc] = Math.min(maxFavor, this.state.favor[npc] + value);
        }
      }

      // 플래그 추가
      effects.flags_added.forEach(flag => {
        this.flags.add(flag);
      });
    }

    this.occurredEvents.add(event.event_id);
    return event;
  }

  // 턴 진행
  processTurn(actionId) {
    // 활동 적용
    const action = this.applyAction(actionId);
    
    // 이벤트 체크
    const triggeredEvents = this.getTriggeredEvents();
    const occurredEvents = triggeredEvents.map(event => this.applyEvent(event));
    
    // 대화 트리거 체크
    let dialogueTrigger = null;
    let dialogue = null;
    
    // 현재 대화가 없을 때만 새 대화 체크
    if (!this.currentDialogue) {
      dialogueTrigger = this.checkDialogueTrigger();
      if (dialogueTrigger) {
        dialogue = this.startDialogue(dialogueTrigger);
      }
    }
    
    // 턴/나이 계산
    this.state.turn++;
    this.state.month++;
    if (this.state.month > 12) {
      this.state.month = 1;
      this.state.age++;
    }
    
    // 엔딩 체크
    const ending = this.checkEnding();

    return {
      action: action,
      events: occurredEvents,
      dialogue: dialogue,
      state: this.getState(),
      ending: ending
    };
  }

  // 엔딩 체크
  checkEnding() {
    if (this.state.age < 18) {
      return null;
    }

    // 우선순위 높은 순으로 체크
    const sortedEndings = [...this.endings].sort((a, b) => b.priority - a.priority);

    for (const ending of sortedEndings) {
      let satisfied = true;

      // 스탯 조건 체크
      for (const [stat, condition] of Object.entries(ending.req_stats)) {
        const value = this.state.stats[stat];
        
        if (condition.includes('+')) {
          const min = parseInt(condition.replace('+', ''));
          if (value < min) {
            satisfied = false;
            break;
          }
        } else if (condition.includes('-')) {
          const [min, max] = condition.split('-').map(parseInt);
          if (value < min || value > max) {
            satisfied = false;
            break;
          }
        } else if (condition.includes('<=')) {
          const max = parseInt(condition.replace('<=', ''));
          if (value > max) {
            satisfied = false;
            break;
          }
        }
      }

      // 호감도 조건 체크
      if (satisfied) {
        for (const [npc, condition] of Object.entries(ending.req_favor)) {
          const value = this.state.favor[npc];
          
          if (condition.includes('+')) {
            const min = parseInt(condition.replace('+', ''));
            if (!value || value < min) {
              satisfied = false;
              break;
            }
          } else if (condition.includes('-')) {
            const [min, max] = condition.split('-').map(parseInt);
            if (!value || value < min || value > max) {
              satisfied = false;
              break;
            }
          }
        }
      }

      // 이벤트 조건 체크
      if (satisfied && ending.req_events) {
        const requiredEvents = ending.req_events.split(',').filter(e => e);
        for (const eventId of requiredEvents) {
          if (!this.occurredEvents.has(eventId)) {
            satisfied = false;
            break;
          }
        }
      }

      // 플래그 조건 체크
      if (satisfied && ending.req_flags) {
        const requiredFlags = ending.req_flags.split(',').filter(f => f);
        for (const flag of requiredFlags) {
          if (!this.flags.has(flag)) {
            satisfied = false;
            break;
          }
        }
      }

      if (satisfied) {
        return ending;
      }
    }

    return null;
  }
}

module.exports = GameEngine;
