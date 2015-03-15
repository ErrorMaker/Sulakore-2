using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Sulakore.Extensions;

namespace Sulakore.Components
{
    public class SKoreExtensionView : SKoreListView
    {
        private readonly Dictionary<ExtensionBase, ListViewItem> _items;
        private readonly Dictionary<ListViewItem, ExtensionBase> _extensions;

        public SKoreExtensionView()
        {
            _items = new Dictionary<ExtensionBase, ListViewItem>();
            _extensions = new Dictionary<ListViewItem, ExtensionBase>();
        }

        public void InitializeItemExtension()
        {
            ExtensionBase extension = GetItemExtension();
            if (extension == null) return;

            ((Contractor)extension.Contractor).Initialize(extension);
        }
        public ExtensionBase GetItemExtension()
        {
            return SelectedItems.Count > 0 && _extensions.ContainsKey(SelectedItems[0]) ?
                _extensions[SelectedItems[0]] : null;
        }

        public void Install(ExtensionBase extension)
        {
            if (extension == null) return;

            ListViewItem item = FocusAdd(extension.Name,
                extension.Author, extension.Version.ToString());

            _items[extension] = item;
            _extensions[item] = extension;

        }
        protected override void RemoveItem(ListViewItem listViewItem)
        {
            ExtensionBase extension = GetItemExtension();
            if (extension == null) return;
            ((Contractor)extension.Contractor).Uninstall(extension);

            _items.Remove(extension);
            _extensions.Remove(listViewItem);

            base.RemoveItem(listViewItem);
        }
    }
}