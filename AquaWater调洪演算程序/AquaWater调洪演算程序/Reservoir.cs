using System;
using System.Collections.Generic;
using System.Text;

namespace AquaWater调洪演算程序
{
    class Reservoir
    {

        //double V_si;
        //double V_fang;
        //double V_lan;
        //double V_xing;
        //double V_zong;
        //double V_gong;

        //double Z_si;
        //double Z_fangX;
        //double Z_fangG;
        //double Z_zheng;
        //double Z_she;
        //double Z_jiao;
        //double Z_ba;

        private double Z_current;//现时水位
        private double Z_fangX;//防洪限制水位

        public class Relation_line//单调递增关系线
        {
            double[] K;//存放斜率
            double[] X;//自变量
            double[] Y;//因变量
            int n;//数据长度
            public Relation_line(double[] x, double[] y, int n)//构造函数，输入时自变量按从小到大排列
            {
                X = new double[n];
                Y = new double[n];
                K = new double[n - 1];

                this.n = n;//数据长度
                for (int i = 0; i < n; i++)//初始化内部数组
                {
                    X[i] = x[i];
                    Y[i] = y[i];
                }
                for (int i = 0; i < n - 1; i++)//初始化斜率数组
                {
                    K[i] = (Y[i + 1] - Y[i]) / (X[i + 1] - X[i]);
                }

            }
            public double Get_Y(double x)//由自变量x求因变量y
            {
                double y = 0;
                int i;
                if (x >= X[0] && x <= X[n - 1])//判断是否出界
                {
                    for (i = 1; i < n; i++)//查找分段
                    {
                        if (x <= X[i])
                            break;
                    }
                    y = K[i - 1] * (x - X[i - 1]) + Y[i - 1];//直线插值计算库容

                }
                else//数据越界按最后一个斜率外延
                {
                    if (x < X[0])
                    {
                        y = (x - X[0]) * K[0] + Y[0];
                    }
                    else
                    {
                        y = (x - X[n - 1]) * K[n - 2] + Y[n - 1];
                    }
                    //MessageBox.Show("输入自变量超出计算范围，请检查！","出错啦!",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                if (y < 0)
                    y = 0;
                return y;//返回因变量值
            }
            public double Get_X(double y)//由因变量y反查自变量x
            {
                double x = 0;
                int i;
                if (y >= Y[0] && y <= Y[n - 1])
                {
                    for (i = 1; i < n; i++)//确定y的在曲线中的位置
                    {
                        if (y <= Y[i])
                            break;
                    }
                    x = (y - Y[i - 1]) / K[i - 1] + X[i - 1];
                }
                else//数据越界按最近斜率直线外延
                {
                    if (y < Y[0])
                    {
                        x = (y - Y[0]) / K[0] + X[0];
                    }
                    else
                    {
                        x = (y - Y[n - 1]) / K[n - 2] + X[n - 1];
                    }
                    //MessageBox.Show("输入因变量超出计算范围，请检查！","出错啦!",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                if (x < 0)
                    x = 0;
                return x;

            }

        }


        public Relation_line ZtoV;//水位库容曲线
        public Relation_line ZtoQm;//水库泄水能力曲线
        //Relation_line QutoZx;//下游流量水位曲线

        //public Reservoir(double Znow,double Z0,double[] ZtoV_Z,double[] ZtoV_V,int ZtoV_n,double[] ZtoQm_Z,double[] ZtoQm_Q,int ZtoQm_n,double[] QutoZx_Q,double[] QutoZx_Z,int QutoZx_n)


        /// <summary>
        /// 水库这个类的构造函数
        /// </summary>
        /// <param name="Znow">初始水位</param>
        /// <param name="Z0">防洪限制水位</param>
        /// <param name="ZtoV_Z">水位库容曲线中水位数据</param>
        /// <param name="ZtoV_V">水位库容曲线中库容数据</param>
        /// <param name="ZtoV_n">水位库容曲线中数据的个数</param>
        /// <param name="ZtoQm_Z">水位泻量曲线中水位数据</param>
        /// <param name="ZtoQm_Q">水位泻量曲线中流量数据</param>
        /// <param name="ZtoQm_n">水位泻量曲线中数据的个数</param>
        public Reservoir(double Znow, double Z0, double[] ZtoV_Z, double[] ZtoV_V, int ZtoV_n, double[] ZtoQm_Z, double[] ZtoQm_Q, int ZtoQm_n)
        {
            ZtoV = new Relation_line(ZtoV_Z, ZtoV_V, ZtoV_n);//实例化一条水位库容曲线
            ZtoQm = new Relation_line(ZtoQm_Z, ZtoQm_Q, ZtoQm_n);//实例化一条水库水位—最大泻水流量曲线
            //QutoZx=new Relation_line(QutoZx_Q,QutoZx_Z,QutoZx_n);
            Z_current = Znow;//初始化初始水位
            Z_fangX = Z0;//初始化防洪限制水位
        }

        /// <summary>
        /// 演算下一个水位
        /// </summary>
        /// <param name="Zt">现时水位</param>
        /// <param name="Qct">时段平均流量</param>
        /// <param name="dt">时段长度</param>
        /// 计算无法保证水量平衡，算法虽然可行，但结果无法使用！！！！！！
        //private void C_ZnextVVV(double Zt, double Qct, double dt)//库容单位1000000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        //{
        //    //通过库容假设试算
        //    double V1;
        //    double V2;
        //    double Vt;
        //    double Zx;
        //    double e = 0.001F;//误差允许限
        //    Vt = ZtoV.Get_Y(Zt);//获得初始水位对应的初始库容
        //    V1 = Vt + Qct * dt * 0.36F;//假设不放水时，时段末的库容
        //    Zx = ZtoV.Get_X(V1);//反求得水位
        //    if (Zx <= Z_fangX)//如果不泄水时段末都比防洪限制水位低，当然不放水
        //    {
        //        this.Z_current = Zx;
        //    }
        //    else//否则有出流
        //    {
        //        V1 = Vt;//初始假定为初始库容
        //        V2 = (Qct - ZtoQm.Get_Y(ZtoV.Get_X((V1 + Vt) / 2))) * dt * 0.36F + Vt;//初始假定求得的时段末库容   方法1:按平均库容查算
        //        //V2 = (Qct - (ZtoQm.Get_Y(V1) + ZtoQm.Get_Y(Vt)) / 2) * dt * 0.36F + Vt;//方法2：按出流平均值查算
        //        while (System.Math.Abs(V1 - V2) >= e)//假设库容与计算求得库容之差小于允许误差则满足条件
        //        {
        //            V1 = (V1 + V2) / 2;//二分法再次假定，当然这里也可用0.618法
        //            V2 = (Qct - ZtoQm.Get_Y(ZtoV.Get_X((V1 + Vt) / 2))) * dt * 0.36F + Vt;//再算时段末库容  方法1:按平均库容查算
        //            //V2 = (Qct - (ZtoQm.Get_Y(V1) + ZtoQm.Get_Y(Vt)) / 2) * dt * 0.36F + Vt;//方法2：按出流平均值查算
        //        }
                
        //        V1 = (V1 + V2) / 2;//最后在二分，以减小误差
        //        this.Z_current = ZtoV.Get_X(V1);//查得水位，并付给现时水位
        //        if (this.Z_current < this.Z_fangX)//如果求得的水位比防洪限制水位都低，那末取防限水位，放水就可以不按下泻能力下泻
        //        {
        //            this.Z_current = Z_fangX;
        //        }
        //    }
        //    return;
        //}
        private void C_ZnextQQQ(double Zt, double Qct, double dt, out double outQ)//库容单位10000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        {
            //通过泄流假设试算
            double Z0;
            double Z1;
            double Z2;
            double V0;
            double V1;
            double V2;
            double Q0;
            double Q1;
            double Q2;

            double YY0;
            double YY1;
            double YY2;


            double Qt;
            double Vt;

            double e = 0.0001F;//误差允许限

            double tempV;
            outQ = 0;

            Qt = ZtoQm.Get_Y(Zt);//获得初始水位对应的初始泄流

            Vt = ZtoV.Get_Y(Zt);//获得初始水位对应的库容

            Z0 = Zt;
            Z2 = ZtoV.Get_X(Vt + (Qct - Qt) * dt * 0.36F);//假定时段末和时段初泄流流量一样得到的水位值
            YY0 = Z2 - Z0;

            Q2 = ZtoQm.Get_Y(Z2);
            V2 = Vt + (Qct - (Qt + Q2) / 2) * dt * 0.36F;
            YY2 = ZtoV.Get_X(V2) - Z2;
            if (Math.Abs(Z2 - Z0) < e)
            {
                this.Z_current = (Z2 + Z0) / 2;
                outQ = Qt;
                return;
            }
            if (YY0 * YY2 > 0)
            {
                return;//给定的范围不够~~~~~
            }

            while (Math.Abs(Z2 - Z0) > e)
            {
                Z1 = (Z0 + Z2) / 2.0;
                Q1 = ZtoQm.Get_Y(Z1);
                V1 = Vt + (Qct - (Qt + Q1) / 2) * dt * 0.36F;
                YY1 = ZtoV.Get_X(V1) - Z1;

                if (YY1 * YY0 < 0)
                {
                    Z2 = Z1;
                    Z1 = (Z2 + Z0) / 2;
                }
                else if (YY1 * YY2 < 0)
                {
                    Z0 = Z1;
                    Z1 = (Z2 + Z0) / 2;
                }
                else
                {
                    this.Z_current = Z1;
                    return;
                }
            }
            Z1 = (Z0 + Z2) / 2;//最后在二分，以减小误差

            if (Z1 > Z_fangX)
            {
                this.Z_current = Z1;//查得水位，并付给现时水位
                outQ = (Qt + ZtoQm.Get_Y(Z1)) / 2;
                return;
            }
            else
            {
                double ChaoQ = (Vt - ZtoV.Get_Y(Z_fangX)) / dt / 0.36 + Qct;
                if (ChaoQ > 0)
                {
                    this.Z_current = Z_fangX;
                    outQ = ChaoQ;
                }
                else
                {
                    outQ = 0;
                    this.Z_current= ZtoV.Get_X(Vt + (Qct) * dt * 0.36F);
                }
            }
            return;
        }

