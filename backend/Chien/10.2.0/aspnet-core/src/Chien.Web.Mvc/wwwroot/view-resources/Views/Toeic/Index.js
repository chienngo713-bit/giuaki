(function () {
  'use strict';

  var api = '/api/toeic/exams';
  var state = { exams: [], selectedExam: null, questionIndex: 0, answers: {}, timer: 75 * 60, timerId: null, validatedFile: null, validation: null };

  function view(name) { document.querySelectorAll('[data-view]').forEach(function (el) { el.classList.toggle('hidden', el.dataset.view !== name); }); window.scrollTo(0, 0); }
  function esc(value) { return String(value == null ? '' : value).replace(/[&<>'"]/g, function (c) { return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[c]; }); }
  function request(url, options) { return fetch(url, options).then(function (r) { return r.text().then(function (text) { var body = {}; try { body = text ? JSON.parse(text) : {}; } catch (e) { body = { message: 'Máy chủ trả về dữ liệu không hợp lệ (' + r.status + ').' }; } if (!r.ok) { body.message = body.message || ('Yêu cầu thất bại (' + r.status + ').'); throw body; } return body; }); }); }
  function formatDate(value) { return value ? new Date(value).toLocaleDateString('vi-VN') : '--'; }

  function loadExams() {
    request(api).then(function (items) { state.exams = items || []; renderExams(); renderAdmin(); }).catch(function () { document.getElementById('exam-list').innerHTML = '<div class="loading-card">Không tải được danh sách đề. Hãy kiểm tra API backend.</div>'; });
  }
  function renderExams() {
    var target = document.getElementById('exam-list');
    if (!state.exams.length) { target.innerHTML = '<div class="loading-card">Chưa có đề thi. Hãy vào Admin để import file Word.</div>'; return; }
    target.innerHTML = state.exams.map(function (exam) { return '<article class="exam-card"><div class="exam-card-top"><h3>' + esc(exam.examTitle || exam.examCode) + '</h3><span class="tag">Reading</span></div><div class="exam-meta"><span>▤ ' + (exam.totalQuestions || 0) + ' câu</span><span>◷ ' + (exam.timeLimitMinutes || 75) + ' phút</span></div><div class="card-footer"><span class="muted">' + esc(exam.status || 'DRAFT') + '</span><button class="primary-button compact" data-exam-id="' + exam.examId + '" data-action="open-detail" type="button">Bắt đầu →</button></div></article>'; }).join('');
  }
  function renderAdmin() {
    var target = document.getElementById('admin-list');
    target.innerHTML = state.exams.length ? state.exams.map(function (exam) { return '<article class="exam-card"><div class="exam-card-top"><h3>' + esc(exam.examTitle || exam.examCode) + '</h3><span class="tag">' + esc(exam.status || 'DRAFT') + '</span></div><div class="exam-meta"><span>' + (exam.totalQuestions || 0) + ' câu</span><span>' + (exam.timeLimitMinutes || 75) + ' phút</span><span>' + formatDate(exam.createdAt) + '</span></div><div class="admin-card-actions"><button data-exam-id="' + exam.examId + '" data-action="open-detail" type="button">Xem trước</button><button data-exam-id="' + exam.examId + '" data-action="open-edit" type="button">Chỉnh sửa</button></div></article>'; }).join('') : '<div class="loading-card">Chưa có đề thi.</div>';
  }
  function openDetail(id) { request(api + '/' + id + '/preview').then(function (exam) { state.selectedExam = exam; document.getElementById('exam-detail-content').innerHTML = '<div class="detail-hero"><span class="eyebrow">LUYỆN ĐỌC</span><h1>' + esc(exam.examTitle) + '</h1><p>Rèn kỹ năng đọc hiểu tiếng Anh trong môi trường công việc hàng ngày.</p><div class="detail-stats"><div class="stat"><strong>' + exam.parts.reduce(function (n, p) { return n + p.questions.length; }, 0) + '</strong><span>Câu hỏi</span></div><div class="stat"><strong>' + exam.timeLimitMinutes + ' phút</strong><span>Thời gian</span></div></div></div><div class="instruction-list"><strong>Trước khi bắt đầu</strong><br/>• Đọc kỹ và chọn đáp án chính xác.<br/>• Bạn có thể thay đổi đáp án trước khi nộp.<br/>• Bài thi sẽ tự động nộp khi hết giờ.</div><button class="primary-button full" data-action="start-exam" type="button">Bắt đầu làm bài →</button>'; view('exam-detail'); }).catch(function () { alert('Không thể xem trước đề thi.'); }); }
  function questions() { return state.selectedExam ? state.selectedExam.parts.reduce(function (all, p) { return all.concat(p.questions); }, []) : []; }
  function startExam() { state.questionIndex = 0; state.answers = {}; state.timer = (state.selectedExam.timeLimitMinutes || 75) * 60; clearInterval(state.timerId); state.timerId = setInterval(function () { state.timer--; renderTimer(); if (state.timer <= 0) submitExam(); }, 1000); renderQuestion(); view('taking'); }
  function renderTimer() { var m = Math.max(0, Math.floor(state.timer / 60)); var s = Math.max(0, state.timer % 60); document.getElementById('timer').textContent = String(m).padStart(2, '0') + ':' + String(s).padStart(2, '0'); }
  function renderQuestion() { var qs = questions(); var q = qs[state.questionIndex]; if (!q) return; var selected = state.answers[q.questionId] || ''; document.getElementById('question-content').innerHTML = '<div class="question-label">Câu ' + q.questionNumber + ' / ' + qs.length + '</div><div class="passage">' + esc(q.questionText) + '</div><div class="answer-list">' + q.options.map(function (o) { return '<button class="answer-option ' + (selected === o.optionKey ? 'selected' : '') + '" data-answer="' + o.optionKey + '" type="button"><span class="answer-key">' + o.optionKey + '</span><span>' + esc(o.optionText) + '</span></button>'; }).join('') + '</div><div class="question-grid">' + qs.slice(0, 40).map(function (item, index) { return '<button class="' + (index === state.questionIndex ? 'current ' : '') + (state.answers[item.questionId] ? 'done' : '') + '" data-question-index="' + index + '" type="button">' + (index + 1) + '</button>'; }).join('') + '</div><div class="question-nav"><button class="secondary-button" data-action="previous-question" type="button">‹ Trước</button><button class="secondary-button" data-action="next-question" type="button">Tiếp theo ›</button><button class="primary-button" data-action="open-submit" type="button">Nộp bài</button></div>'; renderTimer(); }
  function submitConfirm() { var qs = questions(); document.getElementById('unanswered-count').textContent = qs.filter(function (q) { return !state.answers[q.questionId]; }).length; document.getElementById('submit-summary').innerHTML = '<strong>' + (state.selectedExam.examTitle || 'Đề thi') + '</strong><p>' + Object.keys(state.answers).length + '/' + qs.length + ' câu đã trả lời</p>'; view('submit-confirm'); }
  function submitExam() { clearInterval(state.timerId); var qs = questions(); var correct = qs.filter(function (q) { var answer = q.options.find(function (o) { return o.isCorrect; }); return answer && state.answers[q.questionId] === answer.optionKey; }).length; var score = qs.length ? Math.round(correct / qs.length * 100) : 0; document.getElementById('result-content').innerHTML = '<div class="result-score"><span class="eyebrow">' + esc(state.selectedExam.examTitle || 'Đề thi') + '</span><h1>Bạn đã làm rất tốt!</h1><div class="score-ring">' + score + '<small>/100</small></div><p>Đã hoàn thành bài thi.</p></div><div class="result-grid"><div class="result-stat"><strong class="success">' + correct + '</strong><span>Câu đúng</span></div><div class="result-stat"><strong class="danger">' + (qs.length - correct) + '</strong><span>Câu sai/chưa làm</span></div></div><button class="primary-button full" data-action="back-list" type="button">Về danh sách đề</button>'; view('result'); }

  document.addEventListener('click', function (event) { var action = event.target.closest('[data-action]'); if (event.target.closest('[data-answer]')) { var button = event.target.closest('[data-answer]'); var q = questions()[state.questionIndex]; state.answers[q.questionId] = button.dataset.answer; renderQuestion(); return; } if (event.target.closest('[data-question-index]')) { state.questionIndex = Number(event.target.closest('[data-question-index]').dataset.questionIndex); renderQuestion(); return; } if (!action) return; var id = action.dataset.examId; switch (action.dataset.action) { case 'open-detail': openDetail(id); break; case 'start-exam': startExam(); break; case 'previous-question': state.questionIndex = Math.max(0, state.questionIndex - 1); renderQuestion(); break; case 'next-question': state.questionIndex = Math.min(questions().length - 1, state.questionIndex + 1); renderQuestion(); break; case 'open-submit': submitConfirm(); break; case 'submit-exam': submitExam(); break; case 'back-list': clearInterval(state.timerId); view('student-list'); break; case 'back-detail': view('exam-detail'); break; case 'back-taking': view('taking'); break; case 'nav-student': view('student-list'); break; case 'nav-admin': view('admin-list'); renderAdmin(); break; case 'open-admin-import': view('admin-import'); break; case 'back-admin': view('admin-list'); break; case 'cancel-import': resetImport(); break; case 'validate-file': validateFile(); break; case 'import-file': importFile(); break; case 'open-edit': openEdit(id); break; case 'save-edit': saveEdit(); break; } });
  document.getElementById('word-file').addEventListener('change', function () { var file = this.files[0]; if (!file) return; state.validatedFile = file; var info = document.getElementById('file-info'); info.classList.remove('hidden'); info.textContent = file.name + ' · ' + Math.round(file.size / 1024) + ' KB'; });
  function validateFile() { if (!state.validatedFile) { alert('Vui lòng chọn file .docx.'); return; } var data = new FormData(); data.append('file', state.validatedFile); request(api + '/validate', { method: 'POST', body: data }).then(function (result) { state.validation = result; var box = document.getElementById('validation-result'); box.classList.remove('hidden'); box.innerHTML = result.isValid ? '<strong class="validation-ok">✓ File hợp lệ</strong><ul><li>Tags hợp lệ</li><li>Đáp án đầy đủ</li><li>Số thứ tự câu đúng</li><li>' + result.preview.totalQuestions + ' câu hỏi sẵn sàng import</li></ul>' : '<strong class="validation-error">✕ File không hợp lệ</strong><ul>' + (result.errors || []).map(function (e) { return '<li>' + esc(e) + '</li>'; }).join('') + '</ul>'; document.querySelector('[data-action="import-file"]').classList.toggle('hidden', !result.isValid); renderEditPanel(result.preview); }).catch(function (error) { alert(error.message || 'Không thể kiểm tra file.'); }); }
  function renderEditPanel(preview) { if (!preview || !preview.questions || !preview.questions.length) return; var panel = document.getElementById('edit-panel'); panel.classList.remove('hidden'); document.getElementById('edit-question-list').innerHTML = preview.questions.slice(0, 10).map(function (q) { var optionMap = {}; (q.options || []).forEach(function (option) { optionMap[option.optionKey] = option; }); return '<div class="edit-row"><label>Câu ' + q.number + '</label><textarea data-edit-text="' + (q.questionId || q.number) + '" rows="2">' + esc(q.text || q.questionText) + '</textarea>' + ['A', 'B', 'C', 'D'].map(function (key) { return '<input data-edit-option="' + (q.questionId || q.number) + '-' + key + '" value="' + esc(optionMap[key] && optionMap[key].optionText) + '" placeholder="Đáp án ' + key + '" />'; }).join('') + '<select data-edit-answer="' + (q.questionId || q.number) + '">' + ['A', 'B', 'C', 'D'].map(function (key) { var correct = q.answer || (optionMap[key] && optionMap[key].isCorrect ? key : ''); return '<option ' + (correct === key ? 'selected' : '') + '>' + key + '</option>'; }).join('') + '</select></div>'; }).join(''); }
  function importFile() { if (!state.validatedFile || !state.validation || !state.validation.isValid) return; var data = new FormData(); data.append('file', state.validatedFile); request(api + '/import', { method: 'POST', body: data }).then(function () { alert('Import đề thi thành công.'); resetImport(); loadExams(); view('admin-list'); }).catch(function (error) { alert(error.message || 'Import thất bại.'); }); }
  function resetImport() { state.validatedFile = null; state.validation = null; document.getElementById('word-file').value = ''; document.getElementById('file-info').classList.add('hidden'); document.getElementById('validation-result').classList.add('hidden'); document.getElementById('edit-panel').classList.add('hidden'); document.querySelector('[data-action="import-file"]').classList.add('hidden'); }
  function openEdit(id) { request(api + '/' + id + '/preview').then(function (exam) { state.selectedExam = exam; document.getElementById('file-info').classList.remove('hidden'); document.getElementById('file-info').textContent = 'Đang chỉnh sửa: ' + (exam.examTitle || 'Đề thi'); document.getElementById('validation-result').classList.remove('hidden'); document.getElementById('validation-result').innerHTML = '<strong class="validation-ok">✓ Đã tải đề thi để chỉnh sửa</strong>'; renderEditPanel({ questions: exam.parts.reduce(function (all, p) { return all.concat(p.questions); }, []) }); view('admin-import'); }).catch(function () { alert('Không thể tải đề thi để chỉnh sửa.'); }); }
  function saveEdit() { var rows = Array.prototype.slice.call(document.querySelectorAll('[data-edit-text]')); if (!rows.length) return; var updates = rows.map(function (textArea) { var id = textArea.dataset.editText; if (!/^\d+$/.test(id)) return Promise.resolve(); var options = {}; ['A', 'B', 'C', 'D'].forEach(function (key) { var input = document.querySelector('[data-edit-option="' + id + '-' + key + '"]'); options[key] = input ? input.value : ''; }); var answer = document.querySelector('[data-edit-answer="' + id + '"]').value; return request(api + '/questions/' + id, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ text: textArea.value, options: options, answer: answer }) }); }); Promise.all(updates).then(function () { alert('Đã lưu thay đổi câu hỏi.'); loadExams(); }).catch(function (error) { alert(error.message || 'Không thể lưu thay đổi.'); }); }
  function openDetail(id) {
    request(api + '/' + id + '/questions').then(function (exam) {
      state.selectedExam = exam;
      var total = exam.parts.reduce(function (n, p) { return n + p.questions.length; }, 0);
      document.getElementById('exam-detail-content').innerHTML = '<div class="detail-hero"><span class="eyebrow">LUYỆN ĐỌC</span><h1>' + esc(exam.examTitle) + '</h1><p>Rèn kỹ năng đọc hiểu tiếng Anh trong môi trường công việc hàng ngày.</p><div class="detail-stats"><div class="stat"><strong>' + total + '</strong><span>Câu hỏi</span></div><div class="stat"><strong>' + exam.timeLimitMinutes + ' phút</strong><span>Thời gian</span></div></div></div><div class="instruction-list"><strong>Trước khi bắt đầu</strong><br/>• Đọc kỹ và chọn đáp án chính xác.<br/>• Bạn có thể thay đổi đáp án trước khi nộp.<br/>• Bài thi sẽ tự động nộp khi hết giờ.</div><button class="primary-button full" data-action="start-exam" type="button">Bắt đầu làm bài →</button>';
      view('exam-detail');
    }).catch(function () { alert('Không thể tải đề thi.'); });
  }

  function submitExam() {
    clearInterval(state.timerId);
    request(api + '/' + state.selectedExam.examId + '/submit', {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ answers: state.answers })
    }).then(function (result) {
      document.getElementById('result-content').innerHTML = '<div class="result-score"><span class="eyebrow">' + esc(state.selectedExam.examTitle || 'Đề thi') + '</span><h1>Bạn đã hoàn thành!</h1><div class="score-ring">' + result.score + '<small>/100</small></div><p>Kết quả đã được chấm trên máy chủ.</p></div><div class="result-grid"><div class="result-stat"><strong class="success">' + result.correctAnswers + '</strong><span>Câu đúng</span></div><div class="result-stat"><strong class="danger">' + result.wrongAnswers + '</strong><span>Câu sai</span></div><div class="result-stat"><strong>' + result.answeredQuestions + '</strong><span>Đã trả lời</span></div><div class="result-stat"><strong>' + result.unansweredQuestions + '</strong><span>Chưa làm</span></div></div><button class="primary-button full" data-action="back-list" type="button">Về danh sách đề</button>';
      view('result');
    }).catch(function () { alert('Không thể chấm điểm bài thi.'); });
  }

  // ABP wraps controller responses in { result, success, error }.
  // Normalize that envelope so validation/import/list all use one shape.
  function request(url, options) {
    return fetch(url, options).then(function (response) {
      return response.text().then(function (text) {
        var envelope = {};
        try { envelope = text ? JSON.parse(text) : {}; } catch (_) { envelope = { error: { message: 'Máy chủ trả về dữ liệu không hợp lệ.' } }; }
        if (!response.ok) {
          var error = envelope.error || envelope.result || envelope;
          throw { message: (error && (error.message || error.details)) || ('Yêu cầu thất bại (' + response.status + ').'), errors: error && error.errors };
        }
        return envelope && Object.prototype.hasOwnProperty.call(envelope, 'result') ? envelope.result : envelope;
      });
    });
  }

  function validateFile() {
    if (!state.validatedFile) { alert('Vui lòng chọn file .docx.'); return; }
    var data = new FormData();
    data.append('file', state.validatedFile);
    request(api + '/validate', { method: 'POST', body: data }).then(function (result) {
      state.validation = result;
      var box = document.getElementById('validation-result');
      var errors = result.errors || [];
      box.classList.remove('hidden');
      box.innerHTML = result.isValid
        ? '<strong class="validation-ok">✓ File hợp lệ</strong><ul><li>Tags hợp lệ</li><li>Đáp án đầy đủ</li><li>Số thứ tự câu đúng</li><li>' + result.preview.totalQuestions + ' câu hỏi sẵn sàng import</li></ul>'
        : '<strong class="validation-error">✕ File không hợp lệ — chưa được lưu</strong><ul>' + errors.map(function (error) { return '<li>' + esc(error) + '</li>'; }).join('') + '</ul>';
      document.querySelector('[data-action="import-file"]').classList.toggle('hidden', !result.isValid);
      if (result.preview) renderEditPanel(result.preview);
    }).catch(function (error) {
      var box = document.getElementById('validation-result');
      box.classList.remove('hidden');
      box.innerHTML = '<strong class="validation-error">✕ Không thể kiểm tra file</strong><p>' + esc(error.message || 'Lỗi không xác định') + '</p>' + ((error.errors || []).length ? '<ul>' + error.errors.map(function (item) { return '<li>' + esc(item) + '</li>'; }).join('') + '</ul>' : '');
    });
  }

  // Demo edit flow: demo exam id 0 has no database row, so edit it locally.
  function openEdit(id) {
    if (String(id) !== '0') {
      request(api + '/' + id + '/preview').then(function (exam) {
        state.selectedExam = exam;
        document.getElementById('file-info').classList.remove('hidden');
        document.getElementById('file-info').textContent = 'Đang chỉnh sửa: ' + (exam.examTitle || 'Đề thi');
        document.getElementById('validation-result').classList.remove('hidden');
        document.getElementById('validation-result').innerHTML = '<strong class="validation-ok">✓ Đã tải đề thi để chỉnh sửa</strong>';
        renderEditPanel({ questions: exam.parts.reduce(function (all, p) { return all.concat(p.questions); }, []) });
        view('admin-import');
      }).catch(function () { alert('Không thể tải đề thi để chỉnh sửa.'); });
      return;
    }
    var demo = demoExam();
    state.selectedExam = demo;
    document.getElementById('file-info').classList.remove('hidden');
    document.getElementById('file-info').textContent = 'Đang chỉnh sửa: ' + demo.examTitle + ' (demo)';
    document.getElementById('validation-result').classList.remove('hidden');
    document.getElementById('validation-result').innerHTML = '<strong class="validation-ok">✓ Đã tải đề demo để chỉnh sửa giao diện</strong>';
    renderEditPanel({ questions: demo.parts[0].questions.slice(0, 10) });
    view('admin-import');
  }

  // Local demo fallback keeps the student UI usable while the database is empty.
  function demoSummary() { return { examId: 0, examCode: 'TOEIC-DEMO', examTitle: 'TOEIC Reading Test 4', status: 'DRAFT · DEMO', timeLimitMinutes: 75, totalQuestions: 100, createdAt: new Date().toISOString() }; }
  function demoExam() { var qs = []; for (var i = 0; i < 100; i++) qs.push({ questionId: i + 1, questionNumber: i + 101, questionText: 'Choose the best answer for question ' + (i + 101) + '.', options: [{ optionKey: 'A', optionText: 'The first available answer' }, { optionKey: 'B', optionText: 'The correct response' }, { optionKey: 'C', optionText: 'Another possible response' }, { optionKey: 'D', optionText: 'None of the above' }] }); return { examId: 0, examTitle: 'TOEIC Reading Test 4', timeLimitMinutes: 75, totalQuestions: 100, parts: [{ partNumber: 5, questions: qs }] }; }
  function loadExams() { request(api).then(function (items) { state.exams = items && items.length ? items : [demoSummary()]; renderExams(); renderAdmin(); }).catch(function () { state.exams = [demoSummary()]; renderExams(); renderAdmin(); }); }
  function openDetail(id) { if (String(id) !== '0') { request(api + '/' + id + '/questions').then(function (exam) { state.selectedExam = exam; view('exam-detail'); }).catch(function () { alert('Khong the tai de thi.'); }); return; } state.selectedExam = demoExam(); document.getElementById('exam-detail-content').innerHTML = '<div class="detail-hero"><span class="eyebrow">LUYEN DOC · DEMO</span><h1>TOEIC Reading Test 4</h1><p>De demo de kiem tra giao dien lam bai.</p><div class="detail-stats"><div class="stat"><strong>100</strong><span>Cau hoi</span></div><div class="stat"><strong>75 phut</strong><span>Thoi gian</span></div></div></div><button class="primary-button full" data-action="start-exam" type="button">Bat dau lam bai →</button>'; view('exam-detail'); }
  function submitExam() { clearInterval(state.timerId); if (state.selectedExam && state.selectedExam.examId === 0) { var qs = questions(); var correct = Object.keys(state.answers).filter(function (id) { return state.answers[id] === 'B'; }).length; document.getElementById('result-content').innerHTML = '<div class="result-score"><span class="eyebrow">TOEIC Reading Test 4 · DEMO</span><h1>Ban da hoan thanh!</h1><div class="score-ring">' + Math.round(correct / qs.length * 100) + '<small>/100</small></div><p>Ket qua demo de kiem tra giao dien.</p></div><button class="primary-button full" data-action="back-list" type="button">Ve danh sach de</button>'; view('result'); return; } request(api + '/' + state.selectedExam.examId + '/submit', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ answers: state.answers }) }).then(function (result) { alert('Da cham diem: ' + result.score + '/100'); view('student-list'); }); }
  // Always open the student exam list when entering /Toeic.
  // Admin is available only after the user taps the Admin tab.
  view('student-list');
  loadExams();
}());
