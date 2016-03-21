using Be.Mcq8.EidReader;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SampleEidUsage
{
    public partial class CardDisplay : Form
    {
        delegate void UpdateTitleDelegate(string text);
        delegate void UpdateLabelTextForIdDelegate(IdFile id);
        delegate void UpdateLabelTextForAddressDelegate(AddressFile address);
        delegate void UpdatePictureBoxDelegate(byte[] picture);

        private Reader reader;
        public CardDisplay(Reader reader)
        {
            InitializeComponent();
            lblReader.Text = reader.name;
            this.reader = reader;
            reader.OnStartedReading += Reader_OnStartedReading;
            reader.OnIdRead += reader_OnIdRead;
            reader.OnAddressRead += reader_OnAddressRead;
            reader.OnPhotoRead += reader_OnPhotoRead;
            reader.OnDataCleared += reader_OnDataCleared;
        }

        private void Reader_OnStartedReading(object sender, ReaderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateTitleDelegate(updateTilte), new object[] { "Reading ..." });
            }
            else
            {
                updateTilte("Reading ...");
            }
        }

        protected override void Dispose(bool disposing)
        {
            reader.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void reader_OnAddressRead(object sender, ReaderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateLabelTextForAddressDelegate(UpdateLabelTextForAddress), new object[] { e.Reader.addressFile });
            }
            else
            {
                UpdateLabelTextForAddress(e.Reader.addressFile);
            }
        }

        void reader_OnIdRead(object sender, ReaderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateLabelTextForIdDelegate(UpdateLabelTextForId), new object[] { e.Reader.idFile });
            }
            else
            {
                UpdateLabelTextForId(e.Reader.idFile);
            }
        }

        void reader_OnPhotoRead(object sender, ReaderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateTitleDelegate(updateTilte), new object[] { "Read" });
                Invoke(new UpdatePictureBoxDelegate(UpdatePictureBox), new object[] { e.Reader.photoFile });
            }
            else
            {
                updateTilte("Read");
                UpdatePictureBox(e.Reader.photoFile);
            }
        }

        void reader_OnDataCleared(object sender, ReaderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateLabelTextForIdDelegate(UpdateLabelTextForId), new object[] { null });
                Invoke(new UpdateLabelTextForAddressDelegate(UpdateLabelTextForAddress), new object[] { null });
                Invoke(new UpdatePictureBoxDelegate(UpdatePictureBox), new object[] { null });
                Invoke(new UpdateTitleDelegate(updateTilte), new object[] { "No Card" });
            }
            else
            {
                UpdateLabelTextForId(null);
                UpdateLabelTextForAddress(null);
                UpdatePictureBox(null);
                updateTilte("No Card");
            }

        }

        private void UpdateLabelTextForId(IdFile id)
        {
            if (id == null)
            {
                lblFirstNames.Text = "";
            }
            else
            {
                lblFirstNames.Text = id.FirstNames;
            }
        }

        private void UpdateLabelTextForAddress(AddressFile address)
        {
            if (address == null)
            {
                lblStreet.Text = "";
            }
            else
            {
                lblStreet.Text = address.StreetAndNumber;
            }
        }

        private void updateTilte(string text)
        {
            Text = text;
        }

        public void UpdatePictureBox(byte[] picture)
        {
            if (picture == null)
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                pictureBox1.Image = null;
            }
            else
            {
                MemoryStream ms = new MemoryStream(picture);
                pictureBox1.Image = Image.FromStream(ms);
            }
        }
    }
}
