/** AquaWater 调洪演算程序 Web 版 — 前端交互逻辑 */

// ===== State =====
const state = {
    zv: { data: null, loaded: false },
    zq: { data: null, loaded: false },
    flood: { data: null, loaded: false },
    results: null,
    summary: null,
    chartData: null,
    pasteTarget: null,
    charts: {},
};

// ===== Utility =====
const $ = (sel) => document.querySelector(sel);
const $$ = (sel) => document.querySelectorAll(sel);

function toast(msg, type = 'info') {
    const container = $('#toastContainer');
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.textContent = msg;
    container.appendChild(el);
    setTimeout(() => { el.style.opacity = '0'; el.style.transition = 'opacity 0.3s'; setTimeout(() => el.remove(), 300); }, 3000);
}

function formatNum(v, decimals = 2) {
    if (v === null || v === undefined || isNaN(v)) return '—';
    return Number(v).toFixed(decimals);
}

function updateStep(step) {
    const steps = $$('#progressSteps .step');
    steps.forEach((s) => {
        const sNum = parseInt(s.dataset.step);
        s.classList.remove('active', 'done');
        if (sNum < step) s.classList.add('done');
        if (sNum === step) s.classList.add('active');
    });
}

// ===== Download Sample Data =====
function downloadSample(type) {
    const link = document.createElement('a');
    link.href = `/api/download-sample/${type}`;
    link.download = '';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    const names = { zv: '库容曲线', zq: '泄流曲线', flood: '洪水过程线' };
    toast(`正在下载 ${names[type] || ''} 示例数据...`, 'info');
}

function checkAllReady() {
    const allLoaded = state.zv.loaded && state.zq.loaded && state.flood.loaded;
    const btn = $('#btnCalculate');
    btn.disabled = !allLoaded;
    if (allLoaded) {
        btn.innerHTML = '<span class="btn-icon">▶</span> 开始调洪演算';
        updateStep(2);
    }
    // 更新数据状态标签
    const dataStatus = $('#dataStatus');
    const loadedCount = [state.zv, state.zq, state.flood].filter(d => d.loaded).length;
    if (loadedCount === 3) {
        dataStatus.textContent = '✓ 已全部导入';
        dataStatus.className = 'section-badge ready';
    } else if (loadedCount > 0) {
        dataStatus.textContent = `已导入 ${loadedCount}/3`;
        dataStatus.className = 'section-badge partial';
    } else {
        dataStatus.textContent = '等待导入';
        dataStatus.className = 'section-badge';
    }
}

// ===== File Upload =====
function setupFileUpload(cardId, type) {
    const uploadZone = $(`#upload${cardId}`);
    const fileInput = $(`#file${cardId}`);
    const tag = $(`#tag${cardId}`);
    const tableWrapper = $(`#tableWrapper${cardId}`);
    const tableBody = $(`#table${cardId} tbody`);
    const actions = $(`#actions${cardId}`);

    // Stop file input click from bubbling up to upload zone (prevents double dialog)
    fileInput.addEventListener('click', (e) => e.stopPropagation());

    // Click to select file
    uploadZone.addEventListener('click', () => fileInput.click());

    // File selected
    fileInput.addEventListener('change', () => {
        if (fileInput.files.length > 0) {
            uploadFile(fileInput.files[0], type);
            fileInput.value = ''; // clear so same file can be re-selected
        }
    });

    // Drag & drop
    uploadZone.addEventListener('dragover', (e) => { e.preventDefault(); uploadZone.classList.add('drag-over'); });
    uploadZone.addEventListener('dragleave', () => { uploadZone.classList.remove('drag-over'); });
    uploadZone.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) uploadFile(file, type);
    });
}

