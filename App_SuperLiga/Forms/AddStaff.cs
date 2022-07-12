using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace App_SuperLiga
{
    public partial class AddStaff : Form
    {
        DataClasses1DataContext dc;

        Dashboard form2;
        int idEquipaSel;

        public AddStaff(Dashboard form, int idEquipa)
        {
            dc = new DataClasses1DataContext();

            InitializeComponent();

            form2 = form;
            idEquipaSel = idEquipa;
        }

        private void lbl_Confirmar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNomeStaff.Text) || comboBoxFuncao.SelectedItem == null)
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                string comboBoxSel = comboBoxFuncao.SelectedItem.ToString();

                if (comboBoxSel == "Treinador")
                {
                    if (ValidarExistenciaTreinador())
                    {
                        MessageBox.Show("Ja existe um Treinador", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (comboBoxSel == "Presidente")
                {
                    if (ValidarExistenciaPresidente())
                    {
                        MessageBox.Show("Ja existe um Presidente", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                AddStaffMember();
            }
        }

        public void AddStaffMember()
        {
            var maxIdQuery = from Staff in dc.Staffs
                             orderby Staff.id_staff descending
                             select Staff.id_staff;

            int maxStaffID;

            if (maxIdQuery.Count() == 0)
            {
                maxStaffID = 0;
            }
            else
            {
                maxStaffID = maxIdQuery.Max();
            }

            Staff novoStaff = new Staff
            {
                id_staff = (maxStaffID + 1),
                nome = txtNomeStaff.Text,
                funcao = comboBoxFuncao.SelectedItem.ToString(),
                id_equipa = idEquipaSel,
            };

            //adicionar novoFuncionario a bd
            dc.Staffs.InsertOnSubmit(novoStaff);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            form2.RefreshAllGrids();
            form2.RefreshTeamGrid();

            MessageBox.Show("Adicionado com sucesso", "Novo membro no Staff");

            this.Close();

        }

        public bool ValidarExistenciaTreinador()
        {
            bool output = false;

            var procurarTreinador = from Staff in dc.Staffs
                                    where Staff.funcao == "Treinador"
                                    && Staff.id_equipa == idEquipaSel
                                    select Staff;

            if (procurarTreinador.Any())
            {
                output = true;
            }


            return output;
        }

        public bool ValidarExistenciaPresidente()
        {
            bool output = false;

            var procurarPresidente = from Staff in dc.Staffs
                                     where Staff.funcao == "Presidente"
                                     && Staff.id_equipa == idEquipaSel
                                     select Staff;

            if (procurarPresidente.Any())
            {

                output = true;
            }

            return output;
        }

        private void txtNomeStaff_TextChanged(object sender, EventArgs e)
        {
            foreach (char car in txtNomeStaff.Text)
            {
                if ((char.IsDigit(car)))
                {
                    MessageBox.Show("Insira apenas letras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNomeStaff.Text = txtNomeStaff.Text.Remove(txtNomeStaff.Text.Length - 1, 1);
                    txtNomeStaff.Focus();
                    break;
                }
            }
        }

        private void lbl_fechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
