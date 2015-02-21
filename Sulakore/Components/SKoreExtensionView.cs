using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Sulakore.Extensions;

namespace Sulakore.Components
{
    public class SKoreExtensionView : ListView
    {
        private bool _suppressSelectionChanged;

        private readonly Dictionary<ExtensionBase, ListViewItem> _items;
        private readonly Dictionary<ListViewItem, ExtensionBase> _extensions;

        public bool LockColumns { get; set; }
        public Contractor Contractor { get; set; }

        public SKoreExtensionView()
        {
            _items = new Dictionary<ExtensionBase, ListViewItem>();
            _extensions = new Dictionary<ListViewItem, ExtensionBase>();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            var nameCol = new ColumnHeader { Name = "NameCol", Text = "Name", Width = 90 };
            var authorCol = new ColumnHeader { Name = "AuthorsCol", Text = "Author(s)", Width = 90 };
            var versionCol = new ColumnHeader { Name = "VersionCol", Text = "Version", Width = 90 };
            var statusCol = new ColumnHeader { Name = "StatusCol", Text = "Status", Width = 174 };
            Columns.AddRange(new[]
            {
                nameCol,
                authorCol,
                versionCol,
                statusCol,
            });

            GridLines = true;
            LockColumns = true;
            MultiSelect = false;
            View = View.Details;
            FullRowSelect = true;
            Dock = DockStyle.Fill;
            ShowItemToolTips = true;
            Size = new Size(465, 307);
            UseCompatibleStateImageBehavior = false;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        public void OpenSelected()
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem extItem = SelectedItems[0];
            ExtensionBase extension = _extensions[extItem];
            Contractor.Initialize(extension);

            extItem.SubItems[3].Text = "Launched";
        }
        public ExtensionBase GetSelected()
        {
            if (SelectedItems.Count < 1) return null;
            return _extensions[SelectedItems[0]];
        }
        public void UninstallSelected()
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            ExtensionBase extension = _extensions[item];
            Contractor.Uninstall(extension);

            Items.Remove(item);
            _items.Remove(extension);
            _extensions.Remove(item);
        }
        public void InstallExtension(string path)
        {
            ExtensionBase extension = Contractor.Install(path);
            if (extension != null)
            {
                var item = new ListViewItem(new[]
                {
                    extension.Name,
                    extension.Author,
                    extension.Version.ToString(),
                    "Dormant"
                });
                Items.Add(item);

                _items[extension] = item;
                _extensions[item] = extension;
            }
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != 0x14)
                base.OnNotifyMessage(m);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _suppressSelectionChanged = (GetItemAt(e.X, e.Y) != null);
            base.OnMouseDown(e);
        }
        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            if (LockColumns)
            {
                e.Cancel = true;
                e.NewWidth = Columns[e.ColumnIndex].Width;
            }
            base.OnColumnWidthChanging(e);
        }
        protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged && !e.IsSelected) _suppressSelectionChanged = false;
            else
            {
                base.OnItemSelectionChanged(e);
                if (e.IsSelected) _suppressSelectionChanged = false;
            }
        }
    }
}