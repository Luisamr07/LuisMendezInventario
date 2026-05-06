using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{

    public partial class frmNegocio : Form
    {
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"; // Reemplaza esto con tu cadena de conexión a la base de datos
        private int idNegocio = 1; // Cambia esto al ID del negocio que estás editando

        public frmNegocio()
        {
            InitializeComponent();
        }

        // Convertir un arreglo de bytes en una imagen
        private Image ByteToImage(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        // Cargar los datos al iniciar el formulario
        private void frmNegocio_Load(object sender, EventArgs e)
        {
            byte[] byteimage = ObtenerLogo(out bool obtenido);

            if (obtenido && byteimage != null)
            {
                picLogo.Image = ByteToImage(byteimage);
            }

            Negocio datos = ObtenerDatos();

            if (datos != null)
            {
                txtnombre.Text = datos.Nombre;
                txtruc.Text = datos.RUC;
                txtdireccion.Text = datos.Direccion;
            }
        }

        // Manejar la carga de una nueva imagen para el logo
        private void btnsubir_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oOpenFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png"
            })
            {
                if (oOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    byte[] byteimage = File.ReadAllBytes(oOpenFileDialog.FileName);
                    string mensaje;
                    bool respuesta = ActualizarLogo(byteimage, out mensaje);

                    if (respuesta)
                    {
                        picLogo.Image = ByteToImage(byteimage);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        // Guardar los datos del formulario en la base de datos
        private void btnguardarcambios_Click(object sender, EventArgs e)
        {
            Negocio obj = new Negocio
            {
                Nombre = txtnombre.Text,
                RUC = txtruc.Text,
                Direccion = txtdireccion.Text
            };

            string mensaje;
            bool respuesta = GuardarDatos(obj, out mensaje);

            if (respuesta)
            {
                MessageBox.Show("Los cambios fueron guardados", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo guardar los cambios", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // Obtener el logo desde la base de datos
        private byte[] ObtenerLogo(out bool obtenido)
        {
            obtenido = false;
            byte[] logo = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Logo FROM NEGOCIO WHERE IdNegocio = @IdNegocio";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdNegocio", idNegocio);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        logo = (byte[])result;
                        obtenido = true;
                    }
                }
            }
            return logo;
        }

        // Obtener los datos del negocio desde la base de datos
        private Negocio ObtenerDatos()
        {
            Negocio negocio = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Nombre, RIF, Direccion FROM NEGOCIO WHERE IdNegocio = @IdNegocio";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdNegocio", idNegocio);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            negocio = new Negocio
                            {
                                Nombre = reader["Nombre"].ToString(),
                                RUC = reader["RIF"].ToString(),
                                Direccion = reader["Direccion"].ToString()
                            };
                        }
                    }
                }
            }
            return negocio;
        }

        // Actualizar el logo en la base de datos
        private bool ActualizarLogo(byte[] logo, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE NEGOCIO SET Logo = @Logo WHERE IdNegocio = @IdNegocio";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Logo", (object)logo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdNegocio", idNegocio);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        // Guardar los datos en la base de datos
        private bool GuardarDatos(Negocio negocio, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE NEGOCIO SET Nombre = @Nombre, RIF = @RIF, Direccion = @Direccion WHERE IdNegocio = @IdNegocio";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", negocio.Nombre);
                        cmd.Parameters.AddWithValue("@RIF", negocio.RUC);
                        cmd.Parameters.AddWithValue("@Direccion", negocio.Direccion);
                        cmd.Parameters.AddWithValue("@IdNegocio", idNegocio);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }

    // Clase de modelo para los datos del negocio
    public class Negocio
    {
        public string Nombre { get; set; }
        public string RUC { get; set; }
        public string Direccion { get; set; }
    }

}
