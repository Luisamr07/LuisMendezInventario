using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class PdfViewerForm : Form
    {
        private string pdfPath;

        public PdfViewerForm(string path)
        {
            InitializeComponent();
            pdfPath = path;
            LoadPdf();
        }

        private void LoadPdf()
        {
            try
            {
                // Asegúrate de que el control WebBrowser esté en el formulario
                webBrowser1.Navigate(pdfPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el PDF: " + ex.Message);
            }
        }


    }
}