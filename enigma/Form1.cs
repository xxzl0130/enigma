using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using enigma.proxy;

namespace enigma
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Proxy.Instance.Port = 18888;
            Proxy.Instance.DataEvent += Instance_DataEvent;
            Proxy.Instance.EnableBlocking = false;
            Proxy.Instance.Start();
        }

        private void Instance_DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            Console.WriteLine(jsonObject);
        }
    }
}
