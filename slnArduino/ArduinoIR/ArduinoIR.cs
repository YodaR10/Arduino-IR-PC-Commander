using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ArduinoClass;
using System.IO;
using System.Diagnostics;

namespace Arduino1
{
    
    public partial class Form1 : Form
    {
        private string lastMessage = "";
        private bool _isrun = false;
        private StopW sStopW = new StopW();

        ArduinoClass.Arduino Arduino = new ArduinoClass.Arduino();
        public Form1()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Inizializza arduino
            string sFase = "inizializzazione";
            try {
                Arduino.Initialize();

                //Carica file condizioni
                sFase = "caricemento parametri";
                ReadParamList();
                sFase = "start timer";
                timer1.Start();
                sFase = "start stopwatch";
                sStopW.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore nella fase di " + sFase + ": " + ex.Message);
                Application.Exit();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool bRicevuto = true;
            if (!_isrun)
            {
                _isrun = true;
                while (bRicevuto)
                {
                    Arduino.IRMessage IRMsg = new Arduino.IRMessage();
                    IRMsg = Arduino.ReadIR(100);
                    if (IRMsg.recieved)
                    {
                        if (lastMessage == IRMsg.rowMessage && !sStopW.OverWait())
                        {
                            //Se è ripetuto in un tempo al di sotto del wait 
                            //Non faccio nulla
                        }
                        else
                        {
                            sStopW.Restart(lastMessage == IRMsg.rowMessage); //resetto il timer
                            lastMessage = IRMsg.rowMessage;
                            lstRis.Items.Add(IRMsg.rowMessage);
                            foreach(Condizione cond in Condizioni)
                            {
                                if(cond.coding == IRMsg.coding && cond.value == IRMsg.valueIR)
                                {
                                    //Se la condizione è verificata
                                    //MessageBox.Show("Condizione verificata, eseguo azione " + cond.action);
                                    cond.Execute(Arduino);
                                }
                            }
                        }
                    }
                    bRicevuto = IRMsg.recieved;
                    IRMsg = null;
                }
                //string Ricevuto = Arduino.ReadString(100);
                //if (!string.IsNullOrEmpty(Ricevuto)) lstRis.Items.Add(Ricevuto);
                _isrun = false;
            }
        }

        public void ReadParamList()
        {
            try {
                string line;
                if (File.Exists("config.txt"))
                {
                    StreamReader file = new StreamReader("config.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        // coding|value|action#condizione#param1|param2|param3
                        Condizione Cond = new Condizione();
                        string[] lin1 = line.Split('#');
                        string[] lin2 = lin1[0].Split('|');
                        Cond.coding = lin2[0];
                        Cond.value = Convert.ToInt32(lin2[1], 16);
                        Cond.action = lin2[2];
                        if (!string.IsNullOrEmpty(lin1[1]))
                        {
                            Cond.cond = lin1[1];
                        } else
                        {
                            Cond.cond = null;
                        }
                        if (!string.IsNullOrEmpty(lin1[2]))
                        {
                            string[] lin3 = lin1[2].Split('|');
                            Cond.param = lin3;
                        }
                        Condizioni.Add(Cond);

                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Errore nel caricamento del file di configurazione: " + ex.Message);
            }
        }

        public List<Condizione> Condizioni = new List<Condizione>();


        public class Condizione
        {
            public string coding;
            public long value;
            public string action;
            public string cond;
            public string[] param;
            public bool CheckCond()
            {
                //Interpreta e verifica le condizioni
                return true;
            }
            public void Execute(ArduinoClass.Arduino ard)
            {
                switch (action)
                {
                    case "SENDKEY":
                        { //Invia le key indicate
                            for (int i = 0; i < param.Length; i++)
                            {
                                string strToSend = param[i];
                                //if (strToSend == "{ENTER}")
                                //{
                                //    SendKeys.SendWait("{ENTER}");
                                //}
                                //else
                                {
                                    SendKeys.Send(param[i]);
                                }
                            }
                            break;
                        }
                    //case "SENDKEYK": //Trasmette in contemporanea un key
                    //    {
                    //        for (int i = 1; i < param.Length; i++)
                    //        {
                    //            string IRstring = "#" + param[0].Replace('@', '|');
                    //            ard.SendString(IRstring);
                    //            string strToSend = param[i];
                    //            SendKeys.Send(param[i]);
                    //        }
                    //        break;
                    //    }
                    case "RUNEXE":
                        {
                            for (int i = 0; i < param.Length; i++)
                            {
                                string strToSend = param[i];
                                Process.Start(param[i]);
                            }
                            break;
                        }

                }

            }

        }

        
        public class StopW
        {
            private Stopwatch _sWatch = Stopwatch.StartNew();
            public long WaitMs;
            const long InitialWait = 200;
            //const long MsAfterReset = 1000; //Millisecondi dopo di che viene resettato
            public void Start()
            {
                _sWatch.Start();
                WaitMs = InitialWait;
            }
            public bool OverWait()
            {
                if(_sWatch.ElapsedMilliseconds  > WaitMs)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } 
            public void Restart(bool shortTime)
            {
                _sWatch.Reset();
                _sWatch.Start();

                WaitMs = InitialWait; 
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Scrive su file
            using (StreamWriter fileout = new StreamWriter("RecievedLog_" + DateTime.Today.ToString("yyyyMMdd") + ".txt"))
                foreach (var ListItem in lstRis.Items)
                {
                    fileout.Write(ListItem.ToString());
                }
            MessageBox.Show("Scritto file: RecievedLog_" + DateTime.Today.ToString("yyyyMMdd") + ".txt");
               
        }


        private void cdmClear_Click(object sender, EventArgs e)
        {
            lstRis.Items.Clear();
        }
    }
}