        /// <summary>
        /// 通过公式直接推导时段末库水位
        /// </summary>
        /// <param name="Zt"></param>
        /// <param name="Qct"></param>
        /// <param name="dt"></param>
        /// <param name="outQ"></param>
        private void C_ZnextQX(double Z0, double Qct, double dt, out double outQ)//库容单位10000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        {
            double chushiQ0;
            double chushiV0;
            double xielvK;//S-V关系斜率
            double zhishuKT;//斜率K和时段dt的乘积，库容单位10000立方米，时段单位小时，换算需注意
            double shimoV1;
            double shimoZ1;
            outQ = 0;

            chushiQ0 = ZtoQm.Get_Y(Z0);//获得初始水位对应的初始泄流
            chushiV0 = ZtoV.Get_Y(Z0);//获得初始水位对应的库容

            if (Qct > chushiQ0)
            {
                xielvK = (ZtoQm.Get_Y(Z0 + 0.2F) - chushiQ0) / (ZtoV.Get_Y(Z0 + 0.2F) - chushiV0);
            }
            else
            {
                xielvK = (ZtoQm.Get_Y(Z0 - 0.2F) - chushiQ0) / (ZtoV.Get_Y(Z0 - 0.2F) - chushiV0);
            }
            zhishuKT = xielvK * dt * 0.36;
            double expKT=Math.Exp (zhishuKT);
            double tempK;
            if (zhishuKT != 0)
            {
                tempK = (expKT - 1) / (expKT * zhishuKT);
            }
            else
            {
                tempK = 1;
            }
           
