using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{

    public partial class frmClientes : Form
    {
        public frmClientes()
        {
            InitializeComponent();
            LoadEstados(); // Cargar estados en el ComboBox al iniciar
            InitializeComboBoxBusqueda();
            llenar_tabla(); // Cargar los datos al iniciar
        }

        private void frmClientes_Load(object sender, EventArgs e)
        {
            // Llenar la tabla de clientes al cargar el formulario
            llenar_tabla();
        }

        private void LoadEstados()
        {
            // Llenar el ComboBox de estados (Activo/Inactivo)
            DataTable dtEstados = new DataTable();
            dtEstados.Columns.Add("Text", typeof(string));
            dtEstados.Columns.Add("Value", typeof(bool));

            dtEstados.Rows.Add("Activo", true);
            dtEstados.Rows.Add("Inactivo", false);

            cboestado.DisplayMember = "Text";
            cboestado.ValueMember = "Value";
            cboestado.DataSource = dtEstados;
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            // Verificar si todos los campos están llenos
            if (string.IsNullOrWhiteSpace(txtdocumento.Text) ||
                string.IsNullOrWhiteSpace(txtnombrecompleto.Text) ||
                string.IsNullOrWhiteSpace(txtcorreo.Text) ||
                string.IsNullOrWhiteSpace(txttelefono.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            // Verificar que el documento solo contenga números
            if (!txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe contener solo números.");
                return;
            }
            // Verificar longitud de txtdocumento
            if (txtdocumento.Text.Length != 8 || !txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El Documento debe contener 8 digitos.");
                return;
            }

                // Verificar que el teléfono solo contenga números
                if (!txttelefono.Text.All(char.IsDigit))
            {
                MessageBox.Show("El teléfono debe contener solo números.");
                return;
            }

            // Verificar que el nombre completo solo contenga letras y espacios
            if (!txtnombrecompleto.Text.All(c => Char.IsLetter(c) || Char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre completo debe contener solo letras y espacios.");
                return;
            }

            // Verificar si el cliente ya existe
            if (ClientExists(txtdocumento.Text))
            {
                MessageBox.Show("El cliente con el documento especificado ya existe.");
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = @"
    INSERT INTO CLIENTE (Documento, NombreCompleto, Correo, Telefono, Estado) 
    VALUES (@documento, @nombrecompleto, @correo, @telefono, @estado)";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@documento", txtdocumento.Text);
                        comando.Parameters.AddWithValue("@nombrecompleto", txtnombrecompleto.Text);
                        comando.Parameters.AddWithValue("@correo", txtcorreo.Text);
                        comando.Parameters.AddWithValue("@telefono", txttelefono.Text);
                        comando.Parameters.AddWithValue("@estado", (bool)cboestado.SelectedValue); // Estado desde el ComboBox

                        comando.ExecuteNonQuery();
                    }

                    llenar_tabla(); // Actualizar tabla después de agregar cliente
                    MessageBox.Show("Registro agregado");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private bool ClientExists(string documento)
        {
            bool exists = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "SELECT COUNT(*) FROM CLIENTE WHERE Documento = @documento";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@documento", documento);
                        int count = (int)comando.ExecuteScalar();
                        exists = (count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al verificar el cliente: " + ex.Message);
            }

            return exists;
        }

        private void llenar_tabla(string filtro = "")
        {
            using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
            {
                string consulta = @"
SELECT  
    Documento, 
    NombreCompleto, 
    Correo, 
    Telefono, 
    CASE 
        WHEN Estado = 1 THEN 'Activo' 
        ELSE 'Inactivo' 
    END AS Estado, 
    FechaRegistro
FROM 
    CLIENTE" + filtro;

                SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                dgvdata.DataSource = dt;
            }
        }

        private void btnmodificar_Click(object sender, EventArgs e)
        {
            // Validar que el campo Documento no esté vacío
            if (string.IsNullOrWhiteSpace(txtdocumento.Text))
            {
                MessageBox.Show("Por favor, ingrese el documento del cliente.");
                return;
            }

            // Validar que todos los campos necesarios estén llenos
            if (string.IsNullOrWhiteSpace(txtnombrecompleto.Text) ||
                string.IsNullOrWhiteSpace(txtcorreo.Text) ||
                string.IsNullOrWhiteSpace(txttelefono.Text) ||
                cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            // Verificar que el documento solo contenga números
            if (!txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe contener solo números.");
                return;
            }

            // Verificar que el teléfono solo contenga números
            if (!txttelefono.Text.All(char.IsDigit))
            {
                MessageBox.Show("El teléfono debe contener solo números.");
                return;
            }

            // Verificar que el nombre completo solo contenga letras y espacios
            if (!txtnombrecompleto.Text.All(c => Char.IsLetter(c) || Char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre completo debe contener solo letras y espacios.");
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = @"
UPDATE CLIENTE 
SET 
    NombreCompleto = @nombrecompleto, 
    Correo = @correo, 
    Telefono = @telefono, 
    Estado = @estado
WHERE 
    Documento = @documento";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@documento", txtdocumento.Text);
                        comando.Parameters.AddWithValue("@nombrecompleto", txtnombrecompleto.Text);
                        comando.Parameters.AddWithValue("@correo", txtcorreo.Text);
                        comando.Parameters.AddWithValue("@telefono", txttelefono.Text);
                        comando.Parameters.AddWithValue("@estado", (bool)cboestado.SelectedValue); // Estado desde el ComboBox

                        comando.ExecuteNonQuery();
                    }

                    MessageBox.Show("Registro modificado");
                    llenar_tabla(); // Actualizar tabla después de modificar el cliente
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            txtdocumento.Clear();
            txtnombrecompleto.Clear();
            txtcorreo.Clear();
            txttelefono.Clear();
            cboestado.SelectedIndex = 0; // Resetear el ComboBox a la primera opción
        }

        private void InitializeComboBoxBusqueda()
        {
            cbobusqueda.Items.Add("Documento");
            cbobusqueda.Items.Add("Nombre Completo");
            cbobusqueda.SelectedIndex = 0; // Seleccionar el primer ítem por defecto
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string filtro = "";
            string busqueda = txtbusqueda.Text.Trim();

            // Verificar si el ComboBox tiene una selección válida
            if (cbobusqueda.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un campo de búsqueda.");
                return;
            }

            // Obtener el campo de búsqueda seleccionado
            string campoBusqueda = cbobusqueda.SelectedItem.ToString();

            // Construir el filtro basado en el campo de búsqueda y el texto ingresado
            if (!string.IsNullOrEmpty(busqueda))
            {
                switch (campoBusqueda)
                {
                    case "Documento":
                        filtro = $" WHERE Documento LIKE '%{busqueda}%'";
                        break;
                    case "Nombre Completo":
                        filtro = $" WHERE NombreCompleto LIKE '%{busqueda}%'";
                        break;
                    default:
                        MessageBox.Show("Campo de búsqueda no válido.");
                        return; // Salir del método para evitar la búsqueda con un campo no válido
                }
            }

            // Llamar al método para llenar la tabla con el filtro aplicado
            llenar_tabla(filtro);
        }



        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvdata.Rows[e.RowIndex];
                txtdocumento.Text = row.Cells["Documento"].Value.ToString();
                txtnombrecompleto.Text = row.Cells["NombreCompleto"].Value.ToString();
                txtcorreo.Text = row.Cells["Correo"].Value.ToString();
                txttelefono.Text = row.Cells["Telefono"].Value.ToString();
                cboestado.SelectedValue = row.Cells["Estado"].Value.ToString() == "Activo"; // Ajustar el valor del ComboBox
            }
        }

       
    }


}
