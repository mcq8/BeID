using Be.Mcq8.EidReader;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SampleEidUsage
{
    public partial class CardDisplay : Form
    {
        delegate void UpdateLabelTextForIdDelegate(IdFile id);
        delegate void UpdateLabelTextForAddressDelegate(AddressFile address);
        delegate void UpdatePictureBoxDelegate(byte[] picture);

        private Reader reader;
        public CardDisplay(Reader reader)
        {
            InitializeComponent();
            lblReader.Text = reader.name;
            this.reader = reader;
            reader.OnIdRead += reader_OnIdRead;
            reader.OnAddressRead += reader_OnAddressRead;
            reader.OnPhotoRead += reader_OnPhotoRead;
            reader.OnDataCleared += reader_OnDataCleared;
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
            if (lblFirstNames.InvokeRequired)
            {
                lblFirstNames.Invoke(new UpdateLabelTextForAddressDelegate(UpdateLabelTextForAddress), new object[] { e.Reader.addressFile });
            }
            else
            {
                UpdateLabelTextForAddress(e.Reader.addressFile);
            }
        }

        void reader_OnIdRead(object sender, ReaderEventArgs e)
        {
            if (lblFirstNames.InvokeRequired)
            {
                lblFirstNames.Invoke(new UpdateLabelTextForIdDelegate(UpdateLabelTextForId), new object[] { e.Reader.idFile });
            }
            else
            {
                UpdateLabelTextForId(e.Reader.idFile);
            }
        }

        void reader_OnPhotoRead(object sender, ReaderEventArgs e)
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new UpdatePictureBoxDelegate(UpdatePictureBox), new object[] { e.Reader.photoFile });
            }
            else
            {
                UpdatePictureBox(e.Reader.photoFile);
            }
        }

        void reader_OnDataCleared(object sender, ReaderEventArgs e)
        {
            if (lblFirstNames.InvokeRequired)
            {
                lblFirstNames.Invoke(new UpdateLabelTextForIdDelegate(UpdateLabelTextForId), new object[] { null });
                lblFirstNames.Invoke(new UpdateLabelTextForAddressDelegate(UpdateLabelTextForAddress), new object[] { null });
                lblFirstNames.Invoke(new UpdatePictureBoxDelegate(UpdatePictureBox), new object[] { null });
            }
            else
            {
                UpdateLabelTextForId(null);
                UpdateLabelTextForAddress(null);
                UpdatePictureBox(null);
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
