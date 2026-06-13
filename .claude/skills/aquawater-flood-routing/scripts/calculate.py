"""水库调洪演算 CLI 工具 — AquaWater Skill 自带脚本

用法:
    python calculate.py --zv 库容曲线.txt --zq 泄流曲线.txt --flood 洪水过程线.txt \
        --z-start 1024 [--z0 0] [--dt 0.1] [--method 0] [--chart result.html]

输出: JSON 格式的计算结果到 stdout
"""

import argparse
import json
import sys
import os

# 将脚本所在目录加入 path，确保能 import reservoir
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from reservoir import run_flood_routing


def parse_data_file(filepath):
    """解析二列数据文件，支持 Tab/逗号/空格 分隔，返回 [(col1, col2), ...]"""
    results = []
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            parts = line.replace('\t', ' ').replace(',', ' ').split()
            if len(parts) >= 2:
                try:
                    results.append((float(parts[0]), float(parts[1])))
                except ValueError:
                    continue
    return results


def check_monotonic(data, name):
    """检查数据第一列是否单调递增，返回问题点列表"""
    issues = []
    for i in range(1, len(data)):
        if data[i][0] <= data[i-1][0]:
            issues.append(f"  第{i}行→第{i+1}行: {data[i-1][0]} → {data[i][0]} (未递增)")
    if issues:
        print(f"⚠️ 警告: {name} 数据不单调递增:", file=sys.stderr)
        for issue in issues:
            print(issue, file=sys.stderr)
    return len(issues) == 0