async function uploadFile(file, type) {
    const formData = new FormData();
    formData.append('file', file);

    try {
        const resp = await fetch('/api/preview', { method: 'POST', body: formData });
        const result = await resp.json();

        if (!resp.ok) {
            toast(result.error || '文件解析失败', 'error');
            return;
        }

        // Store data
        const stateKey = type === 'zv' ? 'zv' : type === 'zq' ? 'zq' : 'flood';
        state[stateKey].data = result.data;
        state[stateKey].loaded = true;

        // Update UI
        const cardId = type === 'zv' ? 'ZV' : type === 'zq' ? 'ZQ' : 'Flood';
        const tag = $(`#tag${cardId}`);
        tag.textContent = `✓ ${result.count} 条`;
        tag.className = 'status-tag loaded';

        // Render table
        renderTable(type, result.data);

        // Show table & actions, hide upload zone & sample download
        $(`#tableWrapper${cardId}`).classList.remove('hidden');
        $(`#actions${cardId}`).classList.remove('hidden');
        $(`#upload${cardId}`).classList.add('hidden');
        const sampleEl = document.querySelector(`#card${cardId} .sample-download`);
        if (sampleEl) sampleEl.classList.add('hidden');

        // Draw chart
        if (type === 'zv') drawChartZV(result.data);
        if (type === 'zq') drawChartZQ(result.data);

        toast(`${file.name} 加载成功 (${result.count} 条数据)`, 'success');
        checkAllReady();
        updateStep(Math.min(1 + [state.zv, state.zq, state.flood].filter(d => d.loaded).length, 2));

    } catch (err) {
        toast(`上传失败: ${err.message}`, 'error');
    }
}

function renderTable(type, data) {
    const cardId = type === 'zv' ? 'ZV' : type === 'zq' ? 'ZQ' : 'Flood';
    const col1Label = type === 'flood' ? '时间 (h)' : '水位 (m)';
    const col2Label = type === 'flood' ? '流量 (m³/s)' : type === 'zv' ? '库容 (万m³)' : '流量 (m³/s)';

    // Update header
    const thead = $(`#table${cardId} thead tr`);
    thead.innerHTML = `<th>序号</th><th>${col1Label}</th><th>${col2Label}</th>`;

    const tbody = $(`#table${cardId} tbody`);
    tbody.innerHTML = data.map((r, i) =>
        `<tr><td>${i + 1}</td><td contenteditable="true">${formatNum(r.col1, 4)}</td><td contenteditable="true">${formatNum(r.col2, 4)}</td></tr>`
    ).join('');

    // Sync edits back to state
    tbody.addEventListener('blur', () => syncTableEdits(type), true);
}

function syncTableEdits(type) {
    const cardId = type === 'zv' ? 'ZV' : type === 'zq' ? 'ZQ' : 'Flood';
    const rows = $$(`#table${cardId} tbody tr`);
    const stateKey = type === 'zv' ? 'zv' : type === 'zq' ? 'zq' : 'flood';
    const newData = [];
    rows.forEach((row) => {
        const cells = row.querySelectorAll('td');
        if (cells.length >= 3) {
            const col1 = parseFloat(cells[1].textContent);
            const col2 = parseFloat(cells[2].textContent);
            if (!isNaN(col1) && !isNaN(col2)) {
                newData.push({ col1, col2 });
            }
        }
    });
    if (newData.length > 0) {
        state[stateKey].data = newData;
        // Redraw charts
        if (type === 'zv') drawChartZV(newData);
        if (type === 'zq') drawChartZQ(newData);
        if (type === 'flood') drawChartFlood(newData);
    }
}

function clearData(type) {
    const cardId = type === 'zv' ? 'ZV' : type === 'zq' ? 'ZQ' : 'Flood';
    const stateKey = type === 'zv' ? 'zv' : type === 'zq' ? 'zq' : 'flood';
    state[stateKey].data = null;
    state[stateKey].loaded = false;

    $(`#tag${cardId}`).textContent = '未加载';
    $(`#tag${cardId}`).className = 'status-tag';
    $(`#tableWrapper${cardId}`).classList.add('hidden');
    $(`#actions${cardId}`).classList.add('hidden');
    $(`#upload${cardId}`).classList.remove('hidden');
    $(`#file${cardId}`).value = '';
    const sampleEl3 = document.querySelector(`#card${cardId} .sample-download`);
    if (sampleEl3) sampleEl3.classList.remove('hidden');

    checkAllReady();
    updateStep(state.zv.loaded || state.zq.loaded || state.flood.loaded ? 1 : 0);
}

