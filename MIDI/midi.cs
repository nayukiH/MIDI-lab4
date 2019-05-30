using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MIDI
{

    class midi
    {

        private byte[] receive_buffer;
        private byte[] send_buffer;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public midi()
        {
            send_buffer = new byte[3];
            send_buffer[0] = 0;
            send_buffer[1] = 0;
            send_buffer[2] = 0;
            
            receive_buffer = new byte[3];
            receive_buffer[0] = 0;
            receive_buffer[1] = 0;
            receive_buffer[2] = 0;
        }
        
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public byte[] getReceive
        {
            get
            {
                return receive_buffer;
            }
            set
            {
                receive_buffer = value;
                NotifyPropertyChanged();
            }
        }
        
        public byte[] getSend
        {
            get
            {
                return send_buffer;
            }
            set
            {
                if (send_buffer != value)
                {
                    send_buffer = value;
                }
            }
        }

        public int index { get; set; }
        
        public int byte_to_int
        {
            get
            {
                return (int)(receive_buffer[2] << 7) + receive_buffer[1];
            }

        }
        
        private string receiveMessage;
        public string ReceiveMesssage
        {
            get
            {
                return receiveMessage;
            }
            set
            {
                if (receiveMessage != value)
                {
                    receiveMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        private string sendMessage;
        public string SendMessage
        {
            get
            {
                return sendMessage;
            }
            set
            {
                if (sendMessage != value)
                {
                    sendMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte[] SendMessageToByte
        {
            get
            {
                if (string.IsNullOrEmpty(sendMessage))
                {
                    return null;
                }

                string[] temp = sendMessage.Split(new Char[] { ' ', ',', '.', ':', '\t' });
                byte[] result = new byte[temp.Length];

                for (int i = 0; i < temp.Length; i++)
                {
                    if ((byte.TryParse(temp[i], System.Globalization.NumberStyles.HexNumber,
                        null, out result[i])) == false)
                    {
                        result[i] = 0;
                    }
                }

                return result;
            }
        }

        public void CreateMidiPWM(int k, int PWMPin)
        {
            this.getSend[0] = (byte)(0xd0 | PWMPin);
            this.getSend[1] = (byte)(k & 0x7f);
            this.getSend[2] = (byte)((k >> 7) & 0x7f);
        }

        //温度
        private int degree;
        public int getDegree
        {
            get
            {
                return degree;
            }
            set
            {
                if (degree != value)
                {
                    degree = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //光强
        private int light_intense;
        public int getLight
        {
            get
            {
                return light_intense;
            }
            set
            {
                if (light_intense != value)
                {
                    light_intense = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
