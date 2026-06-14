"""截取公众号文章所需截图 — AquaWater Skill 介绍"""
import asyncio
import os
import sys
from pathlib import Path

BASE = Path(r"G:\4-软件研发\AquaWater调洪演算程序(CC)")
SCREENSHOT_DIR = BASE / "screenshots"

async def main():
    from playwright.async_api import async_playwright

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context(
            viewport={"width": 1400, "height": 900},
            device_scale_factor=1,
        )

        # ---- 1. Terminal mock ----
        print("1/5 Capturing terminal mock...")
        page = await context.new_page()
        await page.goto(f"file:///{SCREENSHOT_DIR / 'terminal_mock.html'}")
        await page.wait_for_timeout(500)
        await page.screenshot(path=str(SCREENSHOT_DIR / "skill_terminal.png"))
        await page.close()

        # ---- 2. Web app data import ----
        print("2/5 Capturing web app data import...")
        page = await context.new_page()
        await page.goto("http://localhost:8090")
        await page.wait_for_timeout(1500)

        # Inject ZV data
        await page.evaluate("""
            fetch('/api/preview', {
                method: 'POST',
                body: (() => {
                    const fd = new FormData();
                    fd.append('file', new Blob([`986\t0
990\t5
995\t20
1000\t44
1005\t133
1010\t244
1014.3\t378
1015.5\t438
1020\t644
1025\t942
1030\t1301
1035\t1740
1040\t2273`], {type:'text/plain'}), 'zv.txt');
                    return fd;
                })()
            }).then(r=>r.json()).then(d=>{window.__zvData=d.data;});
        """)
        await page.wait_for_timeout(800)

        # Inject ZQ data
        await page.evaluate("""
            fetch('/api/preview', {
                method: 'POST',
                body: (() => {
                    const fd = new FormData();
                    fd.append('file', new Blob([`1024.00 \\t0.00
1024.20 \\t5.66
1024.40 \\t16.00
1024.60 \\t29.40
1024.80 \\t45.26
1025.00 \\t63.25
1025.20 \\t83.15
1025.40 \\t104.78
1025.60 \\t128.01
1025.80 \\t152.75
1026.00 \\t178.91
1026.20 \\t206.40
1026.40 \\t235.18
1026.60 \\t265.18
1026.80 \\t296.36
1027.00 \\t328.67
1027.20 \\t362.08
1027.40 \\t396.55
1027.60 \\t432.05
1027.80 \\t468.55
1028.00 \\t506.02
1028.20 \\t544.44
1028.40 \\t583.79
1028.60 \\t624.04
1028.80 \\t665.18
1029.00 \\t707.18
1029.20 \\t750.04
1029.40 \\t793.72
1029.60 \\t838.22
1029.80 \\t883.53
1030.00 \\t929.62`], {type:'text/plain'}), 'zq.txt');
                    return fd;
                })()
            }).then(r=>r.json()).then(d=>{window.__zqData=d.data;});
        """)
        await page.wait_for_timeout(800)

        # Inject Flood data
        await page.evaluate("""
            fetch('/api/preview', {
                method: 'POST',
                body: (() => {
                    const fd = new FormData();
                    fd.append('file', new Blob([`0\t0
6.592\t57.45
7.952\t156
8.672\t351
9.232\t547.5
9.792\t742.5
10.192\t889.5
10.752\t979.5
11.712\t889.5
12.832\t742.5
14.992\t547.5
18.192\t351
26.432\t156
35.232\t57.45
42.912\t4.5
58.272\t0`], {type:'text/plain'}), 'flood.txt');
                    return fd;
                })()
            }).then(r=>r.json()).then(d=>{window.__floodData=d.data;});
        """)
        await page.wait_for_timeout(1000)

        # Now trigger the UI to show data tables and charts
        await page.evaluate("""
            if (window.__zvData && window.__zqData && window.__floodData) {
                var evt = new CustomEvent('data-loaded', {detail:{zv:window.__zvData, zq:window.__zqData, flood:window.__floodData}});
                document.dispatchEvent(evt);
            }
        """)
        await page.wait_for_timeout(1500)

        # Try to directly manipulate the page state
        await page.evaluate("""
            // Directly call internal state setters if available
            if (typeof state !== 'undefined') {
                if (window.__zvData) state.zvData = window.__zvData;
                if (window.__zqData) state.zqData = window.__zqData;
                if (window.__floodData) state.floodData = window.__floodData;
            }
            // Try to render tables by dispatching events
            ['zvData', 'zqData', 'floodData'].forEach(function(key) {
                var el = document.querySelector('[data-file-type="' + key + '"]');
                if (el) el.dispatchEvent(new Event('change'));
            });
        """)
        await page.wait_for_timeout(1000)

        await page.screenshot(path=str(SCREENSHOT_DIR / "web_data_import.png"), full_page=False)
        await page.close()

        # ---- 3. Web app results via calculate API ----
        print("3/5 Capturing web app results...")
        page = await context.new_page()
        await page.goto("http://localhost:8090")
        await page.wait_for_timeout(1000)

        # Use browser fetch via page.evaluate with Promise pattern
        result_json = await page.evaluate("""
            () => {
                const zv = [{col1:986,col2:0},{col1:990,col2:5},{col1:995,col2:20},{col1:1000,col2:44},{col1:1005,col2:133},{col1:1010,col2:244},{col1:1014.3,col2:378},{col1:1015.5,col2:438},{col1:1020,col2:644},{col1:1025,col2:942},{col1:1030,col2:1301},{col1:1035,col2:1740},{col1:1040,col2:2273}];
                const zq = [{col1:1024,col2:0},{col1:1024.2,col2:5.66},{col1:1024.4,col2:16},{col1:1024.6,col2:29.4},{col1:1024.8,col2:45.26},{col1:1025,col2:63.25},{col1:1025.2,col2:83.15},{col1:1025.4,col2:104.78},{col1:1025.6,col2:128.01},{col1:1025.8,col2:152.75},{col1:1026,col2:178.91},{col1:1026.2,col2:206.4},{col1:1026.4,col2:235.18},{col1:1026.6,col2:265.18},{col1:1026.8,col2:296.36},{col1:1027,col2:328.67},{col1:1027.2,col2:362.08},{col1:1027.4,col2:396.55},{col1:1027.6,col2:432.05},{col1:1027.8,col2:468.55},{col1:1028,col2:506.02},{col1:1028.2,col2:544.44},{col1:1028.4,col2:583.79},{col1:1028.6,col2:624.04},{col1:1028.8,col2:665.18},{col1:1029,col2:707.18},{col1:1029.2,col2:750.04},{col1:1029.4,col2:793.72},{col1:1029.6,col2:838.22},{col1:1029.8,col2:883.53},{col1:1030,col2:929.62}];
                const flood = [{col1:0,col2:0},{col1:6.592,col2:57.45},{col1:7.952,col2:156},{col1:8.672,col2:351},{col1:9.232,col2:547.5},{col1:9.792,col2:742.5},{col1:10.192,col2:889.5},{col1:10.752,col2:979.5},{col1:11.712,col2:889.5},{col1:12.832,col2:742.5},{col1:14.992,col2:547.5},{col1:18.192,col2:351},{col1:26.432,col2:156},{col1:35.232,col2:57.45},{col1:42.912,col2:4.5},{col1:58.272,col2:0}];
                return fetch('/api/calculate', {
                    method: 'POST',
                    headers: {'Content-Type':'application/json'},
                    body: JSON.stringify({zv_data:zv, zq_data:zq, flood_data:flood, z_start:1024, z0:0, dt:0.1, method:0})
                }).then(r => r.json());
            }
        """)
        print("  Calculation done, z_max:", result_json.get('summary',{}).get('z_max'))

        # Inject results into page state and render
        await page.evaluate("""
            (data) => {
                if (typeof drawChartResult === 'function') {
                    drawChartResult(data.chart_data);
                }
                if (typeof updateSummaryCards === 'function') {
                    updateSummaryCards(data.summary);
                }
                if (typeof renderResultsTable === 'function') {
                    renderResultsTable(data.results);
                }
            }
        """, result_json)
        await page.wait_for_timeout(2000)

        # Switch to result chart tab if possible
        await page.evaluate("""
            var tab = document.querySelector('[data-tab="chartResult"]');
            if (tab) tab.click();
        """)
        await page.wait_for_timeout(1000)

        await page.screenshot(path=str(SCREENSHOT_DIR / "web_results.png"), full_page=False)
        await page.close()

        # ---- 4. Standalone chart HTML ----
        print("4/5 Capturing standalone chart...")
        page = await context.new_page()
        chart_path = BASE / "调洪演算成果.html"
        await page.goto(f"file:///{chart_path}")
        await page.wait_for_timeout(2000)
        # Wait for ECharts to render
        await page.wait_for_selector("canvas", timeout=5000)
        await page.wait_for_timeout(1500)
        await page.screenshot(path=str(SCREENSHOT_DIR / "standalone_chart.png"), full_page=True)
        await page.close()

        # ---- 5. GitHub release (try, fallback to local mock) ----
        print("5/5 Capturing GitHub release...")
        page = await context.new_page()
        try:
            await page.goto("https://github.com/aquawater123/AquaWater/releases", timeout=15000)
            await page.wait_for_timeout(3000)
            await page.screenshot(path=str(SCREENSHOT_DIR / "github_release.png"))
        except Exception as e:
            print(f"  GitHub not accessible ({e}), using mock")
            # Fallback: create a simple mock page
            await page.set_content(f"""
<!DOCTYPE html>
<html lang="zh-CN">
<head><meta charset="UTF-8"><style>
*{{margin:0;padding:0;box-sizing:border-box;}}
body{{background:#0d1117;color:#c9d1d9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;padding:40px;display:flex;justify-content:center;}}
.card{{width:680px;background:#161b22;border:1px solid #30363d;border-radius:8px;overflow:hidden;}}
.header{{background:#0d1117;padding:20px 24px;border-bottom:1px solid #30363d;}}
.header h2{{color:#f0f6fc;font-size:20px;}}
.header p{{color:#8b949e;font-size:13px;margin-top:4px;}}
.body{{padding:24px;}}
.asset{{display:flex;align-items:center;justify-content:space-between;padding:12px 16px;background:#0d1117;border:1px solid #30363d;border-radius:6px;}}
.asset-name{{color:#58a6ff;font-size:15px;font-weight:600;}}
.asset-size{{color:#8b949e;font-size:12px;}}
.asset-dl{{display:inline-block;padding:6px 20px;background:#238636;color:#fff;border-radius:6px;text-decoration:none;font-size:13px;font-weight:600;}}
.desc{{margin-top:20px;}}
.desc h3{{color:#f0f6fc;font-size:16px;margin-bottom:8px;}}
.desc ul{{margin-left:20px;color:#8b949e;font-size:13px;line-height:1.8;}}
.tag{{display:inline-block;background:#1f6feb33;color:#58a6ff;padding:2px 8px;border-radius:12px;font-size:11px;margin-right:4px;}}
</style></head>
<body>
<div class="card">
  <div class="header">
    <h2><span class="tag">v1.0.0</span> AquaWater 水库调洪演算 Skill</h2>
    <p>aquawater123 released this 2026-06-14</p>
  </div>
  <div class="body">
    <div class="asset">
      <div><div class="asset-name">📦 水库调洪.skill</div><div class="asset-size">19.3 KB · 下载后 claude install 一键安装</div></div>
      <a class="asset-dl" href="#">⬇ 下载</a>
    </div>
    <div class="desc">
      <h3>功能</h3>
      <ul>
        <li>三种演算方法：积分法（专利公式）、龙格库塔法（四阶）、迭代法（二分）</li>
        <li>输出调洪最高水位、最大泄流、滞洪库容等特征值</li>
        <li>生成 ECharts 交互式成果图表（含完整数据表 + CSV 下载）</li>
        <li>含示例数据（库容曲线、泄流曲线、P=5% 洪水过程线）</li>
      </ul>
    </div>
  </div>
</div>
</body>
</html>""")
            await page.wait_for_timeout(500)
            await page.screenshot(path=str(SCREENSHOT_DIR / "github_release.png"), full_page=True)
        await page.close()

        await browser.close()
        print("\nDone! Screenshots saved to:", str(SCREENSHOT_DIR))

if __name__ == "__main__":
    asyncio.run(main())
