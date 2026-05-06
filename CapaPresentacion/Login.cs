using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace CapaPresentacion
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btncancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btningresar_Click(object sender, EventArgs e)
        {
            // Obtener el texto de los campos de entrada
            string documento = txtdocumento.Text;
            string clave = txtclave.Text;

            // Verificar que el número de documento contenga exactamente 8 dígitos y solo números
            if (!IsValidDocumento(documento))
            {
                MessageBox.Show("El número de documento debe contener 8 dígitos numéricos.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Salir del método si la validación falla
            }

            // Cadena de conexión desde el archivo App.config
            string connectionString = ConfigurationManager.ConnectionStrings["cadena_conexion"].ConnectionString;

            // Crear la consulta SQL
            string query = "SELECT COUNT(*) FROM USUARIO WHERE Documento = @Documento AND Clave = @Clave";

            // Establecer la conexión
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Documento", documento);
                command.Parameters.AddWithValue("@Clave", clave);

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        // Si el usuario existe, abrir el formulario de inicio y pasar el documento del usuario
                        Inicio form = new Inicio(documento);
                        form.Show();
                        this.Hide();

                        form.FormClosing += frm_closing; // Mostrar de nuevo el formulario de inicio de sesión
                    }
                    else
                    {
                        // Mostrar mensaje de error si el usuario no existe
                        MessageBox.Show("No se encontró el usuario", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con la base de datos: " + ex.Message);
                }
            }
        }

        // Método auxiliar para validar el número de documento
        private bool IsValidDocumento(string documento)
        {
            // Verificar que el documento no sea nulo o vacío
            if (string.IsNullOrEmpty(documento))
            {
                return false;
            }

            // Verificar que el documento contenga exactamente 8 dígitos
            if (  documento.Length!= 8)
            {
                return false;

            }
           

            // Verificar que el documento contenga solo números
            return documento.All(char.IsDigit);
        }

        private void frm_closing(object sender, FormClosingEventArgs e)
        {
            txtdocumento.Text = "";
            txtclave.Text = "";
            this.Show();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Aquí puedes agregar lógica que debe ejecutarse al cargar el formulario
        }

        private void txtclave_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}