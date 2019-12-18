using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;                           //System.net modülünü kullanacağımız için bu modülü ekledik.
using System.Net.Sockets;                   //Aynı zamanda soketleri kullanacağımız için bu modülü de ekledik.

namespace Chat_App
{
    public partial class Form1 : Form
    {
        Socket sck;                     //Socket ve endpoint tanımlamaları yapıldı.
        EndPoint epLocal, epRemote;     //epLocal ve epRemote tanımlamaları yapıldı. (Endpointler)
        
        public Form1()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            /* socket oluşturuldu adress family internetwork olarak ayarlandı soket tipi datagram ve protokol tipi
             udp olarak ayarlandı. Soket ayarlamaları yapıldı. 
            */

            textLocalIP.Text = GetLocalIP();            //Oluşturulan GetLocalIP fonksiyonu LocalIP ve FriendsIP
            textFriendsIP.Text = GetLocalIP();          //Bölümlerine yazdırıldı.
        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();                   //Alınan ip adresi her iki durum için de yazılmak için foreach fonksiyonu ile açıldı.
                }                                           //Alınan ip adresinin adres ailesi internetwork'e eşitse alınan ip adresini string veri tipine dönüştürdük.
            }                                               //String veri tipine dönüştürmemizin sebebi textbox bölümüne yazdırabilmek için
            return "127.0.0.1";                             //Eğer if koşuluna girmezse return değerini ayarladık. 
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);       //Mesaj gönderebilmek için MessageCallBack fonksiyonunu oluşturduk.
                if(size > 0)                                                //Gönderilen mesajın boyu 0'dan büyükse yani boş mesaj değilse koşulunu oluşturduk.
                {                                   
                    byte[] receivedData = new byte[1400];                   //iletilecek olan mesajı byte türüne dönüştürdük.
                    receivedData = (byte[])aResult.AsyncState;              //Arka arkaya 1 den fazla mesaj gönderilmesi koşulunu ayarladık.
                    ASCIIEncoding eEncoding = new ASCIIEncoding();          //Girilen mesajı ASCII ile kodladık.

                    string receivedMessage = eEncoding.GetString(receivedData);     //Gönderilen mesajı tekrar string tipine dönüştürdük bu sayede listboxa ekleme yapabileceğiz.
                    listMessage.Items.Add(FriendUserName.ToString() + " => " + receivedMessage);             //Listbox'a gönderilen mesajlar eklendi ve her iki kullanıcının görmesi sağlandı. 

                }
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                //Yeni mesaj göndeimi için buffer boşaltıldı. soket parametreleri girildi.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());        //Hata yakalaması yapıldı. Alınan hata messagebox üzerinde gösterildi.

            }
        }
        
        private void GroupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIP.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                //Local endpoint oluşturuldu. Oluşturulan endpoint ile soket eşleştirildi.

                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIP.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);

                //Remote endpoint oluşturuldu. Oluşturulan endpoint ile soket bağlanması gerçekleştirildi.

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                
                //Gönderilecek mesaj için 1500 byte'lık yer açıldı. Soket üzerinden mesaj alımı sağlandı. 

                button1.Text = "Connected";         //Start butonu basıldığında içine connected yazması sağlandı. 
                button1.Enabled = false;            //Bu yazımdan sonra buton inaktif edildi. Tekrar kullanımı engellendi. 
                
                btnSend.Enabled = true;             //Send button kullanılmak üzere aktif tutuldu.
                textMessage.Focus();                //Klavyeden girilen mesajı bir yere aktarmak için kullanıldı. 


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());     //hata oluşması durumunda hatanın gösterilmesini sağladım.

            }   
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ColorDialog Renk = new ColorDialog();                   //Renk butonu oluşturuldu.Renk paleti aktif edildi.
            Renk.ShowDialog();                                      //Renk paletinden seçilen rengin form1 üzerinden aktif olması ve görünmesi sağlandı.
            this.BackColor = Renk.Color;
        }

        private void textMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick();
            }
        }

        private void HostUserName_TextChanged(object sender, EventArgs e)
        {
            string YourName = HostUserName.ToString();
        }

        private void FriendUserName_TextChanged(object sender, EventArgs e)
        {
            string FriendName = FriendUserName.ToString();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();    //Gönderilecek olan mesaj ASCII code tipine dönüştürüldü.
                byte[] msg = new byte[1500];                                        //String veri tipinde yazılan mesaj byte şeklinde ayarlandı.
                msg = enc.GetBytes(textMessage.Text);                               //textbox'dan yazılan mesajın msg değişkenine aktarımı sağlandı.

                sck.Send(msg);                              //Socket üzerinden mesajın gönderilmesi sağlandı. 
                listMessage.Items.Add(HostUserName.ToString() + " => " + textMessage.Text);      //Gönderilen mesaj listmessage kısmına aktarıldı ve host tarafından gönderildiği için mesajın başına you yazdırdık.
                textMessage.Clear();                            //Mesaj gönderildikten sonra textbox temizlendi. 
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());                 //Gönderilen mesajın messagebox üzerinde görünmesi sssağlandı. 
            }
        }
    }
}
