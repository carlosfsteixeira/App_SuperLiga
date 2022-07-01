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
    public partial class Form4 : Form
    {
        DataClasses1DataContext dc;

        Form2 form2;

        int idEquipaSel;

        public Form4(Form2 form, int idEquipa)
        {
            dc = new DataClasses1DataContext();

            InitializeComponent();

            form2 = form;
            idEquipaSel = idEquipa;
        }

        

        private void txtNomeJogador_TextChanged(object sender, EventArgs e)
        {
            foreach (char car in txtNomeJogador.Text)
            {
                if ((char.IsDigit(car)))
                {
                    MessageBox.Show("Input invalido. Não são aceites numeros", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNomeJogador.Text = txtNomeJogador.Text.Remove(txtNomeJogador.Text.Length - 1, 1);
                    txtNomeJogador.Focus();
                    break;
                }
            }
        }

        private void btAddJogador_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNomeJogador.Text) || comboBoxNumCam.SelectedItem == null || comboBoxPosicao.SelectedItem == null)
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                var maxIdQuery = from Jogadore in dc.Jogadores
                                 orderby Jogadore.id_jogador descending
                                 select Jogadore.id_jogador;

                int maxJogadorID;

                if (maxIdQuery.Count() == 0)
                {
                    maxJogadorID = 0;
                }
                else
                {
                    maxJogadorID = maxIdQuery.Max();
                }

                if (CheckNumeroCamisola())
                {
                    return;
                }
                else
                {
                    Jogadore novoJogador = new Jogadore
                    {
                        id_jogador = (maxJogadorID + 1),
                        nome = txtNomeJogador.Text,
                        posicao = comboBoxPosicao.SelectedItem.ToString(),
                        numero = Convert.ToInt32(comboBoxNumCam.SelectedItem),
                        id_equipa = idEquipaSel,
                    };


                    //adicionar novo Jogador a bd
                    dc.Jogadores.InsertOnSubmit(novoJogador);

                    try
                    {
                        dc.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                form2.RefreshAllGrids();
                form2.RefreshTeamGrid();

                MessageBox.Show("Adicionado com sucesso", "Novo jogador", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }

           
        }

        public bool CheckNumeroCamisola()
        {
            bool output = false;

            var listaNum = from Jogadore in dc.Jogadores
                           where Jogadore.id_equipa == idEquipaSel
                           select Jogadore.numero;

            foreach (var num in listaNum)
            {
                if (num == (Convert.ToInt16(comboBoxNumCam.SelectedItem)))
                {
                    MessageBox.Show("Já existe um jogador com o numero " + num + " nesta equipa", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    output = true;
                }
            }

            return output;
        }

        private void lbl_fechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
