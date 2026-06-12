"""Take screenshots of AquaWater app for article"""
import os, sys, time, json, base64
from playwright.sync_api import sync_playwright

OUTPUT_DIR = os.path.dirname(os.path.abspath(__file__))
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Sample data to inject via JS
ZV_DATA = [
    [120, 0], [122, 50], [124, 150], [126, 320], [128, 560],
    [130, 880], [132, 1280], [134, 1800], [136, 2450], [138, 3200],
    [140, 4100], [142, 5100], [144, 6300], [146, 7600], [148, 9200],
]
ZQ_DATA = [
    [120, 0], [122, 30], [124, 90], [126, 180], [128, 300],
    [130, 450], [132, 630], [134, 850], [136, 1100], [138, 1400],
    [140, 1750], [142, 2150], [144, 2600], [146, 3100], [148, 3650],
]
FLOOD_DATA = [
    [0, 0], [6.592, 57.45], [7.952, 156], [8.672, 351], [9.232, 547.5],
    [9.792, 742.5], [10.192, 889.5], [10.752, 979.5], [11.712, 889.5],
    [12.832, 742.5], [14.992, 547.5], [18.192, 351], [26.432, 156],
    [35.232, 57.45], [42.912, 4.5], [58.272, 0],
]

def inject_data(page, data_type, data):
    """Inject data into the app's state and draw chart"""
    js = f"""
    const data = {json.dumps(data)};
    const typedData = data.map(d => ({{col1: d[0], col2: d[1]}}));
    if ('{data_type}' === 'zv') {{
        state.zv.data = typedData;
        state.zv.loaded = true;
        document.querySelector('#tagZV').textContent = '✓ ' + typedData.length + ' 条';
        document.querySelector('#tagZV').className = 'status-tag loaded';
        document.querySelector('#tableWrapperZV').classList.remove('hidden');
        document.querySelector('#actionsZV').classList.remove('hidden');
        document.querySelector('#uploadZV').classList.add('hidden');
        const sd = document.querySelector('#cardZV .sample-download');
        if (sd) sd.classList.add('hidden');
        // render table
        const tbody = document.querySelector('#tableZV tbody');
        tbody.innerHTML = typedData.map((r,i) => '<tr><td>'+(i+1)+'</td><td>'+r.col1.toFixed(0)+'</td><td>'+r.col2.toFixed(0)+'</td></tr>').join('');
    }} else if ('{data_type}' === 'zq') {{
        state.zq.data = typedData;
        state.zq.loaded = true;
        document.querySelector('#tagZQ').textContent = '✓ ' + typedData.length + ' 条';
        document.querySelector('#tagZQ').className = 'status-tag loaded';
        document.querySelector('#tableWrapperZQ').classList.remove('hidden');
        document.querySelector('#actionsZQ').classList.remove('hidden');
        document.querySelector('#uploadZQ').classList.add('hidden');
        const sd2 = document.querySelector('#cardZQ .sample-download');
        if (sd2) sd2.classList.add('hidden');
        const tbody2 = document.querySelector('#tableZQ tbody');
        tbody2.innerHTML = typedData.map((r,i) => '<tr><td>'+(i+1)+'</td><td>'+r.col1.toFixed(0)+'</td><td>'+r.col2.toFixed(0)+'</td></tr>').join('');
    }} else if ('{data_type}' === 'flood') {{
        state.flood.data = typedData;
        state.flood.loaded = true;
        document.querySelector('#tagFlood').textContent = '✓ ' + typedData.length + ' 条';
        document.querySelector('#tagFlood').className = 'status-tag loaded';
        document.querySelector('#tableWrapperFlood').classList.remove('hidden');
        document.querySelector('#actionsFlood').classList.remove('hidden');
        document.querySelector('#uploadFlood').classList.add('hidden');
        const sd3 = document.querySelector('#cardFlood .sample-download');
        if (sd3) sd3.classList.add('hidden');
        const tbody3 = document.querySelector('#tableFlood tbody');
        tbody3.innerHTML = typedData.map((r,i) => '<tr><td>'+(i+1)+'</td><td>'+r.col1.toFixed(3)+'</td><td>'+r.col2.toFixed(1)+'</td></tr>').join('');
    }}
    // Update status
    const loadedCount = [state.zv, state.zq, state.flood].filter(d => d.loaded).length;
    document.querySelector('#dataStatus').textContent = loadedCount + '/3 已导入';
    document.querySelector('#dataStatus').className = 'section-badge partial';
    """
    page.evaluate(js)

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page(viewport={"width": 1400, "height": 900})
    page.goto("http://localhost:8090", wait_until="networkidle")
    time.sleep(1)

    # Screenshot 1: Empty state (before data import)
    page.screenshot(path=os.path.join(OUTPUT_DIR, "01_initial.png"), full_page=False)
    print("[OK] 01_initial.png - Empty state")

    # Inject ZV data and wait for chart
    inject_data(page, 'zv', ZV_DATA)
    page.evaluate("drawChartZV(state.zv.data); switchChartTab('chartZV')")
    time.sleep(1)
    page.screenshot(path=os.path.join(OUTPUT_DIR, "02_zv_imported.png"), full_page=False)
    print("[OK] 02_zv_imported.png - ZV data imported with chart")

    # Inject ZQ data
    inject_data(page, 'zq', ZQ_DATA)
    page.evaluate("drawChartZQ(state.zq.data); switchChartTab('chartZQ')")
    time.sleep(0.5)
    page.screenshot(path=os.path.join(OUTPUT_DIR, "03_zq_imported.png"), full_page=False)
    print("[OK] 03_zq_imported.png - ZQ data imported with chart")

    # Inject Flood data
    inject_data(page, 'flood', FLOOD_DATA)
    page.evaluate("drawChartFlood(state.flood.data); switchChartTab('chartResult')")
    time.sleep(0.5)
    page.screenshot(path=os.path.join(OUTPUT_DIR, "04_flood_imported.png"), full_page=False)
    print("[OK] 04_flood_imported.png - Flood data imported with chart")

    # Now trigger calculation via fetch API
    calc_result = page.evaluate("""
    async () => {
        const resp = await fetch('/api/calculate', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({
                zv_data: state.zv.data,
                zq_data: state.zq.data,
                flood_data: state.flood.data,
                z_start: 130,
                z0: 880,
                dt: 0.1,
                method: 0
            })
        });
        return await resp.json();
    }
    """)
    if 'error' in calc_result:
        print(f"Calc error: {calc_result['error']}")
    else:
        # Inject results into the page
        page.evaluate(f"""
        const results = {json.dumps(calc_result['results'])};
        const summary = {json.dumps(calc_result['summary'])};
        // Update summary cards
        document.querySelector('#summaryCards').innerHTML = `
            <div class="summary-card"><div class="summary-label">起调水位</div><div class="summary-value">${{summary.z_start}}</div><div class="summary-unit">m</div></div>
            <div class="summary-card"><div class="summary-label">起调库容</div><div class="summary-value">${{summary.v_start}}</div><div class="summary-unit">万m³</div></div>
            <div class="summary-card highlight"><div class="summary-label">调洪最高水位</div><div class="summary-value">${{summary.z_max}}</div><div class="summary-unit">m</div></div>
            <div class="summary-card highlight"><div class="summary-label">最大下泄流量</div><div class="summary-value">${{summary.q_max}}</div><div class="summary-unit">m³/s</div></div>
            <div class="summary-card"><div class="summary-label">滞洪库容</div><div class="summary-value">${{summary.v_retention}}</div><div class="summary-unit">万m³</div></div>
        `;
        // Draw result chart
        const chartData = {{
            time: results.map(r => r.time),
            inflow: results.map(r => r.inflow_avg),
            outflow: results.map(r => r.outflow_avg),
            water_level: results.map(r => r.z_end),
        }};
        drawChartResult(chartData);
        // Show results section
        document.querySelector('#summaryCards').parentElement.classList.remove('hidden');
        document.querySelector('#resultTableWrapper').classList.remove('hidden');
        document.querySelector('#btnExport').classList.remove('hidden');
        // Render result table
        const resultTbody = document.querySelector('#resultTable tbody');
        resultTbody.innerHTML = results.map((r,i) =>
            '<tr><td>'+(i+1)+'</td><td>'+r.time.toFixed(1)+'</td><td>'+r.inflow_avg.toFixed(1)+'</td><td>'+r.outflow_avg.toFixed(1)+'</td><td>'+r.z_begin.toFixed(2)+'</td><td>'+r.z_end.toFixed(2)+'</td><td>'+r.v_begin.toFixed(0)+'</td><td>'+r.v_end.toFixed(0)+'</td></tr>'
        ).join('');
        // Update step
        document.querySelectorAll('#progressSteps .step').forEach(s => s.classList.add('done'));
        document.querySelector('#btnCalculate').disabled = true;
        document.querySelector('#btnCalculate').innerHTML = '<span class="btn-icon">✓</span> 计算完成';
        """)
        time.sleep(1)
        # Scroll result area into view
        page.evaluate("document.querySelector('#sectionResults').scrollIntoView()")
        time.sleep(0.5)
        page.screenshot(path=os.path.join(OUTPUT_DIR, "05_results.png"), full_page=False)
        print("[OK] 05_results.png - Full results after calculation")

        # Scroll back to top for full-page screenshot
        page.evaluate("window.scrollTo(0, 0)")
        time.sleep(0.5)
        page.screenshot(path=os.path.join(OUTPUT_DIR, "06_full_app.png"), full_page=True)
        print("[OK] 06_full_app.png - Full page overview")

    browser.close()
    print("\n[DONE] All screenshots saved to:", OUTPUT_DIR)
