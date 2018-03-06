using Be.Mcq8.EidReader;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SampleEidUsage
{
    public partial class Form1 : Form, IDisposable
    {

        private Be.Mcq8.EidReader.ReaderManager det;
        private Dictionary<Reader, CardDisplay> readerDisplayMap;

        public Form1()
        {
            InitializeComponent();
            readerDisplayMap = new Dictionary<Reader, CardDisplay>();
            det = new Be.Mcq8.EidReader.ReaderManager();
            det.OnReaderConnected += det_OnReaderConnected;
            det.OnReaderDisconnected += det_OnReaderDisconnected;
            det.Init();
        }
        private void det_OnReaderConnected(object sender, ReaderEventArgs e)
        {
            CardDisplay cd = new CardDisplay(e.Reader);
            readerDisplayMap.Add(e.Reader, cd);
            cd.Show();
            label1.Text = readerDisplayMap.Count.ToString();
        }

        private void det_OnReaderDisconnected(object sender, ReaderEventArgs e)
        {
            CardDisplay cd = readerDisplayMap[e.Reader];
            cd.Close();
            readerDisplayMap.Remove(e.Reader);
            label1.Text = readerDisplayMap.Count.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            det.Dispose(disposing);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
