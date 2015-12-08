using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports; 

namespace ArduinoClass
{
    public class Arduino
    {
        public System.IO.Ports.SerialPort serialPort1;
        public void Initialize()
        {
            System.ComponentModel.IContainer components = new System.ComponentModel.Container();
            serialPort1 = new System.IO.Ports.SerialPort(components);
            serialPort1.PortName = "COM3";
            serialPort1.BaudRate = 38400;
            serialPort1.DtrEnable = true;
            serialPort1.NewLine = "\n";
            serialPort1.Open();
        }
        public void Initialize(string Port, int Baud, bool DtrEnable, string Newline)
        {
            System.ComponentModel.IContainer components = new System.ComponentModel.Container();
            serialPort1 = new System.IO.Ports.SerialPort(components);
            serialPort1.PortName = Port;
            serialPort1.BaudRate = Baud;
            serialPort1.DtrEnable = DtrEnable;
            serialPort1.NewLine = Newline;
            serialPort1.Open();
        }
        public void SendString(string strToSend)
        {
            serialPort1.WriteLine(strToSend);
        }
        public void CloseConnection()
        {
            serialPort1.Close();
        }
        public string ReadString(int tTimeout)
        {
            string Ris = "";
            serialPort1.ReadTimeout = tTimeout;
            try
            {
                Ris = serialPort1.ReadLine();
            }
            catch (TimeoutException) { }
            return Ris;
        }
        public IRMessage ReadIR(int tTimeout)
        {
            IRMessage Ris = new IRMessage();
            Ris.recieved = true;
            serialPort1.ReadTimeout = tTimeout;
            try
            {
                string Codice = serialPort1.ReadLine();
                Ris.rowMessage = Codice;
                if (!Codice.StartsWith("#IR")) throw new Exception("Non riconosciuto");
                string[] Cod = Codice.Split('<')[1].Split('>')[0].Split('|');
                Ris.valueIR = Convert.ToInt32(Cod[0], 16);
                Ris.coding = Cod[1]; 
            }
            catch (Exception) {
                Ris.recieved = false;
            }
            return Ris;
        }
        public void SendIR(IRMessage Msg)
        {
            string strToSend = "#";
            strToSend += Msg.valueIR.ToString();
            strToSend += "|";
            strToSend += Msg.coding;
            serialPort1.WriteLine(strToSend);
        }

        public class IRMessage
        {
            public int valueIR;
            public string coding;
            public bool recieved;
            public string rowMessage;
        }
        public class Coding
        {
            public string NEC = "NEC";
            public string SONY = "SONY";
            public string Unknow = "Unknow";
            public string RC5 = "RC5";
            public string PANASONIC = "PANASONIC";
            public string LG = "LG";
            public string JVC = "JVC";
            public string RC6 = "RC6";
            // non mappate in input
            public string Denon = "DEN";
            public string DISH = "DISH";
            public string SAMSUNG = "SAMS";
            public string Sharp = "SHARP";
            public string SharpRaw = "SHARPR";
            public string Whynter = "WHY";


            private string _coding = "Unknow";
            public string Value
            {
                get
                {
                    return _coding;
                }
                set
                {
                    _coding = value;
                }
            }
        }



    }
}