def generate_chart_html(chart_data, summary, results, output_path):
    """生成独立的 ECharts 交互式图表 HTML 文件（含数据表+下载功能）"""
    # 水位 Y 轴自适应范围（water_level 格式: [[t, z], ...]）
    z_vals = [p[1] for p in chart_data['water_level']]
    z_min_data = min(z_vals)
    z_max_data = max(z_vals)
    z_range = z_max_data - z_min_data if z_max_data != z_min_data else 1.0
    z_axis_min = round(z_min_data - z_range * 0.08, 2)
    z_axis_max = round(z_max_data + z_range * 0.08, 2)

    # 流量 Y 轴自适应范围（inflow/outflow 格式: [[t, q], ...]）
    q_in = [p[1] for p in chart_data['inflow']]
    q_out = [p[1] for p in chart_data['outflow']]
    q_max_data = max(q_in + q_out) if (q_in or q_out) else 1
    q_axis_max = round(q_max_data * 1.08, 0)

    # 结果数据 JSON（嵌入页面供下载使用）
    results_json = json.dumps(results, ensure_ascii=False)

    html = f'''<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>调洪演算成果</title>
<script src="https://cdn.jsdelivr.net/npm/echarts@5.6.0/dist/echarts.min.js"></script>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  body {{ background: #0a0e17; color: #c0e0f0; font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif; padding: 20px; }}
  h1 {{ text-align: center; color: #00d4ff; margin: 0 0 6px 0; font-size: 24px; }}
  .subtitle {{ text-align: center; color: #6b9dc0; font-size: 13px; margin-bottom: 16px; }}
  .summary {{ display: flex; flex-wrap: wrap; justify-content: center; gap: 10px; margin-bottom: 20px; }}
  .card {{ background: linear-gradient(135deg, #0d2137, #0f172a); border: 1px solid #1e3a5f; border-radius: 8px; padding: 10px 16px; min-width: 130px; text-align: center; }}
  .card .label {{ font-size: 12px; color: #6b9dc0; }}
  .card .value {{ font-size: 22px; font-weight: bold; color: #00d4ff; }}
  .card .unit {{ font-size: 11px; color: #6b9dc0; }}
  .card.highlight {{ border-color: #ff6b35; }}
  .card.highlight .value {{ color: #ff6b35; }}
  #chart {{ width: 100%; height: 500px; }}
  .section-title {{ color: #00d4ff; font-size: 18px; margin: 30px 0 12px 0; padding-bottom: 8px; border-bottom: 1px solid #1e3a5f; display: flex; justify-content: space-between; align-items: center; }}
  .btn-group {{ display: flex; gap: 8px; }}
  .btn {{ padding: 6px 16px; border: 1px solid #1e5a8a; border-radius: 4px; background: #0d2847; color: #a0d8ef; cursor: pointer; font-size: 13px; font-family: inherit; transition: all 0.2s; }}
  .btn:hover {{ background: #1a4470; border-color: #3a8ac0; color: #fff; }}
  .btn.primary {{ background: #1a5a3a; border-color: #2a8a5a; }}
  .btn.primary:hover {{ background: #2a7a5a; }}
  .table-wrap {{ max-height: 500px; overflow-y: auto; border: 1px solid #1e3a5f; border-radius: 6px; }}
  .table-wrap::-webkit-scrollbar {{ width: 8px; }}
  .table-wrap::-webkit-scrollbar-track {{ background: #0a0e17; }}
  .table-wrap::-webkit-scrollbar-thumb {{ background: #1e3a5f; border-radius: 4px; }}
  table {{ width: 100%; border-collapse: collapse; font-size: 13px; }}
  thead {{ position: sticky; top: 0; z-index: 2; }}
  th {{ background: #112240; color: #00d4ff; padding: 10px 8px; text-align: center; border-bottom: 2px solid #1e3a5f; white-space: nowrap; }}
  td {{ padding: 6px 8px; text-align: center; border-bottom: 1px solid #0f1e35; }}
  tr:hover td {{ background: #0f1e35; }}
  tr:nth-child(even) td {{ background: #0a1222; }}
  tr:nth-child(even):hover td {{ background: #0f1e35; }}
  .footer {{ text-align: center; color: #4a6a8a; font-size: 12px; margin-top: 30px; padding-top: 16px; border-top: 1px solid #1e3a5f; }}
</style>
</head>
<body>
<h1>水库调洪演算成果</h1>
<p class="subtitle">起调水位 {summary['z_start']}m | 防洪限制水位 {summary.get('z0', 0)}m | 计算时段 {summary.get('dt', 0.1)}h</p>
<div class="summary">
  <div class="card"><div class="label">起调水位</div><div class="value">{summary['z_start']}</div><div class="unit">m</div></div>
  <div class="card"><div class="label">起调库容</div><div class="value">{summary['v_start']}</div><div class="unit">万m³</div></div>
  <div class="card highlight"><div class="label">最高水位 ▲</div><div class="value">{summary['z_max']}</div><div class="unit">m</div></div>
  <div class="card"><div class="label">最大库容</div><div class="value">{summary['v_max']}</div><div class="unit">万m³</div></div>
  <div class="card"><div class="label">最大下泄流量</div><div class="value">{summary['q_max']}</div><div class="unit">m³/s</div></div>
  <div class="card"><div class="label">滞洪库容</div><div class="value">{summary['v_retention']}</div><div class="unit">万m³</div></div>
</div>

<div id="chart"></div>

<div class="section-title">
  <span>逐时段成果表（共 {len(results)} 行）</span>
  <div class="btn-group">
    <button class="btn primary" onclick="downloadCSV()">📥 下载 CSV</button>
  </div>
</div>
<div class="table-wrap">
  <table>
    <thead>
      <tr>
        <th>序号</th>
        <th>时间 (h)</th>
        <th>平均入库 (m³/s)</th>
        <th>平均出库 (m³/s)</th>
        <th>时段初水位 (m)</th>
        <th>时段末水位 (m)</th>
        <th>时段初库容 (万m³)</th>
        <th>时段末库容 (万m³)</th>
      </tr>
    </thead>
    <tbody id="table-body">
    </tbody>
  </table>
</div>

<div class="footer">AquaWater 调洪演算程序 · 生成于 <span id="gen-time"></span></div>

<script>
// ===== 嵌入数据（各线自带 [x,y] 坐标）=====
var RESULTS = {results_json};
var INFLOW = {json.dumps(chart_data['inflow'])};
var OUTFLOW = {json.dumps(chart_data['outflow'])};
var WATER_LEVEL = {json.dumps(chart_data['water_level'])};

// ===== 渲染表格 =====
(function renderTable() {{
  var tbody = document.getElementById('table-body');
  var rows = '';
  for (var i = 0; i < RESULTS.length; i++) {{
    var r = RESULTS[i];
    rows += '<tr>' +
      '<td>' + (i + 1) + '</td>' +
      '<td>' + r.time + '</td>' +
      '<td>' + r.inflow_avg + '</td>' +
      '<td>' + r.outflow_avg + '</td>' +
      '<td>' + r.z_begin + '</td>' +
      '<td>' + r.z_end + '</td>' +
      '<td>' + r.v_begin + '</td>' +
      '<td>' + r.v_end + '</td>' +
      '</tr>';
  }}
  tbody.innerHTML = rows;
}})();

// ===== 下载 CSV =====
function downloadCSV() {{
  var header = ['序号','时间(h)','平均入库(m3/s)','平均出库(m3/s)','时段初水位(m)','时段末水位(m)','时段初库容(万m3)','时段末库容(万m3)'];
  var lines = [header.join(',')];
  for (var i = 0; i < RESULTS.length; i++) {{
    var r = RESULTS[i];
    lines.push([
      i + 1, r.time, r.inflow_avg, r.outflow_avg,
      r.z_begin, r.z_end, r.v_begin, r.v_end
    ].join(','));
  }}
  var csv = lines.join('\\n');
  var blob = new Blob(['\\uFEFF' + csv], {{ type: 'text/csv;charset=utf-8;' }});
  var url = URL.createObjectURL(blob);
  var a = document.createElement('a');
  a.href = url;
  a.download = '调洪演算成果.csv';
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}}

// ===== 渲染图表（value 轴，各线自带 [x,y] 独立坐标）=====
(function renderChart() {{
  var chart = echarts.init(document.getElementById('chart'));
  var option = {{
    tooltip: {{ trigger: 'axis', axisPointer: {{ type: 'cross' }} }},
    legend: {{ data: ['时段平均入库','时段平均出库','水位'], textStyle: {{ color: '#c0e0f0' }}, top: 5 }},
    grid: {{ left: 65, right: 65, top: 50, bottom: 45 }},
    xAxis: {{
      type: 'value',
      name: '时间 (h)',
      nameTextStyle: {{ color: '#6b9dc0' }},
      axisLabel: {{ color: '#8aaccc' }},
      splitLine: {{ lineStyle: {{ color: '#152040' }} }},
      min: 0
    }},
    yAxis: [
      {{
        type: 'value',
        name: '流量 (m³/s)',
        nameTextStyle: {{ color: '#6b9dc0' }},
        axisLabel: {{ color: '#c0e0f0' }},
        splitLine: {{ lineStyle: {{ color: '#152040' }} }},
        max: {q_axis_max},
        min: 0
      }},
      {{
        type: 'value',
        name: '水位 (m)',
        nameTextStyle: {{ color: '#6b9dc0' }},
        axisLabel: {{ color: '#c0e0f0' }},
        splitLine: {{ show: false }},
        min: {z_axis_min},
        max: {z_axis_max},
        scale: true
      }}
    ],
    series: [
      {{
        name: '时段平均入库', type: 'line', data: INFLOW,
        smooth: true, symbol: 'none',
        lineStyle: {{ color: '#ff4444', width: 2 }},
        itemStyle: {{ color: '#ff4444' }}
      }},
      {{
        name: '时段平均出库', type: 'line', data: OUTFLOW,
        smooth: true, symbol: 'none',
        lineStyle: {{ color: '#44ff44', width: 2 }},
        itemStyle: {{ color: '#44ff44' }}
      }},
      {{
        name: '水位', type: 'line', yAxisIndex: 1, data: WATER_LEVEL,
        smooth: true, symbol: 'none',
        lineStyle: {{ color: '#ffaa00', width: 2.5 }},
        itemStyle: {{ color: '#ffaa00' }}
      }}
    ]
  }};
  chart.setOption(option);
  window.addEventListener('resize', function() {{ chart.resize(); }});
}})();

// ===== 生成时间戳 =====
document.getElementById('gen-time').textContent = new Date().toLocaleString('zh-CN');
</script>
</body>
</html>'''
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(html)