// ===== Paste from Clipboard =====
function pasteFromClipboard(target) {
    state.pasteTarget = target;
    $('#pasteModal').classList.remove('hidden');
    $('#pasteTextarea').value = '';
    $('#pasteTextarea').focus();
}

function closePasteModal() {
    $('#pasteModal').classList.add('hidden');
    state.pasteTarget = null;
}

$('#btnPasteConfirm').addEventListener('click', () => {
    const text = $('#pasteTextarea').value.trim();
    if (!text) { toast('请先粘贴数据', 'error'); return; }
    const lines = text.split('\n').filter(l => l.trim());
    const data = [];
    for (const line of lines) {
        const parts = line.trim().split(/[\t,\s]+/);
        if (parts.length >= 2) {
            const col1 = parseFloat(parts[0]);
            const col2 = parseFloat(parts[1]);
            if (!isNaN(col1) && !isNaN(col2)) data.push({ col1, col2 });
        }
    }
    if (data.length === 0) { toast('未能解析到有效数据', 'error'); return; }

    const type = state.pasteTarget;
    const stateKey = type === 'zv' ? 'zv' : type === 'zq' ? 'zq' : 'flood';
    const cardId = type === 'zv' ? 'ZV' : type === 'zq' ? 'ZQ' : 'Flood';
    state[stateKey].data = data;
    state[stateKey].loaded = true;

    $(`#tag${cardId}`).textContent = `✓ ${data.length} 条 (粘贴)`;
    $(`#tag${cardId}`).className = 'status-tag loaded';
    renderTable(type, data);
    $(`#tableWrapper${cardId}`).classList.remove('hidden');
    $(`#actions${cardId}`).classList.remove('hidden');
    $(`#upload${cardId}`).classList.add('hidden');
    const sampleEl2 = document.querySelector(`#card${cardId} .sample-download`);
    if (sampleEl2) sampleEl2.classList.add('hidden');

    if (type === 'zv') drawChartZV(data);
    if (type === 'zq') drawChartZQ(data);
    if (type === 'flood') drawChartFlood(data);

    toast(`粘贴导入成功 (${data.length} 条数据)`, 'success');
    closePasteModal();
    checkAllReady();
});

$('#pasteTextarea').addEventListener('keydown', (e) => {
    if (e.ctrlKey && e.key === 'Enter') $('#btnPasteConfirm').click();
});

// Close modal on overlay click
$('#pasteModal').addEventListener('click', (e) => {
    if (e.target === $('#pasteModal')) closePasteModal();
});

// ===== Charts =====
function initChart(domId) {
    const dom = $(`#${domId}`);
    if (!dom) return null;
    if (state.charts[domId]) state.charts[domId].dispose();
    const chart = echarts.init(dom, 'dark');
    state.charts[domId] = chart;
    window.addEventListener('resize', () => chart.resize());
    return chart;
}

