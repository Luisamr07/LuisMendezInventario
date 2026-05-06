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

using CapaPresentacion.Utilidades;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CapaPresentacion
{

    public partial class frmUsuarios : Form
    {
        private DataTable dtUsuarios;

        public frmUsuarios()
        {
            InitializeComponent();
            LoadRoles();
            LoadEstados();
            InitializeComboBoxBusqueda();
            llenar_tabla(); // Cargar los datos al iniciar
        }

        private void LoadRoles()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "SELECT IdRol, Descripcion FROM ROL";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(comando);
                        DataTable dtRoles = new DataTable();
                        adapter.Fill(dtRoles);

                        cborol.DisplayMember = "Descripcion";
                        cborol.ValueMember = "IdRol";
                        cborol.DataSource = dtRoles;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar roles: " + ex.Message);
            }
        }

        private void LoadEstados()
        {
            // Llenar el combo de estados (Activo/Inactivo)
            DataTable dtEstados = new DataTable();
            dtEstados.Columns.Add("Text", typeof(string));
            dtEstados.Columns.Add("Value", typeof(bool));

            dtEstados.Rows.Add("Activo", true);
            dtEstados.Rows.Add("Inactivo", false);

            cboestado.DisplayMember = "Text";
            cboestado.ValueMember = "Value";
            cboestado.DataSource = dtEstados;
        }

        private void cborol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cborol.SelectedValue != null)
            {
                int selectedIdRol = (int)cborol.SelectedValue;
                MessageBox.Show($"Rol seleccionado: {selectedIdRol}");
            }
        }

        private void cboestado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboestado.SelectedValue != null)
            {
                bool selectedEstado = (bool)cboestado.SelectedValue;
                MessageBox.Show($"Estado seleccionado: {selectedEstado}");
            }
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            // Verificar si todos los campos están llenos
            if (string.IsNullOrWhiteSpace(txtdocumento.Text) ||
                string.IsNullOrWhiteSpace(txtnombrecompleto.Text) ||
                string.IsNullOrWhiteSpace(txtcorreo.Text) ||
                string.IsNullOrWhiteSpace(txtclave.Text) ||
                string.IsNullOrWhiteSpace(txtconfirmarclave.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            // Verificar si la clave y la confirmación coinciden
            if (txtclave.Text != txtconfirmarclave.Text)
            {
                MessageBox.Show("Las claves no coinciden.");
                return;
            }

            // Verificar longitud de clave
            if (txtclave.Text.Length < 6)
            {
                MessageBox.Show("La clave debe tener al menos 6 caracteres.");
                return;
            }

            // Verificar longitud de confirmación de clave
            if (txtconfirmarclave.Text.Length < 6)
            {
                MessageBox.Show("La confirmación de la clave debe tener al menos 6 caracteres.");
                return;
            }

            // Verificar que el campo Documento solo contenga números
            if (!txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe contener solo números.");
                return;
            }
            // Verificar longitud de txtdocumento
            if (txtdocumento.Text.Length != 8 || !txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe tener 8 dígitos.");
                return;
            }

            // Verificar que el campo Nombre Completo solo contenga letras y espacios
            if (!txtnombrecompleto.Text.All(c => Char.IsLetter(c) || Char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre completo debe contener solo letras y espacios.");
                return;
            }

            // Verificar si el usuario ya existe
            if (UserExists(txtdocumento.Text))
            {
                MessageBox.Show("El usuario con el documento especificado ya existe.");
                return;
            }

            // Verificar si se seleccionaron rol y estado
            if (cborol.SelectedValue == null || cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione un rol y un estado.");
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "INSERT INTO USUARIO (Documento, NombreCompleto, Correo, Clave, IdRol, Estado) VALUES (@documento, @nombrecompleto, @correo, @clave, @idrol, @estado)";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@documento", txtdocumento.Text);
                        comando.Parameters.AddWithValue("@nombrecompleto", txtnombrecompleto.Text);
                        comando.Parameters.AddWithValue("@correo", txtcorreo.Text);
                        comando.Parameters.AddWithValue("@clave", txtclave.Text);
                        comando.Parameters.AddWithValue("@idrol", cborol.SelectedValue);
                        comando.Parameters.AddWithValue("@estado", cboestado.SelectedValue);

                        comando.ExecuteNonQuery();
                    }
                    llenar_tabla(); // actualizar tabla después de agregar usuario
                    MessageBox.Show("Registro agregado");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private bool UserExists(string documento)
        {
            bool exists = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "SELECT COUNT(*) FROM USUARIO WHERE Documento = @documento";

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
                MessageBox.Show("Error al verificar el usuario: " + ex.Message);
            }

            return exists;
        }

        private void llenar_tabla(string filtro = "")
        {
            using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
            {
                string consulta = @"
        SELECT 
            u.Documento, 
            u.NombreCompleto, 
            u.Correo, 
            r.Descripcion AS Rol, 
            CASE 
                WHEN u.Estado = 1 THEN 'Activo' 
                ELSE 'Inactivo' 
            END AS Estado
        FROM 
            USUARIO u
        INNER JOIN 
            ROL r ON u.IdRol = r.IdRol" + filtro;

                SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                dgvdata.DataSource = dt;
            }
        }

        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            llenar_tabla(); // Cargar los datos al iniciar
        }

        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtdocumento.Text = dgvdata.SelectedCells[0].Value.ToString();
            txtnombrecompleto.Text = dgvdata.SelectedCells[1].Value.ToString();
            txtcorreo.Text = dgvdata.SelectedCells[2].Value.ToString();
        }

        private void btneliminar_Click(object sender, EventArgs e)
        {
            SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True");
            conexion.Open();
            string consulta = "delete from USUARIO where Documento='" + txtdocumento.Text + "' ";
            SqlCommand comando = new SqlCommand(consulta, conexion);
            comando.ExecuteNonQuery();
            MessageBox.Show("Usuario eliminado");
            conexion.Close();
            llenar_tabla();
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            txtdocumento.Clear();
            txtnombrecompleto.Clear();
            txtcorreo.Clear();
            txtclave.Clear();
            txtconfirmarclave.Clear();
        }

        private void btnmodificar_Click(object sender, EventArgs e)
        {
            // Validar que el campo Documento no esté vacío
            if (string.IsNullOrWhiteSpace(txtdocumento.Text))
            {
                MessageBox.Show("Por favor, ingrese el documento del usuario.");
                return;
            }

            // Validar que todos los campos necesarios estén llenos
            if (string.IsNullOrWhiteSpace(txtnombrecompleto.Text) ||
                string.IsNullOrWhiteSpace(txtcorreo.Text) ||
                string.IsNullOrWhiteSpace(txtclave.Text) ||
                cborol.SelectedValue == null ||
                cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            // Verificar longitud de la clave
            if (txtclave.Text.Length < 6)
            {
                MessageBox.Show("La clave debe tener al menos 6 caracteres.");
                return;
            }

            // Verificar que el campo Documento solo contenga números
            if (!txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe contener solo números.");
                return;
            }

            // Verificar longitud de txtdocumento
            if (txtdocumento.Text.Length != 8 || !txtdocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El documento debe tener 8 dígitos.");
                return;
            }

            // Verificar que el campo Nombre Completo solo contenga letras y espacios
            if (!txtnombrecompleto.Text.All(c => Char.IsLetter(c) || Char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre completo debe contener solo letras y espacios.");
                return;
            }

            // Verificar que el valor seleccionado del estado es un booleano
            bool estado = (bool)cboestado.SelectedValue;

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = @"
    UPDATE USUARIO
    SET 
        NombreCompleto = @nombrecompleto,
        Correo = @correo,
        Clave = @clave,
        IdRol = @idrol,
        Estado = @estado
    WHERE 
        Documento = @documento";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        // Añadir parámetros a la consulta
                        comando.Parameters.AddWithValue("@documento", txtdocumento.Text);
                        comando.Parameters.AddWithValue("@nombrecompleto", txtnombrecompleto.Text);
                        comando.Parameters.AddWithValue("@correo", txtcorreo.Text);
                        comando.Parameters.AddWithValue("@clave", txtclave.Text);
                        comando.Parameters.AddWithValue("@idrol", cborol.SelectedValue);
                        comando.Parameters.AddWithValue("@estado", estado);

                        // Ejecutar la consulta
                        int filasAfectadas = comando.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            llenar_tabla();
                            MessageBox.Show("Usuario modificado correctamente.");
                        }
                        else
                        {
                            MessageBox.Show("No se encontró el usuario para modificar.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar el usuario: " + ex.Message);
            }
        }


        private void InitializeComboBoxBusqueda()
        {
            cbobusqueda.Items.Add("Documento");
            cbobusqueda.Items.Add("NombreCompleto");
            cbobusqueda.Items.Add("Correo");
            cbobusqueda.SelectedIndex = 0; // Seleccionar el primer ítem por defecto
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            // Inicializar el filtro de búsqueda
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
                        filtro = $" WHERE u.Documento LIKE '%{busqueda}%'";
                        break;
                    case "NombreCompleto":
                        filtro = $" WHERE u.NombreCompleto LIKE '%{busqueda}%'";
                        break;
                    case "Correo":
                        filtro = $" WHERE u.Correo LIKE '%{busqueda}%'";
                        break;
                    default:
                        MessageBox.Show("Campo de búsqueda no válido.");
                        return; // Salir del método para evitar la búsqueda con un campo no válido
                }
            }

            // Llamar al método para llenar la tabla con el filtro aplicado
            llenar_tabla(filtro);
        }


        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Clear();
            cbobusqueda.SelectedIndex = -1; // Limpiar selección del ComboBox
            llenar_tabla(); // Mostrar todos los datos
        }
    }


}
