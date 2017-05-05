using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Reflection;

namespace WindowsFormsApplication8
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        Socket socket;
        Stream stream;
        TcpListener listener;
        TcpClient client;
        UTF8Encoding enc = new UTF8Encoding();

        public delegate void InvokeDelegate();
        string Version_name = "Connect4 Mutiplayer beta1.6";
        string Client_name = "N/A"; /// อยู่ฝั้นของ server
        string Server_name = "N/A"; // อยู่ฝั่งของ Client



        // gg_form_load // gg_form_Close
        private void Form1_Load(object sender, EventArgs e)
        {
            load_UI();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            MediaPlayer1_BG_Soubd.URL = "bg.mp3";
      

            try
            {
                IPHostEntry ipHostEntries = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                TX_Name_Input.Text = Dns.GetHostName();
                //TX_IP_Server_Input.Text = arrIpAddress[arrIpAddress.Length - 2].ToString();


                for (int i = 0; i < arrIpAddress.Count(); i++)
                {
                    //Console.Write("1");
                    if (arrIpAddress[i].ToString().Length > 15)
                    {
                        continue;
                    }
                    else
                    {
                        TX_IP_Server_Input.Text = arrIpAddress[i].ToString();
                    }
                }

                arrIpAddress.ToString();

                bool networkUp = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (networkUp == false)
                {
                    MessageBox.Show("Network Error !!!\nPlease check the network.");
                    Application.Exit();
                }

                chk = true;
            }
            catch (Exception)
            {
                TX_Name_Input.Text = "Get PC Name Error";
                TX_IP_Server_Input.Text = "Get IP Address Error";
                BTN_Sumbit.Enabled = false;

                MessageBox.Show("โปรแกรมไม่สามารถแสดงชื่อเครื่องคอมพิวเตอร์ และ IP Address ได้ \n\n โปรแกรมจะปิดตัวลง", "Network Error!!!");
                Application.Exit();
            }
        }
        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (Chk_server == true && socket.Connected == true)
                {
                    socket.Send(enc.GetBytes("Recevice End"));
                }
                else if (Chk_server == false && client.Connected == true)
                {
                    raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|" + "END";
                    Thread TT = new Thread(Client_SendSmg);
                    TT.Start();
                }

            }
            catch(Exception )
            {}

            Environment.Exit(0);
        }


        //gg update_chat // gg_update_Console
        string temp_chat = "";
        void updateChat()
        {
            if (temp_chat != "")
            {
                string[] words2 = temp_chat.Split('|');
                if (words2[0].Equals("[EMO]"))
                {
                    if (Chk_server == true)
                    {
                        richTextBox_Chat.AppendText("" + Client_name + " Say : ");
                    }
                    else if ( Chk_server == false )
                    {
                        richTextBox_Chat.AppendText("" + Server_name  + " Say : ");
                    }
                    BeginInvoke(new InvokeDelegate(InvokeTT));
                    
                }
                else
                {
                    MediaPlayer2_Quick_sound.URL = "pop.wav";

                    richTextBox_Chat.AppendText(temp_chat);
                    temp_chat = "";
                    richTextBox_Chat.AppendText("\n");
                    richTextBox_Chat.ScrollToCaret();
                    updateConsole("[P][UI][UPDATE_CHATBOX]" + temp_chat);
                }
            }
        }

        public void InvokeTT()
        {
            string[] words2 = temp_chat.Split('|');
            int i = int.Parse(words2[1]);

            MediaPlayer2_Quick_sound.URL = "pop.wav";

            Image pppp = Image.FromFile(@"emo\(" + i + ").jpg");
            Clipboard.SetImage(pppp);
            richTextBox_Chat.Paste();

            richTextBox_Chat.AppendText("\n");
            richTextBox_Chat.ScrollToCaret();
        }



        void updateConsole(object data)
        {
            try
            {
                //copy_data = data;
                rich_console.AppendText("" + data + "\n");
                rich_console.ScrollToCaret();
                ////BeginInvoke(new InvokeDelegate(InvokeMethod1));
            }
            catch (Exception)
            { }
        }


        // gg_onoff_console
        bool chk = false;
        private void BTN_Sumbit_Click(object sender, EventArgs e)
        {
            if (TX_Name_Input.Text != "" && TX_IP_Server_Input.Text != "" && chk != false)
            {
                TB_Server_Name.Text = TX_Name_Input.Text;
                TB_Server_IP.Text = TX_IP_Server_Input.Text;
                panel2_rename.Visible = false;

                panel5_main.Visible = true;
                panel5_in_mode.Visible = true;
            }
        }



        //gg_server_zone
        bool Chk_server = false; //gg_chk_server

        void Server_Receiver() //gg_server_receiver 
        {
            try
            {
                IPAddress ip = IPAddress.Parse(TB_Server_IP.Text); // เปิด listener ไว้รอ 
                listener = new TcpListener(ip, 34567);
                listener.Start();

                updateConsole("[N]Server is running at port 34567");
                updateConsole("[N]LocalEndpoint is " + listener.LocalEndpoint); // LocalEndpoint คือ ฝั่ง Server

                // โปรแกรมจะรอการเชื่อมต่อ... 
                socket = listener.AcceptSocket();

                updateConsole("[N]Socket.listener.AcceptSocket()");
                updateConsole("[N]Connection accepted from " + socket.RemoteEndPoint); // RemoteEndpoint คือ ฝั่ง Cliente

                Chk_server = true;
                panel5_main.Visible = false;

                while (true)
                {
                    byte[] msg = new byte[100]; /// รับค่าเข้ามาจาก Client ASCII CODE
                    int j = socket.Receive(msg); /// Get in >>> j 

                    string msg_recevie = "";
                    char[] Temp = Encoding.UTF8.GetChars (msg );


                    for (int i = 0; i < j; i++)
                    {
                        msg_recevie += Temp[i];
                     
                    }

   
                    updateConsole("[N]Socket.Receive(" + msg_recevie + ")");

                    string[] words = msg_recevie.Split('|');

                    // [C] ระบบ Chat " [C]|[NAME]|"Msg" "
                    if (words[0].Equals("[C]"))
                    {
                        
                        if (words[2].Equals("END")) // ถ้า Client ส่ง END ให้ทำการปิด Server
                        {
                            updateConsole("[N][C]END");
                            updateConsole("[N][P]SEND_ACK");
                            socket.Send(enc.GetBytes("Recevice End"));
                            label_notification_detail.Text = ("Receive command disconnect from client");
                            break;
                        }
                        else if (words[2].Equals("[EMO]"))
                        {
                            int i = int.Parse(words[3].ToString());

                            temp_chat = "";
                            temp_chat = "[EMO]|" + i;

                           

                            Thread Update_chatt = new Thread(updateChat);
                            Update_chatt.Start();

                        }
                        else
                        {
                            updateConsole("[N][C][ChatRecevier]");
                            temp_chat = words[1] + " say : " + words[2];
                            Thread Update_chat = new Thread(updateChat);
                            Update_chat.Start();
                        }

                        // รีเทิร์นค่ากลับไปให้ Client ทำการลบ Buffer
                        socket.Send(enc.GetBytes("Chat Recevice")); // Socket เป็น Obj เก็บข้อมูลของ >> Send (ส่งกลับไป Endpoint) >> Re (รับจาก Endpoint) เป็นทั้ง Re , Send          
                    }

                   // [N][SYS][P] ระบบเกมต่างๆ  " [SYS]|[COMMAND]|[DETAILS]
                    else if (words[0].Equals("[SYS]"))
                    {
                        if (words[1].Equals("[NAME_CLINET]")) // เก็บชื่อ Client
                        {
                            Client_name = words[2].Substring(1, (words[2].Length - 2));
                            Server_name = TB_Server_Name.Text;

                            updateConsole("[N][SYS][NAME_CLIENT] = " + Client_name);
                            updateConsole("[N][SYS][NAME_SERVER] = " + Server_name);

                            raw_msg_send = "[SYS]|[NAME_SERVER]|[" + TB_Server_Name.Text + "]"; // ส่งชื่อของ Server กลับไปให้ Client 
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();

                            //Thread Start_connect_detection_Thread = new Thread(Start_connect_detection);
                            //Start_connect_detection_Thread.Start();

                            BeginInvoke(new InvokeDelegate(Start_connect_detection ));
                       
                            Game_random_turn();
                        }
                        else if (words[1].Equals("[GAME]")) // ค่าตารางที่ได้รับจาก Client
                        {

                            updateConsole("[N][SYS][GAME]" + words[2]);
                            if (words[2].Equals("[1]")) { try_throw(1); }
                            else if (words[2].Equals("[2]")) { try_throw(2); }
                            else if (words[2].Equals("[3]")) { try_throw(3); }
                            else if (words[2].Equals("[4]")) { try_throw(4); }
                            else if (words[2].Equals("[5]")) { try_throw(5); }
                            else if (words[2].Equals("[6]")) { try_throw(6); }
                            else if (words[2].Equals("[7]")) { try_throw(7); }

                        }
                    }
                }

                socket.Close();
                listener.Stop();
               
                updateConsole("[N]Socket.Close");
                updateConsole("[N]listener.Stop");
                updateConsole("[P]End Program");

                if (panel2_stopped.InvokeRequired)
                {
                    BeginInvoke(new InvokeDelegate(InvokeMethod_showStopped_windows));
                }
            }
            catch (Exception)
            {

            }
        }

        void Server_SendSmg()  // gg_server_sendSmg
        {
            try
            {
                if (socket.Connected == true)
                {
                    if (raw_msg_send != "")
                    {
                        string outMsg = raw_msg_send;
                        socket.Send(enc.GetBytes(outMsg));
                        updateConsole("[N][P][SERVER][SEND]" + outMsg);
                        raw_msg_send = "";
                    }
                }
            }
            catch (Exception)
            {
                client.Close();
                updateConsole("[N][P][SERVER][SEND][ERROR][CLIENT.CLOSE]");
            }
        }


        //gg_client_zone 
        string raw_msg_send = "";
    
        void Client_Connection()//gg_client_connection
        {
            if (client.Connected != true)
            {
                int i = 1;
                for (; i <= 3; i++)
                {
                    try
                    {
                        updateConsole("[N][CLIENT.CONECTION][TIME]" + i); status_join.Text = ("Connecting...time" + i);

                        client.Connect(TB_IP_join.Text, 34567); // ตัวนี้เป็น TCP จะลองเชื่อมต่อเอง... ส่วนของ Connection
                        // Client จะเก็บ
                        // Connected << เช็คว่าเชื่อต่อได้ไหม bool 
                        // Endpoint >> ของฝั่ง Server

                        updateConsole("[N][CLIENT.CONECTION][CONNECTED]"); status_join.Text = ("Connected");

                        stream = client.GetStream();

                        Thread TT = new Thread(Client_Receiver);
                        TT.Start();

                        // ส่งชื่อของ Client ไปให้ Server 
                        updateConsole("[N][CLIENT.CONNECTION][SEND][NAME_TO_SERVER]");
                        raw_msg_send = ("[SYS]|[NAME_CLINET]|[" + TB_Server_Name.Text + "]");
                        Thread TTT = new Thread(Client_SendSmg);
                        TTT.Start();


                        panel5_main.Visible = false;
                        updateConsole("[P][panel5_main][VISIBLE=FALSE]");

                        break;

                    }
                    catch (Exception)
                    {
                        updateConsole("[N][CLIENT.CONNECTION][CATCH][ERROR]"); status_join.Text = ("Connect Error");
                        if (i == 3)
                        {
                            BTN_Join.Enabled = true;
                            TB_IP_join.Enabled = true;

                            updateConsole("[P][BTN_Join][ENABLED=TRUE]");
                            updateConsole("[P][TB_IP_join][ENABLED=TRUE]");
                        }
                    }
                }
            }
            else
            {
                updateConsole("[N][CLIENT.CONNECTION][CONNECTED]");
                //Thread TT = new Thread(Client_SendSmg);
                //TT.Start();
            }
        }

        void Client_Receiver()  //gg_client_receiver
        {
            try
            {
                updateConsole("[N][CLIENT.RECEIVER][RUNNING]");
                while (true)
                {
                    byte[] bufferIn = new byte[100];
                    int j = stream.Read(bufferIn, 0, 100);

                    string msg_recevie = "";

                    char[] Temp = Encoding.UTF8.GetChars(bufferIn);
                    for (int i = 0; i < j; i++)
                    {
                        msg_recevie += Temp[i];
                    }

                    updateConsole("[N][CLIENT.RECEIVER][RECEICER]" + msg_recevie);

                    string[] words = msg_recevie.Split('|');

                    if (words[0].Equals("[C]"))
                    {

                        if (words[2].Equals("[EMO]"))
                        {
                            int i = int.Parse(words[3].ToString());

                            temp_chat = "";
                            temp_chat = "[EMO]|" + i;

                            Thread Update_chatt = new Thread(updateChat);
                            Update_chatt.Start();

                        }
                        else
                        {

                            updateConsole("[N][C][ChatRecevier]");

                            temp_chat = words[1] + " say : " + words[2];
                            Thread Update_chat = new Thread(updateChat);
                            Update_chat.Start();
                        }
                    }

                    if (words[0].Equals("[SYS]"))// ข้อความระบบ จาก Server 
                    {
                        if (words[1].Equals("[NAME_SERVER]"))
                        {
                            Server_name = words[2].Substring(1, (words[2].Length - 2)); // เก็บชื่อ Server ไว้
                            Client_name = TB_Server_Name.Text;

                            updateConsole("[N][SYS][NAME_CLIENT] = " + Client_name);
                            updateConsole("[N][SYS][NAME_SERVER] = " + Server_name);

                            //Thread Start_connect_detection_Thread = new Thread(Start_connect_detection);
                            //Start_connect_detection_Thread.Start();

                            BeginInvoke(new InvokeDelegate(Start_connect_detection));

                        }
                        else if (words[1].Equals("[TURN]"))
                        {
                            updateConsole("[N][SYS][TURN]" + words[2]);
                            if (words[2].Equals("[1]"))
                            {
                                Control_Throw_BTN(false);
                                _turn = 1;
                                Show_Turn();
                            }
                            else if (words[2].Equals("[2]"))
                            {
                                Control_Throw_BTN(true);
                                _turn = 2;
                                Show_Turn();
                            }
                        }
                        else if (words[1].Equals("[SHOW]"))
                        {
                            updateConsole("[N][SYS][SHOW]" + words[2]);
                            if (words[2].Equals("[P1WIN]"))
                            {
                                MessageBox.Show("You lost :P \n" + Server_name + " Win!!");
                                Control_Throw_BTN(false);
                            }
                            else if (words[2].Equals("[P2WIN]"))
                            {
                                MessageBox.Show("You Win :) \n" + Server_name + " lost!!");
                                Control_Throw_BTN(false);
                            }
                            else if (words[2].Equals("[DRAW]"))
                            {
                                MessageBox.Show("Draw!!!");
                                Control_Throw_BTN(false);
                            }
                        }
                        else if (words[1].Equals("[GAME]"))
                        {
                            updateConsole("[N][SYS][GAME]" + words[2]);
                            if (words[2].Equals("[1]")) { try_throw(1); }
                            else if (words[2].Equals("[2]")) { try_throw(2); }
                            else if (words[2].Equals("[3]")) { try_throw(3); }
                            else if (words[2].Equals("[4]")) { try_throw(4); }
                            else if (words[2].Equals("[5]")) { try_throw(5); }
                            else if (words[2].Equals("[6]")) { try_throw(6); }
                            else if (words[2].Equals("[7]")) { try_throw(7); }
                        }
                    }

                    else if (msg_recevie == "Recevice End")
                    {
                        updateConsole("[N][SYS][RECEVICE][END]");

                        client.Close();

                        label_notification_detail.Text = ("Receive command for disconnect from Server");

                        if (panel2_stopped.InvokeRequired)
                        {
                            BeginInvoke(new InvokeDelegate(InvokeMethod_showStopped_windows));
                        }
                    
                        updateConsole("[P][CLIENT.CLOES]");
                        updateConsole("[P]End Program");
                        break;
                    }
                }
            }
            catch (Exception)
            { }
        }

        void Client_SendSmg() //gg_client_sendsmg
        {
            try
            {
                updateConsole("[P][CLIENT_SENDSMG]");

                if (raw_msg_send != "")
                {
                    String outMsg = raw_msg_send;

                    //ASCIIEncoding enc = new ASCIIEncoding();
                    byte[] bufferOut = enc.GetBytes(outMsg); // แปลง Char >>> Buff Out [] 
                    outMsg.ToString();

                    bufferOut.ToArray();
                    // Stream stream = client.GetStream();      // ฝั่ง Client จะเป็นตัว Active IO // จะ Stram กลับไป End point
                    //เมื่อ เชื่อมต่อได้แล้วก็สามารถส่งข้อมูลไปที่ server ได้ด้วยการสร้าง object ของ Stream โดยใช้คำสั่ง client.GetStream();
                    stream.Write(bufferOut, 0, bufferOut.Length); // ( ตัวแปร Buffer , Index , lastIndex ) 

                    updateConsole("[P][CLIENT_SENDSMG][SW]" + outMsg);

                    raw_msg_send = "";
                    updateConsole("[P][CLIENT_SENDSMG][RAW.CLEAR]");
                }
            }
            catch (Exception)
            {
                updateConsole("[P][CLIENT_SENDSMG][CATCH][ERROR]");

                client.Close();
                updateConsole("[P][CLENTE.CLOES]");
                updateConsole("[P]End Program");
            }

        }

        // gg_Contlor_Zone

        public void InvokeMethod_showStopped_windows()   //gg_invoke_showstopped_windows
        {
            timer1_wait_time.Stop();  
            planal_wait_.Visible = false;
            panel2_stopped.Visible = true; 
        }
        
        private void BTN_Chat_send_Click(object sender, EventArgs e) //gg_btn_chat //gg_chat_send_click
        {

            if (TX_msg_chat.Text != "" && Chk_server == false) // Client SEND
            {
                updateConsole("[P][C][SEND]");
                richTextBox_Chat.AppendText("You say : " + TX_msg_chat.Text );
                raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|" + TX_msg_chat.Text;
                Thread TT = new Thread(Client_SendSmg);
                TT.Start();
                TX_msg_chat.Text = "";
                richTextBox_Chat.AppendText("\n");
                richTextBox_Chat.ScrollToCaret();
            }
            else if (TX_msg_chat.Text != "" && Chk_server == true)  // SERVER SEND
            {
                if (TX_msg_chat.Text == "END") { socket.Close(); }

                updateConsole("[P][C][SEND]");
                richTextBox_Chat.AppendText("You say : " + TX_msg_chat.Text );
                raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|" + TX_msg_chat.Text;
                Thread TT = new Thread(Server_SendSmg);
                TT.Start();
                TX_msg_chat.Text = "";
                richTextBox_Chat.AppendText("\n");
                richTextBox_Chat.ScrollToCaret();
            }
        }
        private void TX_msg_chat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BTN_Chat_send_Click(null, null);
            }
        }

        // gg_engin_zone
        // Turn 1=Server Turn 2=Client 
        // 1 = Blue 2=Red
        byte _turn = 1;
        byte[,] array = new byte[8, 8]; // ไม่เอา 0 เริ่มต้นที่ 1 X=1 y=1 ---- X=7 Y=7
        
        void try_throw(byte num_Y)
        {
            updateConsole("[P][G][TRY_THROW][TURN][" + _turn + "]");
            byte x = 7;
            while (true)
            {
                if (array[x, num_Y] == 0) // == 0 แสดงว่ายังว่าง
                {
                    if (Chk_server == true)
                    {
                        if (_turn == 1) // ตัว กด
                        {
                            Show_Turn();
                            array[x, num_Y] = 1;
                            update_table(num_Y);

                            raw_msg_send = "[SYS]|[GAME]|[" + num_Y + "]";
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();

                            _turn = 2;

                            Control_Throw_BTN(false);
                            Show_Turn();
                        }
                        else // รับ
                        {
                            array[x, num_Y] = 2;
                            update_table(num_Y);

                            _turn = 1;
                            Show_Turn();

                            Control_Throw_BTN(true);
                        }
                    }

                    else
                    {
                        if (_turn == 1) // รับ 
                        {
                            array[x, num_Y] = 1;
                            update_table(num_Y);

                            _turn = 2;
                            Show_Turn();
                            Control_Throw_BTN(true);
                        }
                        else // กด
                        {
                            Show_Turn();
                            array[x, num_Y] = 2;
                            update_table(num_Y);

                            raw_msg_send = "[SYS]|[GAME]|[" + num_Y + "]";
                            Thread TT = new Thread(Client_SendSmg);
                            TT.Start();

                            _turn = 1;
                            Show_Turn();
                            Control_Throw_BTN(false);

                        }
                    }
                    break;
                }
                else // ถ้าไม่ว่าง ให้เลือนขึ้นมาอีก 1 row  โดยการลบ X 
                {
                    x -= 1;
                    if (x < 1) { updateConsole("[P][G][TABLE_FULL]"); break; }
                }
            }

            if (Chk_server == true)
            {
                chk_conditions();
            }
            Show_Turn();

        }
        void update_table(byte num_YY)
        { 
            MediaPlayer2_Quick_sound.URL = "dics_drop.mp3";
            switch (num_YY)
            {
                    
                case 1:
                    {
                        if (array[7, 1] == 1) p71.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 1] == 1) p61.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 1] == 1) p51.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 1] == 1) p41.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 1] == 1) p31.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 1] == 1) p21.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 1] == 1) p11.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 1] == 2) p71.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 1] == 2) p61.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 1] == 2) p51.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 1] == 2) p41.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 1] == 2) p31.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 1] == 2) p21.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 1] == 2) p11.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 2:
                    {
                        if (array[7, 2] == 1) p72.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 2] == 1) p62.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 2] == 1) p52.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 2] == 1) p42.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 2] == 1) p32.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 2] == 1) p22.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 2] == 1) p12.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 2] == 2) p72.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 2] == 2) p62.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 2] == 2) p52.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 2] == 2) p42.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 2] == 2) p32.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 2] == 2) p22.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 2] == 2) p12.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 3:
                    {
                        if (array[7, 3] == 1) p73.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 3] == 1) p63.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 3] == 1) p53.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 3] == 1) p43.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 3] == 1) p33.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 3] == 1) p23.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 3] == 1) p13.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 3] == 2) p73.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 3] == 2) p63.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 3] == 2) p53.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 3] == 2) p43.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 3] == 2) p33.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 3] == 2) p23.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 3] == 2) p13.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 4:
                    {
                        if (array[7, 4] == 1) p74.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 4] == 1) p64.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 4] == 1) p54.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 4] == 1) p44.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 4] == 1) p34.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 4] == 1) p24.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 4] == 1) p14.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 4] == 2) p74.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 4] == 2) p64.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 4] == 2) p54.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 4] == 2) p44.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 4] == 2) p34.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 4] == 2) p24.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 4] == 2) p14.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 5:
                    {
                        if (array[7, 5] == 1) p75.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 5] == 1) p65.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 5] == 1) p55.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 5] == 1) p45.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 5] == 1) p35.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 5] == 1) p25.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 5] == 1) p15.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 5] == 2) p75.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 5] == 2) p65.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 5] == 2) p55.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 5] == 2) p45.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 5] == 2) p35.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 5] == 2) p25.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 5] == 2) p15.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 6:
                    {
                        if (array[7, 6] == 1) p76.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 6] == 1) p66.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 6] == 1) p56.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 6] == 1) p46.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 6] == 1) p36.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 6] == 1) p26.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 6] == 1) p16.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 6] == 2) p76.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 6] == 2) p66.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 6] == 2) p56.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 6] == 2) p46.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 6] == 2) p36.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 6] == 2) p26.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 6] == 2) p16.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
                case 7:
                    {
                        if (array[7, 7] == 1) p77.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[6, 7] == 1) p67.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[5, 7] == 1) p57.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[4, 7] == 1) p47.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[3, 7] == 1) p37.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[2, 7] == 1) p27.Image = WindowsFormsApplication8.Properties.Resources.Coin2;
                        if (array[1, 7] == 1) p17.Image = WindowsFormsApplication8.Properties.Resources.Coin2;

                        if (array[7, 7] == 2) p77.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[6, 7] == 2) p67.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[5, 7] == 2) p57.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[4, 7] == 2) p47.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[3, 7] == 2) p37.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[2, 7] == 2) p27.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        if (array[1, 7] == 2) p17.Image = WindowsFormsApplication8.Properties.Resources.Coin1;
                        break;
                    }
            }
            updateConsole("[P][G][UPDATE_TABLE]");
        }

        void chk_conditions() //gg_chk_conditions //gg_conditions
        {

            byte sumP1 = 0, sumP2 = 0;
            bool Chk_win = false;
            // เช็ค แถว แนวนอน;
            for (int x = 7; x > 1; x--)
            {
                if (Chk_win == true) { break; }

                for (int y = 1; y < 8; y++)
                {
                    if (array[x, y] == 1)
                    {
                        sumP1 += 1;
                        sumP2 = 0;
                        if (sumP1 == 5)
                        {
                            Chk_win = true;
                            raw_msg_send = "[SYS]|[SHOW]|[P1WIN]";
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();


                            planal_wait_.Visible = false;
                            timer1_wait_time.Stop();

                            MessageBox.Show("You Win :) \n" + Client_name + " lost!!");
                            Control_Throw_BTN(false);

                            break;
                        }
                    }
                    else if (array[x, y] == 2)
                    {
                        sumP2 += 1;
                        sumP1 = 0;
                        if (sumP2 == 5)
                        {
                            raw_msg_send = "[SYS]|[SHOW]|[P2WIN]";
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();


                            planal_wait_.Visible = false;
                            timer1_wait_time.Stop();


                            MessageBox.Show("You lost :P \n" + Client_name + " Win!!");
                            Control_Throw_BTN(false);

                            break;
                        }
                    }
                    else
                    {
                        sumP1 = 0; sumP2 = 0;
                    }
                }
            }

            //เช็ค แถว แนวตั้ง
            sumP1 = 0; sumP2 = 0;
            for (int y = 1; y < 8; y++)
            {
                if (Chk_win == true) { break; }

                for (int x = 7; x > 0; x--)
                {
                    if (array[x, y] == 1)
                    {
                        sumP1 += 1;
                        sumP2 = 0;
                        if (sumP1 == 5)
                        {
                            Chk_win = true;
                            raw_msg_send = "[SYS]|[SHOW]|[P1WIN]";
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();


                            planal_wait_.Visible = false;
                            timer1_wait_time.Stop();

                            MessageBox.Show("You Win :) \n" + Client_name + " lost!!");
                            Control_Throw_BTN(false);

                            break;
                        }
                    }
                    else if (array[x, y] == 2)
                    {
                        sumP2 += 1;
                        sumP1 = 0;
                        if (sumP2 == 5)
                        {
                            raw_msg_send = "[SYS]|[SHOW]|[P2WIN]";
                            Thread TT = new Thread(Server_SendSmg);
                            TT.Start();


                            planal_wait_.Visible = false;
                            timer1_wait_time.Stop();

                            MessageBox.Show("You lost :P \n" + Client_name + " Win!!");
                            Control_Throw_BTN(false);

                            break;
                        }
                    }
                    else
                    {
                        sumP1 = 0; sumP2 = 0;
                    }
                }
            }

            // เช็คแนวทะแยง ซ้ายไปขวา
            sumP1 = 0; sumP2 = 0;
            for (int xx = 7; xx >= 5; xx--) // เลื่อนแกน X ขึ้
            {
                if (Chk_win == true) { break; }
                for (int yy = 1; yy <= 3; yy++) // เลือนแกน Y ออก 
                {
                    int ii = 0;
                    sumP1 = 0; sumP2 = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (array[(xx - ii), (yy + i)] == 1)
                        {
                            sumP1 += 1;
                            sumP2 = 0;
                            if (sumP1 == 5)
                            {
                                Chk_win = true;
                                raw_msg_send = "[SYS]|[SHOW]|[P1WIN]";
                                Thread TT = new Thread(Server_SendSmg);
                                TT.Start();


                                planal_wait_.Visible = false;
                                timer1_wait_time.Stop();


                                MessageBox.Show("You Win :) \n" + Client_name + " lost!!");
                                Control_Throw_BTN(false);

                                break;
                            }
                        }
                        else if (array[(xx - ii), (yy + i)] == 2)
                        {
                            sumP2 += 1;
                            sumP1 = 0;
                            if (sumP2 == 5)
                            {
                                raw_msg_send = "[SYS]|[SHOW]|[P2WIN]";
                                Thread TT = new Thread(Server_SendSmg);
                                TT.Start();


                                planal_wait_.Visible = false;
                                timer1_wait_time.Stop();

                                MessageBox.Show("You lost :P \n" + Client_name + " Win!!");
                                Control_Throw_BTN(false);

                                break;
                            }
                        }
                        else
                        {
                            sumP1 = 0; sumP2 = 0;
                        }
                        ii += 1;
                    }
                }
            }

            // เช็ค แนวทะแยง ขวา ไป ซ้าย
            sumP1 = 0; sumP2 = 0;
            for (int xx = 7; xx >= 5; xx--) // เลื่อนแกน X ขึ้
            {
                if (Chk_win == true) { break; }
                for (int yy = 7; yy >= 5; yy--) // เลือนแกน Y ออก 
                {
                    int ii = 0;
                    sumP1 = 0; sumP2 = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (array[(xx - ii), (yy - i)] == 1)
                        {
                            sumP1 += 1;
                            sumP2 = 0;
                            if (sumP1 == 5)
                            {
                                Chk_win = true;
                                raw_msg_send = "[SYS]|[SHOW]|[P1WIN]";
                                Thread TT = new Thread(Server_SendSmg);
                                TT.Start();

                                planal_wait_.Visible = false;
                                timer1_wait_time.Stop();

                                MessageBox.Show("You Win :) \n" + Client_name + " lost!!");
                                Control_Throw_BTN(false);

                                break;
                            }
                        }
                        else if (array[(xx - ii), (yy - i)] == 2)
                        {
                            sumP2 += 1;
                            sumP1 = 0;
                            if (sumP2 == 5)
                            {
                                raw_msg_send = "[SYS]|[SHOW]|[P2WIN]";
                                Thread TT = new Thread(Server_SendSmg);
                                TT.Start();


                                planal_wait_.Visible = false;
                                timer1_wait_time.Stop();

                                MessageBox.Show("You lost :P \n" + Client_name + " Win!!");
                                Control_Throw_BTN(false);

                                break;
                            }
                        }
                        else
                        {
                            sumP1 = 0; sumP2 = 0;
                        }
                        ii += 1;
                    }
                }
            }

            // เช้คว่าตารางเต็มไหม เต็มเท่ากันเสมอ
            bool chk_draw = true;
            sumP1 = 0; sumP2 = 0;
            for (int xx = 7; xx >= 1; xx--)
            {
                for (int yy = 7; yy >= 1; yy--)
                {
                    if (array[xx, yy] == 0)
                    {
                        chk_draw = false;
                        break;
                    }
                    else
                    { chk_draw = true; }
                }
            }

            if (chk_draw == true)
            {
                raw_msg_send = "[SYS]|[SHOW]|[DRAW]";
                Thread TT = new Thread(Server_SendSmg);
                TT.Start();


                planal_wait_.Visible = false;
                timer1_wait_time.Stop();

                MessageBox.Show("Draw!!");
                Control_Throw_BTN(false);
            }
        }

        void Game_random_turn() //gg_random_turn
        {
            Thread.Sleep(1000);
            Random RR = new Random();

            _turn = (byte)RR.Next(1, 3);
            updateConsole("[P][G][RANDOM=" + _turn + "]");

            //_turn = 2; /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            if (_turn == 1)
            {
                updateConsole("[P][G]Server P1 Start"); // P1 ได้เล่นก่อน แล้วส่งค่าให้ P2 P2 จะไปทำการล็อคจอ

                raw_msg_send = "[SYS]|[TURN]|[1]"; // ส่งค่า Random กลับไปให้ Cliente 
                Thread TT = new Thread(Server_SendSmg);
                TT.Start();
                TX_msg_chat.Text = "";

                Control_Throw_BTN(true);    // ทำการเปิด BTN ของตัวเอง  // อีกฝั่งจะปิด
                Show_Turn();
            }
            else if (_turn == 2)
            {
                updateConsole("[P][G]Server P2 Start");  // P1 ได้เล่นก่อน แล้วส่งค่าให้ P2 P2 จะไปทำการล็อคจอ

                raw_msg_send = "[SYS]|[TURN]|[2]"; // ส่งค่า Random กลับไปให้ Cliente 
                Thread TT = new Thread(Server_SendSmg);
                TT.Start();
                TX_msg_chat.Text = "";

                Control_Throw_BTN(false);    // ทำการปิด BTN ของตัวเอง // อีกฝั่งจะเปิด
                Show_Turn();
            }

        }
        void Show_Turn() //gg_show_turn
        {
            updateConsole("[P][G][SHOW_TURN][" + _turn + "]");

            if (_turn == 1)
            {
                textBox_ShowTurn.Text = "" + Server_name;

                if (Chk_server == true)
                {
                    textBox_ShowTurn.Text = "You";
                }
            }
            else if (_turn == 2)
            {
                textBox_ShowTurn.Text = "" + Client_name;

                if (Chk_server == false)
                {
                    textBox_ShowTurn.Text = "You";
                }
            }

            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.BalloonTipText = "Turn " +textBox_ShowTurn.Text ;
            notifyIcon1.ShowBalloonTip(300);

            Thread TT = new Thread(windows_wait);
            TT.Start();
        }

      

        //gg_detection_network
        public void Start_connect_detection() 
        {
            timer2_connect_detection.Start();
        }
        private void timer2_connect_detection_Tick(object sender, EventArgs e)
        {
            updateConsole("[P]Connected_Detect");
            if (Chk_server == true)
            {
                if (socket.Connected != true)
                {
                    MediaPlayer2_Quick_sound.URL = "error.wav";
                    planal_wait_.Visible = false;
                    panel2_stopped.Visible = true;
                    timer1_wait_time.Stop();
                    timer2_connect_detection.Stop();
                }
            }
            else
            {
                if (client.Connected != true)
                {
                    MediaPlayer2_Quick_sound.URL = "error.wav";
                    planal_wait_.Visible = false;
                    panel2_stopped.Visible = true;
                    timer1_wait_time.Stop();
                    timer2_connect_detection.Stop();
                }
            }
        }

        //gg_timer_wait_time
        int sec_wait_time = 100;
        private void timer1_wait_time_Tick(object sender, EventArgs e)
        {
            sec_wait_time--;
            label3_wait_time_show.Text = "Wait time " + sec_wait_time + "s";


            if (sec_wait_time < 1)
            {
                updateConsole("[P][G][WAIT]Time out ");

                if (Chk_server == true && socket.Connected == true)
                {
                    socket.Send(enc.GetBytes("Recevice End"));
                }
                else if (Chk_server == false && client.Connected == true)
                {
                    raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|" + "END";
                    Thread TT = new Thread(Client_SendSmg);
                    TT.Start();
                }

                if (Chk_server == true)
                {

                    listener.Stop();
                    socket.Close();

                    updateConsole("[N]Socket.Close");
                    updateConsole("[N]listener.Stop");
                    updateConsole("[P]End Program");

                    //timer1_wait_time.Enabled = false;
                    timer1_wait_time.Stop();
                    label_notification_detail.Text = ("time out...\nNo reply from the other players.");


                    planal_wait_.Visible = false;
                    panel2_stopped.Visible = true;

                }
                else
                {
                    client.Close();

                    updateConsole("[N]Client.Close");
                    updateConsole("[P]End Program");


                    //timer1_wait_time.Enabled = false;
                    timer1_wait_time.Stop();
                    label_notification_detail.Text = ("time out...\nNo reply from the other players.");

                    planal_wait_.Visible = false;
                    panel2_stopped.Visible = true;

                }
            }
        }
       
        //gg_exit_btn
        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        //gg_play_sound
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (MediaPlayer1_BG_Soubd.settings.mute == true)
            {
                button_sound_control.BackgroundImage = WindowsFormsApplication8.Properties.Resources.music_on;
                MediaPlayer2_Quick_sound.settings.mute = false;
                MediaPlayer1_BG_Soubd.settings.mute = false;

            }
            else
            {
                button_sound_control.BackgroundImage = WindowsFormsApplication8.Properties.Resources.music_off;
                MediaPlayer1_BG_Soubd.settings.mute = true;
                MediaPlayer2_Quick_sound.settings.mute = true;
            }
        }


        //gg_windows_wait 
        int set_wait = 60;
        void windows_wait() 
        {
            if (_turn == 1)
            {
                if (Chk_server == true)
                {
                    updateConsole("[P][G]Wait Windows = True");
                    if (planal_wait_.InvokeRequired)
                    {
                        BeginInvoke(new InvokeDelegate(InvokeMethod1));
                    }
                }
                else
                {
                    updateConsole("[P][G]Wait Windows = False");
                    label_wait_name.Text = "Turn " + Server_name;
                    if (planal_wait_.InvokeRequired)
                    {
                        BeginInvoke(new InvokeDelegate(InvokeMethod2));
                    }
                }
            }
            else if (_turn == 2)
            {
                if (Chk_server == false)
                {
                    updateConsole("[P][G]Wait Windows = True");
                    if (planal_wait_.InvokeRequired)
                    {
                        BeginInvoke(new InvokeDelegate(InvokeMethod1));
                    }
                }
                else
                {
                    updateConsole("[P][G]Wait Windows = False");
                    label_wait_name.Text = "Turn " + Client_name;
                    if (planal_wait_.InvokeRequired)
                    {
                        BeginInvoke(new InvokeDelegate(InvokeMethod2));
                    }
                }
            }
        }
        public void InvokeMethod1()
        {
            sec_wait_time = set_wait;
            //timer1_wait_time.Enabled = false;
            timer1_wait_time.Stop();
            planal_wait_.Visible = false;
        }
        public void InvokeMethod2()
        {
            sec_wait_time = set_wait;
            //timer1_wait_time.Enabled = true;
            timer1_wait_time.Start();
            planal_wait_.Visible = true;
        }

        //gg_connection_start
        private void P5_b1_Server_Click(object sender, EventArgs e)
        {
            try
            {
                if (TB_Server_Name.Text != "" && TB_Server_IP.Text != "")
                {
                    Thread TT = new Thread(Server_Receiver);
                    TT.Start();

                    BTN_Start_Server.Enabled = false;
                    BTN_Join.Enabled = false;
                    panel5_Tyr_server.Visible = true;
                    panel5_in_mode.Visible = false;
                    button_back_Rename.Enabled = false;
                }
            }
            catch (Exception)
            {
                updateConsole("[N]Cannot Strat Server !! ");
            }
        }
        private void Cancel_server_Click(object sender, EventArgs e)
        {
            listener.Stop();
            updateConsole("[N]listener.Stop");
            panel5_Tyr_server.Visible = false;
            panel5_in_mode.Visible = true;
            BTN_Start_Server.Enabled = true;
            BTN_Join.Enabled = true;
            button_back_Rename.Enabled = true;
        }
        private void BTN_Start_Client_Click(object sender, EventArgs e)
        {
            panel6_client_join.Visible = true;
            panel5_in_mode.Visible = false;
            BTN_Start_Server.Enabled = true;
            BTN_Join.Enabled = true;
            button_back_Rename.Enabled = false;
        }
        private void BTN_Join_Click(object sender, EventArgs e)
        {
            try
            {

                client = new TcpClient();

                if (TB_IP_join.Text != "")
                {
                    IPAddress address;
                    if (IPAddress.TryParse(TB_IP_join.Text, out address))
                    {
                        updateConsole("[P]IP Prass"); status_join.Text = "IP Prass";
                        BTN_Join.Enabled = false;
                        TB_IP_join.Enabled = false;
                        Thread TT = new Thread(Client_Connection);
                        TT.Start();
                    }
                    else
                    {
                        status_join.Text = "IP Error"; updateConsole("[P]IP error ");
                    }
                }
            }
            catch (Exception)
            {
                updateConsole("[P]IP error "); status_join.Text = "IP Error";
                TB_IP_join.Text = "";
                TB_IP_join.Focus();
                BTN_Join.Enabled = true;
                TB_IP_join.Enabled = true;
            }
        }
        private void Close_join_Click(object sender, EventArgs e)
        {

            TB_IP_join.Enabled = true;
            panel6_client_join.Visible = false;
            panel5_in_mode.Visible = true;
            button_back_Rename.Enabled = true;

            client.Close();
        }
        private void button_back_Rename_Click(object sender, EventArgs e)
        {
            if (panel5_in_mode.Visible == true)
            {
                panel2_rename.Visible = true;
                panel5_main.Visible = false;
            }
        }


        //gg_ui_zone
        void load_UI() //gg_load_ui
        {
            //this.Size = new System.Drawing.Size(1175, 685);
            this.Size = new System.Drawing.Size(900, 685);

            //gg_chk_Beta_Version
            int dayy = int.Parse(System.DateTime.Now.ToString("dd"));
            int moun = int.Parse(System.DateTime.Now.ToString("MM"));

            if (dayy >  1 && moun > 5)
            {
                MessageBox.Show(Version_name + " Expired");
                Application.Exit();
            }


            label_version.Text = Version_name;
            label_Version_Console.Text = Version_name;

            // Load Resources 
            BTN_Start_Server.BackgroundImage = WindowsFormsApplication8.Properties.Resources.Server_icon_1;
            BTN_Start_Client.BackgroundImage = WindowsFormsApplication8.Properties.Resources.Client_icon_1;
            panel2_rename.BackgroundImage = WindowsFormsApplication8.Properties.Resources.BG_title;
            panel5_in_mode.BackgroundImage = WindowsFormsApplication8.Properties.Resources.BG3;
            panel5_main.BackgroundImage = WindowsFormsApplication8.Properties.Resources.BG2;

            // Load
            Form.CheckForIllegalCrossThreadCalls = false;


            panel2_rename.Location = new Point(0, 0);
            panel5_main.Location = new Point(0, 0);
            panel5_in_mode.Location = new Point(162, 209);
            panel5_Tyr_server.Location = new Point(162, 209);
            panel6_client_join.Location = new Point(162, 209);

            planal_wait_.Location = new Point(106, 194);
            panel2_stopped.Location = new Point(0, 0);
        }

        bool Chk_console_show = true;
        private void button1_Click(object sender, EventArgs e) //gg_console_box
        {
            if (Chk_console_show == true)
            {
                Chk_console_show = false;
                this.Size = new System.Drawing.Size(1175, 685);
            }
            else
            {
                this.Size = new System.Drawing.Size(900, 685);
                Chk_console_show = true;
            }
        }
        private void button3_Click(object sender, EventArgs e) //gg_help_box
        {
            if (panel4.Visible == false)
            {
                panel4.Visible = true;
                Thread FF = new Thread(box_help1);
                FF.Start();
            }
        }
        void box_help1()
        {
            Thread.Sleep(2000);
            panel4.Visible = false;
        }

        void Control_Throw_BTN(bool on_off)
        {
            if (on_off == false)
            {
                Throw_Y1.Enabled = false;
                Throw_Y2.Enabled = false;
                Throw_Y3.Enabled = false;
                Throw_Y4.Enabled = false;
                Throw_Y5.Enabled = false;
                Throw_Y6.Enabled = false;
                Throw_Y7.Enabled = false;
            }
            else
            {
                Throw_Y1.Enabled = true;
                Throw_Y2.Enabled = true;
                Throw_Y3.Enabled = true;
                Throw_Y4.Enabled = true;
                Throw_Y5.Enabled = true;
                Throw_Y6.Enabled = true;
                Throw_Y7.Enabled = true;
            }
        }

        private void Throw_Y1_Click(object sender, EventArgs e)
        {
            try_throw(1);
        }
        private void Throw_Y2_Click(object sender, EventArgs e)
        {
            try_throw(2);
        }
        private void Throw_Y3_Click(object sender, EventArgs e)
        {
            try_throw(3);
        }
        private void Throw_Y4_Click(object sender, EventArgs e)
        {
            try_throw(4);
        }
        private void Throw_Y5_Click(object sender, EventArgs e)
        {
            try_throw(5);
        }
        private void Throw_Y6_Click(object sender, EventArgs e)
        {
            try_throw(6);
        }
        private void Throw_Y7_Click(object sender, EventArgs e)
        {
            try_throw(7);
        }




        //gg_Emotion

        void send_emo(int i)
        {
            if (Chk_server == false) // Client SEND
            {
                updateConsole("[P][G][EMO]");
                richTextBox_Chat.AppendText("You say : " + TX_msg_chat.Text);
                raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|[EMO]|"+i ;
                Thread TT = new Thread(Client_SendSmg);
                TT.Start();

                Image load_img = Image.FromFile(@"emo\(" + i + ").jpg");

                Clipboard.SetImage(load_img);
                richTextBox_Chat.Paste();
                richTextBox_Chat.AppendText("\n");
                richTextBox_Chat.ScrollToCaret();
            }
            else if (Chk_server == true)  // SERVER SEND
            {
                updateConsole("[P][G][EMO]");
                richTextBox_Chat.AppendText("You say : " + TX_msg_chat.Text);
                raw_msg_send = "[C]|[" + TB_Server_Name.Text + "]|[EMO]|" + i;
                Thread TT = new Thread(Server_SendSmg);
                TT.Start();


                Image load_img = Image.FromFile(@"emo\(" + i + ").jpg");

                Clipboard.SetImage(load_img);
                richTextBox_Chat .Paste();
                richTextBox_Chat.AppendText("\n");
                richTextBox_Chat.ScrollToCaret();
            }
        }
        private void btn_emo1_Click(object sender, EventArgs e)
        {
            send_emo(1);
        }

        private void btn_emo2_Click(object sender, EventArgs e)
        {
            send_emo(2);
        }

        private void btn_emo3_Click(object sender, EventArgs e)
        {
            send_emo(3);
        }

        private void btn_emo4_Click(object sender, EventArgs e)
        {
            send_emo(4);
        }

        private void btn_emo5_Click(object sender, EventArgs e)
        {
            send_emo(5);
        }

        private void btn_emo6_Click(object sender, EventArgs e)
        {
            send_emo(6);
        }


    }
}