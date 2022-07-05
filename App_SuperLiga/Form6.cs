using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App_SuperLiga
{
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void lbl_reporJogos_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Deseja eliminar o atual calendário de jogos e todos os resultados?", "Repor calendario de jogos", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                // drop these motherfuckers até ao calendario
            }
            else
            {
                return;
            }
        }

        private void lbl_eliminarEquipas_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Eliminar todas as equipas e os respectivos jogos?", "Eliminar equipas", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                // drop these motherfuckers até ao crud das equipas
            }
            else
            {
                return;
            }
        }

        private void lbl_reporApp_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Eliminar todos os dados da aplicação incluindo base de dados?", "Repor definições", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                // drop these motherfuckers
            }
            else
            {
                return;
            }
        }

        private void lbl_fechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
