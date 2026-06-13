"""水库调洪演算核心算法 — 从 C# Reservoir.cs 移植"""

import math


class RelationLine:
    """单调递增关系线，支持分段线性插值及正反向查值"""

    def __init__(self, x, y, n):
        self.x = list(x[:n])
        self.y = list(y[:n])
        self.n = n
        self.k = [(y[i + 1] - y[i]) / (x[i + 1] - x[i]) for i in range(n - 1)]

    def get_y(self, x):
        """由自变量 x 求因变量 y（低于下界时 clamp 到下界值，不外推）"""
        if self.x[0] <= x <= self.x[-1]:
            for i in range(1, self.n):
                if x <= self.x[i]:
                    return self.k[i - 1] * (x - self.x[i - 1]) + self.y[i - 1]
        elif x < self.x[0]:
            return self.y[0]  # 低于曲线范围：clamp 到最小 y（不外推，避免物理上不合理的负值）
        else:
            return (x - self.x[-1]) * self.k[-1] + self.y[-1]

        # fallback (should not reach)
        y = self.k[-1] * (x - self.x[-1]) + self.y[-1]
        return max(y, 0)

    def get_x(self, y):
        """由因变量 y 反查自变量 x（低于下界时 clamp 到下界值，不外推）"""
        if self.y[0] <= y <= self.y[-1]:
            for i in range(1, self.n):
                if y <= self.y[i]:
                    return (y - self.y[i - 1]) / self.k[i - 1] + self.x[i - 1]
        elif y < self.y[0]:
            return self.x[0]  # 低于曲线范围：clamp 到最小 x（不外推）
        else:
            return (y - self.y[-1]) / self.k[-1] + self.x[-1]

        # fallback
        x = (y - self.y[-1]) / self.k[-1] + self.x[-1]
        return max(x, 0)