function drawChartZV(data) {
    const chart = initChart('chartZV');
    if (!chart || !data) return;
    const x = data.map(r => r.col2); // 库容
    const y = data.map(r => r.col1); // 水位
    const yMin = Math.min(...y);
    const yMax = Math.max(...y);
    const yPad = Math.max((yMax - yMin) * 0.08, 0.5);
    chart.setOption({
        backgroundColor: 'transparent',
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(10,15,31,0.95)',
            borderColor: 'rgba(0,229,255,0.3)',
            textStyle: { color: '#e0e8f0', fontSize: 12 },
            axisPointer: { lineStyle: { color: 'rgba(0,229,255,0.2)' } }
        },
        xAxis: {
            name: '库容 (万m³)', nameLocation: 'center', nameGap: 35,
            nameTextStyle: { color: '#8899b4', fontSize: 12 },
            axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
            axisLabel: { color: '#8899b4', fontSize: 11 }
        },
        yAxis: {
            name: '水位 (m)', nameLocation: 'center', nameGap: 45,
            min: Math.floor(yMin - yPad), max: +(yMax + yPad).toFixed(2),
            nameTextStyle: { color: '#8899b4', fontSize: 12 },
            axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
            axisLabel: { color: '#8899b4', fontSize: 11 }
        },
        dataZoom: [{ type: 'inside' }],
        toolbox: {
            feature: { saveAsImage: { title: '保存图片' } },
            iconStyle: { borderColor: '#8899b4' }
        },
        series: [{
            type: 'line', data: x.map((v, i) => [v, y[i]]),
            smooth: true,
            lineStyle: { color: '#00e5ff', width: 2.5, shadowBlur: 10, shadowColor: 'rgba(0,229,255,0.5)' },
            symbol: 'circle', symbolSize: 6,
            itemStyle: { color: '#00e5ff', borderColor: '#00e5ff', borderWidth: 1 },
            areaStyle: {
                color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                    { offset: 0, color: 'rgba(0,229,255,0.15)' },
                    { offset: 1, color: 'rgba(0,229,255,0.01)' }
                ])
            },
        }],
        grid: { left: 65, right: 30, top: 20, bottom: 50 },
    });
}

function drawChartZQ(data) {
    const chart = initChart('chartZQ');
    if (!chart || !data) return;
    const x = data.map(r => r.col2); // 流量
    const y = data.map(r => r.col1); // 水位
    const yMin = Math.min(...y);
    const yMax = Math.max(...y);
    const yPad = Math.max((yMax - yMin) * 0.08, 0.5);
    chart.setOption({
        backgroundColor: 'transparent',
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(10,15,31,0.95)',
            borderColor: 'rgba(124,77,255,0.3)',
            textStyle: { color: '#e0e8f0', fontSize: 12 },
            axisPointer: { lineStyle: { color: 'rgba(124,77,255,0.2)' } }
        },
        xAxis: {
            name: '泄流 (m³/s)', nameLocation: 'center', nameGap: 35,
            nameTextStyle: { color: '#8899b4', fontSize: 12 },
            axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
            axisLabel: { color: '#8899b4', fontSize: 11 }
        },
        yAxis: {
            name: '水位 (m)', nameLocation: 'center', nameGap: 45,
            min: Math.floor(yMin - yPad), max: +(yMax + yPad).toFixed(2),
            nameTextStyle: { color: '#8899b4', fontSize: 12 },
            axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
            axisLabel: { color: '#8899b4', fontSize: 11 }
        },
        dataZoom: [{ type: 'inside' }],
        toolbox: {
            feature: { saveAsImage: { title: '保存图片' } },
            iconStyle: { borderColor: '#8899b4' }
        },
        series: [{
            type: 'line', data: x.map((v, i) => [v, y[i]]),
            smooth: true,
            lineStyle: { color: '#7c4dff', width: 2.5, shadowBlur: 10, shadowColor: 'rgba(124,77,255,0.5)' },
            symbol: 'circle', symbolSize: 6,
            itemStyle: { color: '#7c4dff', borderColor: '#7c4dff', borderWidth: 1 },
            areaStyle: {
                color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                    { offset: 0, color: 'rgba(124,77,255,0.15)' },
                    { offset: 1, color: 'rgba(124,77,255,0.01)' }
                ])
            },
        }],
        grid: { left: 65, right: 30, top: 20, bottom: 50 },
    });
}

function drawChartFlood(data) {
    // Flood preview is shown in the result area after calculation;
    // no dedicated standalone preview chart needed.
}

