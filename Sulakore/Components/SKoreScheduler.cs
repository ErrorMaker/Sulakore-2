using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    public class SKoreScheduler : ListView
    {
        #region Subscribable Events
        public event EventHandler<HScheduleTriggeredEventArgs> ScheduleTriggered;
        private void OnScheduleTriggered(object sender, HScheduleTriggeredEventArgs e)
        {
            if (ScheduleTriggered != null)
            {
                ScheduleTriggered(sender, e);
                if (e.Cancel)
                {
                    SchedulesRunning--;
                    _bySchedule[(HSchedule)sender].SubItems[4].Text = StatusSTOPPED;
                }
            }
        }
        #endregion

        #region Private Fields
        private bool _suppressSelectionChanged;

        private readonly Dictionary<HSchedule, ListViewItem> _bySchedule;
        private readonly Dictionary<ListViewItem, HSchedule> _schedules;

        private const string StatusRUNNING = "Running", StatusSTOPPED = "Stopped";
        #endregion

        #region Public Properties
        public bool LockColumns { get; set; }
        public int SchedulesRunning { get; private set; }
        #endregion

        #region Constructor(s)
        public SKoreScheduler()
        {
            _schedules = new Dictionary<ListViewItem, HSchedule>();
            _bySchedule = new Dictionary<HSchedule, ListViewItem>();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            var packetCol = new ColumnHeader { Name = "PacketCol", Text = "Packet", Width = 131 };
            var directionCol = new ColumnHeader { Name = "DirectionCol", Text = "Direction", Width = 63 };
            var burstCol = new ColumnHeader { Name = "BurstCol", Text = "Burst", Width = 44 };
            var intervalCol = new ColumnHeader { Name = "IntervalCol", Text = "Interval", Width = 58 };
            var statusCol = new ColumnHeader { Name = "StatusCol", Text = "Status", Width = 62 };
            Columns.AddRange(new[] { packetCol, directionCol, burstCol, intervalCol, statusCol });

            FullRowSelect = true;
            GridLines = true;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
            MultiSelect = false;
            ShowItemToolTips = true;
            Size = new Size(386, 141);
            UseCompatibleStateImageBehavior = false;
            View = View.Details;
            LockColumns = true;
        }
        #endregion

        #region Public Methods
        public void StopAll()
        {
            BeginUpdate();
            foreach (ListViewItem item in _schedules.Keys)
            {
                _schedules[item].Stop();
                item.SubItems[4].Text = StatusSTOPPED;
            }
            EndUpdate();

            SchedulesRunning = 0;
        }
        public void StartAll()
        {
            BeginUpdate();
            foreach (ListViewItem item in _schedules.Keys)
            {
                _schedules[item].Start();
                item.SubItems[4].Text = StatusRUNNING;
            }
            EndUpdate();

            SchedulesRunning = _schedules.Count;
        }
        public void ToggleSelected()
        {
            ListViewItem selectedItem = SelectedItems[0];
            if (_schedules.ContainsKey(selectedItem))
            {
                HSchedule schedule = _schedules[selectedItem];
                bool shouldStop = (schedule.IsRunning);

                if (shouldStop)
                {
                    schedule.Stop();
                    SchedulesRunning--;
                }
                else
                {
                    schedule.Start();
                    SchedulesRunning++;
                }
                selectedItem.SubItems[4].Text = (shouldStop ? StatusSTOPPED : StatusRUNNING);
            }
        }
        public void RemoveSelected()
        {
            ListViewItem selectedItem = SelectedItems[0];
            if (_schedules.ContainsKey(selectedItem))
            {
                HSchedule schedule = _schedules[selectedItem];

                if (schedule.IsRunning) SchedulesRunning--;
                schedule.Dispose();

                _schedules.Remove(selectedItem);
                _bySchedule.Remove(schedule);

                int index = SelectedIndices[0];
                _suppressSelectionChanged = (Items.Count > 1);
                Items.RemoveAt(index);

                if (Items.Count > 0)
                {
                    _suppressSelectionChanged = true;
                    Items[index - (index < Items.Count - 1 && index != 0 || index == Items.Count ? 1 : 0)].Selected = true;
                }
            }
        }
        public HSchedule GetSelected()
        {
            return _schedules[SelectedItems[0]];
        }

        public string GetSelectedDescription()
        {
            string desc = _bySchedule[GetSelected()].ToolTipText;
            return !string.IsNullOrEmpty(desc) ? desc.Substring(13).Split('\n')[0] : string.Empty;
        }
        public void SetSelectedDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
                _bySchedule[GetSelected()].ToolTipText = string.Format("Description: {0}\n{1}",
                    description, GetSelected().Packet);
        }

        public void AddSchedule(HSchedule schedule, bool autoStart, string description)
        {
            if (_schedules.ContainsValue(schedule)) return;

            var item = new ListViewItem(new string[]
            {
                schedule.Packet.ToString(),
                schedule.Packet.Destination.ToString(),
                schedule.Burst.ToString(),
                schedule.Interval.ToString(),
                autoStart ? StatusRUNNING : StatusSTOPPED
            });

            if (!string.IsNullOrEmpty(description))
                item.ToolTipText = string.Format("Description: {0}\n{1}",
                    description, schedule.Packet);

            Focus();
            Items.Add(item);
            _suppressSelectionChanged = Items.Count > 1;
            item.Selected = true;
            EnsureVisible(Items.Count - 1);

            _schedules.Add(item, schedule);
            _bySchedule.Add(schedule, item);

            schedule.ScheduleTriggered += OnScheduleTriggered;
            if (autoStart)
            {
                SchedulesRunning++;
                schedule.Start();
            }
        }
        #endregion

        #region Overrided Methods
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
        #endregion
    }
}