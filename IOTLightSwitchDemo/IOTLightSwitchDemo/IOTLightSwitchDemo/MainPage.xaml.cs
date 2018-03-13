using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace IOTLightSwitchDemo
{
	public partial class MainPage : ContentPage
	{
        private readonly HttpClient httpClient;
        private readonly UdpClient udpClient;


        public MainPage()
		{
			InitializeComponent();

            httpClient = new HttpClient();

            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 1881));
            startListening();
        }

        private void startListening()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 1881);
                    var data = udpClient.Receive(ref remoteEP); // listen on port 11000
                    var dataString = Encoding.ASCII.GetString(data, 0, data.Length);
                    if(dataString == "Hello")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            LightIpLabel.Text = remoteEP.Address.ToString();
                            ToggleButton.IsEnabled = true;
                        });
                    }
                }
            });
        }

        protected async void Toggle_Clicked(object sender, EventArgs e)
        {
            
            var response = await httpClient.GetAsync($"http://{LightIpLabel.Text}:1880/toggle");
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadAsStringAsync();

                LightBox.Color = value == "0" ? Color.Gray : Color.Yellow;
            }

        }

        protected void Scan_Clicked(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string myIP = "";
            byte[] dgram = Encoding.ASCII.GetBytes("HI");
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();

                    var ipStart = myIP.Substring(0, myIP.LastIndexOf('.'));
                    
                    try
                    {
                        for (int i = 1; i < 255; i++)
                        {
                            udpClient.Send(dgram, dgram.Length, $"{ipStart}.{i}", 1881);
                            //Console.WriteLine($"{ipStart}.{i}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            


            
            //IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.2.125"), 1881);

        }

    }
}