            shimoV1 = tempK * (Qct - chushiQ0) * dt * 0.36 + chushiV0;
            shimoZ1 = ZtoV.Get_X(shimoV1);
           

            if (shimoZ1 > Z_fangX)
            { 
                this.Z_current = shimoZ1;
                outQ = Qct - (shimoV1 - chushiV0) / (dt * 0.36);
                return;
            }
            else
            {
                double ChaoQ = (chushiV0 - ZtoV.Get_Y(Z_fangX)) / dt / 0.36 + Qct;
                if (ChaoQ > 0)
                {
                    this.Z_current = Z_fangX;
                    outQ = ChaoQ;
                }
                else
                {
                    outQ = 0;
                    this.Z_current = ZtoV.Get_X(chushiV0 + (Qct) * dt * 0.36F);
                }
            }
            return;
        }
        /// <summary>
        /// 龙格库塔法
        /// </summary>
        /// <param name="Z0"></param>
        /// <param name="Qct"></param>
        /// <param name="dt"></param>
        /// <param name="outQ"></param>
        private void C_ZnextQLGKT(double Z0, double Qct, double dt, out double outQ)//库容单位10000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        {
            double chushiQ0;
            double chushiV0;
            double k1, k2, k3, k4;
            double tempV;
            double tempS;
            double shimoV1;
            double shimoZ1;

