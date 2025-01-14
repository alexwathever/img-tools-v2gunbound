﻿/*  ----------------------------------------------------------------------------
 *  Copyright (C) 2011 XfsGames <http://www.xfsgames.com.ar/>
 *  ----------------------------------------------------------------------------
 *  Img Tools
 *  ----------------------------------------------------------------------------
 *  File:       Form1.cs
 *  Author:     CARLOSX
 *  ----------------------------------------------------------------------------
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 */
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace ImgTools
{
    public partial class Form1 : Form
    {
        string filename2 = "";
        public Form1()
        {
            InitializeComponent();
            string srr = AppDomain.CurrentDomain.BaseDirectory;
            Log.WriteLine(srr);
            DirectoryInfo DIR = new DirectoryInfo(srr + "/img");
            if (!DIR.Exists)
            {
                Log.WriteLine("Directorio no existe");
                DIR.Create();
            }
            this.AddArchives(srr + "//img//");
        }

        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.AddArchives("\\img\\");
        }

        private void RecurseGetArchives(ArrayList list, string dir)
        {
            foreach (string str in Directory.GetFiles(@dir, "*.img"))
            {
                //MessageBox.Show(str);
                list.Add(Archive.LoadIMG(str));
            }
            foreach (string str2 in Directory.GetDirectories(dir))
            {
                this.RecurseGetArchives(list, str2);
            }
        }
        private ListView m_ListView;
        private bool m_InHere;
        public void AddArchives(string dir)
        {
            ArrayList list = new ArrayList();
            this.RecurseGetArchives(list, dir);
            FolderNode node = new FolderNode("Imagenes");
            for (int i = 0; i < list.Count; i++)
            {
                Archive archive = (Archive)list[i];
                FolderNode node2 = new FolderNode(archive);
                node.Folders.Add(node2);
                node.Nodes.Add(node2);
            }
            this.tvwFolders.Nodes.Add(node);
            node.Expand();
            this.SetList(node);
        }
        private void AddIcon(bool folder, string extension)
        {
            FileIcon icon = new FileIcon(folder, extension, FileIcon.SHGetFileInfoConstants.SHGFI_TYPENAME | FileIcon.SHGetFileInfoConstants.SHGFI_ICON | FileIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON);
            this.ilIcons.Images.Add(icon.ShellIcon);
            icon = new FileIcon(folder, extension, FileIcon.SHGetFileInfoConstants.SHGFI_SELECTED | FileIcon.SHGetFileInfoConstants.SHGFI_TYPENAME | FileIcon.SHGetFileInfoConstants.SHGFI_ICON | FileIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON);
            this.ilIcons.Images.Add(icon.ShellIcon);
            icon = new FileIcon(folder, extension, FileIcon.SHGetFileInfoConstants.SHGFI_TYPENAME | FileIcon.SHGetFileInfoConstants.SHGFI_ICON | FileIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES);
            this.ilLargeIcons.Images.Add(icon.ShellIcon);
            icon = new FileIcon(folder, extension, FileIcon.SHGetFileInfoConstants.SHGFI_SELECTED | FileIcon.SHGetFileInfoConstants.SHGFI_TYPENAME | FileIcon.SHGetFileInfoConstants.SHGFI_ICON | FileIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES);
            this.ilLargeIcons.Images.Add(icon.ShellIcon);
        }
        public void SetList(TreeNode node)
        {
            if (!this.m_InHere)
            {
                this.m_InHere = true;
                for (TreeNode node2 = node.Parent; node2 != null; node2 = node2.Parent)
                {
                    node2.Expand();
                }
                if (this.tvwFolders.SelectedNode != node)
                {
                    this.tvwFolders.SelectedNode = node;
                }
                base.SuspendLayout();
                Control control = null;
                if (this.pnPanel.Controls.Count > 0)
                {
                    control = this.pnPanel.Controls[0];
                }
                this.pnPanel.Controls.Clear();
                if (control != null)
                {
                    control.Dispose();
                }
                if ((this.m_ListView == null) || this.m_ListView.IsDisposed)
                {
                    this.m_ListView = new ListView();
                    this.m_ListView.Dock = DockStyle.Fill;
                    this.m_ListView.SmallImageList = this.ilIcons;
                    this.m_ListView.View = View.Details;
                    this.m_ListView.MultiSelect = false;
                    this.m_ListView.ItemActivate += new EventHandler(this.ListView_ItemActivate);
                    this.m_ListView.Columns.Add("Name", 300, HorizontalAlignment.Left);
                    this.m_ListView.Columns.Add("Size", 100, HorizontalAlignment.Right);
                    this.m_ListView.Columns.Add("Type", 200, HorizontalAlignment.Left);
                    //this.m_ListView.ContextMenu = this.cmContext;
                }
                else
                {
                    this.m_ListView.Items.Clear();
                }
                Panel panel = new Panel
                {
                    Size = this.pnPanel.Size,
                    Dock = DockStyle.Fill
                };
                panel.Controls.Add(this.m_ListView);
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    TreeNode node3 = node.Nodes[i];
                    if (node3 is FileNode)
                    {
                        this.m_ListView.Items.Add(new FileListItem(((FileNode)node3).ArchivedFile, ((FileNode)node3).Icon));
                    }
                    else if (node3 is FolderNode)
                    {
                        this.m_ListView.Items.Add(new FolderListItem(node3));
                    }
                }
                if (node is FolderNode)
                {
                    ArrayList files = ((FolderNode)node).Files;
                    for (int j = 0; j < files.Count; j++)
                    {
                        TreeNode node4 = (TreeNode)files[j];
                        if (node4 is FileNode)
                        {
                            this.m_ListView.Items.Add(new FileListItem(((FileNode)node4).ArchivedFile, ((FileNode)node4).Icon));
                        }
                        else if (node4 is FolderNode)
                        {
                            this.m_ListView.Items.Add(new FolderListItem(node4));
                        }
                    }
                }
                this.pnPanel.Controls.Add(panel);
                base.ResumeLayout();
                this.m_InHere = false;
            }

        }
        private void ListView_ItemActivate(object sender, EventArgs e)
        {
            if (this.m_ListView.SelectedItems.Count != 0)
            {
                ListViewItem item = this.m_ListView.SelectedItems[0];
                if (item is FileListItem)
                {
                    this.SetFile(((FileListItem)item).ArchivedFile);
                }
                else if (item is FolderListItem)
                {
                    this.SetList(((FolderListItem)item).Node);
                }
            }
        }
        public void SetFile(ArchivedFile file)
        {
            TreeNode node = file.Node;
            for (TreeNode node2 = node.Parent; node2 != null; node2 = node2.Parent)
            {
                node2.Expand();
            }
            if (this.tvwFolders.SelectedNode != node)
            {
                this.tvwFolders.SelectedNode = node;
            }
            base.SuspendLayout();
            Panel pn = new Panel
            {
                Size = this.pnPanel.Size,
                Dock = DockStyle.Fill
            };
            FileDecoder.FindDecoder(Path.GetExtension(file.FileName)).FillPanel(file, pn);
            filename2 = file.FileName;
            if (checkBoxAutosave.Checked)
            {
                verifyExtension();
            }
            
            Control control = null;
            if (this.pnPanel.Controls.Count > 0)
            {
                control = this.pnPanel.Controls[0];
            }
            this.pnPanel.Controls.Clear();
            if (control != null)
            {
                control.Dispose();
            }
            this.pnPanel.Controls.Add(pn);
            base.ResumeLayout();
        }

        private void tvwFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node is FileNode)
            {
                FileNode node = e.Node as FileNode;
                if (node != null)
                {
                    this.SetFile(node.ArchivedFile);
                }
            }
            else if ((e.Node is ArchiveNode) || (e.Node is FolderNode))
            {
                this.SetList(e.Node);
            }
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pnPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        private void btnDwFrame_Click(object sender, EventArgs e)
        {
            verifyExtension();

        }
        public bool IsNumeric(string value)
        {
            bool isNumeric = true;
            char[] digits = "0123456789".ToCharArray();
            char[] letters = value.ToCharArray();
            for (int k = 0; k < letters.Length; k++)
            {
                for (int i = 0; i < digits.Length; i++)
                {
                    if (letters[k] != digits[i])
                    {
                        isNumeric = false;
                        break;
                    }
                }
            }
            return isNumeric;
        }
        private int verifyInt(string a)
        {
            if(Regex.IsMatch(a, @"^\d+$"))
            {
                return Convert.ToInt32(a);
            }
            else
            {
                return 0;
            }
        }
        private void verifyExtension()
        {
            

            generateFrameImage gen = new generateFrameImage();
            if (checkPng.Checked)
            {
                gen.generate(filename2, checkTransparent.Checked, txtColor.BackColor.Name, "png", verifyInt(txtMargin.Text), verifyInt(txtRows.Text));
            }
            if (checkBmp.Checked)
            {
                gen.generate(filename2, checkTransparent.Checked, txtColor.BackColor.Name, "bmp", verifyInt(txtMargin.Text), verifyInt(txtRows.Text));
            }
            if (checkBmp.Checked == false && checkPng.Checked == false)
            {
                MessageBox.Show("No se ha generado ningún código debido a que no se selecciono ninguna extensión.\nSelecciona de manera correcta");
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void btnSetColor_Click(object sender, EventArgs e)
        {
            if(colorDialog1.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                txtColor.BackColor = colorDialog1.Color;
            }
        }

        private void checkPng_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }
}