class Reservoir:
    """水库调洪演算类"""

    def __init__(self, z_now, z0, zv_z, zv_v, zv_n, zq_z, zq_q, zq_n):
        """初始化水库对象
        z_now: 初始水位 (m)
        z0: 防洪限制水位 (m)
        zv_z, zv_v, zv_n: 水位-库容曲线数据 (水位m, 库容万m³)
        zq_z, zq_q, zq_n: 水位-泄流曲线数据 (水位m, 流量m³/s)
        """
        self.z_current = z_now
        self.z_fangx = z0
        self.ztov = RelationLine(zv_z, zv_v, zv_n)
        self.ztoqm = RelationLine(zq_z, zq_q, zq_n)

    def get_current_z(self):
        return self.z_current

    def set_current_z(self, z):
        self.z_current = z

    # ---- 方法1: 迭代法 (二分法) ----
    def _c_znext_qqq(self, zt, qct, dt):
        """汛限水位逻辑：
         - 水位 < 汛限：关闭闸门 Q=0，纯蓄水填库
         - 水位 ≥ 汛限：正常 ZQ 泄流，洪水时可超汛限，回落后不低过汛限
        """
        vt = self.ztov.get_y(zt)

        # 低于汛限：闸门关闭，来水全部存蓄
        if self.z_fangx > 0 and zt < self.z_fangx:
            shimo_v1 = vt + qct * dt * 0.36
            shimo_z1 = self.ztov.get_x(shimo_v1)
            if shimo_z1 >= self.z_fangx:
                self.z_current = self.z_fangx
                v_fangx = self.ztov.get_y(self.z_fangx)
                return max(0.0, qct - (v_fangx - vt) / (dt * 0.36))
            self.z_current = shimo_z1
            return 0.0

        # 已≥汛限（或未启用）：正常迭代法演算
        e = 0.0001
        out_q = 0.0
        qt = self.ztoqm.get_y(zt)

        z0 = zt
        z2 = self.ztov.get_x(vt + (qct - qt) * dt * 0.36)
        yy0 = z2 - z0

        q2 = self.ztoqm.get_y(z2)
        v2 = vt + (qct - (qt + q2) / 2) * dt * 0.36
        yy2 = self.ztov.get_x(v2) - z2

        if abs(z2 - z0) < e:
            z_mid = (z2 + z0) / 2
            self.z_current = z_mid
            return max(0.0, qt)

        if yy0 * yy2 > 0:
            return 0

        while abs(z2 - z0) > e:
            z1 = (z0 + z2) / 2.0
            q1 = self.ztoqm.get_y(z1)
            v1 = vt + (qct - (qt + q1) / 2) * dt * 0.36
            yy1 = self.ztov.get_x(v1) - z1

            if yy1 * yy0 < 0:
                z2 = z1
            elif yy1 * yy2 < 0:
                z0 = z1
            else:
                self.z_current = z1
                return max(0.0, (qt + self.ztoqm.get_y(z1)) / 2)

        z1 = (z0 + z2) / 2
        out_q = max(0.0, (qt + self.ztoqm.get_y(z1)) / 2)

        # 防回落：自然水位低于汛限时，钳制在汛限
        if self.z_fangx > 0 and z1 < self.z_fangx:
            v_fangx = self.ztov.get_y(self.z_fangx)
            chao_q = (vt - v_fangx) / dt / 0.36 + qct
            if chao_q > 0:
                self.z_current = self.z_fangx
                return max(0.0, chao_q)

        self.z_current = z1
        return out_q

    # ---- 方法2: 积分法 (专利公式) ----
    def _c_znext_qx(self, z0, qct, dt):
        """汛限水位逻辑：
         - 水位 < 汛限：关闭闸门 Q=0，纯蓄水填库
         - 水位 ≥ 汛限：正常 ZQ 泄流，洪水时可超汛限，回落后不低过汛限
        """
        chushi_v0 = self.ztov.get_y(z0)

        # 低于汛限：闸门关闭，来水全部存蓄
        if self.z_fangx > 0 and z0 < self.z_fangx:
            shimo_v1 = chushi_v0 + qct * dt * 0.36
            shimo_z1 = self.ztov.get_x(shimo_v1)
            # 如果填到了汛限以上，钳制在汛限，多余水量泄出
            if shimo_z1 >= self.z_fangx:
                self.z_current = self.z_fangx
                v_fangx = self.ztov.get_y(self.z_fangx)
                return max(0.0, qct - (v_fangx - chushi_v0) / (dt * 0.36))
            self.z_current = shimo_z1
            return 0.0

        # 已≥汛限（或未启用）：正常积分法演算
        out_q = 0.0
        chushi_q0 = self.ztoqm.get_y(z0)

        if qct > chushi_q0:
            xielv_k = (self.ztoqm.get_y(z0 + 0.2) - chushi_q0) / (self.ztov.get_y(z0 + 0.2) - chushi_v0)
        else:
            xielv_k = (self.ztoqm.get_y(z0 - 0.2) - chushi_q0) / (self.ztov.get_y(z0 - 0.2) - chushi_v0)

        zhishu_kt = xielv_k * dt * 0.36
        exp_kt = math.exp(zhishu_kt)

        if zhishu_kt != 0:
            temp_k = (exp_kt - 1) / (exp_kt * zhishu_kt)
        else:
            temp_k = 1

        shimo_v1 = temp_k * (qct - chushi_q0) * dt * 0.36 + chushi_v0
        shimo_z1 = self.ztov.get_x(shimo_v1)
        out_q = max(0.0, qct - (shimo_v1 - chushi_v0) / (dt * 0.36))

        # 防回落：自然水位低于汛限时，钳制在汛限
        if self.z_fangx > 0 and shimo_z1 < self.z_fangx:
            v_fangx = self.ztov.get_y(self.z_fangx)
            chao_q = (chushi_v0 - v_fangx) / dt / 0.36 + qct
            if chao_q > 0:
                self.z_current = self.z_fangx
                return max(0.0, chao_q)

        self.z_current = shimo_z1
        return out_q

    # ---- 方法3: 龙格库塔法 (四阶) ----
    def _c_znext_lgkt(self, z0, qct, dt):
        """汛限水位逻辑：
         - 水位 < 汛限：关闭闸门 Q=0，纯蓄水填库
         - 水位 ≥ 汛限：正常 ZQ 泄流，洪水时可超汛限，回落后不低过汛限
        """
        chushi_v0 = self.ztov.get_y(z0)

        # 低于汛限：闸门关闭，来水全部存蓄
        if self.z_fangx > 0 and z0 < self.z_fangx:
            shimo_v1 = chushi_v0 + qct * dt * 0.36
            shimo_z1 = self.ztov.get_x(shimo_v1)
            if shimo_z1 >= self.z_fangx:
                self.z_current = self.z_fangx
                v_fangx = self.ztov.get_y(self.z_fangx)
                return max(0.0, qct - (v_fangx - chushi_v0) / (dt * 0.36))
            self.z_current = shimo_z1
            return 0.0

        # 已≥汛限（或未启用）：正常龙格库塔演算
        out_q = 0.0
        chushi_q0 = self.ztoqm.get_y(z0)

        k1 = (qct - chushi_q0) * dt * 0.36
        temp_v = chushi_v0 + k1 / 2
        temp_s = self.ztoqm.get_y(self.ztov.get_x(temp_v))
        k2 = (qct - temp_s) * dt * 0.36

        temp_v = chushi_v0 + k2 / 2
        temp_s = self.ztoqm.get_y(self.ztov.get_x(temp_v))
        k3 = (qct - temp_s) * dt * 0.36

        temp_v = chushi_v0 + k3
        temp_s = self.ztoqm.get_y(self.ztov.get_x(temp_v))
        k4 = (qct - temp_s) * dt * 0.36

        shimo_v1 = chushi_v0 + (k1 + 2 * (k2 + k3) + k4) / 6
        shimo_z1 = self.ztov.get_x(shimo_v1)
        out_q = max(0.0, qct - (shimo_v1 - chushi_v0) / (dt * 0.36))

        # 防回落：自然水位低于汛限时，钳制在汛限
        if self.z_fangx > 0 and shimo_z1 < self.z_fangx:
            v_fangx = self.ztov.get_y(self.z_fangx)
            chao_q = (chushi_v0 - v_fangx) / dt / 0.36 + qct
            if chao_q > 0:
                self.z_current = self.z_fangx
                return max(0.0, chao_q)

        self.z_current = shimo_z1
        return out_q

    def adjust(self, q_come, dt, method):
        """演算一个时段，返回时段末水位
        q_come: 时段平均来水 (m³/s)
        dt: 时段长度 (h)
        method: 0=积分法(专利公式), 1=龙格库塔法, 2=迭代法
        returns: (时段末水位, 时段平均出库流量)
        """
        if method == 1:
            out_q = self._c_znext_lgkt(self.z_current, q_come, dt)
        elif method == 2:
            out_q = self._c_znext_qqq(self.z_current, q_come, dt)
        else:  # 默认积分法
            out_q = self._c_znext_qx(self.z_current, q_come, dt)

        return self.z_current, out_q