            chushiQ0 = ZtoQm.Get_Y(Z0);//获得初始水位对应的初始泄流
            chushiV0 = ZtoV.Get_Y(Z0);//获得初始水位对应的库容

            k1 = (Qct - chushiQ0) * dt * 0.36;
            tempV = chushiV0 + k1 / 2;
            tempS= ZtoQm.Get_Y(ZtoV.Get_X (tempV ));
            k2 = (Qct - tempS) * dt * 0.36;
            tempV = chushiV0 + k2 / 2;
            tempS = ZtoQm.Get_Y(ZtoV.Get_X(tempV));
            k3 = (Qct - tempS) * dt * 0.36;
            tempV = chushiV0 + k3;
            tempS = ZtoQm.Get_Y(ZtoV.Get_X(tempV));
            k4 = (Qct - tempS) * dt * 0.36;

            outQ = 0;

            shimoV1 = chushiV0 +(k1+2*(k2+k3)+k4)/6;
            shimoZ1 = ZtoV.Get_X(shimoV1);


            if (shimoZ1 > Z_fangX)
            {
                this.Z_current = shimoZ1;
                outQ = Qct - (shimoV1 - chushiV0) / (dt * 0.36);
                return;
            }
            else
            {
                double ChaoQ = (chushiV0 - ZtoV.Get_Y(Z_fangX)) / dt / 0.36 + Qct;
                if (ChaoQ > 0)
                {
                    this.Z_current = Z_fangX;
                    outQ = ChaoQ;
                }
                else
                {
                    outQ = 0;
                    this.Z_current = ZtoV.Get_X(chushiV0 + (Qct) * dt * 0.36F);
                }
            }
            return;
        }

        /// <summary>
        /// 调整函数
        /// </summary>
        /// <param name="Qcome">时段平均来水</param>
        /// <param name="dt">时段长度</param>
        /// <returns>返回时段末的水位</returns>
        public double Adjust(double Qcome, double dt,int fangfa,out double outQ)
        {
            if (fangfa == 1)//龙格库塔
            {
                C_ZnextQLGKT(this.Z_current, Qcome, dt, out outQ);
            }
            else if (fangfa == 2)//迭代法
            {
                C_ZnextQQQ(this.Z_current, Qcome, dt, out outQ);
            }
            else//积分法，安氏法
            {
                C_ZnextQX(this.Z_current, Qcome, dt, out outQ);
            }
            //
            //
           
            return this.Z_current;
        }
        /// <summary>
        /// 或取现时水位
        /// </summary>
        /// <returns></returns>
        public double get_currentZ()
        {
            return this.Z_current;
        }
        /// <summary>
        /// 设置现时（初始）水位
        /// </summary>
        /// <param name="Z"></param>
        public void set_currentZ(double Z)
        {
            this.Z_current = Z;
        }

    }			   
}
