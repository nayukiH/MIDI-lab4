using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZedGraph;
using System.IO.Ports;
using System.Windows.Forms;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Color = System.Drawing.Color;
using System.Threading;

namespace MIDI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort port = null;
        midi Receive = new midi();
        midi Send = new midi();
        midi pwm = new midi();
        int tickStart = 0;
        log l = new log();

        public MainWindow()
        {
            InitializeComponent();
            
            Binding binding = new Binding("ReceiveMessage");
            binding.Source = Receive;
            binding.Mode = BindingMode.OneWay;
            receiveData.SetBinding(TextBox.TextProperty, binding);
            
            binding = new Binding("SendMessage");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            sendData.SetBinding(TextBox.TextProperty, binding);
            
            binding = new Binding("getDegree");
            binding.Source = Receive;
            binding.Mode = BindingMode.OneWay;
            Temp.SetBinding(TextBlock.TextProperty, binding);
            
            binding = new Binding("getLight");
            binding.Source = Receive;
            binding.Mode = BindingMode.OneWay;
            light.SetBinding(TextBlock.TextProperty, binding);
            
            setGraph();

            combo2.Items.Clear();
            combo2.Items.Add("9600");
            combo2.Items.Add("19200");
            combo2.Items.Add("38400");
            combo2.Items.Add("57600");
            combo2.Items.Add("115200");
            combo2.Items.Add("921600");
            combo2.SelectedItem = combo2.Items[2];

            yellow.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(S_MouseLeftButtonUp), true);
            green.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(S_MouseLeftButtonUp), true);
            blue.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(S_MouseLeftButtonUp), true);
            red.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(S_MouseLeftButtonUp), true);
            white.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(S_MouseLeftButtonUp), true);
        }

        private void setBind(Slider slider)
        {
            Binding binding = new Binding("Pin");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            slider.SetBinding(Slider.TagProperty, binding);

            binding = new Binding("State");
            binding.Source = Send;
            binding.Mode = BindingMode.OneWayToSource;
            slider.SetBinding(Slider.ValueProperty, binding);
        }
        
        public void portName_DropDownOpened(object sender, EventArgs e)
        {
            string[] portNames = SerialPort.GetPortNames();
            ComboBox combo = sender as ComboBox;
            combo.Items.Clear();
            foreach (string name in portNames)
            {
                combo.Items.Add(name);
            }

        }

        private void closePort()
        {
            if (port != null)
            {
                port.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                port.Close();
                MessageBox.Show("断开成功");
            }
        }
         
        private void closedSerialPort(object sender, RoutedEventArgs e)
        {
            closePort();
        }
        
        private void openSerialPort(object sender, RoutedEventArgs e)
        {
            if (combo1.SelectedItem != null)
            {
                if (port != null)
                {
                    port.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                    port.Close();
                }
                port = new SerialPort(combo1.SelectedItem.ToString());
                port.BaudRate =int.Parse(combo2.SelectedItem.ToString());
                //一些基本设置
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.RtsEnable = false;
                port.ReceivedBytesThreshold = 1;
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                
                try
                {
                    port.Open();
                    MessageBox.Show("连接成功！");
                }
                catch (Exception e1)
                {
                    MessageBox.Show("连接失败！");
                }

            }
            else
            {
                MessageBox.Show("未选中串口");
            }
        }
        
        private void display()
        {
            if ((Receive.getReceive[0] & 0xf0) == 0xe0)
            {
                if ((Receive.getReceive[0] & 0xf) == 0)
                {
                    Receive.getDegree = Receive.byte_to_int;
                    setReturnTextBlock(Temp, Receive.getDegree);
                }
                else if ((Receive.getReceive[0] & 0xf) == 1)
                {
                    Receive.getLight = Receive.byte_to_int;
                    setReturnTextBlock(light, Receive.getLight);
                }
            }
        }
        
        private void DataSend(object sender, RoutedEventArgs e)
        {
            byte[] data = Send.SendMessageToByte;
            
            if (data != null && port != null && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
        }

        private void ColorSend()
        {
            try
            {
                byte val_yellow = (byte)yellow.Value;
                byte val_red = (byte)red.Value;
                byte val_green = (byte)green.Value;
                byte val_blue = (byte)blue.Value;
                byte val_white = (byte)white.Value;
                MessageBox.Show("黄灯亮度： " + val_yellow.ToString() + "\n"
                            + "绿灯亮度： " + val_green.ToString() + "\n"
                            + "蓝灯亮度： " + val_blue.ToString() + "\n"
                            + "红灯亮度： " + val_red.ToString() + "\n"
                            + "白灯亮度： " + val_white.ToString() + "\n");

                pwm.CreateMidiPWM(val_yellow, 3);
                port.Write(pwm.getSend, 0, pwm.getSend.Length);
                pwm.CreateMidiPWM(val_green, 5);
                port.Write(pwm.getSend, 0, pwm.getSend.Length);
                pwm.CreateMidiPWM(val_blue, 6);
                port.Write(pwm.getSend, 0, pwm.getSend.Length);
                pwm.CreateMidiPWM(val_red, 9);
                port.Write(pwm.getSend, 0, pwm.getSend.Length);
                pwm.CreateMidiPWM(val_white, 10);
                port.Write(pwm.getSend, 0, pwm.getSend.Length);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (port == null) return;
            int byte_size = port.BytesToRead;
            for (int i = 0; i < byte_size; i++)
            {
                int inByte = port.ReadByte();
                if ((inByte & 0x80) == 0x80)
                {
                    Receive.index = 0;
                    Receive.getReceive[Receive.index] = (byte)inByte;
                    Receive.index++;
                }
                else if (Receive.index != 0 && Receive.index < Receive.getReceive.Length)
                {
                    Receive.getReceive[Receive.index] = (byte)inByte;
                    Receive.index++;
                }
                if (Receive.index == 3)
                {
                    string s = string.Format("\n 接收数据:{0:X2}-{1:X2}-{2:X2},对应数据：0x{3:X4}",
                        Receive.getReceive[0], Receive.getReceive[1],
                        Receive.getReceive[2], Receive.byte_to_int);
                    
                    setReturnTextBox(receiveData, s);
                    display();
                    if ((Receive.getReceive[0] & 0xf0) == 0xe0)
                        drawline(Receive.getReceive[0] & 0xf, Receive.byte_to_int);
                    if (click == 1)
                    {
                        l.AD = Receive.getReceive;
                        l.PWM = Send.getSend;
                        l.setString();
                    }
                }
            }
        }

        private delegate void setTextBox(TextBox textBox, string s);
        
        public void setReturnTextBox(TextBox textBox, string s)
        {
            if (textBox.Dispatcher.CheckAccess())
            {
                textBox.AppendText(s);
                textBox.ScrollToEnd();
            }
            else
            {
                setTextBox setText = new setTextBox(setReturnTextBox);
                Dispatcher.Invoke(setText, new object[] { textBox, s });
            }
        }

        //发送温度和光强至textblock中
        private delegate void setTextBlock(TextBlock textBlock, int num);

        public void setReturnTextBlock(TextBlock textBlock, int num)
        {
            if (textBlock.Dispatcher.CheckAccess())
            {
                textBlock.Text = num.ToString();
            }
            else
            {
                setTextBlock set = new setTextBlock(setReturnTextBlock);
                Dispatcher.Invoke(set, new object[] { textBlock, num });
            }
        }

        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color Red = Color.Red;
            Color Green = Color.Green;
            Color Yellow = Color.Yellow;
            Color Blue = Color.Blue;
            Color White = Color.White;
            double r =  Yellow.R * yellow.Value / 255 + Green.R * green.Value / 255 +
                Blue.R * blue.Value / 255 + Red.R * red.Value / 255 + White.R * white.Value / 255;
            double g = Yellow.G * yellow.Value / 255 + Green.G * green.Value / 255 +
                Blue.G * blue.Value / 255 + Red.G * red.Value / 255 + White.G * white.Value / 255;
            double b = Yellow.B * yellow.Value / 255 + Green.B * green.Value / 255 +
                Blue.B * blue.Value / 255 + Red.B * red.Value / 255 + White.B * white.Value / 255;
            ShowColor.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        private void setGraph()
        {
            GraphPane graph = zedgraph.GraphPane;
            
            graph.Title.Text = "温度光强实时动态图";
            graph.Title.FontSpec.Size = 30;
            graph.XAxis.Title.Text = "时间/秒";
            graph.XAxis.Title.FontSpec.Size = 20;
            graph.YAxis.Title.Text = "数值";
            graph.YAxis.Title.FontSpec.Size = 20;
            RollingPointPairList list1 = new RollingPointPairList(1200);
            RollingPointPairList list2 = new RollingPointPairList(1200);
            LineItem tempre = graph.AddCurve("温度", list1, System.Drawing.Color.Blue, SymbolType.None);
            LineItem light = graph.AddCurve("光强", list2, System.Drawing.Color.Red, SymbolType.None);
            graph.XAxis.Scale.Min = 0;
            graph.XAxis.Scale.MaxGrace = 0.01;
            graph.XAxis.Scale.MaxGrace = 0.01;
            graph.XAxis.Scale.Max = 30;
            graph.XAxis.Scale.MinorStep = 1;
            graph.XAxis.Scale.MajorStep = 5;
           
            tickStart = Environment.TickCount;
            zedgraph.AxisChange();

        }
        
        private void drawline(int channel, double data)
        {
            if (zedgraph.GraphPane.CurveList.Count <= 0) return;

            LineItem line;
            if (channel == 0)
            {
                //温度
                line = zedgraph.GraphPane.CurveList[0] as LineItem;
            }
            else
            {
                //光强
                line = zedgraph.GraphPane.CurveList[1] as LineItem;
            }

            if (line == null) return;
            IPointListEdit list = line.Points as IPointListEdit;

            if (list == null) return;
            double time = (Environment.TickCount - tickStart) / 1000.0;
            list.Add(time, data);
            
            Scale x = zedgraph.GraphPane.XAxis.Scale;
            if (time > x.Max - x.MajorStep)
            {
                x.Max = time + x.MajorStep;
                x.Min = x.Max - 30.0;
            }
            
            zedgraph.AxisChange();
            zedgraph.Invalidate();
        }

        int click = 0;
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            click = 1;
        }
        
        private async void End_Click(object sender, RoutedEventArgs e)
        {
            if (port == null) return;

            SaveFileDialog save = new SaveFileDialog();
            System.IO.Stream stream;
            save.Filter = "日志|*.json;*.csv;*.xml";
            save.FilterIndex = 3;
            save.RestoreDirectory = true;
            
            DateTime date = DateTime.Now;
            string day = date.ToShortDateString().ToString().Replace('/', '-');
            string time = date.ToLongTimeString().ToString().Replace(':', '-');
            save.FileName = string.Format("log-{0}-{1}",
                day, time);

            string temp = l.getString + "port:" + combo1.SelectedItem.ToString() + "\n" +
                "BPS:" + combo2.SelectedItem.ToString() + "}";

            UnicodeEncoding unicode = new UnicodeEncoding();

            byte[] data = unicode.GetBytes(temp);
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((stream = save.OpenFile()) != null)
                {
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Close();

                }

            }
        }

        private void S_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ColorSend();
        }
    }
}
