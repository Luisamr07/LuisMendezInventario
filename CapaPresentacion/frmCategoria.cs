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

    public partial class frmCategoria : Form
    {
        private DataTable dtCategorias;

        public frmCategoria()
        {
            InitializeComponent();
            LoadEstados();
            InitializeComboBoxBusqueda();
            LoadData(); // Cargar los datos al iniciar
        }

        // Evento Load del formulario
        private void frmCategoria_Load(object sender, EventArgs e)
        {
            // Puedes agregar inicialización adicional aquí si es necesario
        }

        // Cargar los estados (Activo/Inactivo) en el ComboBox
        private void LoadEstados()
        {
            DataTable dtEstados = new DataTable();
            dtEstados.Columns.Add("Text", typeof(string));
            dtEstados.Columns.Add("Value", typeof(bool));

            dtEstados.Rows.Add("Activo", true);
            dtEstados.Rows.Add("Inactivo", false);

            cboestado.DisplayMember = "Text";
            cboestado.ValueMember = "Value";
            cboestado.DataSource = dtEstados;
        }

        // Inicializar el ComboBox de búsqueda
        private void InitializeComboBoxBusqueda()
        {
            cbobusqueda.Items.Add("Id");
            cbobusqueda.Items.Add("Descripcion");
            cbobusqueda.Items.Add("Estado");
            cbobusqueda.SelectedIndex = 0; // Seleccionar el primer ítem por defecto
        }

        // Cargar datos en el DataGridView
        private void LoadData(string filtro = "")
        {
            using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
            {
                string consulta = @"
    SELECT 
        IdCategoria AS Id, 
        Descripcion, 
        CASE 
            WHEN Estado = 1 THEN 'Activo' 
            ELSE 'Inactivo' 
        END AS Estado
    FROM 
        CATEGORIA" + filtro;

                SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                dtCategorias = new DataTable();
                adaptador.Fill(dtCategorias);
                dgvdata.DataSource = dtCategorias;
            }
        }

        // Manejar la selección de una fila en el DataGridView
        private void dgvdata_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvdata.Rows[e.RowIndex];
                txtdescripcion.Text = row.Cells["Descripcion"].Value.ToString();

                // Convertir el valor del estado a índice para el ComboBox
                string estado = row.Cells["Estado"].Value.ToString();
                cboestado.SelectedIndex = estado == "Activo" ? 0 : 1;

                txtIdCategoria.Text = row.Cells["Id"].Value.ToString();
            }
        }



        // Botón Limpiar
        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            txtdescripcion.Clear();
            cboestado.SelectedIndex = -1;
            txtIdCategoria.Clear();
        }

        // Botón Buscar
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string filtro = "";
            string busqueda = txtbusqueda.Text.Trim();

            if (cbobusqueda.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un campo de búsqueda.");
                return;
            }

            string campoBusqueda = cbobusqueda.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(busqueda))
            {
                switch (campoBusqueda)
                {
                    case "Id":
                        filtro = $" WHERE IdCategoria LIKE '%{busqueda}%'";
                        break;
                    case "Descripcion":
                        filtro = $" WHERE Descripcion LIKE '%{busqueda}%'";
                        break;
                    case "Estado":
                        filtro = $" WHERE Estado LIKE '%{busqueda}%'";
                        break;
                    default:
                        MessageBox.Show("Campo de búsqueda no válido.");
                        return;
                }
            }

            LoadData(filtro);
        }

        // Botón Limpiar Buscador
        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Clear();
            cbobusqueda.SelectedIndex = -1;
            LoadData(); // Recargar los datos sin filtro
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            // Verificar si todos los campos están llenos
            if (string.IsNullOrWhiteSpace(txtdescripcion.Text) ||
                cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = @"
            INSERT INTO CATEGORIA (Descripcion, Estado) 
            VALUES (@descripcion, @estado)";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@descripcion", txtdescripcion.Text);
                        comando.Parameters.AddWithValue("@estado", (bool)cboestado.SelectedValue);

                        comando.ExecuteNonQuery();
                    }

                    MessageBox.Show("Categoría agregada");
                    LoadData(); // Recargar datos después de agregar la categoría
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void btnmodificar_Click(object sender, EventArgs e)
        {
            // Validar que el campo IdCategoria no esté vacío
            if (string.IsNullOrWhiteSpace(txtIdCategoria.Text))
            {
                MessageBox.Show("Por favor, seleccione una categoría para modificar.");
                return;
            }

            // Validar que todos los campos necesarios estén llenos
            if (string.IsNullOrWhiteSpace(txtdescripcion.Text) ||
                cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, complete todos los campos.");
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
            UPDATE CATEGORIA 
            SET 
                Descripcion = @descripcion, 
                Estado = @estado
            WHERE 
                IdCategoria = @idcategoria";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@idcategoria", txtIdCategoria.Text);
                        comando.Parameters.AddWithValue("@descripcion", txtdescripcion.Text);
                        comando.Parameters.AddWithValue("@estado", estado);

                        comando.ExecuteNonQuery();
                    }

                    MessageBox.Show("Categoría modificada");
                    LoadData(); // Recargar datos después de modificar la categoría
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

    }

}
