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

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace CapaPresentacion
{
    public partial class Respaldo : Form
    {
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True"; // Ajusta la cadena de conexión

        public Respaldo()
        {
            InitializeComponent();
        }

        private void Respaldo_Load(object sender, EventArgs e)
        {
            // Código que se ejecuta cuando el formulario se carga, si es necesario
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Backup Files (*.bak)|*.bak";
                saveFileDialog.Title = "Select Backup Location";
                saveFileDialog.FileName = "DBSISTEMA_INVENTARIO_Backup.bak"; // Nombre predeterminado del archivo

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string backupFileName = saveFileDialog.FileName;

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = $@"
                     BACKUP DATABASE [DBSISTEMA_INVENTARIO] 
                     TO DISK = '{backupFileName}' 
                     WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                                MessageBox.Show("Backup completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show($"SQL Server Error: {sqlEx.Message}\nError Number: {sqlEx.Number}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Backup Files (*.bak)|*.bak";
                openFileDialog.Title = "Select Backup File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string backupFileName = openFileDialog.FileName;

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Cambiar la base de datos a modo de usuario único
                            string setSingleUserQuery = @"
                    USE master;
                    ALTER DATABASE [DBSISTEMA_INVENTARIO] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    ";

                            // Restaurar la base de datos
                            string restoreQuery = $@"
                    RESTORE DATABASE [DBSISTEMA_INVENTARIO]
                    FROM DISK = '{backupFileName}'
                    WITH REPLACE, STATS = 10;
                    ";

                            // Volver a poner la base de datos en modo de multiusuario
                            string setMultiUserQuery = @"
                    ALTER DATABASE [DBSISTEMA_INVENTARIO] SET MULTI_USER;
                    ";

                            // Ejecutar las consultas
                            using (SqlCommand command = new SqlCommand($"{setSingleUserQuery} {restoreQuery} {setMultiUserQuery}", connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            MessageBox.Show("Restore completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show($"SQL Server Error: {sqlEx.Message}\nError Number: {sqlEx.Number}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
    }
}
