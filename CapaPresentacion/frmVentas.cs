using CapaPresentacion.Modales;
using CapaPresentacion.Utilidades;
using iTextSharp.tool.xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{

    public partial class frmVentas : Form
    {
        private string documentoUsuario;
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"; // Actualiza con tu cadena de conexión
        private string numeroDocumentoVenta;

        public frmVentas(string documentoUsuario)
        {
            this.documentoUsuario = documentoUsuario;
            InitializeComponent();
        }

        private void frmVentas_Load(object sender, EventArgs e)
        {

            ConfigurarDataGridView();

            // Configurar otros controles del formulario
            cbotipodocumento.Items.Add("Boleta");
            cbotipodocumento.Items.Add("Factura");
            cbotipodocumento.SelectedIndex = 0;

            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtdocumentocliente.Text = "";
            txtnombrecliente.Text = "";
            txtidproducto.Text = "0";
            txtpagocon.Text = "";
            txtcambio.Text = "";
            txttotalpagar.Text = "0";
        }

        private void btnbuscarcliente_Click(object sender, EventArgs e)
        {
            using (var modal = new mdCliente())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var cliente = modal.SelectedCliente;
                    txtdocumentocliente.Text = cliente.Documento;
                    txtnombrecliente.Text = cliente.NombreCompleto;
                }
                else
                {
                    txtdocumentocliente.Select();
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
                    txtprecio.Text = producto.PrecioVenta.ToString("0.00", CultureInfo.InvariantCulture);
                    txtstock.Text = producto.Stock.ToString();
                    txtcantidad.Select();
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
                        txtprecio.Text = Convert.ToDecimal(reader["PrecioVenta"]).ToString("0.00", CultureInfo.InvariantCulture);
                        txtstock.Text = reader["Stock"].ToString();
                        txtcantidad.Select();
                    }
                    else
                    {
                        txtcodproducto.BackColor = Color.MistyRose;
                        txtidproducto.Text = "0";
                        txtproducto.Text = "";
                        txtprecio.Text = "";
                        txtstock.Text = "";
                    }

                    reader.Close();
                }
            }
        }

        private void ConfigurarDataGridView()
        {
            DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn
            {
                Name = "btneliminar",
                HeaderText = "Eliminar",
                Text = "Eliminar",
                UseColumnTextForButtonValue = true
            };

            if (!dgvdata.Columns.Contains("btneliminar"))
            {
                dgvdata.Columns.Add(btnEliminar);
            }
        }

        private void btnagregarproducto_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtprecio.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precio))
            {
                MessageBox.Show("Precio - Formato moneda incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtprecio.Select();
                return;
            }

            if (txtidproducto.Text == "0")
            {
                MessageBox.Show("Debe seleccionar un producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (Convert.ToInt32(txtstock.Text) < Convert.ToInt32(txtcantidad.Value))
            {
                MessageBox.Show("La cantidad no puede ser mayor al stock", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            bool producto_existe = false;

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
              txtidproducto.Text,
              txtproducto.Text,
              precio.ToString("0.00", CultureInfo.InvariantCulture),
              txtcantidad.Value.ToString(),
              (txtcantidad.Value * precio).ToString("0.00", CultureInfo.InvariantCulture)
          });

                CalcularTotal();
                LimpiarProducto();
                txtcodproducto.Select();
            }
        }

        private void LimpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.BackColor = Color.White;
            txtproducto.Text = "";
            txtprecio.Text = "";
            txtstock.Text = "";
            txtcantidad.Value = 1;
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.Cells["SubTotal"].Value != null)
                {
                    if (decimal.TryParse(row.Cells["SubTotal"].Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal subtotal))
                    {
                        total += subtotal;
                    }
                }
            }
            txttotalpagar.Text = total.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void CalcularCambio()
        {
            if (string.IsNullOrWhiteSpace(txttotalpagar.Text))
            {
                MessageBox.Show("No existen productos en la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txttotalpagar.Text.Trim(), NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal total))
            {
                MessageBox.Show("Total a pagar - Formato incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtpagocon.Text))
            {
                txtpagocon.Text = "0";
            }

            if (!decimal.TryParse(txtpagocon.Text.Trim(), NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal pagacon))
            {
                MessageBox.Show("Monto pagado - Formato incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (pagacon < total)
            {
                txtcambio.Text = "0.00";
            }
            else
            {
                decimal cambio = pagacon - total;
                txtcambio.Text = cambio.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == 5)
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvdata.Columns["btneliminar"].Index)
            {
                DialogResult result = MessageBox.Show("¿Estás seguro de que deseas eliminar esta fila?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    dgvdata.Rows.RemoveAt(e.RowIndex);
                    CalcularTotal();
                }
            }
        }

        private void txtpagocon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                CalcularCambio();
            }
        }

        private void btnregistrarventa_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtdocumentocliente.Text))
            {
                MessageBox.Show("Debe ingresar documento del cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtnombrecliente.Text))
            {
                MessageBox.Show("Debe ingresar nombre del cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            CalcularTotal();
            CalcularCambio();

            if (!decimal.TryParse(txtpagocon.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal montoPago))
            {
                MessageBox.Show("El monto de pago no tiene un formato válido", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txtcambio.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal montoCambio))
            {
                MessageBox.Show("El monto de cambio no tiene un formato válido", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataTable detalle_venta = new DataTable();
            detalle_venta.Columns.Add("IdProducto", typeof(string));
            detalle_venta.Columns.Add("PrecioVenta", typeof(decimal));
            detalle_venta.Columns.Add("Cantidad", typeof(int));
            detalle_venta.Columns.Add("SubTotal", typeof(decimal));

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue;

                if (decimal.TryParse(row.Cells["Precio"].Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precio) &&
                    int.TryParse(row.Cells["Cantidad"].Value.ToString(), out int cantidad) &&
                    decimal.TryParse(row.Cells["SubTotal"].Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal subtotal))
                {
                    detalle_venta.Rows.Add(
                        row.Cells["Codigo"].Value.ToString(),
                        precio,
                        cantidad,
                        subtotal
                    );
                }
                else
                {
                    MessageBox.Show("Datos en el DataGridView no son válidos.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            decimal montoTotalVenta = 0;

            foreach (DataRow row in detalle_venta.Rows)
            {
                montoTotalVenta += Convert.ToDecimal(row["SubTotal"], CultureInfo.InvariantCulture);
            }

            int idVenta;
            numeroDocumentoVenta = string.Format("{0:00000}", DateTime.Now.Ticks % 100000);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = "INSERT INTO VENTA (IdUsuario, TipoDocumento, NumeroDocumento, DocumentoCliente, NombreCliente, MontoPago, MontoCambio, MontoTotal) " +
                        "OUTPUT INSERTED.IdVenta " +
                        "VALUES (@IdUsuario, @TipoDocumento, @NumeroDocumento, @DocumentoCliente, @NombreCliente, @MontoPago, @MontoCambio, @MontoTotal)";
                    command.Parameters.AddWithValue("@IdUsuario", documentoUsuario);
                    command.Parameters.AddWithValue("@TipoDocumento", cbotipodocumento.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@NumeroDocumento", numeroDocumentoVenta);
                    command.Parameters.AddWithValue("@DocumentoCliente", txtdocumentocliente.Text);
                    command.Parameters.AddWithValue("@NombreCliente", txtnombrecliente.Text);
                    command.Parameters.AddWithValue("@MontoPago", montoPago);
                    command.Parameters.AddWithValue("@MontoCambio", montoCambio);
                    command.Parameters.AddWithValue("@MontoTotal", montoTotalVenta);

                    idVenta = Convert.ToInt32(command.ExecuteScalar());

                    foreach (DataRow row in detalle_venta.Rows)
                    {
                        command.CommandText = "INSERT INTO DETALLE_VENTA (IdVenta, IdProducto, PrecioVenta, Cantidad, SubTotal) " +
                            "VALUES (@IdVenta, @IdProducto, @PrecioVenta, @Cantidad, @SubTotal)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@IdVenta", idVenta);
                        command.Parameters.AddWithValue("@IdProducto", row["IdProducto"]);
                        command.Parameters.AddWithValue("@PrecioVenta", row["PrecioVenta"]);
                        command.Parameters.AddWithValue("@Cantidad", row["Cantidad"]);
                        command.Parameters.AddWithValue("@SubTotal", row["SubTotal"]);

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    // Preguntar al usuario si quiere ver el PDF
                    var result = MessageBox.Show(
                        $"Venta registrada con éxito. El número de documento es {numeroDocumentoVenta}. ¿Desea ver el PDF?",
                        "Mensaje",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (result == DialogResult.Yes)
                    {
                        btnimprimir_Click(sender, e); // Llamar al método para generar y mostrar el PDF
                    }
                    else
                    {
                        LimpiarFormulario();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error al registrar la venta: " + ex.Message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void LimpiarFormulario()
        {
            txtdocumentocliente.Text = "";
            txtnombrecliente.Text = "";
            txttotalpagar.Text = "0";
            txtpagocon.Text = "";
            txtcambio.Text = "";
            dgvdata.Rows.Clear();
        }
        private void btnimprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumentoVenta))
            {
                MessageBox.Show("No hay una venta registrada para imprimir", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string textoHtml = Properties.Resources.PlantillaVenta;

            string nombreNegocio;
            string rifNegocio;
            string direccionNegocio;
            byte[] logoImage = null;
            DataTable detalleVenta = new DataTable();

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

                // Obtener detalles de la venta
                string queryDetalles = @"
            SELECT p.Codigo, p.Nombre, d.PrecioVenta, d.Cantidad, d.SubTotal
            FROM DETALLE_VENTA d
            JOIN PRODUCTO p ON d.IdProducto = p.Codigo
            WHERE d.IdVenta = (SELECT IdVenta FROM VENTA WHERE NumeroDocumento = @NumeroDocumento)";
                using (SqlCommand cmd = new SqlCommand(queryDetalles, conn))
                {
                    cmd.Parameters.AddWithValue("@NumeroDocumento", numeroDocumentoVenta);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(detalleVenta);
                }
            }

            // Reemplazar marcadores de posición en la plantilla HTML
            textoHtml = textoHtml.Replace("@nombrenegocio", nombreNegocio.ToUpper());
            textoHtml = textoHtml.Replace("@docnegocio", rifNegocio);
            textoHtml = textoHtml.Replace("@direcnegocio", direccionNegocio);
            textoHtml = textoHtml.Replace("@tipodocumento", cbotipodocumento.SelectedItem.ToString());
            textoHtml = textoHtml.Replace("@numerodocumento", numeroDocumentoVenta);
            textoHtml = textoHtml.Replace("@doccliente", txtdocumentocliente.Text);
            textoHtml = textoHtml.Replace("@nombrecliente", txtnombrecliente.Text);
            textoHtml = textoHtml.Replace("@fecharegistro", txtfecha.Text);
            textoHtml = textoHtml.Replace("@usuarioregistro", documentoUsuario);

            // Construir las filas para el DataGridView
            string filas = string.Empty;
            foreach (DataRow row in detalleVenta.Rows)
            {
                filas += "<tr>";
                filas += $"<td>{row["Nombre"]}</td>";
                filas += $"<td>{row["PrecioVenta"]}</td>";
                filas += $"<td>{row["Cantidad"]}</td>";
                filas += $"<td>{row["SubTotal"]}</td>";
                filas += "</tr>";
            }
            textoHtml = textoHtml.Replace("@filas", filas);
            textoHtml = textoHtml.Replace("@montototal", detalleVenta.Compute("SUM(SubTotal)", string.Empty).ToString());

            // Guardar archivo PDF temporal
            string rutaPdf = Path.Combine(Path.GetTempPath(), $"Venta_{DateTime.Now.Ticks}.pdf");
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
