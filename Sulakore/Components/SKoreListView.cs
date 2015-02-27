using System.Windows.Forms;
using System.ComponentModel;

namespace Sulakore.Components
{
    public class SKoreListView : ListView
    {
        private bool _suppressSelectionChangedEvent;

        [DefaultValue(true)]
        public bool LockColumnWidth { get; set; }

        public SKoreListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            GridLines = true;
            MultiSelect = false;
            View = View.Details;
            FullRowSelect = true;
            HideSelection = false;
            LockColumnWidth = true;
            ShowItemToolTips = true;
            UseCompatibleStateImageBehavior = false;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        public void RemoveSelectedItem()
        {
            if (SelectedItems.Count < 1) return;
            RemoveItem(SelectedItems[0]);
        }
        protected virtual void RemoveItem(ListViewItem listViewItem)
        {
            int index = listViewItem.Index;
            bool selectNext = Items.Count - 1 > 0;

            _suppressSelectionChangedEvent = selectNext;
            Items.RemoveAt(index);

            if (selectNext)
            {
                if (index >= Items.Count)
                    index = Items.Count - 1;

                _suppressSelectionChangedEvent = true;
                Items[index].Selected = true;

                EnsureVisible(index);
            }
        }

        public void MoveSelectedItemUp()
        {
            if (SelectedItems.Count < 1) return;
            MoveItemUp(SelectedItems[0]);
        }
        protected virtual void MoveItemUp(ListViewItem listViewItem)
        {
            int oldIndex = listViewItem.Index;
            if (oldIndex < 1) return;

            _suppressSelectionChangedEvent = true;

            BeginUpdate();
            Items.RemoveAt(oldIndex);
            Items.Insert(oldIndex - 1, listViewItem);
            EndUpdate();

            _suppressSelectionChangedEvent = true;
            listViewItem.Selected = true;

            int index = listViewItem.Index;
            EnsureVisible(index <= 4 ? 0 : index - 4);
        }

        public void MoveSelectedItemDown()
        {
            if (SelectedItems.Count < 1) return;
            MoveItemDown(SelectedItems[0]);
        }
        protected virtual void MoveItemDown(ListViewItem listViewItem)
        {
            int oldIndex = listViewItem.Index;
            if (oldIndex == Items.Count - 1) return;

            _suppressSelectionChangedEvent = true;

            BeginUpdate();
            Items.RemoveAt(oldIndex);
            Items.Insert(oldIndex + 1, listViewItem);
            EndUpdate();

            _suppressSelectionChangedEvent = true;
            listViewItem.Selected = true;

            int index = listViewItem.Index;
            EnsureVisible(index + 4 >= Items.Count ? Items.Count - 1 : index + 4);
        }

        public void FocusAdd(ListViewItem listViewItem)
        {
            Focus();
            Items.Add(listViewItem);

            _suppressSelectionChangedEvent = Items.Count > 1;
            listViewItem.Selected = true;

            EnsureVisible(listViewItem.Index);
        }
        public ListViewItem FocusAdd(params string[] items)
        {
            var listViewItem = new ListViewItem(items);
            FocusAdd(listViewItem);
            return listViewItem;
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != 0x14)
                base.OnNotifyMessage(m);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _suppressSelectionChangedEvent = (GetItemAt(e.X, e.Y) != null);
            base.OnMouseDown(e);
        }
        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            if (LockColumnWidth && !DesignMode)
            {
                e.Cancel = true;
                e.NewWidth = Columns[e.ColumnIndex].Width;
            }
            base.OnColumnWidthChanging(e);
        }
        protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            if (_suppressSelectionChangedEvent && !e.IsSelected)
                _suppressSelectionChangedEvent = false;
            else
            {
                base.OnItemSelectionChanged(e);

                if (e.IsSelected)
                    _suppressSelectionChangedEvent = false;
            }
        }
    }
}