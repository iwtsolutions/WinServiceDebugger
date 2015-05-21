using System;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace WinServiceDebugger
{
    public partial class DebugForm : Form
    {
        private bool _continueToWatchEvents;
        private int _row;
        private int _col;

        public DebugForm()
        {
            InitializeComponent();
            new System.Threading.Thread(run).Start();

            addCopyMenu();
        }

        private void updateText(LoggingEvent logEvent)
        {
            if (base.InvokeRequired == true)
            {
                base.Invoke(new Action<LoggingEvent>(updateText), new object[] { logEvent });
                return;
            }

            string exception = logEvent.ExceptionObject == null
                ? logEvent.ExceptionObject.ToString()
                : string.Empty;

            dgvResults.Rows.Add(logEvent.LoggerName, logEvent.Level, logEvent.TimeStamp.ToString("hh:mm:ss tt"), logEvent.RenderedMessage, exception);
        }

        private void run()
        {
            // Help from: http://mail-archives.apache.org/mod_mbox/logging-log4net-user/200408.mbox/%3CD7A6243DF5CBBE458881FFFCEA85477233CC02@SFO-EX.provident.local%3E

            // Get the default hierarchy for log4net
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            // Get the appender named "MemoryAppender" from the <root> logger 
            var memoryAppender = (MemoryAppender)hierarchy.Root.GetAppender("MemoryAppender");

            _continueToWatchEvents = true;
            this.Disposed += FrmDebug_Disposed;

            while (_continueToWatchEvents == true)
            {
                var events = memoryAppender.GetEvents();
                memoryAppender.Clear();

                foreach (var e in events)
                {
                    updateText(e);
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        void FrmDebug_Disposed(object sender, EventArgs e)
        {
            _continueToWatchEvents = false;
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continueToWatchEvents = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvResults.Rows.Clear();
        }

        private void dgvResults_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _row = e.RowIndex;
                _col = e.ColumnIndex;

                if (_row >= 0 && _col >= 0)
                    dgvResults.Rows[_row].Cells[_col].Selected = true;
            }
        }

        private void addCopyMenu()
        {
            var menu = new ContextMenuStrip();
            var menuCopy = new ToolStripMenuItem("Copy");
            menuCopy.Click += menuCopy_Click;
            menu.Items.Add(menuCopy);
            dgvResults.ContextMenuStrip = menu;
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            // Hacky way, but required due to dgvResults being in a separate thread, and how Clipboard works...
            var tr = new System.Threading.Thread(copyToClipboard);
            tr.SetApartmentState(System.Threading.ApartmentState.STA);
            tr.Start();
        }

        private void copyToClipboard()
        {
            Clipboard.SetData(DataFormats.Text, dgvResults.Rows[_row].Cells[_col].Value.ToString());
        }
    }
}