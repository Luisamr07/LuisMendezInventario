using CapaPresentacion.Utilidades;
using ClosedXML.Excel;
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

    public partial class frmProductos : Form
    {
        private DataTable dtProductos;

        public frmProductos()
        {
            InitializeComponent();
            LoadCategorias();
            LoadEstados();
            InitializeComboBoxBusqueda();
            llenar_tabla(); // Cargar los datos al iniciar
        }

        private void LoadCategorias()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "SELECT IdCategoria, Descripcion FROM CATEGORIA";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(comando);
                        DataTable dtCategorias = new DataTable();
                        adapter.Fill(dtCategorias);

                        cbocategoria.DisplayMember = "Descripcion";
                        cbocategoria.ValueMember = "IdCategoria";
                        cbocategoria.DataSource = dtCategorias;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
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

        private void InitializeComboBoxBusqueda()
        {
            cbobusqueda.Items.Add("Código");
            cbobusqueda.Items.Add("Nombre");
            cbobusqueda.SelectedIndex = 0; // Seleccionar el primer ítem por defecto
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            // Verificar si todos los campos están llenos
            if (string.IsNullOrWhiteSpace(txtcodigo.Text) ||
                string.IsNullOrWhiteSpace(txtnombre.Text) ||
                string.IsNullOrWhiteSpace(txtdescripcion.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.");
                return;
            }

            // Verificar que el código solo contenga números
            if (!txtcodigo.Text.All(char.IsDigit))
            {
                MessageBox.Show("El código debe contener solo números.");
                return;
            }

            // Verificar si el producto ya existe
            if (ProductExists(txtcodigo.Text))
            {
                MessageBox.Show("El producto con el código especificado ya existe.");
                return;
            }

            // Verificar si se seleccionaron categoría y estado
            if (cbocategoria.SelectedValue == null || cboestado.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione una categoría y un estado.");
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = @"
    INSERT INTO PRODUCTO (Codigo, Nombre, Descripcion, Stock, PrecioCompra, PrecioVenta, IdCategoria, Estado) 
    VALUES (@codigo, @nombre, @descripcion, @stock, @preciocompra, @precioventa, @idcategoria, @estado)";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@codigo", txtcodigo.Text);
                        comando.Parameters.AddWithValue("@nombre", txtnombre.Text);
                        comando.Parameters.AddWithValue("@descripcion", txtdescripcion.Text);
                        comando.Parameters.AddWithValue("@stock", 0); // O el valor correspondiente
                        comando.Parameters.AddWithValue("@preciocompra", 0); // O el valor correspondiente
                        comando.Parameters.AddWithValue("@precioventa", 0); // O el valor correspondiente
                        comando.Parameters.AddWithValue("@idcategoria", cbocategoria.SelectedValue);
                        comando.Parameters.AddWithValue("@estado", (bool)cboestado.SelectedValue);

                        comando.ExecuteNonQuery();
                    }

                    llenar_tabla(); // actualizar tabla después de agregar producto
                    MessageBox.Show("Registro agregado");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private bool ProductExists(string codigo)
        {
            bool exists = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
                {
                    conexion.Open();
                    string consulta = "SELECT COUNT(*) FROM PRODUCTO WHERE Codigo = @codigo";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@codigo", codigo);
                        int count = (int)comando.ExecuteScalar();
                        exists = (count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al verificar el producto: " + ex.Message);
            }

            return exists;
        }

        private void llenar_tabla(string filtro = "")
        {
            using (SqlConnection conexion = new SqlConnection("Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"))
            {
                string consulta = @"
SELECT 
    p.Codigo, 
    p.Nombre, 
    p.Descripcion, 
    c.Descripcion AS Categoria, 
    p.Stock, 
    p.PrecioCompra, 
    p.PrecioVenta, 
    CASE 
        WHEN p.Estado = 1 THEN 'Activo' 
        ELSE 'Inactivo' 
    END AS Estado
FROM 
    PRODUCTO p
INNER JOIN 
    CATEGORIA c ON p.IdCategoria = c.IdCategoria" + filtro;

                SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                dgvdata.DataSource = dt;
            }
        }

        private void frmProductos_Load(object sender, EventArgs e)
        {
            llenar_tabla(); // Cargar los datos al iniciar
        }

        private void dgvdata_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvdata.Rows[e.RowIndex];
                txtcodigo.Text = row.Cells["Codigo"].Value.ToString();
                txtnombre.Text = row.Cells["Nombre"].Value.ToString();
                txtdescripcion.Text = row.Cells["Descripcion"].Value.ToString();
                // Llenar otros campos si es necesario
            }
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            txtcodigo.Clear();
            txtnombre.Clear();
            txtdescripcion.Clear();
            cbocategoria.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;
        }

        private void btnmodificar_Click(object sender, EventArgs e)
        {
            // Validar que el campo Código no esté vacío
            if (string.IsNullOrWhiteSpace(txtcodigo.Text))
            {
                MessageBox.Show("Por favor, ingrese el código del producto.");
                return;
            }

            // Verificar que el código solo contenga números
            if (!txtcodigo.Text.All(char.IsDigit))
            {
                MessageBox.Show("El código debe contener solo números.");
                return;
            }

            // Validar que todos los campos necesarios estén llenos
            if (string.IsNullOrWhiteSpace(txtnombre.Text) ||
                string.IsNullOrWhiteSpace(txtdescripcion.Text) ||
                cbocategoria.SelectedValue == null ||
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
UPDATE PRODUCTO 
SET 
    Nombre = @nombre, 
    Descripcion = @descripcion, 
    IdCategoria = @idcategoria, 
    Estado = @estado
WHERE 
    Codigo = @codigo";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@codigo", txtcodigo.Text);
                        comando.Parameters.AddWithValue("@nombre", txtnombre.Text);
                        comando.Parameters.AddWithValue("@descripcion", txtdescripcion.Text);
                        comando.Parameters.AddWithValue("@idcategoria", cbocategoria.SelectedValue);
                        comando.Parameters.AddWithValue("@estado", estado);

                        comando.ExecuteNonQuery();
                    }
                    MessageBox.Show("Registro modificado");
                    llenar_tabla();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
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
                    case "Código":
                        filtro = $" WHERE p.Codigo LIKE '%{busqueda}%'";
                        break;
                    case "Nombre":
                        filtro = $" WHERE p.Nombre LIKE '%{busqueda}%'";
                        break;
                    default:
                        MessageBox.Show("Campo de búsqueda no válido.");
                        return; // Salir del método para evitar la búsqueda con un campo no válido
                }
            }

            // Llamar al método para llenar la tabla con el filtro aplicado
            llenar_tabla(filtro);
        }

        private void btnexportar_Click(object sender, EventArgs e)
        {
            // Crear un nuevo libro de trabajo
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                // Crear una hoja de cálculo en el libro
                var worksheet = workbook.Worksheets.Add("Productos");

                // Establecer encabezados de columna
                for (int i = 0; i < dgvdata.Columns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = dgvdata.Columns[i].HeaderText;
                }

                // Llenar las filas de la hoja con los datos del DataGridView
                for (int i = 0; i < dgvdata.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvdata.Columns.Count; j++)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = dgvdata.Rows[i].Cells[j].Value?.ToString();
                    }
                }

                // Guardar el libro en un archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Guardar archivo Excel";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Datos exportados con éxito.");
                }
            }
        }

    }
}