function drawChartResult(chartData) {
    const chart = initChart('chartResult');
    if (!chart || !chartData) return;

    // Activate result tab
    $$('.chart-tab').forEach(t => t.classList.remove('active'));
    $('[data-tab="chartResult"]').classList.add('active');
    $('#chartZVContainer').classList.add('hidden');
    $('#chartZQContainer').classList.add('hidden');
    $('#chartResultContainer').classList.remove('hidden');

    // Compute ranges so axes fit data tightly
    const zMin = Math.min(...chartData.water_level);
    const zMax = Math.max(...chartData.water_level);
    const zRange = zMax - zMin;
    const zPad = zRange * 0.08; // 8% padding
    const qMax = Math.max(...chartData.inflow, ...chartData.outflow);

    chart.setOption({
        backgroundColor: 'transparent',
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(10,15,31,0.95)',
            borderColor: 'rgba(0,229,255,0.3)',
            textStyle: { color: '#e0e8f0', fontSize: 12 },
            axisPointer: {
                type: 'cross',
                crossStyle: { color: 'rgba(0,229,255,0.2)' },
                lineStyle: { color: 'rgba(0,229,255,0.2)', type: 'dashed' }
            }
        },
        legend: {
            data: ['入库流量', '出库流量', '库水位'],
            top: 8, right: 10,
            textStyle: { color: '#8899b4', fontSize: 11 },
            itemGap: 16
        },
        xAxis: {
            name: '时间 (h)', nameLocation: 'center', nameGap: 30,
            nameTextStyle: { color: '#8899b4', fontSize: 12 },
            axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
            splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
            axisLabel: { color: '#8899b4', fontSize: 11 }
        },
        yAxis: [
            {
                name: '流量 (m³/s)', nameLocation: 'center', nameGap: 50,
                min: 0, max: Math.ceil(qMax * 1.08),
                nameTextStyle: { color: '#8899b4', fontSize: 12 },
                axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
                axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
                splitLine: { lineStyle: { color: 'rgba(0,229,255,0.05)' } },
                axisLabel: { color: '#8899b4', fontSize: 11 }
            },
            {
                name: '库水位 (m)', nameLocation: 'center', nameGap: 50,
                min: Math.floor(zMin - zPad), max: +(zMax + zPad).toFixed(2),
                nameTextStyle: { color: '#8899b4', fontSize: 12 },
                axisLine: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
                axisTick: { lineStyle: { color: 'rgba(0,229,255,0.15)' } },
                splitLine: { show: false },
                axisLabel: { color: '#8899b4', fontSize: 11 }
            },
        ],
        dataZoom: [
            { type: 'inside', xAxisIndex: 0 },
            { type: 'slider', xAxisIndex: 0, bottom: 5, height: 20,
              borderColor: 'rgba(0,229,255,0.15)', backgroundColor: 'rgba(10,15,31,0.6)',
              dataBackground: { lineStyle: { color: 'rgba(0,229,255,0.2)' }, areaStyle: { color: 'rgba(0,229,255,0.05)' } },
              selectedDataBackground: { lineStyle: { color: '#00e5ff' }, areaStyle: { color: 'rgba(0,229,255,0.15)' } },
              handleStyle: { color: '#00e5ff' }, textStyle: { color: '#8899b4' }
            }
        ],
        toolbox: {
            feature: { saveAsImage: { title: '保存图片' } },
            iconStyle: { borderColor: '#8899b4' }
        },
        series: [
            {
                name: '入库流量', type: 'line',
                data: chartData.time.map((t, i) => [t, chartData.inflow[i]]),
                smooth: true, symbol: 'none',
                lineStyle: { color: '#ff5252', width: 2.5, shadowBlur: 8, shadowColor: 'rgba(255,82,82,0.4)' },
                areaStyle: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(255,82,82,0.12)' },
                        { offset: 1, color: 'rgba(255,82,82,0.0)' }
                    ])
                },
            },
            {
                name: '出库流量', type: 'line',
                data: chartData.time.map((t, i) => [t, chartData.outflow[i]]),
                smooth: true, symbol: 'none',
                lineStyle: { color: '#00e676', width: 2.5, shadowBlur: 8, shadowColor: 'rgba(0,230,118,0.4)' },
                areaStyle: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(0,230,118,0.1)' },
                        { offset: 1, color: 'rgba(0,230,118,0.0)' }
                    ])
                },
            },
            {
                name: '库水位', type: 'line', yAxisIndex: 1,
                data: chartData.time.map((t, i) => [t, chartData.water_level[i]]),
                smooth: true, symbol: 'none',
                lineStyle: { color: '#ffab00', width: 2.5, shadowBlur: 8, shadowColor: 'rgba(255,171,0,0.4)' },
                areaStyle: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(255,171,0,0.12)' },
                        { offset: 1, color: 'rgba(255,171,0,0.0)' }
                    ])
                },
            },
        ],
        grid: { left: 65, right: 80, top: 40, bottom: 60 },
    });
}

