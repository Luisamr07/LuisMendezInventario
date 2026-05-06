using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using CapaPresentacion.Properties;

namespace CapaPresentacion.Modales
{
    public partial class mdAyuda : Form
    {
        public mdAyuda()
        {
            InitializeComponent();
        }
        private void btnpdf_Click(object sender, EventArgs e)
        {
            // Ruta del archivo de origen
            string sourceFilePath = @"C:\Users\ThecnoMacVzla\Downloads\Sistema de inventario\CapaPresentacion\Resources\MANUALDEUSUARIO.pdf";

            // Ruta de destino en la carpeta de Documentos del usuario
            string destinationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MANUALDEUSUARIO.pdf");

            try
            {
                // Verifica si el archivo de origen existe
                if (File.Exists(sourceFilePath))
                {
                    // Copia el archivo al destino
                    File.Copy(sourceFilePath, destinationFilePath, true);
                    MessageBox.Show("El archivo se ha copiado exitosamente a: " + destinationFilePath, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("El archivo no existe en la ruta especificada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al copiar el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




    }


}
