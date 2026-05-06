using CapaPresentacion.Modales;
using CapaPresentacion.Utilidades;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
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

    public partial class frmCompras : Form
    {
        private string codigoCompraGenerado;
        private string documentoUsuario;
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"; // Actualiza con tu cadena de conexión

        public frmCompras(string documentoUsuario)
        {
            this.documentoUsuario = documentoUsuario;
            InitializeComponent();
        }

        private void frmCompras_Load(object sender, EventArgs e)
        {
            ConfigurarDataGridView();

            // Configurar otros controles del formulario
            cbotipodocumento.Items.Add("Boleta");
            cbotipodocumento.Items.Add("Factura");
            cbotipodocumento.SelectedIndex = 0;

            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtidproveedor.Text = "0";
            txtidproducto.Text = "0";

           
        }

        private void btnbuscarproveedor_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProveedor())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var proveedor = modal.SelectedProveedor;
                    txtidproveedor.Text = proveedor.Documento;
                    txtdocproveedor.Text = proveedor.Documento;
                    txtnombreproveedor.Text = proveedor.RazonSocial;
                }
                else
                {
                    txtdocproveedor.Select();
                }
            }
        }

        private void btnbuscarproducto_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProducto())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var producto = modal.SelectedProducto;
                    txtidproducto.Text = producto.Codigo;
                    txtcodproducto.Text = producto.Codigo;
                    txtproducto.Text = producto.Nombre;
                    txtpreciocompra.Select();
                }
                else
                {
                    txtcodproducto.Select();
                }
            }
        }

        private void txtcodproducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM PRODUCTO WHERE Codigo = @Codigo AND Estado = 1";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Codigo", txtcodproducto.Text);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        txtcodproducto.BackColor = Color.Honeydew;
                        txtidproducto.Text = reader["Codigo"].ToString();
                        txtproducto.Text = reader["Nombre"].ToString();
                        txtpreciocompra.Select();
                    }
                    else
                    {
                        txtcodproducto.BackColor = Color.MistyRose;
                        txtidproducto.Text = "0";
                        txtproducto.Text = "";
                    }

                    reader.Close();
                }
            }
        }

        private void ConfigurarDataGridView()
        {
            // Suponiendo que 'dgvdata' es tu DataGridView
            DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn();
            btnEliminar.Name = "btneliminar";
            btnEliminar.HeaderText = "Eliminar";
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseColumnTextForButtonValue = true;

            if (!dgvdata.Columns.Contains("btneliminar"))
            {
                dgvdata.Columns.Add(btnEliminar);
            }
        }

        private void btnagregarproducto_Click(object sender, EventArgs e)
        {
            decimal preciocompra = 0;
            decimal precioventa = 0;
            bool producto_existe = false;

            if (txtidproducto.Text == "0")
            {
                MessageBox.Show("Debe seleccionar un producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txtpreciocompra.Text, out preciocompra))
            {
                MessageBox.Show("Precio Compra - Formato moneda incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtpreciocompra.Select();
                return;
            }

            if (!decimal.TryParse(txtprecioventa.Text, out precioventa))
            {
                MessageBox.Show("Precio Venta - Formato moneda incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtprecioventa.Select();
                return;
            }

            foreach (DataGridViewRow fila in dgvdata.Rows)
            {
                if (fila.Cells["Codigo"].Value != null && fila.Cells["Codigo"].Value.ToString() == txtidproducto.Text)
                {
                    producto_existe = true;
                    break;
                }
            }

            if (!producto_existe)
            {
                dgvdata.Rows.Add(new object[] {
                txtidproducto.Text, // Código del producto
                txtproducto.Text,   // Nombre del producto
                preciocompra.ToString("0.00"), // Precio de compra
                precioventa.ToString("0.00"),   // Precio de venta
                txtcantidad.Value.ToString(),   // Cantidad
                (txtcantidad.Value * preciocompra).ToString("0.00") // SubTotal
            });

                calcularTotal();
                limpiarProducto();
                txtcodproducto.Select();
            }
        }

        private void limpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.BackColor = Color.White;
            txtproducto.Text = "";
            txtpreciocompra.Text = "";
            txtprecioventa.Text = "";
            txtcantidad.Value = 1;
        }

        private void calcularTotal()
        {
            decimal total = 0;
            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells["SubTotal"].Value != null)
                    {
                        total += Convert.ToDecimal(row.Cells["SubTotal"].Value);
                    }
                }
            }
            txttotalpagar.Text = total.ToString("0.00");
        }

        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == 6)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var w = Properties.Resources.delete25.Width;
                var h = Properties.Resources.delete25.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                // Especificar el espacio de nombres completo para evitar ambigüedad
                e.Graphics.DrawImage(Properties.Resources.delete25, new System.Drawing.Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }


        private void dgvdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que se haya hecho clic en una celda válida
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvdata.Columns["btneliminar"].Index)
            {
                // Confirmar eliminación
                DialogResult result = MessageBox.Show("¿Estás seguro de que deseas eliminar esta fila?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Eliminar la fila
                    dgvdata.Rows.RemoveAt(e.RowIndex);
                    calcularTotal(); // Recalcular el total después de la eliminación
                }
            }
        }

        private void txtpreciocompra_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || e.KeyChar == '.')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtprecioventa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || e.KeyChar == '.')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void btnregistrar_Click(object sender, EventArgs e)
        {
            // Código para validar y registrar la compra

            if (txtidproveedor.Text == "0")
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataTable detalle_compra = new DataTable();
            detalle_compra.Columns.Add("IdProducto", typeof(string));
            detalle_compra.Columns.Add("PrecioCompra", typeof(decimal));
            detalle_compra.Columns.Add("PrecioVenta", typeof(decimal));
            detalle_compra.Columns.Add("Cantidad", typeof(int));
            detalle_compra.Columns.Add("MontoTotal", typeof(decimal));

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue;

                detalle_compra.Rows.Add(
                    new object[] {
            row.Cells["Codigo"].Value.ToString(),
            Convert.ToDecimal(row.Cells["PrecioCompra"].Value),
            Convert.ToDecimal(row.Cells["PrecioVenta"].Value),
            Convert.ToInt32(row.Cells["Cantidad"].Value),
            Convert.ToDecimal(row.Cells["SubTotal"].Value)
                    });
            }

            decimal montoTotalCompra = 0;
            foreach (DataRow row in detalle_compra.Rows)
            {
                montoTotalCompra += Convert.ToDecimal(row["MontoTotal"]);
            }

            int idCompra = 0;
            string numeroDocumento = string.Format("{0:00000}", DateTime.Now.Ticks % 100000);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    "INSERT INTO COMPRA (IdUsuario, IdProveedor, TipoDocumento, NumeroDocumento, MontoTotal) " +
                    "OUTPUT INSERTED.IdCompra " +
                    "VALUES (@IdUsuario, @IdProveedor, @TipoDocumento, @NumeroDocumento, @MontoTotal)", connection);
                command.Parameters.AddWithValue("@IdUsuario", documentoUsuario);
                command.Parameters.AddWithValue("@IdProveedor", txtidproveedor.Text);
                command.Parameters.AddWithValue("@TipoDocumento", cbotipodocumento.SelectedItem.ToString());
                command.Parameters.AddWithValue("@NumeroDocumento", numeroDocumento);
                command.Parameters.AddWithValue("@MontoTotal", montoTotalCompra);

                idCompra = (int)command.ExecuteScalar();

                foreach (DataRow row in detalle_compra.Rows)
                {
                    command = new SqlCommand(
                        "INSERT INTO DETALLE_COMPRA (IdCompra, IdProducto, PrecioCompra, PrecioVenta, Cantidad, MontoTotal) " +
                        "VALUES (@IdCompra, @IdProducto, @PrecioCompra, @PrecioVenta, @Cantidad, @MontoTotal)", connection);
                    command.Parameters.AddWithValue("@IdCompra", idCompra);
                    command.Parameters.AddWithValue("@IdProducto", row["IdProducto"]);
                    command.Parameters.AddWithValue("@PrecioCompra", row["PrecioCompra"]);
                    command.Parameters.AddWithValue("@PrecioVenta", row["PrecioVenta"]);
                    command.Parameters.AddWithValue("@Cantidad", row["Cantidad"]);
                    command.Parameters.AddWithValue("@MontoTotal", row["MontoTotal"]);

                    command.ExecuteNonQuery();
                }
            }

            codigoCompraGenerado = numeroDocumento;
            Clipboard.SetText(numeroDocumento);

            DialogResult result = MessageBox.Show("Compra registrada con éxito. El número de documento ha sido copiado al portapapeles. ¿Desea imprimir el PDF ahora?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            dgvdata.Rows.Clear();

            if (result == DialogResult.Yes)
            {
                btnimprimir_Click(sender, e);
            }
        }

        private void btnimprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(codigoCompraGenerado))
            {
                MessageBox.Show("No hay una compra registrada para imprimir", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string textoHtml = Properties.Resources.PlantillaCompra;

            string nombreNegocio;
            string rifNegocio;
            string direccionNegocio;
            byte[] logoImage = null;
            DataTable detalleCompra = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Obtener información del negocio
                string queryNegocio = "SELECT Nombre, RIF, Direccion, Logo FROM NEGOCIO";
                using (SqlCommand cmd = new SqlCommand(queryNegocio, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        nombreNegocio = reader["Nombre"].ToString();
                        rifNegocio = reader["RIF"].ToString();
                        direccionNegocio = reader["Direccion"].ToString();
                        logoImage = reader["Logo"] as byte[];
                    }
                    else
                    {
                        nombreNegocio = "Nombre del Negocio";
                        rifNegocio = "RIF del Negocio";
                        direccionNegocio = "Dirección del Negocio";
                    }
                    reader.Close();
                }

                // Obtener detalles de la compra
                string queryDetalles = @"
            SELECT p.Codigo, p.Nombre, d.PrecioCompra, d.PrecioVenta, d.Cantidad, d.MontoTotal
            FROM DETALLE_COMPRA d
            JOIN PRODUCTO p ON d.IdProducto = p.Codigo
            WHERE d.IdCompra = (SELECT IdCompra FROM COMPRA WHERE NumeroDocumento = @NumeroDocumento)";
                using (SqlCommand cmd = new SqlCommand(queryDetalles, conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroDocumento", codigoCompraGenerado);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(detalleCompra);
                }
            }

            // Reemplazar marcadores de posición en la plantilla HTML
            textoHtml = textoHtml.Replace("@nombrenegocio", nombreNegocio.ToUpper());
            textoHtml = textoHtml.Replace("@docnegocio", rifNegocio);
            textoHtml = textoHtml.Replace("@direcnegocio", direccionNegocio);
            textoHtml = textoHtml.Replace("@tipodocumento", "Compra");
            textoHtml = textoHtml.Replace("@numerodocumento", codigoCompraGenerado);
            textoHtml = textoHtml.Replace("@docproveedor", txtdocproveedor.Text);
            textoHtml = textoHtml.Replace("@nombreproveedor", txtnombreproveedor.Text);
            textoHtml = textoHtml.Replace("@fecharegistro", txtfecha.Text);
            textoHtml = textoHtml.Replace("@usuarioregistro", documentoUsuario);

            // Construir las filas para el DataGridView
            string filas = string.Empty;
            foreach (DataRow row in detalleCompra.Rows)
            {
                filas += "<tr>";
                filas += $"<td>{row["Nombre"]}</td>";
                filas += $"<td>{row["PrecioCompra"]}</td>";
                filas += $"<td>{row["Cantidad"]}</td>";
                filas += $"<td>{row["MontoTotal"]}</td>";
                filas += "</tr>";
            }
            textoHtml = textoHtml.Replace("@filas", filas);
            textoHtml = textoHtml.Replace("@montototal", detalleCompra.Compute("SUM(MontoTotal)", string.Empty).ToString());

            // Guardar archivo PDF temporal
            string rutaPdf = Path.Combine(Path.GetTempPath(), $"Compra_{DateTime.Now.Ticks}.pdf");
            using (FileStream stream = new FileStream(rutaPdf, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                // Opcional: agregar logo al PDF
                if (logoImage != null)
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(logoImage);
                    img.ScaleToFit(60, 60);
                    img.Alignment = iTextSharp.text.Image.UNDERLYING;
                    img.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                    pdfDoc.Add(img);
                }

                using (StringReader sr = new StringReader(textoHtml))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                }

                pdfDoc.Close();
                stream.Close();
            }

            // Mostrar el PDF en una ventana emergente
            var pdfViewerForm = new PdfViewerForm(rutaPdf);
            pdfViewerForm.ShowDialog();
        }


    }

}