// Tab switching
$$('.chart-tab').forEach(tab => {
    tab.addEventListener('click', () => {
        const target = tab.dataset.tab;
        $$('.chart-tab').forEach(t => t.classList.remove('active'));
        tab.classList.add('active');
        $('#chartZVContainer').classList.toggle('hidden', target !== 'chartZV');
        $('#chartZQContainer').classList.toggle('hidden', target !== 'chartZQ');
        $('#chartResultContainer').classList.toggle('hidden', target !== 'chartResult');
        // Resize chart after showing
        setTimeout(() => {
            const chartId = target === 'chartZV' ? 'chartZV' : target === 'chartZQ' ? 'chartZQ' : 'chartResult';
            if (state.charts[chartId]) state.charts[chartId].resize();
        }, 100);
    });
});

// ===== Calculation =====
$('#btnCalculate').addEventListener('click', runCalculation);

async function runCalculation() {
    if (!state.zv.loaded || !state.zq.loaded || !state.flood.loaded) {
        toast('请先导入全部三组数据', 'error');
        return;
    }

    const zStart = parseFloat($('#zStart').value);
    const dt = parseFloat($('#dt').value);
    const z0 = parseFloat($('#z0').value) || 0;
    const method = parseInt($('#method').value);

    if (isNaN(zStart)) { toast('请输入起调水位', 'error'); $('#zStart').focus(); return; }
    if (isNaN(dt) || dt <= 0) { toast('演算时段必须大于0', 'error'); $('#dt').focus(); return; }

    // Button loading state
    const btn = $('#btnCalculate');
    btn.disabled = true;
    btn.innerHTML = '<span class="btn-icon spinning">⏳</span> 计算中...';

    // Skeleton in chart area
    $('#chartResultContainer').classList.remove('hidden');
    $('#chartZVContainer').classList.add('hidden');
    $('#chartZQContainer').classList.add('hidden');
    $$('.chart-tab').forEach(t => t.classList.remove('active'));
    $('[data-tab="chartResult"]').classList.add('active');

    const startTime = performance.now();

    try {
        const resp = await fetch('/api/calculate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                zv_data: state.zv.data,
                zq_data: state.zq.data,
                flood_data: state.flood.data,
                z_start: zStart,
                z0: z0,
                dt: dt,
                method: method,
            }),
        });

        const result = await resp.json();
        const elapsed = ((performance.now() - startTime) / 1000).toFixed(3);

        if (!resp.ok) {
            toast(result.error || '计算失败', 'error');
            btn.disabled = false;
            btn.innerHTML = '<span class="btn-icon">▶</span> 开始调洪演算';
            return;
        }

        // Store results
        state.results = result.results;
        state.summary = result.summary;
        state.chartData = result.chart_data;

        // Update summary cards
        $('#valVStart').textContent = formatNum(result.summary.v_start);
        $('#valZMax').textContent = formatNum(result.summary.z_max);
        $('#valVMax').textContent = formatNum(result.summary.v_max);
        $('#valQMax').textContent = formatNum(result.summary.q_max);
        $('#valVRet').textContent = formatNum(result.summary.v_retention);

        // Update result table
        const tbody = $('#resultTable tbody');
        tbody.innerHTML = result.results.map((r, i) =>
            `<tr>
                <td>${i + 1}</td>
                <td>${formatNum(r.time)}</td>
                <td>${formatNum(r.inflow_avg, 4)}</td>
                <td>${formatNum(r.outflow_avg, 4)}</td>
                <td>${formatNum(r.z_begin, 4)}</td>
                <td>${formatNum(r.z_end, 4)}</td>
                <td>${formatNum(r.v_begin, 4)}</td>
                <td>${formatNum(r.v_end, 4)}</td>
            </tr>`
        ).join('');

        // Show result sections
        $('#resultEmpty').classList.add('hidden');
        $('#summaryCards').classList.remove('hidden');
        $('#resultTableWrapper').classList.remove('hidden');
        $('#btnExport').classList.remove('hidden');
        $('#computeTime').classList.remove('hidden');
        $('#computeTime').textContent = `计算完成，耗时 ${elapsed} 秒`;

        // Draw result chart
        drawChartResult(result.chart_data);

        // Update progress
        updateStep(3);

        toast(`计算完成，耗时 ${elapsed}s`, 'success');

    } catch (err) {
        toast(`网络错误: ${err.message}`, 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<span class="btn-icon">▶</span> 开始调洪演算';
    }
}

