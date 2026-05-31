using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using C1.Win.C1Chart;

namespace AquaWater调洪演算程序
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        bool error = false;//出错标记
        private void Form1_Load(object sender, EventArgs e)
        {
             flexGridExcel1.Rows .Count =1;
             flexGridExcel1.Cols.Count =3;
             flexGridExcel1.Cols[0].Width = 20;
             flexGridExcel1.Rows[0][0] = "NO";
             flexGridExcel1.Rows[0][1] = "水位(m)";
             flexGridExcel1.Rows[0][2] = "库容(万m3)";
             flexGridExcel1.AutoSizeCols(1, 2, -10);

             flexGridExcel2.Rows.Count = 1;
             flexGridExcel2.Cols.Count = 3;
             flexGridExcel2.Cols[0].Width = 20;
             flexGridExcel2.Rows[0][0] = "NO";
             flexGridExcel2.Rows[0][1] = "水位(m)";
             flexGridExcel2.Rows[0][2] = "流量(m3/s)";
             flexGridExcel2.AutoSizeCols(1, 2, -10);

             flexGridExcel3.Rows.Count = 1;
             flexGridExcel3.Cols.Count = 3;
             flexGridExcel3.Cols[0].Width = 20;
             flexGridExcel3.Rows[0][0] = "NO";
             flexGridExcel3.Rows[0][1] = "时间(h)";
             flexGridExcel3.Rows[0][2] = "流量(m3/s)";
             flexGridExcel3.AutoSizeCols(1, 2, -10);

             flexGridExcel4.Rows.Count = 1;
             flexGridExcel4.Cols.Count = 8;
             flexGridExcel4.Cols[0].Width = 20;
             flexGridExcel4.Rows[0][0] = "NO";
             flexGridExcel4.Rows[0][1] = "时间(h)";
             flexGridExcel4.Rows[0][2] = "时段平均入库(m3/s)";
             flexGridExcel4.Rows[0][3] = "时段平均出库(m3/s)";
             flexGridExcel4.Rows[0][4] = "时段初水位(m)";
             flexGridExcel4.Rows[0][5] = "时段末水位(m)";
             flexGridExcel4.Rows[0][6] = "时段初库容(万m3)";
             flexGridExcel4.Rows[0][7] = "时段末库容(万m3)";
             flexGridExcel4.AutoSizeCols(1, 2, -10);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int count = 2;//数据为三列
            char[] gefu ={ '	',',',' ' };//文件数据以制表符分隔
            string file_path;//存放文件对话框获得的文件路径
            string strline;//读数据一行
            int i, j;//循环变量
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "文本文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)//确认文件对话框输入
            {

                flexGridExcel1.Rows.Count = 1;
                flexGridExcel1.Cols.Count = 3;
                flexGridExcel1.Cols[0].Width = 20;
                flexGridExcel1.Rows[0][0] = "NO";
                flexGridExcel1.Rows[0][1] = "水位(m)";
                flexGridExcel1.Rows[0][2] = "库容(万m3)";
                i = 1;
                j = 0;

                file_path = openFileDialog1.FileName;//获得路径
                try
                {
                    FileStream filep = new FileStream(file_path, FileMode.Open);//打开文件
                    StreamReader reader = new StreamReader(filep);//获得一个读取数据的对象，以字符格式读
                    while (!reader.EndOfStream)//逐行读文件，并处理之
                    {
                        strline = reader.ReadLine();
                        string[] str_duan = strline.Split(gefu, count);//将一行字符根据制表符分隔成各个字段，并付给数组
                        flexGridExcel1.Rows.Count++;
                        flexGridExcel1.Rows[i][0] = i.ToString();//将序号输出到界面
                        for (j = 0; j < count; j++)
                        {
                            flexGridExcel1.Rows[i][j+1] = str_duan[j];//将分隔得的字段输入到界面列表中
                        }
                        i++;
                    }
                   
                    reader.Close();//关闭文件    
                    flexGridExcel1.AutoSizeCols(1,2,-10);
                }
                catch(Exception eee)
                {
                    //出错处理，可能错误是文件打开出错
                    MessageBox.Show("文件打开错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tabControl1.SelectedIndex = 0;
                DrawChartOne();
                
            }
			
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int count = 2;//数据为三列
            char[] gefu ={ '	', ',', ' ' };//文件数据以制表符分隔
            string file_path;//存放文件对话框获得的文件路径
            string strline;//读数据一行
            int i, j;//循环变量
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "文本文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)//确认文件对话框输入
            {

                flexGridExcel2.Rows.Count = 1;
                flexGridExcel2.Cols.Count = 3;
                flexGridExcel2.Cols[0].Width = 20;
                flexGridExcel2.Rows[0][0] = "NO";
                flexGridExcel2.Rows[0][1] = "水位(m)";
                flexGridExcel2.Rows[0][2] = "流量(m3/s)";
                i = 1;
                j = 0;

                file_path = openFileDialog1.FileName;//获得路径
                try
                {
                    FileStream filep = new FileStream(file_path, FileMode.Open);//打开文件
                    StreamReader reader = new StreamReader(filep);//获得一个读取数据的对象，以字符格式读
                    while (!reader.EndOfStream)//逐行读文件，并处理之
                    {
                        strline = reader.ReadLine();
                        string[] str_duan = strline.Split(gefu, count);//将一行字符根据制表符分隔成各个字段，并付给数组
                        flexGridExcel2.Rows.Count++;
                        flexGridExcel2.Rows[i][0] = i.ToString();//将序号输出到界面
                        for (j = 0; j < count; j++)
                        {
                            flexGridExcel2.Rows[i][j + 1] = str_duan[j];//将分隔得的字段输入到界面列表中
                        }
                        i++;
                    }

                    reader.Close();//关闭文件    
                    flexGridExcel2.AutoSizeCols(1, 2, -10);
                }
                catch
                {
                    //出错处理，可能错误是文件打开出错
                    MessageBox.Show("文件打开错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tabControl1.SelectedIndex = 1;
                DrawChartTow();
                
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            int count = 2;//数据为三列
            char[] gefu ={ '	', ',', ' ' };//文件数据以制表符分隔
            string file_path;//存放文件对话框获得的文件路径
            string strline;//读数据一行
            int i, j;//循环变量
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "文本文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)//确认文件对话框输入
            {

                flexGridExcel3.Rows.Count = 1;
                flexGridExcel3.Cols.Count = 3;
                flexGridExcel3.Cols[0].Width = 20;
                flexGridExcel3.Rows[0][0] = "NO";
                flexGridExcel3.Rows[0][1] = "时间(h)";
                flexGridExcel3.Rows[0][2] = "流量(m3/s)";
                i = 1;
                j = 0;

                file_path = openFileDialog1.FileName;//获得路径
                try
                {
                    FileStream filep = new FileStream(file_path, FileMode.Open);//打开文件
                    StreamReader reader = new StreamReader(filep);//获得一个读取数据的对象，以字符格式读
                    while (!reader.EndOfStream)//逐行读文件，并处理之
                    {
                        strline = reader.ReadLine();
                        string[] str_duan = strline.Split(gefu, count);//将一行字符根据制表符分隔成各个字段，并付给数组
                        flexGridExcel3.Rows.Count++;
                        flexGridExcel3.Rows[i][0] = i.ToString();//将序号输出到界面
                        for (j = 0; j < count; j++)
                        {
                            flexGridExcel3.Rows[i][j + 1] = str_duan[j];//将分隔得的字段输入到界面列表中
                        }
                        i++;
                    }

                    reader.Close();//关闭文件    
                    flexGridExcel3.AutoSizeCols(1, 2, -10);
                }
                catch
                {
                    //出错处理，可能错误是文件打开出错
                    MessageBox.Show("文件打开错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                tabControl1.SelectedIndex = 2;
                DrawChartThree();
                
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //2012.8.30，增加安氏调洪法和龙格库塔法
           if (this.textBox1.Text == "")
            {
                this.textBox1.Text = Convert.ToString(this.flexGridExcel2[1, 1]);
            }
           if (this.textBox2.Text == "")
            {
                this.textBox2.Text = "0.1";
            }
           if (this.textBox3.Text == "")
           {
               this.textBox3.Text = "0.0";
           }

           if (this.textBox1.Text != "" && this.textBox2.Text != "")
            {
                error = false;
                TiaoHonhCounter(comboBox1.SelectedIndex);
                tabControl1.SelectedIndex = 2;
                DrawChartThree();

            }
            else
            {
                MessageBox.Show("您没有输入初始水位或防洪限制水位，请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TiaoHonhCounter(int fangfa)
        {
            ///////////////////////////////////////////////////////////////////////////
            ///
            double[] Qcome;//时段平均来水过程
            double[] QcomeCB;//插补后的来水过程
            double[] QTCB;//插补后的时间过程
            int QnCB;
            double[] Q_outCB;//计算得到的出流
            double[] Z_proCB;//计算获得水位变化过程

            double[] QT;//来水时间
            int Qn = 0;//来水过程数据个数
            double[] ZV_s;//水位-库容关系线的水位数据
            double[] ZQ_s;//水位-最大出流关系线的水位数据
            double[] V_s;//水位-库容-最大出流关系线的库容数据
            double[] Q_s;//水位-库容-最大出流关系线的最大出流数据
            double[] Q_out_avg;//计算得到的时段平均出流
            
            double[] Z_pro;//计算获得水位变化过程
            int nV = 0;//水位-库容关系线的数据个数
            int nQ = 0;//水位-最大出流关系线的数据个数
            double dt = 1;//时段，单位小时
            double Z_start = 0;//初始水位
            double Z0 = 0;//防洪限制水位
            int i = 0;//循环变量
            double Zmax;//最大水位
            Reservoir shuiku;//一个水库对象
            DateTime STTime, EDTime;
            STTime = EDTime = DateTime.Now;
            /////////////////////////////////////////////////////////////////////////////
            ///
            Qn = this.flexGridExcel3.Rows.Count - 1 ;//获得来水数据个数
            nV = this.flexGridExcel1.Rows.Count - 1;
            nQ = this.flexGridExcel2.Rows.Count - 1;

            Qcome = new double[Qn];
            QT = new double[Qn];
            ZV_s = new double[nV];
            ZQ_s = new double[nQ];
            V_s = new double[nV];
            Q_s = new double[nQ];
            Q_out_avg = new double[Qn];

            Z_pro = new double[Qn + 1];//包括初始数为，故为Qn+1

            /////////////////////////////////////////////////////////////////////////////

            //从输入界面获得数据，并分别付给各个数组
            try
            {
                Z_start = Convert.ToDouble(this.textBox1.Text);//获得初始水位
                Z0 = Convert.ToDouble(this.textBox3.Text);//获得防洪限制水位
                dt = Convert.ToDouble(this.textBox2.Text);//获得计算时间步长
                for (i = 0; i < nV; i++)
                {
                    ZV_s[i] = Convert.ToDouble(flexGridExcel1.Rows[i+1][1].ToString());//获得水库特征曲线的水位
                    V_s[i] = Convert.ToDouble(flexGridExcel1.Rows[i + 1][2].ToString());//获得水库特征曲线的库容
                    V_s[i] = V_s[i];//将原万立方换算为10的6方单位
                    
                }
                for (i = 0; i < nQ; i++)
                {
                    ZQ_s[i] = Convert.ToDouble(flexGridExcel2.Rows[i + 1][1].ToString());//获得水库特征曲线的水位
                    Q_s[i] = Convert.ToDouble(flexGridExcel2.Rows[i + 1][2].ToString());//获得水库特征曲线的最大流量
                }
                for (i = 0; i < Qn; i++)
                {
                    QT[i] = Convert.ToDouble(flexGridExcel3.Rows[i + 1][1].ToString());
                    Qcome[i] = Convert.ToDouble(flexGridExcel3.Rows[i + 1][2].ToString());//获得平均来水过程
                }
                if (nV <= 0 || nQ <= 0 || Qn <= 0)
                {
                    error = true;
                }
            }
            catch
            {
                //出错处理，可能错误是数据转换出错
                MessageBox.Show("数据有错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                error = true;
            }

            //////////////////////////////////////////////////////////////////////////////
            ///
            if (error == false)
            {
                //以下演进计算水库水位变化和平均流量过程，并作输出
                double Z_hou;//时段后时刻水位
                double Z_qian;//时段前时刻水位
                shuiku = new Reservoir(Z_start, Z0, ZV_s, V_s, nV, ZQ_s, Q_s, nQ);//实例化水库，并初始化之

                
                ///////////////
                double tempTspan;
                tempTspan =QT[Qn-1]-QT[0];
                QnCB = (int)(tempTspan / dt) + 1;
                QcomeCB = new double[QnCB];
                QTCB = new double[QnCB];
                Q_outCB =new double[QnCB ];
                Z_proCB =new double [QnCB ];


                int j=1;
                for (i = 0; i < QnCB; i++)
                {
                    QTCB[i] = (int)((QT[0] + dt * i)*100000+0.1F)/100000.0;
                    while (QTCB[i] > QT[j])
                    {
                        j++;

                    }
                    if (QTCB[i] <= QT[j])
                    {
                        QcomeCB[i] = (Qcome[j] - Qcome[j - 1]) / (QT[j] - QT[j - 1]) * (QTCB[i] - QT[j - 1]) + Qcome[j - 1];
                    }
                }
                double outQ;
                outQ = 0;
                double QmaxOut=0;
                try
                {//计算水库水位变化过程及平均流量过程
                    Z_pro[0] = shuiku.get_currentZ();//获得初始水位
                    Z_proCB[0] = shuiku.get_currentZ();
                    //Q_outCB[0] = shuiku.ZtoQm.Get_Y(Z_start);

                   // DateTime start = DateTime.Now;

                    //QmaxOut = shuiku.ZtoQm.Get_Y(Z_pro[0]);
                    //for (i = 0; i < QnCB - 1; i++)
                    //{
                    //    if (QmaxOut < QcomeCB[i])
                    //    {
                    //        break;
                    //    }
                    //}
                    
                    STTime = DateTime.Now;
                    //for (int loop = 0; loop < 10000; loop++)
                    {
                        shuiku.set_currentZ(Z_start);
                        for (i = 0; i < QnCB - 1; i++)
                        {
                            //**//Z_qian = shuiku.get_currentZ();
                            Z_hou = shuiku.Adjust((QcomeCB[i] + QcomeCB[i + 1]) / 2, dt, fangfa, out outQ);
                            //计算平均流量
                            //Q_out_avg[i] = (shuiku.ZtoV.Get_Y(Z_qian) - shuiku.ZtoV.Get_Y(Z_hou)) / (0.36F * dt) + Qcome[i];
                            Q_outCB[i + 1] = outQ;              //shuiku.ZtoQm.Get_Y(Z_hou);
                            //计算水位				
                            //Z_pro[i + 1] = Z_hou;
                            Z_proCB[i + 1] = Z_hou;
                            //将计算得到的时段平均流量输出到form界面上
                            //Q_out_avg[i] = (double)(((int)(Q_out_avg[i] * 10 + 0.5)) / 10.0F);
                        }
                    }
                    //DateTime end = DateTime.Now;
                    //MessageBox.Show((end - start).TotalSeconds.ToString() + "秒");

                    //Q_outCB[0] = Q_outCB[1];
                    //for (i = 0; i < QnCB - 2; i++)
                    //{
                    //    Q_outCB[i + 1] = (Q_outCB[i + 1] + Q_outCB[i + 2]) / 2;
                    //}
                    EDTime = DateTime.Now;
                    //输出到界面
                    flexGridExcel4.Rows.Count = 1;
                    //flexGridExcel4.Cols.Count = 6;
                    for (i = 1; i < QnCB; i++)
                    {
                        flexGridExcel4.Rows.Count++;
                        flexGridExcel4[i, 0] = i.ToString();
                        flexGridExcel4[i, 1] = Convert.ToString(((int)(QTCB[i] * 100F + 0.5F)) / 100F);
                        flexGridExcel4[i, 2] = Convert.ToString(((int)((QcomeCB[i] + QcomeCB[i - 1]) / 2 * 10000F + 0.5F)) / 10000F);
                        flexGridExcel4[i, 3] = Convert.ToString(((int)(Q_outCB[i] * 10000F + 0.5F)) / 10000F);
                        flexGridExcel4[i, 4] = Convert.ToString(((int)(Z_proCB[i - 1] * 10000F + 0.5F)) / 10000F);
                        flexGridExcel4[i, 5] = Convert.ToString(((int)(Z_proCB[i] * 10000F + 0.5F)) / 10000F);
                        flexGridExcel4[i, 6] = Convert.ToString(((int)(shuiku.ZtoV.Get_Y(Z_proCB[i - 1])*10000F + 0.5F)) / 10000F);
                        flexGridExcel4[i, 7] = Convert.ToString(((int)(shuiku.ZtoV.Get_Y(Z_proCB[i])*10000F + 0.5F)) / 10000F);

                    }
                    flexGridExcel4.AutoSizeCols();
                    
                }
                catch
                {
                    //出错处理，最可能是listview出错
                    MessageBox.Show("数据越界！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                ////////////////////////////////////////////////////////////////////////////////
                ///
                /*if (error == false)
                {
                    //以下五点四分法计算调洪后最大水位和最大流量
                    Zmax = Z_pro[0];//给Zmax付一初值
                    int k = 0;//简单变量，作记录最大值位置和循环变量
                    for (i = 1; i < Qn + 1; i++)//求在前面计算的水位过程中，水位最高的时刻（位置）
                    {
                        if (Zmax < Z_pro[i])
                        {
                            Zmax = Z_pro[i];
                            k = i;
                        }
                    }
                    double[] Zscan = new double[5];//定义的一个临时扫描数组，用来在五点四分法求最大水位时存放临时的最大五个值
                    double ii, jj;//记录从水位变化过程中抽出的五个最大值的起始时刻（位置ii）和结束时刻（位置jj）
                    double ddt = 1;//相对时段值，即ddt乘以原来的时段值为当前的时段值，并用此作为循环条件，当之小于一定值结束循环
                    double edt = 0.0083F;//控制求解最高水位精度的量，实际上取0.0083(1/120分钟)是为了求得的最大水位时的计算时段小于1分钟
                    //获得水位变化过程中最大值周围的前一个和后一个位置值
                    if (k - 1 >= 0)
                    {
                        ii = k - 1;
                        Zscan[0] = Z_pro[k - 1];//或得扫描数据的初值，此值即将作为计算最大水位的初始水位
                    }
                    else
                    {
                        ii = k;
                        Zscan[0] = Z_pro[k];
                    }
                    if (k + 1 <= Qn)
                    {
                        jj = k + 1;
                    }
                    else
                    {
                        jj = k;
                    }

                    /////////////////////////////////////////////////////////////////////////////////
                    ///

                    while (ddt > edt)//跳出条件，控制精度
                    {
                        ddt = (jj - ii) / 4;//四分时刻
                        shuiku.set_currentZ(Zscan[0]);//设置初始水位
                        for (k = 1; k < 5; k++)
                        {
                            Zscan[k] = shuiku.Adjust(Qcome[(int)(ii + (k - 0.5) * ddt)], dt * ddt);//计算从此初始水位演进计算的水位变化过程
                        }
                        Zmax = Zscan[0];//设置初始最高水位
                        for (i = 1; i < 5; i++)//查找扫描数组中的最大值
                        {
                            if (Zmax < Zscan[i])
                            {
                                Zmax = Zscan[i];
                                k = i;
                            }
                        }

                        //获得水位变化过程中最大值周围的前一个和后一个位置值
                        if (k - 1 >= 0)
                        {
                            ii = ii + (k - 1) * ddt;
                            Zscan[0] = Zscan[k - 1];//重新设置初始水位，用于作下次计算的初值
                        }
                        else
                        {
                            ii = ii + k * ddt;
                            Zscan[0] = Zscan[k];
                        }
                        if (k + 1 <= 4)
                        {
                            jj = jj - (3 - k) * ddt;//4-k+1
                        }
                        else
                        {
                            jj = jj - (4 - k) * ddt;
                        }
                    }
                }*/

                    ///////////////////////////////////////////////////////////////////////////////
                    ///
                  //输出统计
                    Zmax =0;
                    for(i=0;i<QnCB ;i++)
                    {
                        if(Zmax<Z_proCB [i])
                        {
                            Zmax =Z_proCB [i];
                        }
                    }

               
                    this.textBox6.Text = Convert.ToString(((int)(shuiku.ZtoV.Get_Y(Z_proCB[0])*100+0.5F))/100F);//将10的6次方立方米换算成万立方米
                    this.textBox7.Text = Convert.ToString(((int)(Zmax*100+0.5F))/100F);//将最大值输出到界面上
                    this.textBox8.Text = Convert.ToString(((int)(shuiku.ZtoV.Get_Y(Zmax)*100+0.5F))/100F) ;//将10的6次方立方米换算成万立方米
                    this.textBox9.Text = Convert.ToString(((int)(shuiku.ZtoQm.Get_Y(Zmax)*100+0.5F))/100F);//将最大流量输出到界面上
                    double tempvv = Convert.ToDouble(this.textBox8.Text) - Convert.ToDouble(this.textBox6.Text);
                    this.textBox10.Text =Convert.ToString(((int)(tempvv*100F+0.5F))/100F);
                  
                    MessageBox.Show("计算完毕！耗时" + (EDTime-STTime).TotalSeconds+"s");
               
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string filename = "";
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = filename;
            saveFileDialog1.Filter = "Microsoft Execl 文件|*.xls";
            saveFileDialog1.Title = "保存文件";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = saveFileDialog1.FileName;
                if (File.Exists(filename))
                {
                    try
                    {
                        using (FileStream FS = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
                        {
                            FS.Close();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("文件保存错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                // c1FlexGrid1.SaveExcel(filename, this.labelX1.Text, FileFlags.IncludeFixedCells );
                if (flexGridExcel4.SaveXMLExcel(filename, "调洪成果", "调洪成果", false))
                {

                    MessageBox.Show( "保存成功\n\n位置:" + filename, "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }                
                else
                {
                    MessageBox.Show("文件保存失败", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap (*.bmp)|*.bmp|"
                + "EMF Enhanced Metafile Format (*.emf)|*.emf|"
                + "Graphics Interchange Format (*.gif)|*.gif|"
                + "Joint Photographic Experts Group (*.jpg)|*.jpg|"
                + "W3C Portable Network Graphics (*.png)|*.png";
            sfd.FilterIndex = 2;
            sfd.DefaultExt = "emf";
            sfd.FileName = "";
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = true;
            sfd.RestoreDirectory = false;
            sfd.ValidateNames = true;
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                if (c1Chart1.Visible)
                {
                    c1Chart1.SaveImage(sfd.FileName, getImageFormatFromDlg(sfd.FilterIndex), getSize());
                    MessageBox.Show("文件保存成功");
                }
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap (*.bmp)|*.bmp|"
                + "EMF Enhanced Metafile Format (*.emf)|*.emf|"
                + "Graphics Interchange Format (*.gif)|*.gif|"
                + "Joint Photographic Experts Group (*.jpg)|*.jpg|"
                + "W3C Portable Network Graphics (*.png)|*.png";
            sfd.FilterIndex = 2;
            sfd.DefaultExt = "emf";
            sfd.FileName = "";
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = true;
            sfd.RestoreDirectory = false;
            sfd.ValidateNames = true;
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                if (c1Chart2.Visible)
                {
                    c1Chart2.SaveImage(sfd.FileName, getImageFormatFromDlg(sfd.FilterIndex), getSize());
                    MessageBox.Show("文件保存成功");
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap (*.bmp)|*.bmp|"
                + "EMF Enhanced Metafile Format (*.emf)|*.emf|"
                + "Graphics Interchange Format (*.gif)|*.gif|"
                + "Joint Photographic Experts Group (*.jpg)|*.jpg|"
                + "W3C Portable Network Graphics (*.png)|*.png";
            sfd.FilterIndex = 2;
            sfd.DefaultExt = "emf";
            sfd.FileName = "";
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = true;
            sfd.RestoreDirectory = false;
            sfd.ValidateNames = true;
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                if (c1Chart3.Visible)
                {
                    c1Chart3.SaveImage(sfd.FileName, getImageFormatFromDlg(sfd.FilterIndex), getSize());
                    MessageBox.Show("文件保存成功");
                }
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            DrawChartOne();
        }
        private void button10_Click(object sender, EventArgs e)
        {
            DrawChartTow();
        }
        private void button11_Click(object sender, EventArgs e)
        {
            DrawChartThree();
        }

        private System.Drawing.Imaging.ImageFormat getImageFormatFromDlg(int index)
        {
            switch (index)
            {
                case 1: return System.Drawing.Imaging.ImageFormat.Bmp;
                case 2: return System.Drawing.Imaging.ImageFormat.Emf;
                case 3: return System.Drawing.Imaging.ImageFormat.Gif;
                case 4: return System.Drawing.Imaging.ImageFormat.Jpeg;
                case 5: return System.Drawing.Imaging.ImageFormat.Png;
                default: return System.Drawing.Imaging.ImageFormat.Bmp;
            }
        }
        Size getSize()
        {
            Size sz = Size.Empty;
            try
            {
                sz.Width = this.c1Chart1.Width;// int.Parse(tbWidth.Text);
                sz.Height = this.c1Chart1.Height;// int.Parse(tbHeigth.Text);
            }
            catch
            {
                sz = Size.Empty;
            }
            return sz;
        }


        private void DrawChartOne()
        {
            int nV;
            nV = this.flexGridExcel1.Rows.Count - 1;
            float[] ZV_Z = new float[nV];
            float[] ZV_V = new float[nV];
            int i;
            try
            {
                for (i = 0; i < nV; i++)
                {
                    ZV_Z[i] = Convert.ToSingle(flexGridExcel1.Rows[i + 1][1].ToString());//获得水库特征曲线的水位
                    ZV_V[i] = Convert.ToSingle(flexGridExcel1.Rows[i + 1][2].ToString());//获得水库特征曲线的库容
                   // V_s[i] = V_s[i] / 100;//将原万立方换算为10的6方单位
                }
            }
            catch
            {
                //出错处理，可能错误是数据转换出错
                MessageBox.Show("数据有错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.c1Chart1.ChartGroups.Group0.ChartData.SeriesList.RemoveAll();

            this.c1Chart1.SuspendLayout();
            int seriesCount = this.c1Chart1.ChartGroups.Group0.ChartData.SeriesList.Count;
            ChartDataSeries showDataSeries1 = this.c1Chart1.ChartGroups.Group0.ChartData.SeriesList.AddNewSeries();
            seriesCount++;
            this.c1Chart1.ChartArea.AxisY.Text = "水位（m）";
            this.c1Chart1.ChartArea.AxisX.Text = "库容（万m3）";
            this.c1Chart1.Header.Compass = CompassEnum.North;
            this.c1Chart1.Header.Visible = true;
            this.c1Chart1.Header.Text ="水位库容曲线";
          
            int rowsCount = nV;
            //showDataSeries1.FillStyle.FillType = FillTypeEnum.Gradient;

            //showDataSeries1.FillStyle.Color1 = Color.BlueViolet;
            //showDataSeries1.FillStyle.Color2 = Color.White;
            //showDataSeries1.FillStyle.GradientStyle = GradientStyleEnum.Vertical;
            showDataSeries1.LineStyle.Color = Color.BlueViolet;
            showDataSeries1.LineStyle.Thickness = 2;

            showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.None;
            //showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.Square;
            //showDataSeries1.SymbolStyle.Color = Color.Transparent;
            //showDataSeries1.SymbolStyle.OutlineColor = Color.OrangeRed;

            showDataSeries1.X.Length = rowsCount;
            showDataSeries1.Y.Length = rowsCount;
            //this.c1Chart1.ChartArea.AxisX.ValueLabels.Clear();
            for (i = 0; i < rowsCount; i++)
            {
                showDataSeries1.X[i] = ZV_V[i];
                showDataSeries1.Y[i] = ZV_Z[i];
            }
            this.c1Chart1.ResumeLayout();
        }
        private void DrawChartTow()
        {
            int nQ;
            nQ = this.flexGridExcel2.Rows.Count - 1;
            float[] ZQ_Z = new float[nQ];
            float[] ZQ_Q = new float[nQ];
            int i;
            try
            {
                for (i = 0; i < nQ; i++)
                {
                    ZQ_Z[i] = Convert.ToSingle(flexGridExcel2.Rows[i + 1][1].ToString());
                    ZQ_Q[i] = Convert.ToSingle(flexGridExcel2.Rows[i + 1][2].ToString());
                }
            }
            catch
            {
                MessageBox.Show("数据有错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.c1Chart2.ChartGroups.Group0.ChartData.SeriesList.RemoveAll();

            this.c1Chart2.SuspendLayout();
            int seriesCount = this.c1Chart2.ChartGroups.Group0.ChartData.SeriesList.Count;
            ChartDataSeries showDataSeries1 = this.c1Chart2.ChartGroups.Group0.ChartData.SeriesList.AddNewSeries();
            seriesCount++;
            this.c1Chart2.ChartArea.AxisY.Text = "水位（m）";
            this.c1Chart2.ChartArea.AxisX.Text =  "泄流（m3/s）";
            this.c1Chart2.Header.Compass = CompassEnum.North;
            this.c1Chart2.Header.Visible = true;
            this.c1Chart2.Header.Text = "水位泄流曲线";

            int rowsCount = nQ;
            //showDataSeries1.FillStyle.FillType = FillTypeEnum.Gradient;
            //showDataSeries1.FillStyle.Color1 = Color.BlueViolet;
            //showDataSeries1.FillStyle.Color2 = Color.White;
            //showDataSeries1.FillStyle.GradientStyle = GradientStyleEnum.Vertical;
            showDataSeries1.LineStyle.Color = Color.BlueViolet;
            showDataSeries1.LineStyle.Thickness = 2;
            showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.None;
            //showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.Square;
            //showDataSeries1.SymbolStyle.Color = Color.Transparent;
            //showDataSeries1.SymbolStyle.OutlineColor = Color.OrangeRed;

            showDataSeries1.X.Length = rowsCount;
            showDataSeries1.Y.Length = rowsCount;
            //this.c1Chart2.ChartArea.AxisX.ValueLabels.Clear();
            for (i = 0; i < rowsCount; i++)
            {
                showDataSeries1.X[i] = ZQ_Q[i];
                showDataSeries1.Y[i] = ZQ_Z[i];
            }
            this.c1Chart2.ResumeLayout();
        }
        private void DrawChartThree()
        {
            int nQc;
            int nQo;
            nQc = this.flexGridExcel3.Rows.Count - 1;
            nQo = this.flexGridExcel4.Rows.Count - 1;

            float[] TQc_T = new float[nQc];
            float[] TQc_Q = new float[nQc];
            float[] TQZo_T = new float[nQo];
            float[] TQZo_Q = new float[nQo];
            float[] TQZo_Z = new float[nQo];

            int i;
            try
            {
                for (i = 0; i < nQc; i++)
                {
                    TQc_T[i] = Convert.ToSingle(flexGridExcel3.Rows[i + 1][1].ToString());
                    TQc_Q[i] = Convert.ToSingle(flexGridExcel3.Rows[i + 1][2].ToString());
                }
                for (i = 0; i < nQo; i++)
                {
                    TQZo_T[i] = Convert.ToSingle(flexGridExcel4.Rows[i + 1][1].ToString());
                    TQZo_Q[i] = Convert.ToSingle(flexGridExcel4.Rows[i + 1][3].ToString());
                    TQZo_Z[i] = Convert.ToSingle(flexGridExcel4.Rows[i + 1][4].ToString());
                }
            }
            catch
            {
                MessageBox.Show("数据有错误！请检查！", "出错啦!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.c1Chart3.ChartGroups.Group0.ChartData.SeriesList.RemoveAll();
            this.c1Chart3.ChartGroups.Group1.ChartData.SeriesList.RemoveAll();
            this.c1Chart3.SuspendLayout();
            int seriesCount = this.c1Chart3.ChartGroups.Group0.ChartData.SeriesList.Count;
            this.c1Chart3.ChartArea.AxisY.Text = "流量（m3/s）";
            this.c1Chart3.ChartArea.AxisX.Text = "时间（h）";
            this.c1Chart3.ChartArea.AxisY2.Text = "库水位（m）";
            this.c1Chart3.Header.Compass = CompassEnum.North;
            this.c1Chart3.Header.Visible = true;
            this.c1Chart3.Header.Text = "调洪演算结果";
            ChartDataSeries showDataSeries1 = this.c1Chart3.ChartGroups.Group0.ChartData.SeriesList.AddNewSeries();
            seriesCount++;
            showDataSeries1.LineStyle.Color = Color.Red;
            showDataSeries1.LineStyle.Thickness = 2;
            showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.None;
            showDataSeries1.Label = "入库流量";
            //showDataSeries1.SymbolStyle.Shape = SymbolShapeEnum.Square;
            //showDataSeries1.SymbolStyle.Color = Color.Transparent;
            //showDataSeries1.SymbolStyle.OutlineColor = Color.OrangeRed;
            
            showDataSeries1.X.Length = nQc ;
            showDataSeries1.Y.Length = nQc;
            for (i = 0; i < nQc; i++)
            {
                showDataSeries1.X[i] = TQc_T[i];
                showDataSeries1.Y[i] = TQc_Q[i];
            }
            ChartDataSeries showDataSeries2 = this.c1Chart3.ChartGroups.Group0.ChartData.SeriesList.AddNewSeries();
            seriesCount++;
            showDataSeries2.LineStyle.Color = Color.Green;
            showDataSeries2.LineStyle.Thickness = 2;
            showDataSeries2.SymbolStyle.Shape = SymbolShapeEnum.None;
            showDataSeries2.Label = "出库流量";
            //showDataSeries2.SymbolStyle.Shape = SymbolShapeEnum.Square;
            //showDataSeries2.SymbolStyle.Color = Color.Transparent;
            //showDataSeries2.SymbolStyle.OutlineColor = Color.OrangeRed;

            showDataSeries2.X.Length = nQo ;
            showDataSeries2.Y.Length = nQo;
            for (i = 0; i < nQo; i++)
            {
                showDataSeries2.X[i] = TQZo_T[i];
                showDataSeries2.Y[i] = TQZo_Q[i];
            }
            ChartDataSeries showDataSeries3 = this.c1Chart3.ChartGroups.Group1.ChartData.SeriesList.AddNewSeries();
            showDataSeries3.LineStyle.Color = Color.Orange;
            showDataSeries3.LineStyle.Thickness = 2;
            showDataSeries3.SymbolStyle.Shape = SymbolShapeEnum.None;
            showDataSeries3.Label = "库水位";
            //showDataSeries3.SymbolStyle.Shape = SymbolShapeEnum.Square;
            //showDataSeries3.SymbolStyle.Color = Color.Transparent;
            //showDataSeries3.SymbolStyle.OutlineColor = Color.OrangeRed;

            showDataSeries3.X.Length = nQo;
            showDataSeries3.Y.Length = nQo;
            for (i = 0; i < nQo; i++)
            {
                showDataSeries3.X[i] = TQZo_T[i];
                showDataSeries3.Y[i] = TQZo_Z[i];
            }

            this.c1Chart3.ResumeLayout();
        }

       
        
       



    }


}