def run_flood_routing(zv_data, zq_data, flood_data, z_start, z0, dt, method):
    """执行完整的调洪演算
    zv_data: [(水位, 库容), ...]  库容单位: 万m³
    zq_data: [(水位, 流量), ...]  流量单位: m³/s
    flood_data: [(时间, 流量), ...] 时间单位: h, 流量单位: m³/s
    z_start: 起调水位 (m)
    z0: 防洪限制水位 (m)
    dt: 计算时段 (h)
    method: 0=积分法, 1=龙格库塔法, 2=迭代法
    returns: dict with results
    """
    nv = len(zv_data)
    nq = len(zq_data)
    qn = len(flood_data)

    zv_z = [r[0] for r in zv_data]
    zv_v = [r[1] for r in zv_data]
    zq_z = [r[0] for r in zq_data]
    zq_q = [r[1] for r in zq_data]
    qt = [r[0] for r in flood_data]
    qcome = [r[1] for r in flood_data]

    # 实例化水库
    shuiku = Reservoir(z_start, z0, zv_z, zv_v, nv, zq_z, zq_q, nq)

    # 时间步长插值
    temp_tspan = qt[-1] - qt[0]
    qn_cb = int(temp_tspan / dt) + 1
    qt_cb = []
    qcome_cb = []

    j = 1
    for i in range(qn_cb):
        t_val = round(qt[0] + dt * i, 6)
        qt_cb.append(t_val)
        while j < qn and t_val > qt[j]:
            j += 1
        if j >= qn:
            j = qn - 1
        if t_val <= qt[j]:
            if qt[j] == qt[j - 1]:
                q_val = qcome[j - 1]
            else:
                q_val = (qcome[j] - qcome[j - 1]) / (qt[j] - qt[j - 1]) * (t_val - qt[j - 1]) + qcome[j - 1]
            qcome_cb.append(q_val)
        else:
            qcome_cb.append(qcome[-1])

    # 演进计算
    z_pro_cb = [z_start]
    q_out_cb = [0.0]

    for i in range(qn_cb - 1):
        q_avg = (qcome_cb[i] + qcome_cb[i + 1]) / 2
        z_hou, out_q = shuiku.adjust(q_avg, dt, method)
        z_pro_cb.append(z_hou)
        q_out_cb.append(out_q)

    # 构建结果
    results = []
    for i in range(1, qn_cb):
        results.append({
            'time': round(qt_cb[i], 2),
            'inflow_avg': round((qcome_cb[i] + qcome_cb[i - 1]) / 2, 4),
            'outflow_avg': round(q_out_cb[i], 4),
            'z_begin': round(z_pro_cb[i - 1], 4),
            'z_end': round(z_pro_cb[i], 4),
            'v_begin': round(shuiku.ztov.get_y(z_pro_cb[i - 1]), 4),
            'v_end': round(shuiku.ztov.get_y(z_pro_cb[i]), 4),
        })

    # 统计特征值
    z_max = max(z_pro_cb)
    v_start = shuiku.ztov.get_y(z_start)
    v_max = shuiku.ztov.get_y(z_max)
    q_max = shuiku.ztoqm.get_y(z_max)
    # 防洪库容：最高水位库容 - 汛限水位库容（若未设汛限则为起调库容）
    v_base = shuiku.ztov.get_y(z0) if z0 > 0 else v_start
    v_retention = v_max - v_base

    return {
        'results': results,
        'summary': {
            'z_start': round(z_start, 2),
            'v_start': round(v_start, 2),
            'z_max': round(z_max, 2),
            'v_max': round(v_max, 2),
            'q_max': round(q_max, 2),
            'v_retention': round(v_retention, 2),
        },
        'chart_data': {
            'time': qt_cb,
            'inflow': qcome_cb,
            'outflow': q_out_cb,
            'water_level': z_pro_cb,
        }
    }