// ===== Excel Export =====
function exportExcel() {
    if (!state.results || !state.summary) {
        toast('没有可导出的结果', 'error');
        return;
    }
    // Build workbook with SheetJS
    const rows = [['序号', '时间(h)', '时段平均入库(m³/s)', '时段平均出库(m³/s)',
        '时段初水位(m)', '时段末水位(m)', '时段初库容(万m³)', '时段末库容(万m³)']];
    state.results.forEach((r, i) => {
        rows.push([i + 1, r.time, r.inflow_avg, r.outflow_avg,
            r.z_begin, r.z_end, r.v_begin, r.v_end]);
    });

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(rows);

    // Column widths
    ws['!cols'] = rows[0].map(() => ({ wch: 18 }));

    XLSX.utils.book_append_sheet(wb, ws, '调洪成果');
    XLSX.writeFile(wb, '调洪成果.xlsx');
    toast('Excel 导出成功', 'success');
}

// ===== Keyboard Shortcuts =====
document.addEventListener('keydown', (e) => {
    if (e.ctrlKey && e.key === 'Enter') {
        e.preventDefault();
        if (!$('#btnCalculate').disabled) runCalculation();
    }
    if (e.key === 'Escape') {
        closePasteModal();
    }
});

// ===== Init =====
document.addEventListener('DOMContentLoaded', () => {
    setupFileUpload('ZV', 'zv');
    setupFileUpload('ZQ', 'zq');
    setupFileUpload('Flood', 'flood');

    // Init empty charts
    initChart('chartZV');
    initChart('chartZQ');
    initChart('chartResult');

    // Set chart containers to show first tab by default
    const chartZV = echarts.init($('#chartZV'), 'dark');
    chartZV.setOption({
        backgroundColor: 'transparent',
        xAxis: { show: false },
        yAxis: { show: false },
        series: [],
        graphic: {
            type: 'text', left: 'center', top: 'middle',
            style: { text: '请先导入数据', fill: '#4a5970', fontSize: 16, fontFamily: 'Microsoft YaHei' },
        },
    });
    state.charts['chartZV'] = chartZV;

    const chartZQ = echarts.init($('#chartZQ'), 'dark');
    chartZQ.setOption({
        backgroundColor: 'transparent',
        xAxis: { show: false },
        yAxis: { show: false },
        series: [],
        graphic: {
            type: 'text', left: 'center', top: 'middle',
            style: { text: '请先导入数据', fill: '#4a5970', fontSize: 16, fontFamily: 'Microsoft YaHei' },
        },
    });
    state.charts['chartZQ'] = chartZQ;

    const chartResult = echarts.init($('#chartResult'), 'dark');
    chartResult.setOption({
        backgroundColor: 'transparent',
        xAxis: { show: false },
        yAxis: { show: false },
        series: [],
        graphic: {
            type: 'text', left: 'center', top: 'middle',
            style: { text: '计算完成后显示', fill: '#4a5970', fontSize: 16, fontFamily: 'Microsoft YaHei' },
        },
    });
    state.charts['chartResult'] = chartResult;

    window.addEventListener('resize', () => {
        Object.values(state.charts).forEach(c => { try { c.resize(); } catch (e) {} });
    });

    // Auto-fill z_start from leakage curve first data
    updateStep(0);
    checkAllReady();
});
