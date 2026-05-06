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

namespace CapaPresentacion.Modales
{

    public partial class mdProveedor : Form
    {
        string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True;TrustServerCertificate=True;";
        public Proveedor SelectedProveedor { get; private set; }

        public mdProveedor()
        {
            InitializeComponent();
            // Configura las columnas del DataGridView aquí para asegurarte de que tengan los nombres correctos
            dgvdata.Columns.Add("Documento", "Documento");
            dgvdata.Columns.Add("RazonSocial", "Razón Social");
        }

        private void mdProveedor_Load(object sender, EventArgs e)
        {
            // Llenar el ComboBox de búsqueda
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                cbobusqueda.Items.Add(new { Valor = columna.Name, Texto = columna.HeaderText });
            }

            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar datos desde la base de datos
            LoadProveedores();
        }

        private void LoadProveedores()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Documento, RazonSocial FROM PROVEEDOR WHERE Estado = 1"; // Asegúrate de filtrar por estado si es necesario
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvdata.Rows.Clear(); // Limpiar filas existentes antes de añadir nuevas
                foreach (DataRow row in dataTable.Rows)
                {
                    dgvdata.Rows.Add(row["Documento"], row["RazonSocial"]);
                }
            }
        }

        private void dgvdata_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int iRow = e.RowIndex;
            if (iRow >= 0)
            {
                string documento = dgvdata.Rows[iRow].Cells["Documento"].Value.ToString();
                string razonSocial = dgvdata.Rows[iRow].Cells["RazonSocial"].Value.ToString();

                SelectedProveedor = new Proveedor
                {
                    Documento = documento,
                    RazonSocial = razonSocial
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((dynamic)cbobusqueda.SelectedItem).Valor.ToString();
            string textoBusqueda = txtbusqueda.Text.Trim().ToUpper();

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue; // Evita procesar la fila nueva
                string valorCelda = row.Cells[columnaFiltro].Value.ToString().Trim().ToUpper();
                row.Visible = valorCelda.Contains(textoBusqueda);
            }
        }

        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue; // Evita procesar la fila nueva
                row.Visible = true;
            }
        }
    }


}
