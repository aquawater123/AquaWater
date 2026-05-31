using System;
using System.Collections.Generic;
using System.Text;

namespace AquaWater调洪演算程序
{
    class Reservoir
    {

        //float V_si;
        //float V_fang;
        //float V_lan;
        //float V_xing;
        //float V_zong;
        //float V_gong;

        //float Z_si;
        //float Z_fangX;
        //float Z_fangG;
        //float Z_zheng;
        //float Z_she;
        //float Z_jiao;
        //float Z_ba;

        private float Z_current;//现时水位
        private float Z_fangX;//防洪限制水位

        public class Relation_line//单调递增关系线
        {
            float[] K;//存放斜率
            float[] X;//自变量
            float[] Y;//因变量
            int n;//数据长度
            public Relation_line(float[] x, float[] y, int n)//构造函数，输入时自变量按从小到大排列
            {
                X = new float[n];
                Y = new float[n];
                K = new float[n - 1];

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
            public float Get_Y(float x)//由自变量x求因变量y
            {
                float y = 0;
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
            public float Get_X(float y)//由因变量y反查自变量x
            {
                float x = 0;
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

        //public Reservoir(float Znow,float Z0,float[] ZtoV_Z,float[] ZtoV_V,int ZtoV_n,float[] ZtoQm_Z,float[] ZtoQm_Q,int ZtoQm_n,float[] QutoZx_Q,float[] QutoZx_Z,int QutoZx_n)


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
        public Reservoir(float Znow, float Z0, float[] ZtoV_Z, float[] ZtoV_V, int ZtoV_n, float[] ZtoQm_Z, float[] ZtoQm_Q, int ZtoQm_n)
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
        //private void C_ZnextVVV(float Zt, float Qct, float dt)//库容单位1000000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        //{
        //    //通过库容假设试算
        //    float V1;
        //    float V2;
        //    float Vt;
        //    float Zx;
        //    float e = 0.001F;//误差允许限
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
        private void C_ZnextQQQ(float Zt, float Qct, float dt)//库容单位10000立方米，水位单位米，流量单位立方米每秒，时间单位小时
        {
            //通过泄流假设试算
            float Z0;
            float Z1;
            float Z2;
            float V0;
            float V1;
            float V2;
            float Q0;
            float Q1;
            float Q2;

            float YY0;
            float YY1;
            float YY2;


            float Qt;
            float Vt;

            float e = 0.001F;//误差允许限

            float tempV;

            Qt = ZtoQm.Get_Y(Zt);//获得初始水位对应的初始泄流
            Vt = ZtoV.Get_Y(Zt);//获得初始水位对应的库容

            float tempZ;
            tempZ = ZtoV.Get_X(Vt + (Qct) * dt * 0.36F);//假定不泄水是的时段末水位值

            if (tempZ <= Z_fangX)//如果不泄水时段末都比防洪限制水位低，当然不放水
            {
                this.Z_current = tempZ;
            }
            else//否则有出流
            {
                Z0 = Zt;
                Z2 = ZtoV.Get_X(Vt + (Qct-Qt) * dt * 0.36F);//假定时段膜和时段初泄流流量一样的到的水位值
                YY0=Z2-Z0;

                Q2=ZtoQm .Get_Y (Z2 );
                V2 =Vt + (Qct-(Qt+Q2)/2) * dt * 0.36F;
                YY2=ZtoV.Get_X(V2)-Z2;
                if(Math.Abs(Z2 - Z0) < e)
                {
                    this.Z_current = (Z2+Z0)/2;
                    return ;
                }
                if(YY0*YY2>0)
                {
                    return ;//给定的范围不够~~~~~
                }

                
                while (Math.Abs(Z2 - Z0) > e)
                {
                    Z1 = Convert.ToSingle ((Z0*1.0 + Z2*1.0) / 2.0);
                    Q1=ZtoQm.Get_Y(Z1);
                    V1 =Vt + (Qct-(Qt+Q1)/2) * dt * 0.36F;
                    YY1=ZtoV .Get_X (V1 )-Z1 ;

                    if(YY1*YY0 <0)
                    {
                        Z2=Z1;
                        Z1=(Z2 +Z0 )/2;
                    }
                    else if(YY1*YY2<0)
                    {
                        Z0=Z1;
                        Z1=(Z2+Z0)/2;
                    }
                    else
                    {
                        this.Z_current = Z1;
                        return ;
                    }
                }
                Z1 = (Z0 + Z2) / 2;//最后在二分，以减小误差
                this.Z_current = Z1;//查得水位，并付给现时水位
                if (this.Z_current < this.Z_fangX)//如果求得的水位比防洪限制水位都低，那末取防限水位，放水就可以不按下泻能力下泻
                {
                    this.Z_current = Z_fangX;
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
        public float Adjust(float Qcome, float dt)
        {
            C_ZnextQQQ(this.Z_current, Qcome, dt);
            return this.Z_current;
        }
        /// <summary>
        /// 或取现时水位
        /// </summary>
        /// <returns></returns>
        public float get_currentZ()
        {
            return this.Z_current;
        }
        /// <summary>
        /// 设置现时（初始）水位
        /// </summary>
        /// <param name="Z"></param>
        public void set_currentZ(float Z)
        {
            this.Z_current = Z;
        }

    }			   
}