def main():
    parser = argparse.ArgumentParser(description='水库调洪演算工具')
    parser.add_argument('--zv', required=True, help='库容曲线文件 (水位m 库容万m³)')
    parser.add_argument('--zq', required=True, help='泄流曲线文件 (水位m 流量m³/s)')
    parser.add_argument('--flood', required=True, help='洪水过程线文件 (时间h 流量m³/s)')
    parser.add_argument('--z-start', type=float, required=True, help='起调水位 (m)')
    parser.add_argument('--z0', type=float, default=0, help='防洪限制水位 (m), 默认0=不启用')
    parser.add_argument('--dt', type=float, default=0.1, help='计算时段 (h), 默认0.1')
    parser.add_argument('--method', type=int, default=0, choices=[0, 1, 2],
                        help='演算方法: 0=积分法, 1=龙格库塔法, 2=迭代法 (默认0)')
    parser.add_argument('--chart', default=None, help='输出图表 HTML 文件路径 (可选)')

    args = parser.parse_args()

    # 读取数据文件
    try:
        zv_data = parse_data_file(args.zv)
        zq_data = parse_data_file(args.zq)
        flood_data = parse_data_file(args.flood)
    except FileNotFoundError as e:
        print(json.dumps({'error': f'文件未找到: {e}'}, ensure_ascii=False))
        sys.exit(1)
    except Exception as e:
        print(json.dumps({'error': f'文件读取失败: {e}'}, ensure_ascii=False))
        sys.exit(1)

    # 数据检查
    if len(zv_data) < 2:
        print(json.dumps({'error': '库容曲线数据点不足（至少需要2个点）'}, ensure_ascii=False))
        sys.exit(1)
    if len(zq_data) < 2:
        print(json.dumps({'error': '泄流曲线数据点不足（至少需要2个点）'}, ensure_ascii=False))
        sys.exit(1)
    if len(flood_data) < 2:
        print(json.dumps({'error': '洪水过程线数据点不足（至少需要2个点）'}, ensure_ascii=False))
        sys.exit(1)

    # 单调性检查
    check_monotonic(zv_data, '库容曲线')
    check_monotonic(zq_data, '泄流曲线')

    # 水位范围检查
    z_min = min(zv_data[0][0], zq_data[0][0])
    z_max = max(zv_data[-1][0], zq_data[-1][0])
    if args.z_start < z_min or args.z_start > z_max:
        print(f'⚠️ 警告: 起调水位 {args.z_start}m 超出曲线数据范围 [{z_min}, {z_max}]m，将使用外推', file=sys.stderr)

    # 执行计算
    method_names = {0: '积分法(专利公式)', 1: '龙格库塔法(四阶)', 2: '迭代法(二分)'}
    try:
        result = run_flood_routing(zv_data, zq_data, flood_data,
                                   args.z_start, args.z0, args.dt, args.method)
    except Exception as e:
        print(json.dumps({'error': f'计算失败: {e}'}, ensure_ascii=False))
        sys.exit(1)

    # 添加元信息
    result['meta'] = {
        'method': args.method,
        'method_name': method_names[args.method],
        'dt': args.dt,
        'z0': args.z0,
        'num_steps': len(result['results']),
    }

    # 生成图表（可选）
    if args.chart:
        try:
            # 补充 summary 中的参数信息
            result['summary']['z0'] = args.z0
            result['summary']['dt'] = args.dt
            generate_chart_html(result['chart_data'], result['summary'],
                                result['results'], args.chart)
            result['chart_file'] = os.path.abspath(args.chart)
        except Exception as e:
            result['chart_error'] = str(e)

    # 输出 JSON 结果
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
